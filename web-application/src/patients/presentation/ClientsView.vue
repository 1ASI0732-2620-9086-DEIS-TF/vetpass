<script setup>
/**
 * Clientes de la clínica y su acceso a la aplicación móvil.
 *
 * El dueño no se registra por su cuenta: la clínica le crea el acceso y le
 * entrega una contraseña temporal en recepción (US05). Esta pantalla es el
 * lugar donde eso ocurre, y por eso distingue a quién ya se le entregó.
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

const { t } = useI18n();
const toast = useToast();

const clientes = ref([]);
const cargando = ref(true);

const dialogoAbierto = ref(false);
const clienteElegido = ref(null);
const correo = ref('');
const enviando = ref(false);
const error = ref('');
const contrasena = ref('');
const copiada = ref(false);

const nota = computed(() => t('clientes.dialogoNota', {
  nombre: clienteElegido.value?.fullName ?? ''
}));

async function cargar() {
  cargando.value = true;
  try {
    const { data } = await patientsApi.listarClientes();
    clientes.value = data;
  } finally {
    cargando.value = false;
  }
}

onMounted(cargar);

function abrirDialogo(cliente) {
  clienteElegido.value = cliente;
  correo.value = cliente.email ?? '';
  contrasena.value = '';
  error.value = '';
  copiada.value = false;
  dialogoAbierto.value = true;
}

async function crearAcceso() {
  error.value = '';
  enviando.value = true;
  try {
    const { data } = await authenticationApi.crearAccesoDeDueno(
      correo.value.trim(), clienteElegido.value.fullName, clienteElegido.value.id);

    // La contraseña se muestra aquí y no se guarda en ningún sitio: es lo que
    // recepción dicta al dueño.
    contrasena.value = data.temporaryPassword;
    toast.add({
      severity: 'success',
      summary: t('clientes.creada', { nombre: clienteElegido.value.fullName }),
      life: 4000
    });
    await cargar();
  } catch (fallo) {
    error.value = problemOf(fallo).detail ?? t('comun.errorInesperado');
  } finally {
    enviando.value = false;
  }
}

async function copiar() {
  try {
    await navigator.clipboard.writeText(contrasena.value);
    copiada.value = true;
  } catch {
    // Sin permiso de portapapeles queda a la vista para anotarla.
  }
}
</script>

<template>
  <div class="vp-page">
    <header class="vp-head">
      <h1>{{ t('clientes.titulo') }}</h1>
    </header>

    <div class="vp-card">
      <DataTable :value="clientes" data-key="id" :loading="cargando">
        <Column field="fullName" :header="t('clientes.nombre')">
          <template #body="{ data }"><span class="nombre">{{ data.fullName }}</span></template>
        </Column>

        <Column field="phoneNumber" :header="t('clientes.telefono')" />

        <Column field="email" :header="t('clientes.correo')">
          <template #body="{ data }">
            <span v-if="data.email">{{ data.email }}</span>
            <span v-else class="vp-muted">{{ t('clientes.sinCorreo') }}</span>
          </template>
        </Column>

        <Column :header="t('clientes.acceso')">
          <template #body="{ data }">
            <Tag
              v-if="data.hasAccount"
              :value="t('clientes.conAcceso')"
              icon="pi pi-check"
              severity="success"
            />
            <Button
              v-else
              :label="t('clientes.darAcceso')"
              icon="pi pi-mobile"
              size="small"
              outlined
              @click="abrirDialogo(data)"
            />
          </template>
        </Column>
      </DataTable>
    </div>

    <Dialog
      v-model:visible="dialogoAbierto"
      modal
      :header="t('clientes.dialogoTitulo')"
      :style="{ width: '30rem' }"
      :breakpoints="{ '640px': '95vw' }"
    >
      <!-- Antes de crear la cuenta -->
      <template v-if="!contrasena">
        <p class="vp-small vp-muted nota">{{ nota }}</p>

        <div class="vp-field">
          <label for="acceso-correo">{{ t('clientes.correo') }}</label>
          <InputText id="acceso-correo" v-model="correo" type="email" autofocus />
          <small v-if="!clienteElegido?.email" class="vp-muted">
            {{ t('clientes.correoRequerido') }}
          </small>
        </div>

        <Message v-if="error" severity="error" :closable="false" class="aviso">{{ error }}</Message>
      </template>

      <!-- Después: la contraseña, una sola vez -->
      <template v-else>
        <p class="vp-small vp-muted nota">{{ t('clientes.entregar') }}</p>
        <div class="contrasena">
          <div>
            <p class="vp-caption vp-muted">{{ t('clientes.contrasenaTemporal') }}</p>
            <p class="contrasena__valor">{{ contrasena }}</p>
          </div>
          <Button
            :label="copiada ? t('clientes.copiada') : t('clientes.copiar')"
            :icon="copiada ? 'pi pi-check' : 'pi pi-copy'"
            size="small"
            text
            @click="copiar"
          />
        </div>
        <p class="vp-caption vp-muted correo-cuenta">{{ correo }}</p>
      </template>

      <template #footer>
        <template v-if="!contrasena">
          <Button :label="t('comun.cancelar')" text @click="dialogoAbierto = false" />
          <Button
            :label="t('clientes.crear')"
            :disabled="!correo.trim()"
            :loading="enviando"
            @click="crearAcceso"
          />
        </template>
        <Button v-else :label="t('comun.cerrar')" @click="dialogoAbierto = false" />
      </template>
    </Dialog>
  </div>
</template>

<style scoped>
.nombre { font-weight: 600; }
.nota { margin-bottom: var(--vp-space-3); }
.aviso { margin-top: var(--vp-space-3); }
.contrasena {
  display: flex; align-items: center; justify-content: space-between; gap: var(--vp-space-3);
  padding: var(--vp-space-3);
  background: var(--vp-primary-50); border-radius: var(--vp-radius);
}
.contrasena__valor {
  font-family: ui-monospace, 'Cascadia Code', monospace;
  font-size: 20px; font-weight: 600; letter-spacing: .06em;
  color: var(--vp-primary-800);
}
.correo-cuenta { margin-top: var(--vp-space-2); }
</style>
