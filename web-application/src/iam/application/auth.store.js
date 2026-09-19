import { defineStore } from 'pinia';
import { authenticationApi } from '../infrastructure/authentication.api';

const ALMACEN = 'vetpass.sesion';

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
    usuario: null,
    cargando: false
  }),

  getters: {
    autenticado: (estado) => Boolean(estado.token),
    esPersonalDeClinica: (estado) => estado.usuario?.role === 'ClinicStaff',
    iniciales: (estado) => (estado.usuario?.fullName ?? '')
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((palabra) => palabra[0]?.toUpperCase())
      .join('')
  },

  actions: {
    restaurar() {
      try {
        const guardado = localStorage.getItem(ALMACEN);
        if (!guardado) return;
        const sesion = JSON.parse(guardado);
        this.token = sesion.token;
        this.refreshToken = sesion.refreshToken;
        this.usuario = sesion.usuario;
      } catch { this.cerrarSesion(); }
    },

    async iniciarSesion(email, password) {
      this.cargando = true;
      try {
        const { data } = await authenticationApi.signIn(email, password);
        this.token = data.accessToken;
        this.refreshToken = data.refreshToken;
        this.usuario = data.user;
        this.persistir();
        return data.user;
      } finally {
        this.cargando = false;
      }
    },

    cerrarSesion() {
      this.token = null;
      this.refreshToken = null;
      this.usuario = null;
      try { localStorage.removeItem(ALMACEN); } catch { /* sin almacenamiento */ }
    },

    persistir() {
      try {
        localStorage.setItem(ALMACEN, JSON.stringify({
          token: this.token, refreshToken: this.refreshToken, usuario: this.usuario
        }));
      } catch { /* sin almacenamiento */ }
    }
  }
});
