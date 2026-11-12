<script setup>
/**
 * Listado y búsqueda de pacientes (US08). El campo acepta indistintamente el
 * nombre de la mascota o el de su dueño, sin exigir elegir el criterio de
 * antemano, y los filtros de especie y estado responden a la necesidad de
 * identificar a los pacientes con dosis vencidas sin revisarlos uno por uno
 * (secciones 3.1 y 4.2.4).
 */
import { computed, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import Button from 'primevue/button';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Select from 'primevue/select';
import ProgressSpinner from 'primevue/progressspinner';
import StatusTag from '../../shared/presentation/StatusTag.vue';
import { patientsApi } from '../infrastructure/patients.api';
import { formatDate, formatAge } from '../../shared/i18n';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();

const patients = ref([]);
const loading = ref(false);
const species = ref(null);
const status = ref(null);

const speciesOptions = computed(() => [
  { label: t('patients.allSpecies'), value: null },
  { label: t('species.Canine'), value: 'Canine' },
  { label: t('species.Feline'), value: 'Feline' }
]);

const statusOptions = computed(() => [
  { label: t('patients.allStatuses'), value: null },
  { label: t('status.UpToDate'), value: 'UpToDate' },
  { label: t('status.Pending'), value: 'Pending' },
  { label: t('status.Overdue'), value: 'Overdue' }
]);

async function load() {
  loading.value = true;
  try {
    const { data } = await patientsApi.search({
      search: route.query.q?.toString(),
      species: species.value ?? undefined,
      cardStatus: status.value ?? undefined
    });
    patients.value = data;
  } finally {
    loading.value = false;
  }
}

watch([() => route.query.q, species, status], load, { immediate: true });

function open(patient) {
  router.push({ name: 'patient', params: { petId: patient.id } });
}
</script>

<template>
  <div class="vp-page">
    <header class="vp-head">
      <div>
        <h1>{{ t('patients.title') }}</h1>
        <p v-if="!loading" class="vp-small vp-muted">
          {{ patients.length }} {{ t('patients.count', patients.length) }}
        </p>
      </div>
      <Button
        :label="t('patients.register')"
        icon="pi pi-plus"
        @click="router.push({ name: 'new-patient' })"
      />
    </header>

    <div class="filters">
      <div class="vp-field">
        <label for="f-species" class="vp-caption vp-muted">{{ t('patients.species') }}</label>
        <Select
          v-model="species"
          input-id="f-species"
          :options="speciesOptions"
          option-label="label"
          option-value="value"
        />
      </div>
      <div class="vp-field">
        <label for="f-status" class="vp-caption vp-muted">{{ t('patients.cardStatus') }}</label>
        <Select
          v-model="status"
          input-id="f-status"
          :options="statusOptions"
          option-label="label"
          option-value="value"
        />
      </div>
    </div>

    <div v-if="loading" class="loading">
      <ProgressSpinner style="width: 40px; height: 40px" :aria-label="t('common.loading')" />
    </div>

    <div v-else class="vp-card">
      <DataTable
        :value="patients"
        data-key="id"
        selection-mode="single"
        :row-hover="true"
        @row-click="open($event.data)"
      >
        <template #empty>
          <div class="empty">
            <p>{{ t('patients.noResults') }}</p>
            <p class="vp-small vp-muted">{{ t('patients.noResultsAction') }}</p>
            <Button
              class="empty__action"
              :label="t('patients.register')"
              icon="pi pi-plus"
              outlined
              @click="router.push({ name: 'new-patient' })"
            />
          </div>
        </template>

        <Column field="name" :header="t('patients.pet')">
          <template #body="{ data }">
            <span class="name">{{ data.name }}</span>
            <span v-if="data.breed" class="vp-caption vp-muted"> · {{ data.breed }}</span>
          </template>
        </Column>

        <Column field="species" :header="t('patients.species')">
          <template #body="{ data }">{{ t(`species.${data.species}`) }}</template>
        </Column>

        <Column field="owner.fullName" :header="t('patients.owner')">
          <template #body="{ data }">{{ data.owner.fullName }}</template>
        </Column>

        <Column field="cardStatus" :header="t('patients.cardStatus')">
          <template #body="{ data }">
            <StatusTag v-if="data.cardStatus" :status="data.cardStatus" />
            <span v-else class="vp-muted">—</span>
          </template>
        </Column>

        <Column field="birthDate" :header="t('registration.birthDate')">
          <template #body="{ data }">{{ formatDate(data.birthDate) }}</template>
        </Column>

        <!-- Ordenar por edad es ordenar por nacimiento, al revés. -->
        <Column :header="t('patients.age')">
          <template #body="{ data }"><span class="age">{{ formatAge(data.birthDate, t) }}</span></template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<style scoped>
.filters { display: flex; gap: var(--vp-space-3); flex-wrap: wrap; margin-bottom: var(--vp-space-3); }
.filters .vp-field { min-width: 180px; }
.loading { display: grid; place-items: center; padding: var(--vp-space-6); }
.name { font-weight: 600; }
.age { white-space: nowrap; }
.empty { padding: var(--vp-space-5); text-align: center; display: grid; gap: var(--vp-space-2); }
.empty__action { justify-self: center; margin-top: var(--vp-space-2); }
:deep(.p-datatable-tbody > tr) { cursor: pointer; }
</style>
