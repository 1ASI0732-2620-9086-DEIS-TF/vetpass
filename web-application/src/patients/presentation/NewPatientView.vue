<script setup>
/**
 * Alta de un cliente y de su mascota (US06, US07). El aviso anticipa que al
 * guardar se generará la cartilla según la especie (US09), de modo que el
 * usuario sepa lo que el sistema hará por él.
 *
 * Las validaciones de especie, de fecha de nacimiento y de documento las
 * resuelve la API: la interfaz no duplica reglas de negocio, solo presenta lo
 * que aquella responde. Si el documento ya pertenece a un cliente, la
 * interfaz ofrece usarlo en lugar de registrar a la misma persona dos veces
 * (US06-E4).
 */
import { computed, ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';
import DatePicker from 'primevue/datepicker';
import Message from 'primevue/message';
import SelectButton from 'primevue/selectbutton';
import InputGroup from 'primevue/inputgroup';
import InputGroupAddon from 'primevue/inputgroupaddon';
import Breadcrumb from 'primevue/breadcrumb';
import { patientsApi } from '../infrastructure/patients.api';
import { problemOf } from '../../shared/infrastructure/http';
import { formatDocument } from '../../shared/i18n';

const { t } = useI18n();
const router = useRouter();
const toast = useToast();

const clientMode = ref('existing');
const clients = ref([]);
const clientId = ref(null);
const client = ref({ fullName: '', documentType: 'Dni', documentNumber: '', phoneNumber: '', email: '' });
const pet = ref({ name: '', species: 'Canine', breed: '', sex: 'Female', birthDate: null });

const saving = ref(false);
const error = ref('');
const clientNotice = ref('');

// Avisos que la API devuelve sobre un campo concreto: se muestran junto a ese
// campo y no al pie del formulario, conforme a la sección 4.1.2. El texto se
// compone a partir del código y de los valores que la API entrega; la regla
// sigue viviendo solo en la API.
const fieldErrors = ref({ phone: '', birth: '', document: '' });
const duplicate = ref(null);
watch(() => client.value.phoneNumber, () => { fieldErrors.value.phone = ''; });
watch(() => [client.value.documentType, client.value.documentNumber], () => {
  fieldErrors.value.document = '';
  duplicate.value = null;
});
watch(() => [pet.value.birthDate, pet.value.species], () => { fieldErrors.value.birth = ''; });

function fieldNotice(problem) {
  switch (problem.code) {
    case 'invalid-identity-document':
      fieldErrors.value.document = t(`registration.invalidDocument.${client.value.documentType}`);
      return true;
    case 'duplicate-client':
      duplicate.value = { id: problem.existingClientId, name: problem.existingClientName };
      fieldErrors.value.document = t('registration.duplicateClient', { name: problem.existingClientName });
      return true;
    case 'invalid-phone-number':
      fieldErrors.value.phone = t('registration.invalidPhone');
      return true;
    case 'future-birth-date':
      fieldErrors.value.birth = t('registration.futureBirth');
      return true;
    case 'implausible-birth-date':
      fieldErrors.value.birth = t('registration.implausibleBirth', {
        age: problem.ageInYears,
        max: problem.maximumAgeInYears,
        species: t(problem.species === 'Canine' ? 'species.canineFeminine' : 'species.felineFeminine')
      });
      return true;
    default:
      return false;
  }
}

const breadcrumb = computed(() => [{ label: t('patients.title'), route: { name: 'patients' } }]);
const home = { icon: 'pi pi-home', route: { name: 'patients' } };

const modeOptions = computed(() => [
  { label: t('registration.existingClient'), value: 'existing' },
  { label: t('registration.newClient'), value: 'new' }
]);
const documentOptions = computed(() => [
  { label: t('document.short.Dni'), value: 'Dni' },
  { label: t('document.short.ForeignerCard'), value: 'ForeignerCard' }
]);

// El documento acompaña al nombre para distinguir a dos clientes homónimos, y
// el filtro lo encuentra igual que al nombre.
const clientOptions = computed(() => clients.value.map((c) => ({
  id: c.id,
  label: `${c.fullName} · ${formatDocument(c, t)}`
})));

const speciesOptions = computed(() => [
  { label: t('species.Canine'), value: 'Canine' },
  { label: t('species.Feline'), value: 'Feline' }
]);
const sexOptions = computed(() => [
  { label: t('sex.Female'), value: 'Female' },
  { label: t('sex.Male'), value: 'Male' }
]);

const scheduleNotice = computed(() => {
  if (!pet.value.name) return t('registration.noticeWithoutName');
  return t('registration.notice', {
    name: pet.value.name,
    species: t(pet.value.species === 'Canine' ? 'species.canineMasculine' : 'species.felineMasculine')
  });
});

onMounted(async () => {
  const { data } = await patientsApi.listClients();
  clients.value = data;
  if (data.length) clientId.value = data[0].id;
  else clientMode.value = 'new';
});

/** El documento ya era de un cliente: se continúa con él. */
async function useExistingClient() {
  const { id } = duplicate.value;
  if (!clients.value.some((c) => c.id === id)) {
    const { data } = await patientsApi.listClients();
    clients.value = data;
  }
  clientId.value = id;
  clientMode.value = 'existing';
  client.value = { fullName: '', documentType: 'Dni', documentNumber: '', phoneNumber: '', email: '' };
  duplicate.value = null;
  fieldErrors.value.document = '';
}

function asISO(date) {
  if (!date) return null;
  return new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}

async function save() {
  error.value = '';
  fieldErrors.value = { phone: '', birth: '', document: '' };
  duplicate.value = null;
  clientNotice.value = '';
  saving.value = true;
  let justCreatedClient = false;
  try {
    let ownerId = clientId.value;

    if (clientMode.value === 'new') {
      const { data } = await patientsApi.registerClient({
        fullName: client.value.fullName,
        documentType: client.value.documentType,
        documentNumber: client.value.documentNumber,
        phoneNumber: client.value.phoneNumber,
        email: client.value.email || null
      });
      ownerId = data.id;

      // El cliente ya existe. Si la mascota es rechazada, un segundo intento
      // debe asociarla a este cliente y no crear otro igual: el formulario
      // pasa a «cliente ya registrado» con él seleccionado.
      clients.value = [...clients.value, data];
      clientId.value = data.id;
      clientMode.value = 'existing';
      justCreatedClient = true;
    }

    const { data: created } = await patientsApi.registerPet({
      clientId: ownerId,
      name: pet.value.name,
      species: pet.value.species,
      breed: pet.value.breed || null,
      sex: pet.value.sex,
      birthDate: asISO(pet.value.birthDate)
    });

    toast.add({
      severity: 'success',
      summary: t('registration.saved', { name: created.name }),
      life: 4000
    });
    router.push({ name: 'patient', params: { petId: created.id } });
  } catch (failure) {
    const problem = problemOf(failure);
    if (!fieldNotice(problem)) error.value = problem.detail ?? t('common.unexpectedError');
    if (justCreatedClient) clientNotice.value = t('registration.clientAlreadyCreated');
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <div class="vp-page vp-page--narrow">
    <Breadcrumb :home="home" :model="breadcrumb" class="breadcrumb">
      <template #item="{ item }">
        <RouterLink v-if="item.route" :to="item.route">
          <i v-if="item.icon" :class="item.icon" aria-hidden="true" />
          <span v-else>{{ item.label }}</span>
        </RouterLink>
      </template>
    </Breadcrumb>

    <header class="vp-head">
      <h1>{{ t('registration.title') }}</h1>
    </header>

    <form @submit.prevent="save">
      <section class="vp-card">
        <div class="vp-card__body">
          <h3>{{ t('registration.clientDetails') }}</h3>

          <SelectButton
            v-model="clientMode"
            :options="modeOptions"
            option-label="label"
            option-value="value"
            :allow-empty="false"
            class="mode"
          />

          <div v-if="clientMode === 'existing'" class="vp-field">
            <label for="client">{{ t('registration.selectClient') }}</label>
            <Select
              v-model="clientId"
              input-id="client"
              :options="clientOptions"
              option-label="label"
              option-value="id"
              filter
              required
            />
          </div>

          <div v-else class="vp-form-grid vp-form-grid--2">
            <div class="vp-field">
              <label for="c-name">{{ t('registration.fullName') }}</label>
              <InputText id="c-name" v-model="client.fullName" required />
            </div>
            <div class="vp-field">
              <label for="c-document">{{ t('registration.documentType') }}</label>
              <InputGroup>
                <Select
                  v-model="client.documentType"
                  :options="documentOptions"
                  option-label="label"
                  option-value="value"
                  :aria-label="t('registration.documentType')"
                  class="type-document"
                />
                <InputText
                  id="c-document"
                  v-model="client.documentNumber"
                  :inputmode="client.documentType === 'Dni' ? 'numeric' : 'text'"
                  :maxlength="client.documentType === 'Dni' ? 8 : 14"
                  :aria-label="t('registration.documentNumber')"
                  :invalid="Boolean(fieldErrors.document)"
                  aria-describedby="c-document-help"
                  required
                />
              </InputGroup>
              <div v-if="fieldErrors.document" id="c-document-help" class="field-error duplicate">
                <small>{{ fieldErrors.document }}</small>
                <Button
                  v-if="duplicate"
                  type="button"
                  :label="t('registration.useClient')"
                  icon="pi pi-user"
                  size="small"
                  text
                  @click="useExistingClient"
                />
              </div>
              <span v-else id="c-document-help" class="vp-caption vp-muted">
                {{ t(`registration.documentHelp.${client.documentType}`) }}
              </span>
            </div>
            <div class="vp-field">
              <label for="c-phone">{{ t('registration.phone') }}</label>
              <InputGroup>
                <InputGroupAddon>+51</InputGroupAddon>
                <InputText
                  id="c-phone"
                  v-model="client.phoneNumber"
                  inputmode="tel"
                  autocomplete="tel-national"
                  placeholder="987 654 321"
                  :invalid="Boolean(fieldErrors.phone)"
                  aria-describedby="c-phone-help"
                  required
                />
              </InputGroup>
              <small v-if="fieldErrors.phone" id="c-phone-help" class="field-error">
                {{ fieldErrors.phone }}
              </small>
              <span v-else id="c-phone-help" class="vp-caption vp-muted">{{ t('registration.phoneHelp') }}</span>
            </div>
            <div class="vp-field">
              <label for="c-email">{{ t('registration.email') }}</label>
              <InputText id="c-email" v-model="client.email" type="email" />
            </div>
          </div>
        </div>
      </section>

      <section class="vp-card">
        <div class="vp-card__body">
          <h3>{{ t('registration.petDetails') }}</h3>

          <div class="vp-form-grid vp-form-grid--2">
            <div class="vp-field">
              <label for="m-name">{{ t('registration.name') }}</label>
              <InputText id="m-name" v-model="pet.name" required />
            </div>
            <div class="vp-field">
              <label for="m-species">{{ t('patients.species') }}</label>
              <Select
                v-model="pet.species"
                input-id="m-species"
                :options="speciesOptions"
                option-label="label"
                option-value="value"
              />
            </div>
            <div class="vp-field">
              <label for="m-breed">{{ t('registration.breed') }}</label>
              <InputText id="m-breed" v-model="pet.breed" />
            </div>
            <div class="vp-field">
              <label for="m-sex">{{ t('registration.sex') }}</label>
              <Select
                v-model="pet.sex"
                input-id="m-sex"
                :options="sexOptions"
                option-label="label"
                option-value="value"
              />
            </div>
            <div class="vp-field">
              <label for="m-birth">{{ t('registration.birthDate') }}</label>
              <DatePicker
                v-model="pet.birthDate"
                input-id="m-birth"
                date-format="dd/mm/yy"
                :max-date="new Date()"
                :invalid="Boolean(fieldErrors.birth)"
                aria-describedby="m-birth-error"
                show-icon
                required
              />
              <small v-if="fieldErrors.birth" id="m-birth-error" class="field-error">
                {{ fieldErrors.birth }}
              </small>
            </div>
          </div>
        </div>
      </section>

      <Message severity="info" :closable="false" class="notice">
        <strong>{{ t('registration.noticeTitle') }}</strong>
        <span class="vp-small">{{ scheduleNotice }}</span>
      </Message>

      <Message v-if="clientNotice" severity="warn" :closable="false">{{ clientNotice }}</Message>
      <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>

      <div class="vp-actions">
        <Button
          type="button"
          :label="t('common.cancel')"
          text
          @click="router.push({ name: 'patients' })"
        />
        <Button type="submit" :label="t('registration.save')" :loading="saving" />
      </div>
    </form>
  </div>
</template>

<style scoped>
.vp-page--narrow { max-width: 880px; }
.breadcrumb { margin-bottom: var(--vp-space-3); padding: 0; background: none; border: 0; }
.breadcrumb :deep(a) { color: var(--vp-neutral-600); text-decoration: none; }
form { display: grid; gap: var(--vp-space-3); }
h3 { margin-bottom: var(--vp-space-3); }
.mode { margin-bottom: var(--vp-space-3); }
.notice :deep(.p-message-text) { display: grid; gap: 4px; }
.field-error { color: var(--vp-danger); font-size: 13px; line-height: 18px; }
.duplicate { display: flex; align-items: center; justify-content: space-between; gap: var(--vp-space-2); flex-wrap: wrap; }
/* Un aviso bajo un campo no debe estirar a su vecino de fila. */
.vp-form-grid { align-items: start; }
.p-inputgroup .type-document { flex: 0 0 6.5rem; width: 6.5rem; }
</style>
