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
import { formatAge } from '../../shared/i18n';
import { vaccinationApi } from '../../vaccination/infrastructure/vaccination.api';
import { medicalRecordsApi } from '../../medical-records/infrastructure/medical-records.api';

const props = defineProps({ petId: { type: String, required: true } });

const { t } = useI18n();
const router = useRouter();
const toast = useToast();

const patient = ref(null);
const card = ref(null);
const visits = ref([]);
const loading = ref(true);

const dialogOpen = ref(false);
const selectedDose = ref(null);

const breadcrumb = computed(() => [
  { label: t('patients.title'), route: { name: 'patients' } },
  { label: patient.value?.name ?? '' }
]);
const home = { icon: 'pi pi-home', route: { name: 'patients' } };

const description = computed(() => {
  const p = patient.value;
  if (!p) return '';

  // La edad de calendario, como en el listado. Mientras es cachorro se añaden
  // las semanas, que es como el esquema mide su edad.
  const age = p.ageInWeeks < 16
    ? `${formatAge(p.birthDate, t)} (${t('profile.weeks', { n: p.ageInWeeks })})`
    : formatAge(p.birthDate, t);

  return [t(`species.${p.species}`), p.breed, t(`sex.${p.sex}`), age,
    `${t('profile.owner')}: ${p.owner.fullName}`].filter(Boolean).join(' · ');
});

async function load() {
  loading.value = true;
  try {
    const [patientResponse, cardResponse, visitsResponse] = await Promise.all([
      patientsApi.get(props.petId),
      vaccinationApi.getCard(props.petId),
      medicalRecordsApi.listVisits(props.petId)
    ]);
    patient.value = patientResponse.data;
    card.value = cardResponse.data;
    visits.value = visitsResponse.data;
  } finally {
    loading.value = false;
  }
}

watch(() => props.petId, load, { immediate: true });

function openDialog(dose) {
  selectedDose.value = dose;
  dialogOpen.value = true;
}

async function afterRegister() {
  toast.add({
    severity: 'success',
    summary: t('dose.registered', { name: patient.value.name }),
    life: 4000
  });
  const { data } = await vaccinationApi.getCard(props.petId);
  card.value = data;
}
</script>

<template>
  <div class="vp-page">
    <Breadcrumb :home="home" :model="breadcrumb" class="breadcrumb">
      <template #item="{ item }">
        <RouterLink v-if="item.route" :to="item.route">
          <i v-if="item.icon" :class="item.icon" aria-hidden="true" />
          <span v-else>{{ item.label }}</span>
        </RouterLink>
        <span v-else>{{ item.label }}</span>
      </template>
    </Breadcrumb>

    <div v-if="loading" class="loading">
      <ProgressSpinner style="width: 40px; height: 40px" :aria-label="t('common.loading')" />
    </div>

    <template v-else-if="patient">
      <header class="profile">
        <div class="profile__identity">
          <span class="profile__avatar" aria-hidden="true">
            <i :class="patient.species === 'Canine' ? 'pi pi-heart-fill' : 'pi pi-heart'" />
          </span>
          <div>
            <h1>{{ patient.name }}</h1>
            <p class="vp-small vp-muted">{{ description }}</p>
          </div>
        </div>
        <StatusTag v-if="card" :status="card.status" card />
      </header>

      <Tabs value="card">
        <TabList>
          <Tab value="card">{{ t('profile.card') }}</Tab>
          <Tab value="history">{{ t('profile.history') }}</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="card">
            <VaccinationCardPanel
              :card="card"
              :patient="patient"
              @register-dose="openDialog"
            />
          </TabPanel>
          <TabPanel value="history">
            <MedicalRecordPanel
              :visits="visits"
              :owner="patient.owner.fullName"
              @new-visit="router.push({ name: 'new-visit', params: { petId } })"
            />
          </TabPanel>
        </TabPanels>
      </Tabs>

      <RegisterDoseDialog
        v-model:visible="dialogOpen"
        :pet-id="petId"
        :patient="patient"
        :dose="selectedDose"
        @registered="afterRegister"
      />
    </template>
  </div>
</template>

<style scoped>
.breadcrumb { margin-bottom: var(--vp-space-3); padding: 0; background: none; border: 0; }
.breadcrumb :deep(a) { color: var(--vp-neutral-600); text-decoration: none; }
.loading { display: grid; place-items: center; padding: var(--vp-space-6); }

.profile {
  display: flex; align-items: center; justify-content: space-between;
  gap: var(--vp-space-3); flex-wrap: wrap; margin-bottom: var(--vp-space-4);
}
.profile__identity { display: flex; align-items: center; gap: var(--vp-space-3); }
.profile__avatar {
  display: grid; place-items: center; width: 48px; height: 48px;
  border-radius: 999px; background: var(--vp-primary-50); color: var(--vp-primary-700);
}
:deep(.p-tabpanels) { padding: var(--vp-space-4) 0 0; background: none; }
:deep(.p-tablist-tab-list) { background: none; }
</style>
