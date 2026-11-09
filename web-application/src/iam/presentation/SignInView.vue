<script setup>
/**
 * Inicio de sesión del personal de la clínica (US04). Las credenciales
 * inválidas se comunican en el propio formulario y no exponen información de
 * pacientes (US04-E2).
 */
import { ref } from 'vue';
import { TERMS_URL } from '../../shared/legal';
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
const session = useAuthStore();

const email = ref('');
const password = ref('');
const error = ref('');

async function signIn() {
  error.value = '';
  try {
    await session.signIn(email.value, password.value);
    router.push(route.query.destination?.toString() ?? { name: 'patients' });
  } catch (failure) {
    const problem = problemOf(failure);
    error.value = problem.status === 401
      ? t('access.invalidCredentials')
      : (problem.status === 0 ? t('access.offline') : (problem.detail ?? t('common.unexpectedError')));
  }
}
</script>

<template>
  <div class="access">
    <section class="access__brand">
      <div class="access__brandInner">
        <p class="access__logo">
          <img src="/favicon.svg" alt="" width="32" height="32">
          {{ t('brand.name') }}
        </p>
        <h1>{{ t('access.title') }}</h1>
        <p class="access__subtitle">{{ t('access.subtitle') }}</p>
      </div>
    </section>

    <section class="access__panel">
      <div class="access__language"><LanguageSwitch /></div>

      <form class="access__form" @submit.prevent="signIn">
        <h2>{{ t('access.header') }}</h2>
        <p class="vp-small vp-muted">{{ t('access.note') }}</p>

        <Message v-if="error" severity="error" :closable="false" class="access__error">
          {{ error }}
        </Message>

        <div class="vp-field">
          <label for="email">{{ t('access.email') }}</label>
          <InputText id="email" v-model="email" type="email" autocomplete="username" required />
        </div>

        <div class="vp-field">
          <label for="password">{{ t('access.password') }}</label>
          <Password
            v-model="password"
            input-id="password"
            toggle-mask
            :feedback="false"
            autocomplete="current-password"
            required
            fluid
          />
        </div>

        <Button
          type="submit"
          :label="t('access.signIn')"
          :loading="session.loading"
          fluid
        />

        <p class="vp-caption vp-muted">{{ t('access.help') }}</p>
        <a class="vp-caption terms" :href="TERMS_URL" target="_blank" rel="noopener">
          {{ t('common.terms') }}
        </a>
      </form>
    </section>
  </div>
</template>

<style scoped>
.terms { color: var(--vp-neutral-600); text-align: center; }
.access { display: grid; min-height: 100vh; }

.access__brand {
  display: none;
  background: linear-gradient(160deg, var(--vp-primary-800), var(--vp-primary-700) 60%);
  color: #fff; padding: var(--vp-space-6);
  align-items: center;
}
.access__brandInner { max-width: 28rem; }
.access__logo {
  display: flex; align-items: center; gap: var(--vp-space-2);
  margin-bottom: var(--vp-space-5);
  font-family: var(--vp-font-brand); font-size: 20px; font-weight: 600;
}
.access__brand h1 { font-size: 32px; line-height: 40px; }
.access__subtitle { margin-top: var(--vp-space-3); color: rgba(255, 255, 255, .88); }

.access__panel {
  display: grid; place-items: center; position: relative;
  padding: var(--vp-space-5) var(--vp-space-4); background: #fff;
}
.access__language { position: absolute; top: var(--vp-space-4); right: var(--vp-space-4); }

.access__form { display: grid; gap: var(--vp-space-3); width: 100%; max-width: 22rem; }
.access__form h2 { font-family: var(--vp-font-brand); font-size: 24px; }
.access__error { margin: 0; }

@media (min-width: 992px) {
  .access { grid-template-columns: 1.1fr 1fr; }
  .access__brand { display: flex; }
}
</style>
