<script setup>
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import { patientsApi } from '../infrastructure/patients.api';

const { t } = useI18n();
const clientes = ref([]);

onMounted(async () => {
  const { data } = await patientsApi.listarClientes();
  clientes.value = data;
});
</script>

<template>
  <div class="vp-page">
    <header class="vp-head">
      <h1>{{ t('clientes.titulo') }}</h1>
    </header>

    <div class="vp-card">
      <DataTable :value="clientes" data-key="id">
        <Column field="fullName" :header="t('clientes.nombre')" />
        <Column field="phoneNumber" :header="t('clientes.telefono')" />
        <Column field="email" :header="t('clientes.correo')">
          <template #body="{ data }">
            <span v-if="data.email">{{ data.email }}</span>
            <span v-else class="vp-muted">{{ t('clientes.sinCorreo') }}</span>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
