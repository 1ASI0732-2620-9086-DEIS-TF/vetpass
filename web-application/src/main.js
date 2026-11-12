import { createApp } from 'vue';
import { createPinia } from 'pinia';
import PrimeVue from 'primevue/config';
import ToastService from 'primevue/toastservice';
import ConfirmationService from 'primevue/confirmationservice';

import 'primeicons/primeicons.css';
import './style.css';

import App from './App.vue';
import { router } from './router';
import { i18n } from './shared/i18n';
import { VetPassPreset } from './shared/theme/vetpass-preset';
import { configureHttp } from './shared/infrastructure/http';
import { useAuthStore } from './iam/application/auth.store';

const app = createApp(App);
const pinia = createPinia();

app.use(pinia);
app.use(i18n);
app.use(PrimeVue, {
  theme: { preset: VetPassPreset, options: { darkModeSelector: false } },
  ripple: false
});
app.use(ToastService);
app.use(ConfirmationService);

const session = useAuthStore(pinia);
session.restore();

configureHttp({
  getToken: () => session.token,
  onUnauthorized: () => {
    session.signOut();
    if (router.currentRoute.value.name !== 'access') router.push({ name: 'access' });
  }
});

app.use(router);
app.mount('#app');
