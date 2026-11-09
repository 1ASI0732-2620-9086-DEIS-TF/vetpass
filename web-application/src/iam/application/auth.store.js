import { defineStore } from 'pinia';
import { authenticationApi } from '../infrastructure/authentication.api';

const STORAGE_KEY = 'vetpass.sesion';

/**
 * Sesión del personal de la clínica. Guarda el token que emitió la API y el
 * perfil que esta devuelve; el rol y la clínica del usuario viajan dentro del
 * token y la API los verifica en cada petición, de modo que lo que aquí se
 * conserva sirve para la interfaz, no para decidir permisos.
 */
export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: null,
    refreshToken: null,
    user: null,
    loading: false
  }),

  getters: {
    authenticated: (status) => Boolean(status.token),
    isClinicStaff: (status) => status.user?.role === 'ClinicStaff',
    initials: (status) => (status.user?.fullName ?? '')
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((word) => word[0]?.toUpperCase())
      .join('')
  },

  actions: {
    restore() {
      try {
        const saved = localStorage.getItem(STORAGE_KEY);
        if (!saved) return;
        const session = JSON.parse(saved);
        this.token = session.token;
        this.refreshToken = session.refreshToken;
        this.user = session.user;
      } catch { this.signOut(); }
    },

    async signIn(email, password) {
      this.loading = true;
      try {
        const { data } = await authenticationApi.signIn(email, password);
        this.token = data.accessToken;
        this.refreshToken = data.refreshToken;
        this.user = data.user;
        this.persist();
        return data.user;
      } finally {
        this.loading = false;
      }
    },

    signOut() {
      this.token = null;
      this.refreshToken = null;
      this.user = null;
      try { localStorage.removeItem(STORAGE_KEY); } catch { /* sin almacenamiento */ }
    },

    persist() {
      try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify({
          token: this.token, refreshToken: this.refreshToken, user: this.user
        }));
      } catch { /* sin almacenamiento */ }
    }
  }
});
