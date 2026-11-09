<script setup>
/**
 * Registro de una dosis aplicada (US10), en sus dos resultados posibles.
 *
 * La validación corre al ingresar la fecha y no al confirmar, conforme al
 * criterio de la sección 4.1.2: el usuario no descubre el error al final. La
 * comprobación que hace la interfaz se apoya en la fecha más temprana
 * admisible que la API ya calculó para esa dosis, de modo que no duplica la
 * regla del dominio; la autoridad sigue siendo la API, que responde 422 con
 * la regla incumplida, su valor exigido y la fecha más temprana admisible.
 *
 * Esa fecha no es la esperada: la esperada nunca cae antes de hoy, mientras
 * que una dosis aplicada hace años —el historial de una mascota adulta— se
 * registra con su fecha real.
 */
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import Dialog from 'primevue/dialog';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import DatePicker from 'primevue/datepicker';
import Message from 'primevue/message';
import { vaccinationApi } from '../infrastructure/vaccination.api';
import { problemOf } from '../../shared/infrastructure/http';
import { formatDate, doseLabel } from '../../shared/i18n';

const props = defineProps({
  visible: { type: Boolean, default: false },
  petId: { type: String, required: true },
  patient: { type: Object, default: null },
  dose: { type: Object, default: null }
});

const emit = defineEmits(['update:visible', 'registered', 'another-dose']);

const { t } = useI18n();

const date = ref(new Date());
const batch = ref('');
const sending = ref(false);
const rejection = ref(null);

watch(() => props.visible, (open) => {
  if (!open) return;
  date.value = new Date();
  batch.value = '';
  rejection.value = null;
});

const dateISO = computed(() => {
  const f = date.value;
  if (!f) return null;
  return new Date(f.getTime() - f.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
});

const todayISO = new Date().toLocaleDateString('sv-SE');

/** Aviso previo al envío, contra la fecha más temprana que la API admite. */
const localNotice = computed(() => {
  if (!props.dose || !dateISO.value) return null;

  if (dateISO.value > todayISO) {
    return { severity: 'error', text: t('dose.future', { date: formatDate(todayISO) }) };
  }

  const min = props.dose.earliestAdmissibleDate;
  if (min && dateISO.value < min) {
    return {
      severity: 'warn',
      text: t('dose.notYet', { date: formatDate(min) })
    };
  }

  return { severity: 'success', text: t('dose.valid') };
});

const canRegister = computed(() =>
  Boolean(batch.value.trim()) && localNotice.value?.severity === 'success' && !rejection.value);

async function register() {
  sending.value = true;
  rejection.value = null;
  try {
    await vaccinationApi.registerDose(props.petId, props.dose.id, {
      applicationDate: dateISO.value,
      batchCode: batch.value.trim()
    });
    emit('registered');
    close();
  } catch (failure) {
    rejection.value = problemOf(failure);
  } finally {
    sending.value = false;
  }
}

/**
 * El rechazo en el idioma de la interfaz cuando el código lo permite; si no,
 * el detalle que redactó la API.
 */
const rejectionText = computed(() => {
  const r = rejection.value;
  if (r?.code === 'dose-out-of-order' && props.dose) {
    const missing = { ...props.dose, sequenceNumber: r.pendingSequenceNumber, isBooster: false };
    return t('dose.outOfOrder', { dose: doseLabel(missing, t) });
  }
  return r?.detail;
});

function close() { emit('update:visible', false); }

function anotherDose() { emit('another-dose'); close(); }
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    :header="t('dose.title')"
    :style="{ width: '32rem' }"
    :breakpoints="{ '640px': '95vw' }"
    @update:visible="emit('update:visible', $event)"
  >
    <p v-if="patient" class="vp-small vp-muted patient">
      {{ patient.name }} · {{ t(`species.${patient.species}`) }} ·
      {{ t('profile.weeks', { n: patient.ageInWeeks }) }}
    </p>

    <form class="form" @submit.prevent="register">
      <div class="vp-field">
        <label for="d-vaccine">{{ t('dose.vaccine') }}</label>
        <InputText id="d-vaccine" :value="dose ? doseLabel(dose, t) : ''" readonly />
      </div>

      <div class="vp-field">
        <label for="d-date">{{ t('dose.applicationDate') }}</label>
        <DatePicker
          v-model="date"
          input-id="d-date"
          date-format="dd/mm/yy"
          :max-date="new Date()"
          show-icon
        />
        <span class="vp-caption vp-muted">{{ t('dose.historical') }}</span>
      </div>

      <!-- Resultado de la validación: el mensaje nombra la regla y su valor
           exigido, no un aviso genérico (sección 4.6.3). -->
      <Message
        v-if="rejection"
        severity="error"
        :closable="false"
        class="result"
      >
        <strong>{{ rejection.title }}</strong>
        <span class="vp-small">{{ rejectionText }}</span>
      </Message>

      <Message
        v-else-if="localNotice"
        :severity="localNotice.severity"
        :closable="false"
        class="result"
      >
        <span class="vp-small">{{ localNotice.text }}</span>
      </Message>

      <div class="vp-field">
        <label for="d-batch">{{ t('dose.batch') }}</label>
        <InputText id="d-batch" v-model="batch" required />
      </div>
    </form>

    <template #footer>
      <Button
        v-if="rejection"
        :label="t('dose.another')"
        text
        @click="anotherDose"
      />
      <Button :label="t('common.cancel')" text @click="close" />
      <Button
        :label="t('dose.register')"
        :disabled="!canRegister"
        :loading="sending"
        @click="register"
      />
    </template>
  </Dialog>
</template>

<style scoped>
.patient { margin-bottom: var(--vp-space-3); }
.form { display: grid; gap: var(--vp-space-3); }
.result :deep(.p-message-text) { display: grid; gap: 2px; }
</style>
