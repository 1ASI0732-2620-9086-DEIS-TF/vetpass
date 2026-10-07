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
import { formatearFecha, formatearEdad } from '../../shared/i18n';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();

const pacientes = ref([]);
const cargando = ref(false);
const especie = ref(null);
const estado = ref(null);

const opcionesEspecie = computed(() => [
  { etiqueta: t('pacientes.todas'), valor: null },
  { etiqueta: t('especie.Canine'), valor: 'Canine' },
  { etiqueta: t('especie.Feline'), valor: 'Feline' }
]);

const opcionesEstado = computed(() => [
  { etiqueta: t('pacientes.todos'), valor: null },
  { etiqueta: t('estado.UpToDate'), valor: 'UpToDate' },
  { etiqueta: t('estado.Pending'), valor: 'Pending' },
  { etiqueta: t('estado.Overdue'), valor: 'Overdue' }
]);

async function cargar() {
  cargando.value = true;
  try {
    const { data } = await patientsApi.buscar({
      search: route.query.q?.toString(),
      species: especie.value ?? undefined,
      cardStatus: estado.value ?? undefined
    });
    pacientes.value = data;
  } finally {
    cargando.value = false;
  }
}

watch([() => route.query.q, especie, estado], cargar, { immediate: true });

function abrir(paciente) {
  router.push({ name: 'paciente', params: { petId: paciente.id } });
}
</script>

<template>
  <div class="vp-page">
    <header class="vp-head">
      <div>
        <h1>{{ t('pacientes.titulo') }}</h1>
        <p v-if="!cargando" class="vp-small vp-muted">
          {{ pacientes.length }} {{ t('pacientes.conteo', pacientes.length) }}
        </p>
      </div>
      <Button
        :label="t('pacientes.registrar')"
        icon="pi pi-plus"
        @click="router.push({ name: 'nuevo-paciente' })"
      />
    </header>

    <div class="filtros">
      <div class="vp-field">
        <label for="f-especie" class="vp-caption vp-muted">{{ t('pacientes.especie') }}</label>
        <Select
          input-id="f-especie"
          v-model="especie"
          :options="opcionesEspecie"
          option-label="etiqueta"
          option-value="valor"
        />
      </div>
      <div class="vp-field">
        <label for="f-estado" class="vp-caption vp-muted">{{ t('pacientes.estadoCartilla') }}</label>
        <Select
          input-id="f-estado"
          v-model="estado"
          :options="opcionesEstado"
          option-label="etiqueta"
          option-value="valor"
        />
      </div>
    </div>

    <div v-if="cargando" class="cargando">
      <ProgressSpinner style="width: 40px; height: 40px" :aria-label="t('comun.cargando')" />
    </div>

    <div v-else class="vp-card">
      <DataTable
        :value="pacientes"
        data-key="id"
        selection-mode="single"
        :row-hover="true"
        @row-click="abrir($event.data)"
      >
        <template #empty>
          <div class="vacio">
            <p>{{ t('pacientes.sinResultados') }}</p>
            <p class="vp-small vp-muted">{{ t('pacientes.sinResultadosAccion') }}</p>
            <Button
              class="vacio__accion"
              :label="t('pacientes.registrar')"
              icon="pi pi-plus"
              outlined
              @click="router.push({ name: 'nuevo-paciente' })"
            />
          </div>
        </template>

        <Column field="name" :header="t('pacientes.mascota')">
          <template #body="{ data }">
            <span class="nombre">{{ data.name }}</span>
            <span v-if="data.breed" class="vp-caption vp-muted"> · {{ data.breed }}</span>
          </template>
        </Column>

        <Column field="species" :header="t('pacientes.especie')">
          <template #body="{ data }">{{ t(`especie.${data.species}`) }}</template>
        </Column>

        <Column field="owner.fullName" :header="t('pacientes.dueno')">
          <template #body="{ data }">{{ data.owner.fullName }}</template>
        </Column>

        <Column field="cardStatus" :header="t('pacientes.estadoCartilla')">
          <template #body="{ data }">
            <StatusTag v-if="data.cardStatus" :estado="data.cardStatus" />
            <span v-else class="vp-muted">—</span>
          </template>
        </Column>

        <Column field="birthDate" :header="t('registro.fechaNacimiento')">
          <template #body="{ data }">{{ formatearFecha(data.birthDate) }}</template>
        </Column>

        <!-- Ordenar por edad es ordenar por nacimiento, al revés. -->
        <Column :header="t('pacientes.edad')">
          <template #body="{ data }"><span class="edad">{{ formatearEdad(data.birthDate, t) }}</span></template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<style scoped>
.filtros { display: flex; gap: var(--vp-space-3); flex-wrap: wrap; margin-bottom: var(--vp-space-3); }
.filtros .vp-field { min-width: 180px; }
.cargando { display: grid; place-items: center; padding: var(--vp-space-6); }
.nombre { font-weight: 600; }
.edad { white-space: nowrap; }
.vacio { padding: var(--vp-space-5); text-align: center; display: grid; gap: var(--vp-space-2); }
.vacio__accion { justify-self: center; margin-top: var(--vp-space-2); }
:deep(.p-datatable-tbody > tr) { cursor: pointer; }
</style>
