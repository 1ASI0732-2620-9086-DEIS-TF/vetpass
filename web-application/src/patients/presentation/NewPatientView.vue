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
import { formatearDocumento } from '../../shared/i18n';

const { t } = useI18n();
const router = useRouter();
const toast = useToast();

const modoCliente = ref('existente');
const clientes = ref([]);
const clienteId = ref(null);
const cliente = ref({ fullName: '', documentType: 'Dni', documentNumber: '', phoneNumber: '', email: '' });
const mascota = ref({ name: '', species: 'Canine', breed: '', sex: 'Female', birthDate: null });

const guardando = ref(false);
const error = ref('');
const avisoCliente = ref('');

// Avisos que la API devuelve sobre un campo concreto: se muestran junto a ese
// campo y no al pie del formulario, conforme a la sección 4.1.2. El texto se
// compone a partir del código y de los valores que la API entrega; la regla
// sigue viviendo solo en la API.
const erroresCampo = ref({ telefono: '', nacimiento: '', documento: '' });
const duplicado = ref(null);
watch(() => cliente.value.phoneNumber, () => { erroresCampo.value.telefono = ''; });
watch(() => [cliente.value.documentType, cliente.value.documentNumber], () => {
  erroresCampo.value.documento = '';
  duplicado.value = null;
});
watch(() => [mascota.value.birthDate, mascota.value.species], () => { erroresCampo.value.nacimiento = ''; });

function avisoDeCampo(problema) {
  switch (problema.code) {
    case 'invalid-identity-document':
      erroresCampo.value.documento = t(`registro.documentoInvalido.${cliente.value.documentType}`);
      return true;
    case 'duplicate-client':
      duplicado.value = { id: problema.existingClientId, nombre: problema.existingClientName };
      erroresCampo.value.documento = t('registro.clienteDuplicado', { nombre: problema.existingClientName });
      return true;
    case 'invalid-phone-number':
      erroresCampo.value.telefono = t('registro.telefonoInvalido');
      return true;
    case 'future-birth-date':
      erroresCampo.value.nacimiento = t('registro.nacimientoFuturo');
      return true;
    case 'implausible-birth-date':
      erroresCampo.value.nacimiento = t('registro.nacimientoImplausible', {
        edad: problema.ageInYears,
        maximo: problema.maximumAgeInYears,
        especie: t(problema.species === 'Canine' ? 'especie.canina' : 'especie.felina')
      });
      return true;
    default:
      return false;
  }
}

const migas = computed(() => [{ label: t('pacientes.titulo'), route: { name: 'pacientes' } }]);
const inicio = { icon: 'pi pi-home', route: { name: 'pacientes' } };

const opcionesModo = computed(() => [
  { etiqueta: t('registro.clienteExistente'), valor: 'existente' },
  { etiqueta: t('registro.clienteNuevo'), valor: 'nuevo' }
]);
const opcionesDocumento = computed(() => [
  { etiqueta: t('documento.corto.Dni'), valor: 'Dni' },
  { etiqueta: t('documento.corto.ForeignerCard'), valor: 'ForeignerCard' }
]);

// El documento acompaña al nombre para distinguir a dos clientes homónimos, y
// el filtro lo encuentra igual que al nombre.
const opcionesCliente = computed(() => clientes.value.map((c) => ({
  id: c.id,
  etiqueta: `${c.fullName} · ${formatearDocumento(c, t)}`
})));

const opcionesEspecie = computed(() => [
  { etiqueta: t('especie.Canine'), valor: 'Canine' },
  { etiqueta: t('especie.Feline'), valor: 'Feline' }
]);
const opcionesSexo = computed(() => [
  { etiqueta: t('sexo.Female'), valor: 'Female' },
  { etiqueta: t('sexo.Male'), valor: 'Male' }
]);

const avisoEsquema = computed(() => {
  if (!mascota.value.name) return t('registro.avisoSinNombre');
  return t('registro.aviso', {
    nombre: mascota.value.name,
    especie: t(mascota.value.species === 'Canine' ? 'especie.canino' : 'especie.felino')
  });
});

onMounted(async () => {
  const { data } = await patientsApi.listarClientes();
  clientes.value = data;
  if (data.length) clienteId.value = data[0].id;
  else modoCliente.value = 'nuevo';
});

/** El documento ya era de un cliente: se continúa con él. */
async function usarClienteExistente() {
  const { id } = duplicado.value;
  if (!clientes.value.some((c) => c.id === id)) {
    const { data } = await patientsApi.listarClientes();
    clientes.value = data;
  }
  clienteId.value = id;
  modoCliente.value = 'existente';
  cliente.value = { fullName: '', documentType: 'Dni', documentNumber: '', phoneNumber: '', email: '' };
  duplicado.value = null;
  erroresCampo.value.documento = '';
}

