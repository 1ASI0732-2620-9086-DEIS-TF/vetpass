<script setup>
/**
 * Inicio de sesión del personal de la clínica (US04). Las credenciales
 * inválidas se comunican en el propio formulario y no exponen información de
 * pacientes (US04-E2).
 */
import { ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Password from 'primevue/password';
import Message from 'primevue/message';
import LanguageSwitch from '../../shared/presentation/LanguageSwitch.vue';
import { useAuthStore } from '../application/auth.store';
import { problemOf } from '../../shared/infrastructure/http';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const sesion = useAuthStore();

const correo = ref('');
const contrasena = ref('');
const error = ref('');

async function ingresar() {
  error.value = '';
  try {
    await sesion.iniciarSesion(correo.value, contrasena.value);
    router.push(route.query.destino?.toString() ?? { name: 'pacientes' });
  } catch (fallo) {
    const problema = problemOf(fallo);
    error.value = problema.status === 401
      ? t('acceso.credencialesInvalidas')
      : (problema.status === 0 ? t('acceso.sinConexion') : (problema.detail ?? t('comun.errorInesperado')));
  }
}
</script>

<template>
  <div class="acceso">
    <section class="acceso__marca">
      <div class="acceso__marcaInterior">
        <p class="acceso__logo">
          <img src="/favicon.svg" alt="" width="32" height="32">
          {{ t('marca.nombre') }}
        </p>
        <h1>{{ t('acceso.titulo') }}</h1>
        <p class="acceso__subtitulo">{{ t('acceso.subtitulo') }}</p>
      </div>
    </section>

    <section class="acceso__panel">
      <div class="acceso__idioma"><LanguageSwitch /></div>

      <form class="acceso__form" @submit.prevent="ingresar">
        <h2>{{ t('acceso.encabezado') }}</h2>
        <p class="vp-small vp-muted">{{ t('acceso.nota') }}</p>

        <Message v-if="error" severity="error" :closable="false" class="acceso__error">
          {{ error }}
        </Message>

        <div class="vp-field">
          <label for="correo">{{ t('acceso.correo') }}</label>
          <InputText id="correo" v-model="correo" type="email" autocomplete="username" required />
        </div>

        <div class="vp-field">
          <label for="contrasena">{{ t('acceso.contrasena') }}</label>
          <Password
            input-id="contrasena"
            v-model="contrasena"
            toggle-mask
            :feedback="false"
            autocomplete="current-password"
            required
            fluid
          />
        </div>

        <Button
          type="submit"
          :label="t('acceso.ingresar')"
          :loading="sesion.cargando"
          fluid
        />

        <p class="vp-caption vp-muted">{{ t('acceso.ayuda') }}</p>
      </form>
    </section>
  </div>
</template>

<style scoped>
.acceso { display: grid; min-height: 100vh; }

.acceso__marca {
  display: none;
  background: linear-gradient(160deg, var(--vp-primary-800), var(--vp-primary-700) 60%);
  color: #fff; padding: var(--vp-space-6);
  align-items: center;
}
.acceso__marcaInterior { max-width: 28rem; }
.acceso__logo {
  display: flex; align-items: center; gap: var(--vp-space-2);
  margin-bottom: var(--vp-space-5);
  font-family: var(--vp-font-brand); font-size: 20px; font-weight: 600;
}
.acceso__marca h1 { font-size: 32px; line-height: 40px; }
.acceso__subtitulo { margin-top: var(--vp-space-3); color: rgba(255, 255, 255, .88); }

.acceso__panel {
  display: grid; place-items: center; position: relative;
  padding: var(--vp-space-5) var(--vp-space-4); background: #fff;
}
.acceso__idioma { position: absolute; top: var(--vp-space-4); right: var(--vp-space-4); }

.acceso__form { display: grid; gap: var(--vp-space-3); width: 100%; max-width: 22rem; }
.acceso__form h2 { font-family: var(--vp-font-brand); font-size: 24px; }
.acceso__error { margin: 0; }

@media (min-width: 992px) {
  .acceso { grid-template-columns: 1.1fr 1fr; }
  .acceso__marca { display: flex; }
}
</style>
