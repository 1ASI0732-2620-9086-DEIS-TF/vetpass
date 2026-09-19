<script setup>
/**
 * Ficha del paciente. Desde aquí se alcanza su cartilla y su historial
 * mediante pestañas, que es el recorrido descendente de la sección 4.2.5: la
 * profundidad no pasa de tres niveles.
 */
import { computed, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import Breadcrumb from 'primevue/breadcrumb';
import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import ProgressSpinner from 'primevue/progressspinner';
import StatusTag from '../../shared/presentation/StatusTag.vue';
import VaccinationCardPanel from '../../vaccination/presentation/VaccinationCardPanel.vue';
import RegisterDoseDialog from '../../vaccination/presentation/RegisterDoseDialog.vue';
import MedicalRecordPanel from '../../medical-records/presentation/MedicalRecordPanel.vue';
import { patientsApi } from '../infrastructure/patients.api';
import { vaccinationApi } from '../../vaccination/infrastructure/vaccination.api';
import { medicalRecordsApi } from '../../medical-records/infrastructure/medical-records.api';

const props = defineProps({ petId: { type: String, required: true } });

const { t } = useI18n();
const router = useRouter();
const toast = useToast();

const paciente = ref(null);
const cartilla = ref(null);
const atenciones = ref([]);
const cargando = ref(true);

const dialogoAbierto = ref(false);
const dosisElegida = ref(null);

const migas = computed(() => [
  { label: t('pacientes.titulo'), route: { name: 'pacientes' } },
  { label: paciente.value?.name ?? '' }
]);
const inicio = { icon: 'pi pi-home', route: { name: 'pacientes' } };

const descripcion = computed(() => {
  const p = paciente.value;
  if (!p) return '';

  const edad = p.ageInWeeks < 52
    ? t('ficha.semanas', { n: p.ageInWeeks })
    : t('ficha.anos', Math.floor(p.ageInWeeks / 52), { named: { n: Math.floor(p.ageInWeeks / 52) } });

  return [t(`especie.${p.species}`), p.breed, t(`sexo.${p.sex}`), edad,
    `${t('ficha.dueno')}: ${p.owner.fullName}`].filter(Boolean).join(' · ');
});

async function cargar() {
  cargando.value = true;
  try {
    const [respuestaPaciente, respuestaCartilla, respuestaAtenciones] = await Promise.all([
      patientsApi.obtener(props.petId),
      vaccinationApi.obtenerCartilla(props.petId),
      medicalRecordsApi.listarAtenciones(props.petId)
    ]);
    paciente.value = respuestaPaciente.data;
    cartilla.value = respuestaCartilla.data;
    atenciones.value = respuestaAtenciones.data;
  } finally {
    cargando.value = false;
  }
}

watch(() => props.petId, cargar, { immediate: true });

function abrirDialogo(dosis) {
  dosisElegida.value = dosis;
  dialogoAbierto.value = true;
}

async function trasRegistrar() {
  toast.add({
    severity: 'success',
    summary: t('dosis.registrada', { nombre: paciente.value.name }),
    life: 4000
  });
  const { data } = await vaccinationApi.obtenerCartilla(props.petId);
  cartilla.value = data;
}
</script>

<template>
  <div class="vp-page">
    <Breadcrumb :home="inicio" :model="migas" class="migas">
      <template #item="{ item }">
        <RouterLink v-if="item.route" :to="item.route">
          <i v-if="item.icon" :class="item.icon" aria-hidden="true" />
          <span v-else>{{ item.label }}</span>
        </RouterLink>
        <span v-else>{{ item.label }}</span>
      </template>
    </Breadcrumb>

    <div v-if="cargando" class="cargando">
      <ProgressSpinner style="width: 40px; height: 40px" :aria-label="t('comun.cargando')" />
    </div>

    <template v-else-if="paciente">
      <header class="ficha">
        <div class="ficha__identidad">
          <span class="ficha__avatar" aria-hidden="true">
            <i :class="paciente.species === 'Canine' ? 'pi pi-heart-fill' : 'pi pi-heart'" />
          </span>
          <div>
            <h1>{{ paciente.name }}</h1>
            <p class="vp-small vp-muted">{{ descripcion }}</p>
          </div>
        </div>
        <StatusTag v-if="cartilla" :estado="cartilla.status" cartilla />
      </header>

      <Tabs value="cartilla">
        <TabList>
          <Tab value="cartilla">{{ t('ficha.cartilla') }}</Tab>
          <Tab value="historial">{{ t('ficha.historial') }}</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="cartilla">
            <VaccinationCardPanel
              :cartilla="cartilla"
              :paciente="paciente"
              @registrar-dosis="abrirDialogo"
            />
          </TabPanel>
          <TabPanel value="historial">
            <MedicalRecordPanel
              :atenciones="atenciones"
              :dueno="paciente.owner.fullName"
              @nueva-atencion="router.push({ name: 'nueva-atencion', params: { petId } })"
            />
          </TabPanel>
        </TabPanels>
      </Tabs>

      <RegisterDoseDialog
        v-model:visible="dialogoAbierto"
        :pet-id="petId"
        :paciente="paciente"
        :dosis="dosisElegida"
        @registrada="trasRegistrar"
      />
    </template>
  </div>
</template>

<style scoped>
.migas { margin-bottom: var(--vp-space-3); padding: 0; background: none; border: 0; }
.migas :deep(a) { color: var(--vp-neutral-600); text-decoration: none; }
.cargando { display: grid; place-items: center; padding: var(--vp-space-6); }

.ficha {
  display: flex; align-items: center; justify-content: space-between;
  gap: var(--vp-space-3); flex-wrap: wrap; margin-bottom: var(--vp-space-4);
}
.ficha__identidad { display: flex; align-items: center; gap: var(--vp-space-3); }
.ficha__avatar {
  display: grid; place-items: center; width: 48px; height: 48px;
  border-radius: 999px; background: var(--vp-primary-50); color: var(--vp-primary-700);
}
:deep(.p-tabpanels) { padding: var(--vp-space-4) 0 0; background: none; }
:deep(.p-tablist-tab-list) { background: none; }
</style>