function comoISO(fecha) {
  if (!fecha) return null;
  return new Date(fecha.getTime() - fecha.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}

async function guardar() {
  error.value = '';
  erroresCampo.value = { telefono: '', nacimiento: '', documento: '' };
  duplicado.value = null;
  avisoCliente.value = '';
  guardando.value = true;
  let clienteRecienCreado = false;
  try {
    let duenoId = clienteId.value;

    if (modoCliente.value === 'nuevo') {
      const { data } = await patientsApi.registrarCliente({
        fullName: cliente.value.fullName,
        documentType: cliente.value.documentType,
        documentNumber: cliente.value.documentNumber,
        phoneNumber: cliente.value.phoneNumber,
        email: cliente.value.email || null
      });
      duenoId = data.id;

      // El cliente ya existe. Si la mascota es rechazada, un segundo intento
      // debe asociarla a este cliente y no crear otro igual: el formulario
      // pasa a «cliente ya registrado» con él seleccionado.
      clientes.value = [...clientes.value, data];
      clienteId.value = data.id;
      modoCliente.value = 'existente';
      clienteRecienCreado = true;
    }

    const { data: creada } = await patientsApi.registrarMascota({
      clientId: duenoId,
      name: mascota.value.name,
      species: mascota.value.species,
      breed: mascota.value.breed || null,
      sex: mascota.value.sex,
      birthDate: comoISO(mascota.value.birthDate)
    });

    toast.add({
      severity: 'success',
      summary: t('registro.guardado', { nombre: creada.name }),
      life: 4000
    });
    router.push({ name: 'paciente', params: { petId: creada.id } });
  } catch (fallo) {
    const problema = problemOf(fallo);
    if (!avisoDeCampo(problema)) error.value = problema.detail ?? t('comun.errorInesperado');
    if (clienteRecienCreado) avisoCliente.value = t('registro.clienteYaCreado');
  } finally {
    guardando.value = false;
  }
}
</script>

<template>
  <div class="vp-page vp-page--estrecha">
    <Breadcrumb :home="inicio" :model="migas" class="migas">
      <template #item="{ item }">
        <RouterLink v-if="item.route" :to="item.route">
          <i v-if="item.icon" :class="item.icon" aria-hidden="true" />
          <span v-else>{{ item.label }}</span>
        </RouterLink>
      </template>
    </Breadcrumb>

    <header class="vp-head">
      <h1>{{ t('registro.titulo') }}</h1>
    </header>

    <form @submit.prevent="guardar">
      <section class="vp-card">
        <div class="vp-card__body">
          <h3>{{ t('registro.datosCliente') }}</h3>

          <SelectButton
            v-model="modoCliente"
            :options="opcionesModo"
            option-label="etiqueta"
            option-value="valor"
            :allow-empty="false"
            class="modo"
          />

          <div v-if="modoCliente === 'existente'" class="vp-field">
            <label for="cliente">{{ t('registro.seleccionaCliente') }}</label>
            <Select
              input-id="cliente"
              v-model="clienteId"
              :options="opcionesCliente"
              option-label="etiqueta"
              option-value="id"
              filter
              required
            />
          </div>

          <div v-else class="vp-form-grid vp-form-grid--2">
            <div class="vp-field">
              <label for="c-nombre">{{ t('registro.nombreCompleto') }}</label>
              <InputText id="c-nombre" v-model="cliente.fullName" required />
            </div>
            <div class="vp-field">
              <label for="c-documento">{{ t('registro.tipoDocumento') }}</label>
              <InputGroup>
                <Select
                  v-model="cliente.documentType"
                  :options="opcionesDocumento"
                  option-label="etiqueta"
                  option-value="valor"
                  :aria-label="t('registro.tipoDocumento')"
                  class="tipo-documento"
                />
                <InputText
                  id="c-documento"
                  v-model="cliente.documentNumber"
                  :inputmode="cliente.documentType === 'Dni' ? 'numeric' : 'text'"
                  :maxlength="cliente.documentType === 'Dni' ? 8 : 14"
                  :aria-label="t('registro.numeroDocumento')"
                  :invalid="Boolean(erroresCampo.documento)"
                  aria-describedby="c-documento-ayuda"
                  required
                />
              </InputGroup>
              <div v-if="erroresCampo.documento" id="c-documento-ayuda" class="campo-error duplicado">
                <small>{{ erroresCampo.documento }}</small>
                <Button
                  v-if="duplicado"
                  type="button"
                  :label="t('registro.usarCliente')"
                  icon="pi pi-user"
                  size="small"
                  text
                  @click="usarClienteExistente"
                />
              </div>
              <span v-else id="c-documento-ayuda" class="vp-caption vp-muted">
                {{ t(`registro.documentoAyuda.${cliente.documentType}`) }}
              </span>
            </div>
            <div class="vp-field">
              <label for="c-telefono">{{ t('registro.telefono') }}</label>
              <InputGroup>
                <InputGroupAddon>+51</InputGroupAddon>
                <InputText
                  id="c-telefono"
                  v-model="cliente.phoneNumber"
                  inputmode="tel"
                  autocomplete="tel-national"
                  placeholder="987 654 321"
                  :invalid="Boolean(erroresCampo.telefono)"
                  aria-describedby="c-telefono-ayuda"
                  required
                />
              </InputGroup>
              <small v-if="erroresCampo.telefono" id="c-telefono-ayuda" class="campo-error">
                {{ erroresCampo.telefono }}
              </small>
              <span v-else id="c-telefono-ayuda" class="vp-caption vp-muted">{{ t('registro.telefonoAyuda') }}</span>
            </div>
            <div class="vp-field">
              <label for="c-correo">{{ t('registro.correo') }}</label>
              <InputText id="c-correo" v-model="cliente.email" type="email" />
            </div>
          </div>
        </div>
      </section>

      <section class="vp-card">
        <div class="vp-card__body">
          <h3>{{ t('registro.datosMascota') }}</h3>

          <div class="vp-form-grid vp-form-grid--2">
            <div class="vp-field">
              <label for="m-nombre">{{ t('registro.nombre') }}</label>
              <InputText id="m-nombre" v-model="mascota.name" required />
            </div>
            <div class="vp-field">
              <label for="m-especie">{{ t('pacientes.especie') }}</label>
              <Select
                input-id="m-especie"
                v-model="mascota.species"
                :options="opcionesEspecie"
                option-label="etiqueta"
                option-value="valor"
              />
            </div>
            <div class="vp-field">
              <label for="m-raza">{{ t('registro.raza') }}</label>
              <InputText id="m-raza" v-model="mascota.breed" />
            </div>
            <div class="vp-field">
              <label for="m-sexo">{{ t('registro.sexo') }}</label>
              <Select
                input-id="m-sexo"
                v-model="mascota.sex"
                :options="opcionesSexo"
                option-label="etiqueta"
                option-value="valor"
              />
            </div>
            <div class="vp-field">
              <label for="m-nacimiento">{{ t('registro.fechaNacimiento') }}</label>
              <DatePicker
                input-id="m-nacimiento"
                v-model="mascota.birthDate"
                date-format="dd/mm/yy"
                :max-date="new Date()"
                :invalid="Boolean(erroresCampo.nacimiento)"
                aria-describedby="m-nacimiento-error"
                show-icon
                required
              />
              <small v-if="erroresCampo.nacimiento" id="m-nacimiento-error" class="campo-error">
                {{ erroresCampo.nacimiento }}
              </small>
            </div>
          </div>
        </div>
      </section>

      <Message severity="info" :closable="false" class="aviso">
        <strong>{{ t('registro.avisoTitulo') }}</strong>
        <span class="vp-small">{{ avisoEsquema }}</span>
      </Message>

      <Message v-if="avisoCliente" severity="warn" :closable="false">{{ avisoCliente }}</Message>
      <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>

      <div class="vp-actions">
        <Button
          type="button"
          :label="t('comun.cancelar')"
          text
          @click="router.push({ name: 'pacientes' })"
        />
        <Button type="submit" :label="t('registro.guardar')" :loading="guardando" />
      </div>
    </form>
  </div>
</template>

<style scoped>
.vp-page--estrecha { max-width: 880px; }
.migas { margin-bottom: var(--vp-space-3); padding: 0; background: none; border: 0; }
.migas :deep(a) { color: var(--vp-neutral-600); text-decoration: none; }
form { display: grid; gap: var(--vp-space-3); }
h3 { margin-bottom: var(--vp-space-3); }
.modo { margin-bottom: var(--vp-space-3); }
.aviso :deep(.p-message-text) { display: grid; gap: 4px; }
.campo-error { color: var(--vp-danger); font-size: 13px; line-height: 18px; }
.duplicado { display: flex; align-items: center; justify-content: space-between; gap: var(--vp-space-2); flex-wrap: wrap; }
/* Un aviso bajo un campo no debe estirar a su vecino de fila. */
.vp-form-grid { align-items: start; }
.p-inputgroup .tipo-documento { flex: 0 0 6.5rem; width: 6.5rem; }
</style>
