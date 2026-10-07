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
import { formatearFecha, esHoy, etiquetaDosis } from '../../shared/i18n';

const props = defineProps({
  cartilla: { type: Object, default: null },
  paciente: { type: Object, default: null }
});

const emit = defineEmits(['registrar-dosis']);

const { t } = useI18n();

const especie = computed(() => (props.cartilla?.species === 'Canine'
  ? t('especie.canino') : t('especie.felino')));

const proxima = computed(() => {
  const dosis = props.cartilla?.nextDose;
  if (!dosis) return null;

  const fecha = esHoy(dosis.expectedDate)
    ? t('cartilla.hoy', { fecha: formatearFecha(dosis.expectedDate) })
    : formatearFecha(dosis.expectedDate);

  return t('cartilla.proximaDosis', { fecha });
});

/** La dosis pendiente más temprana de la misma vacuna: la que va primero. */
function anteriorPendiente(dosis) {
  return props.cartilla.doses
    .filter((d) => d.vaccineId === dosis.vaccineId && d.sequenceNumber < dosis.sequenceNumber
      && d.status !== 'Applied')
    .sort((a, b) => a.sequenceNumber - b.sequenceNumber)[0];
}

const claseFila = (dosis) => (esHoy(dosis.expectedDate) && dosis.status === 'Pending'
  ? 'vp-row-today' : null);
</script>

<template>
  <section v-if="cartilla" class="vp-card">
    <header class="encabezado">
      <div>
        <h3>{{ t('cartilla.esquema', { especie }) }}</h3>
        <p v-if="proxima" class="vp-small vp-muted">{{ proxima }}</p>
        <p v-else class="vp-small vp-muted">{{ t('cartilla.sinPendientes') }}</p>
      </div>
    </header>

    <DataTable :value="cartilla.doses" data-key="id" :row-class="claseFila">
      <Column :header="t('cartilla.vacuna')">
        <template #body="{ data }">
          <span class="vacuna">{{ etiquetaDosis(data, t) }}</span>
        </template>
      </Column>

      <Column :header="t('cartilla.fecha')">
        <template #body="{ data }">
          <span v-if="data.applicationDate">{{ formatearFecha(data.applicationDate) }}</span>
          <span v-else class="vp-muted">
            {{ t('cartilla.esperada', { fecha: formatearFecha(data.expectedDate) }) }}
          </span>
        </template>
      </Column>

      <Column :header="t('cartilla.lote')">
        <template #body="{ data }">{{ data.batchCode ?? '—' }}</template>
      </Column>

      <Column :header="t('cartilla.estado')">
        <template #body="{ data }">
          <StatusTag :estado="data.status === 'Applied' ? 'Applied' : (data.isOverdue ? 'Overdue' : 'Pending')" />
        </template>
      </Column>

      <Column class="columna-accion">
        <template #body="{ data }">
          <Button
            v-if="data.status === 'Pending' && data.isNextInSequence"
            :label="t('cartilla.registrarDosis')"
            size="small"
            outlined
            @click="emit('registrar-dosis', data)"
          />
          <span v-else-if="data.status === 'Pending' && anteriorPendiente(data)" class="vp-caption vp-muted en-espera">
            <i class="pi pi-lock" aria-hidden="true" />
            {{ t('cartilla.antesLaDosis', { dosis: etiquetaDosis(anteriorPendiente(data), t) }) }}
          </span>
        </template>
      </Column>
    </DataTable>

    <footer class="pie vp-caption vp-muted">{{ t('cartilla.nota') }}</footer>
  </section>
</template>

<style scoped>
.encabezado { padding: var(--vp-space-4); border-bottom: 1px solid var(--vp-neutral-200); }
/* El nombre de la especie llega en minúscula porque en español va dentro de la
   frase; aquí encabeza, de modo que la mayúscula la pone la presentación. */
.encabezado h3::first-letter { text-transform: uppercase; }
.vacuna { font-weight: 600; }
.pie { padding: var(--vp-space-3) var(--vp-space-4); border-top: 1px solid var(--vp-neutral-200); }
:deep(.columna-accion) { text-align: right; }
.en-espera { display: inline-flex; gap: 6px; align-items: center; white-space: nowrap; }
</style>
