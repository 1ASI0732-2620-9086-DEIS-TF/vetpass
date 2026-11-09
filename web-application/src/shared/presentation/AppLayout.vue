<script setup>
/**
 * Marco de la aplicación: navegación lateral persistente con los tres
 * destinos de la sección 4.2.5 y un encabezado con la búsqueda, que es la
 * función de entrada al producto porque responde a la necesidad más frecuente
 * del personal de la clínica (4.2.4).
 */
import { ref } from 'vue';
import { TERMS_URL } from '../legal';
import { useRouter, useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Menu from 'primevue/menu';
import LanguageSwitch from './LanguageSwitch.vue';
import { useAuthStore } from '../../iam/application/auth.store';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const session = useAuthStore();

const query = ref('');
const userMenu = ref();
const sidebarOpen = ref(false);

const destinations = [
  { name: 'patients', label: 'nav.patients', icon: 'pi pi-heart' },
  { name: 'clients', label: 'nav.clients', icon: 'pi pi-users' }
];

const userOptions = [
  {
    label: t('nav.signOut'),
    icon: 'pi pi-sign-out',
    command: () => { session.signOut(); router.push({ name: 'access' }); }
  }
];

function search() {
  router.push({ name: 'patients', query: query.value ? { q: query.value } : {} });
  sidebarOpen.value = false;
}
</script>

<template>
  <div class="layout" :class="{ 'layout--open': sidebarOpen }">
    <aside class="sidebar">
      <RouterLink class="brand" :to="{ name: 'patients' }">
        <img src="/favicon.svg" alt="" width="28" height="28">
        <span>{{ t('brand.name') }}</span>
      </RouterLink>

      <nav class="menu" :aria-label="t('nav.patients')">
        <RouterLink
          v-for="destination in destinations"
          :key="destination.name"
          class="menu__item"
          :class="{ 'menu__item--active': route.name?.toString().startsWith(destination.name) }"
          :to="{ name: destination.name }"
          @click="sidebarOpen = false"
        >
          <i :class="destination.icon" aria-hidden="true" />
          {{ t(destination.label) }}
        </RouterLink>
      </nav>

      <div class="user">
        <button class="user__btn" type="button" @click="userMenu.toggle($event)">
          <span class="user__initials" aria-hidden="true">{{ session.initials }}</span>
          <span class="user__details">
            <span class="user__name">{{ session.user?.fullName }}</span>
            <span class="user__rol vp-caption">{{ session.user?.email }}</span>
          </span>
          <i class="pi pi-ellipsis-v" aria-hidden="true" />
        </button>
        <Menu ref="userMenu" :model="userOptions" popup />
        <a class="terms vp-caption" :href="TERMS_URL" target="_blank" rel="noopener">
          {{ t('common.terms') }}
        </a>
      </div>
    </aside>

    <div class="main">
      <header class="top">
        <Button
          class="top__menu"
          text
          rounded
          icon="pi pi-bars"
          :aria-label="t('nav.patients')"
          @click="sidebarOpen = !sidebarOpen"
        />

        <form class="search" role="search" @submit.prevent="search">
          <i class="pi pi-search" aria-hidden="true" />
          <InputText
            v-model="query"
            :placeholder="t('nav.searchPatient')"
            :aria-label="t('nav.searchPatient')"
          />
        </form>

        <LanguageSwitch />
      </header>

      <main class="content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style scoped>
.layout { display: grid; grid-template-columns: var(--vp-sidebar) 1fr; min-height: 100vh; }

.sidebar {
  display: flex; flex-direction: column; gap: var(--vp-space-4);
  padding: var(--vp-space-4) var(--vp-space-3);
  background: #fff; border-right: 1px solid var(--vp-neutral-200);
  position: sticky; top: 0; height: 100vh;
}

.brand {
  display: flex; align-items: center; gap: var(--vp-space-2);
  padding-inline: var(--vp-space-2);
  font-family: var(--vp-font-brand); font-weight: 600; font-size: 20px;
  color: var(--vp-neutral-900); text-decoration: none;
}

.menu { display: flex; flex-direction: column; gap: 2px; flex: 1; }
.menu__item {
  display: flex; align-items: center; gap: var(--vp-space-2);
  min-height: 40px; padding: 0 var(--vp-space-2);
  color: var(--vp-neutral-600); text-decoration: none;
  border-radius: var(--vp-radius-control); font-size: 15px; font-weight: 500;
}
.menu__item:hover { background: var(--vp-surface-50); color: var(--vp-neutral-900); }
.menu__item--active { background: var(--vp-primary-50); color: var(--vp-primary-800); font-weight: 600; }

.user__btn {
  display: flex; align-items: center; gap: var(--vp-space-2); width: 100%;
  padding: var(--vp-space-2); background: none; cursor: pointer; text-align: left;
  border: 1px solid var(--vp-neutral-200); border-radius: var(--vp-radius-control);
  font-family: inherit;
}
.user__btn:hover { background: var(--vp-surface-50); }
.user__initials {
  display: grid; place-items: center; flex-shrink: 0;
  width: 32px; height: 32px; border-radius: 999px;
  background: var(--vp-primary-700); color: #fff; font-size: 13px; font-weight: 600;
}
.user__details { display: grid; min-width: 0; flex: 1; }
.user__name {
  font-size: 14px; font-weight: 600;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.user__rol { color: var(--vp-neutral-600); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.terms { display: block; margin-top: var(--vp-space-2); padding: 0 var(--vp-space-2); color: var(--vp-neutral-600); }

.main { display: flex; flex-direction: column; min-width: 0; }

.top {
  display: flex; align-items: center; gap: var(--vp-space-3);
  min-height: var(--vp-topbar); padding-inline: var(--vp-space-4);
  background: #fff; border-bottom: 1px solid var(--vp-neutral-200);
  position: sticky; top: 0; z-index: 10;
}
.top__menu { display: none !important; }

.search { position: relative; flex: 1; max-width: 420px; }
.search i {
  position: absolute; left: 12px; top: 50%; transform: translateY(-50%);
  color: var(--vp-neutral-600); font-size: 14px;
}
.search :deep(input) { width: 100%; padding-left: 36px; height: 40px; }

.content { padding: var(--vp-space-5) var(--vp-space-4); }

@media (max-width: 991px) {
  .layout { grid-template-columns: 1fr; }
  .sidebar {
    position: fixed; inset: 0 auto 0 0; width: var(--vp-sidebar); z-index: 30;
    transform: translateX(-100%); transition: transform .2s ease;
    box-shadow: var(--vp-shadow-2);
  }
  .layout--open .sidebar { transform: none; }
  .top__menu { display: inline-flex !important; }
}
</style>
