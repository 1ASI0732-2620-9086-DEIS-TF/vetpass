<script setup>
/**
 * Alta de un cliente y de su mascota (US06, US07). El aviso anticipa que al
 * guardar se generará la cartilla según la especie (US09), de modo que el
 * usuario sepa lo que el sistema hará por él.
 *
 * Las validaciones de especie y de fecha de nacimiento las resuelve la API:
 * la interfaz no duplica reglas de negocio, solo presenta lo que aquella
 * responde.
 */
import { computed, ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Select from 'primevue/select';
import DatePicker from 'primevue/datepicker';
import Message from 'primevue/message';
import SelectButton from 'primevue/selectbutton';
import Breadcrumb from 'primevue/breadcrumb';
import { patientsApi } from '../infrastructure/patients.api';
import { problemOf } from '../../shared/infrastructure/http';
import { hoyISO } from '../../shared/i18n';

const { t } = useI18n();
const router = useRouter();
const toast = useToast();

const modoCliente = ref('existente');
const clientes = ref([]);
const clienteId = ref(null);
const cliente = ref({ fullName: '', phoneNumber: '', email: '' });
const mascota = ref({ name: '', species: 'Canine', breed: '', sex: 'Female', birthDate: null });

const guardando = ref(false);
const error = ref('');

const migas = computed(() => [{ label: t('pacientes.titulo'), route: { name: 'pacientes' } }]);
const inicio = { icon: 'pi pi-home', route: { name: 'pacientes' } };

const opcionesModo = computed(() => [
  { etiqueta: t('registro.clienteExistente'), valor: 'existente' },
  { etiqueta: t('registro.clienteNuevo'), valor: 'nuevo' }
]);
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

function comoISO(fecha) {
  if (!fecha) return null;
  return new Date(fecha.getTime() - fecha.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}

async function guardar() {
  error.value = '';
  guardando.value = true;
  try {
    let duenoId = clienteId.value;

    if (modoCliente.value === 'nuevo') {
      const { data } = await patientsApi.registrarCliente({
        fullName: cliente.value.fullName,
        phoneNumber: cliente.value.phoneNumber,
        email: cliente.value.email || null
      });
      duenoId = data.id;
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
    error.value = problemOf(fallo).detail ?? t('comun.errorInesperado');
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
              :options="clientes"
              option-label="fullName"
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
              <label for="c-telefono">{{ t('registro.telefono') }}</label>
              <InputText id="c-telefono" v-model="cliente.phoneNumber" required />
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
                show-icon
                required
              />
            </div>
          </div>
        </div>
      </section>

      <Message severity="info" :closable="false" class="aviso">
        <strong>{{ t('registro.avisoTitulo') }}</strong>
        <span class="vp-small">{{ avisoEsquema }}</span>
      </Message>

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
</style>
