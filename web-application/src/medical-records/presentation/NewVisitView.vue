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

const patient = ref(null);
const saving = ref(false);
const error = ref('');

const visit = ref({
  visitDate: new Date(),
  reason: '',
  findings: '',
  diagnosis: '',
  treatment: '',
  weightKg: null
});

const prescription = ref([]);

const breadcrumb = computed(() => [
  { label: t('patients.title'), route: { name: 'patients' } },
  { label: patient.value?.name ?? '', route: { name: 'patient', params: { petId: props.petId } } },
  { label: t('visit.title') }
]);
const home = { icon: 'pi pi-home', route: { name: 'patients' } };

const prescriptionNotice = computed(() => t('visit.prescriptionNotice', {
  owner: patient.value?.owner?.fullName ?? ''
}));

watch(() => props.petId, async (id) => {
  const { data } = await patientsApi.get(id);
  patient.value = data;
}, { immediate: true });

function addMedication() {
  prescription.value.push({ medication: '', dosage: '', duration: '' });
}

function removeMedication(index) {
  prescription.value.splice(index, 1);
}

function asISO(date) {
  return new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}

async function save() {
  error.value = '';
  saving.value = true;
  try {
    const complete = prescription.value.filter(
      (item) => item.medication.trim() && item.dosage.trim() && item.duration.trim());

    await medicalRecordsApi.registerVisit(props.petId, {
      visitDate: asISO(visit.value.visitDate),
      reason: visit.value.reason,
      findings: visit.value.findings || null,
      diagnosis: visit.value.diagnosis,
      treatment: visit.value.treatment || null,
      weightKg: visit.value.weightKg,
      prescription: complete.length ? complete : null
    });

    toast.add({
      severity: 'success',
      summary: t('visit.saved', { name: patient.value.name }),
      life: 4000
    });
    router.push({ name: 'patient', params: { petId: props.petId } });
  } catch (failure) {
    error.value = problemOf(failure).detail ?? t('common.unexpectedError');
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
        <span v-else>{{ item.label }}</span>
      </template>
    </Breadcrumb>

    <header class="vp-head">
      <div>
        <h1>{{ t('visit.title') }}</h1>
        <p v-if="patient" class="vp-small vp-muted">
          {{ patient.name }} · {{ t(`species.${patient.species}`) }}
        </p>
      </div>
    </header>

    <form @submit.prevent="save">
      <section class="vp-card">
        <div class="vp-card__body vp-form-grid">
          <div class="vp-form-grid vp-form-grid--2">
            <div class="vp-field">
              <label for="a-date">{{ t('visit.date') }}</label>
              <DatePicker
                v-model="visit.visitDate"
                input-id="a-date"
                date-format="dd/mm/yy"
                :max-date="new Date()"
                show-icon
              />
            </div>
            <div class="vp-field">
              <label for="a-weight">{{ t('visit.weight') }}</label>
              <InputNumber
                v-model="visit.weightKg"
                input-id="a-weight"
                :min-fraction-digits="1"
                :max-fraction-digits="2"
                :min="0"
                :max="200"
              />
            </div>
          </div>

          <div class="vp-field">
            <label for="a-reason">{{ t('visit.reason') }}</label>
            <InputText id="a-reason" v-model="visit.reason" required />
          </div>

          <div class="vp-field">
            <label for="a-findings">{{ t('visit.findings') }}</label>
            <Textarea id="a-findings" v-model="visit.findings" rows="3" auto-resize />
          </div>

          <div class="vp-field">
            <label for="a-diagnosis">{{ t('visit.diagnosis') }}</label>
            <Textarea id="a-diagnosis" v-model="visit.diagnosis" rows="2" auto-resize required />
          </div>

          <div class="vp-field">
            <label for="a-treatment">{{ t('visit.treatment') }}</label>
            <Textarea id="a-treatment" v-model="visit.treatment" rows="2" auto-resize />
          </div>
        </div>
      </section>

      <section class="vp-card">
        <div class="vp-card__body">
          <h3>{{ t('visit.prescription') }}</h3>

          <table v-if="prescription.length" class="prescription">
            <thead>
              <tr>
                <th>{{ t('visit.medication') }}</th>
                <th>{{ t('visit.dosage') }}</th>
                <th>{{ t('visit.duration') }}</th>
                <th><span class="sr-only">{{ t('visit.remove') }}</span></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, index) in prescription" :key="index">
                <td><InputText v-model="item.medication" :aria-label="t('visit.medication')" fluid /></td>
                <td><InputText v-model="item.dosage" :aria-label="t('visit.dosage')" fluid /></td>
                <td><InputText v-model="item.duration" :aria-label="t('visit.duration')" fluid /></td>
                <td>
                  <Button
                    type="button"
                    icon="pi pi-trash"
                    text
                    severity="danger"
                    :aria-label="t('visit.remove')"
                    @click="removeMedication(index)"
                  />
                </td>
              </tr>
            </tbody>
          </table>

          <Button
            type="button"
            :label="t('visit.add')"
            icon="pi pi-plus"
            text
            @click="addMedication"
          />

          <p v-if="prescription.length" class="vp-caption vp-muted notice">{{ prescriptionNotice }}</p>
        </div>
      </section>

      <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>

      <div class="vp-actions">
        <Button
          type="button"
          :label="t('common.cancel')"
          text
          @click="router.push({ name: 'patient', params: { petId } })"
        />
        <Button type="submit" :label="t('visit.save')" :loading="saving" />
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
.prescription { width: 100%; border-collapse: collapse; margin-bottom: var(--vp-space-2); }
.prescription th {
  text-align: left; padding-bottom: var(--vp-space-1);
  font-size: 12px; font-weight: 600; color: var(--vp-neutral-600);
  text-transform: uppercase; letter-spacing: .04em;
}
.prescription td { padding: 4px 8px 4px 0; vertical-align: top; }
.prescription td:last-child { width: 48px; padding-right: 0; }
.notice { margin-top: var(--vp-space-2); }
.sr-only {
  position: absolute; width: 1px; height: 1px; overflow: hidden;
  clip: rect(0 0 0 0); white-space: nowrap;
}
</style>
