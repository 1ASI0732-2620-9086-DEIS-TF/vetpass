<script setup>
/**
 * Registro de una atención con su receta (US13, US15). El formato es
 * preestablecido —motivo, hallazgos, diagnóstico y tratamiento— porque resulta
 * más rápido de completar que un campo de texto libre.
 *
 * Una receta sin medicamentos no puede emitirse (US15-E2): si no se agrega
 * ninguno, la atención se guarda sin receta.
 */
import { computed, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import Breadcrumb from 'primevue/breadcrumb';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import Textarea from 'primevue/textarea';
import DatePicker from 'primevue/datepicker';
import Message from 'primevue/message';
import { patientsApi } from '../../patients/infrastructure/patients.api';
import { medicalRecordsApi } from '../infrastructure/medical-records.api';
import { problemOf } from '../../shared/infrastructure/http';

const props = defineProps({ petId: { type: String, required: true } });

const { t } = useI18n();
const router = useRouter();
const toast = useToast();

const paciente = ref(null);
const guardando = ref(false);
const error = ref('');

const atencion = ref({
  visitDate: new Date(),
  reason: '',
  findings: '',
  diagnosis: '',
  treatment: '',
  weightKg: null
});

const receta = ref([]);

const migas = computed(() => [
  { label: t('pacientes.titulo'), route: { name: 'pacientes' } },
  { label: paciente.value?.name ?? '', route: { name: 'paciente', params: { petId: props.petId } } },
  { label: t('atencion.titulo') }
]);
const inicio = { icon: 'pi pi-home', route: { name: 'pacientes' } };

const avisoReceta = computed(() => t('atencion.avisoReceta', {
  dueno: paciente.value?.owner?.fullName ?? ''
}));

watch(() => props.petId, async (id) => {
  const { data } = await patientsApi.obtener(id);
  paciente.value = data;
}, { immediate: true });

function agregarMedicamento() {
  receta.value.push({ medication: '', dosage: '', duration: '' });
}

function quitarMedicamento(indice) {
  receta.value.splice(indice, 1);
}

function comoISO(fecha) {
  return new Date(fecha.getTime() - fecha.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}

async function guardar() {
  error.value = '';
  guardando.value = true;
  try {
    const completos = receta.value.filter(
      (item) => item.medication.trim() && item.dosage.trim() && item.duration.trim());

    await medicalRecordsApi.registrarAtencion(props.petId, {
      visitDate: comoISO(atencion.value.visitDate),
      reason: atencion.value.reason,
      findings: atencion.value.findings || null,
      diagnosis: atencion.value.diagnosis,
      treatment: atencion.value.treatment || null,
      weightKg: atencion.value.weightKg,
      prescription: completos.length ? completos : null
    });

    toast.add({
      severity: 'success',
      summary: t('atencion.guardada', { nombre: paciente.value.name }),
      life: 4000
    });
    router.push({ name: 'paciente', params: { petId: props.petId } });
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
        <span v-else>{{ item.label }}</span>
      </template>
    </Breadcrumb>

    <header class="vp-head">
      <div>
        <h1>{{ t('atencion.titulo') }}</h1>
        <p v-if="paciente" class="vp-small vp-muted">
          {{ paciente.name }} · {{ t(`especie.${paciente.species}`) }}
        </p>
      </div>
    </header>

    <form @submit.prevent="guardar">
      <section class="vp-card">
        <div class="vp-card__body vp-form-grid">
          <div class="vp-form-grid vp-form-grid--2">
            <div class="vp-field">
              <label for="a-fecha">{{ t('atencion.fecha') }}</label>
              <DatePicker
                input-id="a-fecha"
                v-model="atencion.visitDate"
                date-format="dd/mm/yy"
                :max-date="new Date()"
                show-icon
              />
            </div>
            <div class="vp-field">
              <label for="a-peso">{{ t('atencion.peso') }}</label>
              <InputNumber
                input-id="a-peso"
                v-model="atencion.weightKg"
                :min-fraction-digits="1"
                :max-fraction-digits="2"
                :min="0"
                :max="200"
              />
            </div>
          </div>

          <div class="vp-field">
            <label for="a-motivo">{{ t('atencion.motivo') }}</label>
            <InputText id="a-motivo" v-model="atencion.reason" required />
          </div>

          <div class="vp-field">
            <label for="a-hallazgos">{{ t('atencion.hallazgos') }}</label>
            <Textarea id="a-hallazgos" v-model="atencion.findings" rows="3" auto-resize />
          </div>

          <div class="vp-field">
            <label for="a-diagnostico">{{ t('atencion.diagnostico') }}</label>
            <Textarea id="a-diagnostico" v-model="atencion.diagnosis" rows="2" auto-resize required />
          </div>

          <div class="vp-field">
            <label for="a-tratamiento">{{ t('atencion.tratamiento') }}</label>
            <Textarea id="a-tratamiento" v-model="atencion.treatment" rows="2" auto-resize />
          </div>
        </div>
      </section>

      <section class="vp-card">
        <div class="vp-card__body">
          <h3>{{ t('atencion.receta') }}</h3>

          <table v-if="receta.length" class="receta">
            <thead>
              <tr>
                <th>{{ t('atencion.medicamento') }}</th>
                <th>{{ t('atencion.dosificacion') }}</th>
                <th>{{ t('atencion.duracion') }}</th>
                <th><span class="sr-only">{{ t('atencion.quitar') }}</span></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, indice) in receta" :key="indice">
                <td><InputText v-model="item.medication" :aria-label="t('atencion.medicamento')" fluid /></td>
                <td><InputText v-model="item.dosage" :aria-label="t('atencion.dosificacion')" fluid /></td>
                <td><InputText v-model="item.duration" :aria-label="t('atencion.duracion')" fluid /></td>
                <td>
                  <Button
                    type="button"
                    icon="pi pi-trash"
                    text
                    severity="danger"
                    :aria-label="t('atencion.quitar')"
                    @click="quitarMedicamento(indice)"
                  />
                </td>
              </tr>
            </tbody>
          </table>

          <Button
            type="button"
            :label="t('atencion.agregar')"
            icon="pi pi-plus"
            text
            @click="agregarMedicamento"
          />

          <p v-if="receta.length" class="vp-caption vp-muted aviso">{{ avisoReceta }}</p>
        </div>
      </section>

      <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>

      <div class="vp-actions">
        <Button
          type="button"
          :label="t('comun.cancelar')"
          text
          @click="router.push({ name: 'paciente', params: { petId } })"
        />
        <Button type="submit" :label="t('atencion.guardar')" :loading="guardando" />
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
.receta { width: 100%; border-collapse: collapse; margin-bottom: var(--vp-space-2); }
.receta th {
  text-align: left; padding-bottom: var(--vp-space-1);
  font-size: 12px; font-weight: 600; color: var(--vp-neutral-600);
  text-transform: uppercase; letter-spacing: .04em;
}
.receta td { padding: 4px 8px 4px 0; vertical-align: top; }
.receta td:last-child { width: 48px; padding-right: 0; }
.aviso { margin-top: var(--vp-space-2); }
.sr-only {
  position: absolute; width: 1px; height: 1px; overflow: hidden;
  clip: rect(0 0 0 0); white-space: nowrap;
}
</style>
