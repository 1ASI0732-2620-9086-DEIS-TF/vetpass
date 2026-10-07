<script setup>
/**
 * Marco de la aplicación: navegación lateral persistente con los tres
 * destinos de la sección 4.2.5 y un encabezado con la búsqueda, que es la
 * función de entrada al producto porque responde a la necesidad más frecuente
 * del personal de la clínica (4.2.4).
 */
import { ref } from 'vue';
import { TERMINOS_URL } from '../legal';
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
const sesion = useAuthStore();

const busqueda = ref('');
const menuUsuario = ref();
const lateralAbierto = ref(false);

const destinos = [
  { nombre: 'pacientes', etiqueta: 'nav.pacientes', icono: 'pi pi-heart' },
  { nombre: 'clientes', etiqueta: 'nav.clientes', icono: 'pi pi-users' }
];

const opcionesUsuario = [
  {
    label: t('nav.cerrarSesion'),
    icon: 'pi pi-sign-out',
    command: () => { sesion.cerrarSesion(); router.push({ name: 'acceso' }); }
  }
];

function buscar() {
  router.push({ name: 'pacientes', query: busqueda.value ? { q: busqueda.value } : {} });
  lateralAbierto.value = false;
}
</script>

<template>
  <div class="layout" :class="{ 'layout--abierto': lateralAbierto }">
    <aside class="lateral">
      <RouterLink class="marca" :to="{ name: 'pacientes' }">
        <img src="/favicon.svg" alt="" width="28" height="28">
        <span>{{ t('marca.nombre') }}</span>
      </RouterLink>

      <nav class="menu" :aria-label="t('nav.pacientes')">
        <RouterLink
          v-for="destino in destinos"
          :key="destino.nombre"
          class="menu__item"
          :class="{ 'menu__item--activo': route.name?.toString().startsWith(destino.nombre) }"
          :to="{ name: destino.nombre }"
          @click="lateralAbierto = false"
        >
          <i :class="destino.icono" aria-hidden="true" />
          {{ t(destino.etiqueta) }}
        </RouterLink>
      </nav>

      <div class="usuario">
        <button class="usuario__btn" type="button" @click="menuUsuario.toggle($event)">
          <span class="usuario__iniciales" aria-hidden="true">{{ sesion.iniciales }}</span>
          <span class="usuario__datos">
            <span class="usuario__nombre">{{ sesion.usuario?.fullName }}</span>
            <span class="usuario__rol vp-caption">{{ sesion.usuario?.email }}</span>
          </span>
          <i class="pi pi-ellipsis-v" aria-hidden="true" />
        </button>
        <Menu ref="menuUsuario" :model="opcionesUsuario" popup />
        <a class="terminos vp-caption" :href="TERMINOS_URL" target="_blank" rel="noopener">
          {{ t('comun.terminos') }}
        </a>
      </div>
    </aside>

    <div class="principal">
      <header class="superior">
        <Button
          class="superior__menu"
          text
          rounded
          icon="pi pi-bars"
          :aria-label="t('nav.pacientes')"
          @click="lateralAbierto = !lateralAbierto"
        />

        <form class="buscador" role="search" @submit.prevent="buscar">
          <i class="pi pi-search" aria-hidden="true" />
          <InputText
            v-model="busqueda"
            :placeholder="t('nav.buscarPaciente')"
            :aria-label="t('nav.buscarPaciente')"
          />
        </form>

        <LanguageSwitch />
      </header>

      <main class="contenido">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style scoped>
.layout { display: grid; grid-template-columns: var(--vp-sidebar) 1fr; min-height: 100vh; }

.lateral {
  display: flex; flex-direction: column; gap: var(--vp-space-4);
  padding: var(--vp-space-4) var(--vp-space-3);
  background: #fff; border-right: 1px solid var(--vp-neutral-200);
  position: sticky; top: 0; height: 100vh;
}

.marca {
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
.menu__item--activo { background: var(--vp-primary-50); color: var(--vp-primary-800); font-weight: 600; }

.usuario__btn {
  display: flex; align-items: center; gap: var(--vp-space-2); width: 100%;
  padding: var(--vp-space-2); background: none; cursor: pointer; text-align: left;
  border: 1px solid var(--vp-neutral-200); border-radius: var(--vp-radius-control);
  font-family: inherit;
}
.usuario__btn:hover { background: var(--vp-surface-50); }
.usuario__iniciales {
  display: grid; place-items: center; flex-shrink: 0;
  width: 32px; height: 32px; border-radius: 999px;
  background: var(--vp-primary-700); color: #fff; font-size: 13px; font-weight: 600;
}
.usuario__datos { display: grid; min-width: 0; flex: 1; }
.usuario__nombre {
  font-size: 14px; font-weight: 600;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.usuario__rol { color: var(--vp-neutral-600); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.terminos { display: block; margin-top: var(--vp-space-2); padding: 0 var(--vp-space-2); color: var(--vp-neutral-600); }

.principal { display: flex; flex-direction: column; min-width: 0; }

.superior {
  display: flex; align-items: center; gap: var(--vp-space-3);
  min-height: var(--vp-topbar); padding-inline: var(--vp-space-4);
  background: #fff; border-bottom: 1px solid var(--vp-neutral-200);
  position: sticky; top: 0; z-index: 10;
}
.superior__menu { display: none !important; }

.buscador { position: relative; flex: 1; max-width: 420px; }
.buscador i {
  position: absolute; left: 12px; top: 50%; transform: translateY(-50%);
  color: var(--vp-neutral-600); font-size: 14px;
}
.buscador :deep(input) { width: 100%; padding-left: 36px; height: 40px; }

.contenido { padding: var(--vp-space-5) var(--vp-space-4); }

@media (max-width: 991px) {
  .layout { grid-template-columns: 1fr; }
  .lateral {
    position: fixed; inset: 0 auto 0 0; width: var(--vp-sidebar); z-index: 30;
    transform: translateX(-100%); transition: transform .2s ease;
    box-shadow: var(--vp-shadow-2);
  }
  .layout--abierto .lateral { transform: none; }
  .superior__menu { display: inline-flex !important; }
}
</style>
