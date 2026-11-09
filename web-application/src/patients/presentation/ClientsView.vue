<script setup>
/**
 * Clientes de la clínica y su acceso a la aplicación móvil.
 *
 * El dueño no se registra por su cuenta: la clínica le crea el acceso y le
 * entrega una contraseña temporal en recepción (US05). Esta pantalla es el
 * lugar donde eso ocurre, y por eso distingue a quién ya se le entregó.
 *
 * Si el dueño olvida su contraseña, la clínica no puede verla —no se guarda
 * en ningún sitio legible—: la reemplaza por una temporal nueva, que el
 * dueño cambia por una propia al ingresar (US18).
 */
import { computed, ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import Dialog from 'primevue/dialog';
import InputText from 'primevue/inputtext';
import Message from 'primevue/message';
import Tag from 'primevue/tag';
import { patientsApi } from '../infrastructure/patients.api';
import { authenticationApi } from '../../iam/infrastructure/authentication.api';
import { problemOf } from '../../shared/infrastructure/http';
import { formatPhone, formatDocument } from '../../shared/i18n';

const { t } = useI18n();
const toast = useToast();

const clients = ref([]);
const loading = ref(true);

const dialogOpen = ref(false);
// «crear»: dar acceso a quien no lo tiene. «restablecer»: contraseña olvidada.
const mode = ref('create');
const selectedClient = ref(null);
const email = ref('');
const sending = ref(false);
const error = ref('');
const password = ref('');
const copied = ref(false);

const note = computed(() => t(mode.value === 'create' ? 'clients.dialogNote' : 'clients.resetNote', {
  name: selectedClient.value?.fullName ?? ''
}));

const title = computed(() => t(mode.value === 'create' ? 'clients.dialogTitle' : 'clients.resetTitle'));

async function load() {
  loading.value = true;
  try {
    const { data } = await patientsApi.listClients();
    clients.value = data;
  } finally {
    loading.value = false;
  }
}

onMounted(load);

function openDialog(client, inMode = 'create') {
  mode.value = inMode;
  selectedClient.value = client;
  email.value = client.email ?? '';
  password.value = '';
  error.value = '';
  copied.value = false;
  dialogOpen.value = true;
}

async function createAccess() {
  error.value = '';
  sending.value = true;
  try {
    const { data } = await authenticationApi.createOwnerAccess(
      email.value.trim(), selectedClient.value.fullName, selectedClient.value.id);

    // La contraseña se muestra aquí y no se guarda en ningún sitio: es lo que
    // recepción dicta al dueño.
    password.value = data.temporaryPassword;
    toast.add({
      severity: 'success',
      summary: t('clients.created', { name: selectedClient.value.fullName }),
      life: 4000
    });
    await load();
  } catch (failure) {
    error.value = problemOf(failure).detail ?? t('common.unexpectedError');
  } finally {
    sending.value = false;
  }
}

async function reset() {
  error.value = '';
  sending.value = true;
  try {
    const { data } = await authenticationApi.resetOwnerPassword(selectedClient.value.id);

    // Como al crear el acceso: se muestra una vez y no se guarda.
    password.value = data.temporaryPassword;
    email.value = data.user.email;
    toast.add({
      severity: 'success',
      summary: t('clients.resetDone', { name: selectedClient.value.fullName }),
      life: 4000
    });
  } catch (failure) {
    error.value = problemOf(failure).detail ?? t('common.unexpectedError');
  } finally {
    sending.value = false;
  }
}

async function copy() {
  try {
    await navigator.clipboard.writeText(password.value);
    copied.value = true;
  } catch {
    // Sin permiso de portapapeles queda a la vista para anotarla.
  }
}
</script>

<template>
  <div class="vp-page">
    <header class="vp-head">
      <h1>{{ t('clients.title') }}</h1>
    </header>

    <div class="vp-card">
      <DataTable :value="clients" data-key="id" :loading="loading">
        <Column field="fullName" :header="t('clients.name')">
          <template #body="{ data }"><span class="name">{{ data.fullName }}</span></template>
        </Column>

        <Column field="documentNumber" :header="t('clients.document')">
          <template #body="{ data }">
            <span class="document">{{ formatDocument(data, t) }}</span>
          </template>
        </Column>

        <Column field="phoneNumber" :header="t('clients.phone')">
          <template #body="{ data }">
            <span class="phone">{{ formatPhone(data.phoneNumber) }}</span>
          </template>
        </Column>

        <Column field="email" :header="t('clients.email')">
          <template #body="{ data }">
            <span v-if="data.email">{{ data.email }}</span>
            <span v-else class="vp-muted">{{ t('clients.noEmail') }}</span>
          </template>
        </Column>

        <Column :header="t('clients.access')">
          <template #body="{ data }">
            <Tag
              v-if="data.hasAccount"
              :value="t('clients.withAccess')"
              icon="pi pi-check"
              severity="success"
            />
            <Button
              v-else
              :label="t('clients.grantAccess')"
              icon="pi pi-mobile"
              size="small"
              outlined
              @click="openDialog(data)"
            />
          </template>
        </Column>

        <Column :header="t('clients.password')">
          <template #body="{ data }">
            <Button
              v-if="data.hasAccount"
              :label="t('clients.reset')"
              icon="pi pi-key"
              size="small"
              text
              @click="openDialog(data, 'reset')"
            />
            <span v-else class="vp-muted">—</span>
          </template>
        </Column>
      </DataTable>
    </div>

    <Dialog
      v-model:visible="dialogOpen"
      modal
      :header="title"
      :style="{ width: '30rem' }"
      :breakpoints="{ '640px': '95vw' }"
    >
      <!-- Antes de crear la cuenta -->
      <template v-if="!password">
        <p class="vp-small vp-muted note">{{ note }}</p>

        <p v-if="mode === 'reset'" class="vp-caption vp-muted privacy">
          <i class="pi pi-lock" aria-hidden="true" /> {{ t('clients.privacy') }}
        </p>

        <div v-if="mode === 'create'" class="vp-field">
          <label for="access-email">{{ t('clients.email') }}</label>
          <InputText id="access-email" v-model="email" type="email" autofocus />
          <small v-if="!selectedClient?.email" class="vp-muted">
            {{ t('clients.emailRequired') }}
          </small>
        </div>

        <Message v-if="error" severity="error" :closable="false" class="notice">{{ error }}</Message>
      </template>

      <!-- Después: la contraseña, una sola vez -->
      <template v-else>
        <p class="vp-small vp-muted note">{{ t('clients.handOver') }}</p>
        <div class="password">
          <div>
            <p class="vp-caption vp-muted">{{ t('clients.temporaryPassword') }}</p>
            <p class="password__value">{{ password }}</p>
          </div>
          <Button
            :label="copied ? t('clients.copied') : t('clients.copy')"
            :icon="copied ? 'pi pi-check' : 'pi pi-copy'"
            size="small"
            text
            @click="copy"
          />
        </div>
        <p class="vp-caption vp-muted email-account">{{ email }}</p>
      </template>

      <template #footer>
        <template v-if="!password">
          <Button :label="t('common.cancel')" text @click="dialogOpen = false" />
          <Button
            v-if="mode === 'create'"
            :label="t('clients.create')"
            :disabled="!email.trim()"
            :loading="sending"
            @click="createAccess"
          />
          <Button
            v-else
            :label="t('clients.resetConfirm')"
            icon="pi pi-key"
            severity="warn"
            :loading="sending"
            @click="reset"
          />
        </template>
        <Button v-else :label="t('common.close')" @click="dialogOpen = false" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.name { font-weight: 600; }
.phone, .document { font-variant-numeric: tabular-nums; white-space: nowrap; }
.privacy { display: flex; gap: var(--vp-space-2); align-items: baseline; margin-bottom: var(--vp-space-2); }
.note { margin-bottom: var(--vp-space-3); }
.notice { margin-top: var(--vp-space-3); }
.password {
  display: flex; align-items: center; justify-content: space-between; gap: var(--vp-space-3);
  padding: var(--vp-space-3);
  background: var(--vp-primary-50); border-radius: var(--vp-radius);
}
.password__value {
  font-family: ui-monospace, 'Cascadia Code', monospace;
  font-size: 20px; font-weight: 600; letter-spacing: .06em;
  color: var(--vp-primary-800);
}
.email-account { margin-top: var(--vp-space-2); }
</style>
