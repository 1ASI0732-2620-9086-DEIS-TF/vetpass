<script setup>
/**
 * Cartilla de vacunación de un paciente (US09, US11). Reproduce la estructura
 * tabular de la cartilla física, para que el usuario no tenga que reaprender
 * un formato que ya conoce (sección 4.2.1).
 *
 * Solo la dosis que su serie espera admite registro (US10-E5). Si es o no la
 * siguiente lo decide la API; aquí solo se nombra la que va primero.
 */
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import Button from 'primevue/button';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import StatusTag from '../../shared/presentation/StatusTag.vue';
import { formatDate, isToday, doseLabel } from '../../shared/i18n';

const props = defineProps({
  card: { type: Object, default: null },
  patient: { type: Object, default: null }
});

const emit = defineEmits(['register-dose']);

const { t } = useI18n();

const species = computed(() => (props.card?.species === 'Canine'
  ? t('species.canineMasculine') : t('species.felineMasculine')));

const next = computed(() => {
  const dose = props.card?.nextDose;
  if (!dose) return null;

  const date = isToday(dose.expectedDate)
    ? t('card.today', { date: formatDate(dose.expectedDate) })
    : formatDate(dose.expectedDate);

  return t('card.nextDose', { date });
});

/** La dosis pendiente más temprana de la misma vacuna: la que va primero. */
function previousPending(dose) {
  return props.card.doses
    .filter((d) => d.vaccineId === dose.vaccineId && d.sequenceNumber < dose.sequenceNumber
      && d.status !== 'Applied')
    .sort((a, b) => a.sequenceNumber - b.sequenceNumber)[0];
}

const rowClass = (dose) => (isToday(dose.expectedDate) && dose.status === 'Pending'
  ? 'vp-row-today' : null);
</script>

<template>
  <section v-if="card" class="vp-card">
    <header class="header">
      <div>
        <h3>{{ t('card.schedule', { species }) }}</h3>
        <p v-if="next" class="vp-small vp-muted">{{ next }}</p>
        <p v-else class="vp-small vp-muted">{{ t('card.nothingPending') }}</p>
      </div>
    </header>

    <DataTable :value="card.doses" data-key="id" :row-class="rowClass">
      <Column :header="t('card.vaccine')">
        <template #body="{ data }">
          <span class="vaccine">{{ doseLabel(data, t) }}</span>
        </template>
      </Column>

      <Column :header="t('card.date')">
        <template #body="{ data }">
          <span v-if="data.applicationDate">{{ formatDate(data.applicationDate) }}</span>
          <span v-else class="vp-muted">
            {{ t('card.expected', { date: formatDate(data.expectedDate) }) }}
          </span>
        </template>
      </Column>

      <Column :header="t('card.batch')">
        <template #body="{ data }">{{ data.batchCode ?? '—' }}</template>
      </Column>

      <Column :header="t('card.status')">
        <template #body="{ data }">
          <StatusTag :status="data.status === 'Applied' ? 'Applied' : (data.isOverdue ? 'Overdue' : 'Pending')" />
        </template>
      </Column>

      <Column class="column-action">
        <template #body="{ data }">
          <Button
            v-if="data.status === 'Pending' && data.isNextInSequence"
            :label="t('card.registerDose')"
            size="small"
            outlined
            @click="emit('register-dose', data)"
          />
          <span v-else-if="data.status === 'Pending' && previousPending(data)" class="vp-caption vp-muted en-waiting">
            <i class="pi pi-lock" aria-hidden="true" />
            {{ t('card.beforeTheDose', { dose: doseLabel(previousPending(data), t) }) }}
          </span>
        </template>
      </Column>
    </DataTable>

    <footer class="footer vp-caption vp-muted">{{ t('card.note') }}</footer>
  </section>
</template>

<style scoped>
.header { padding: var(--vp-space-4); border-bottom: 1px solid var(--vp-neutral-200); }
/* El nombre de la especie llega en minúscula porque en español va dentro de la
   frase; aquí encabeza, de modo que la mayúscula la pone la presentación. */
.header h3::first-letter { text-transform: uppercase; }
.vaccine { font-weight: 600; }
.footer { padding: var(--vp-space-3) var(--vp-space-4); border-top: 1px solid var(--vp-neutral-200); }
:deep(.column-action) { text-align: right; }
.en-waiting { display: inline-flex; gap: 6px; align-items: center; white-space: nowrap; }
</style>
