import { http } from '../../shared/infrastructure/http';

export const authenticationApi = {
  signIn: (email, password) => http.post('/authentication/sign-in', { email, password }),
  me: () => http.get('/authentication/me'),

  /** Crea el acceso móvil de un dueño. Devuelve su contraseña temporal, que
   *  solo viaja en esta respuesta (US05). */
  crearAccesoDeDueno: (email, fullName, clientId) =>
    http.post('/authentication/owner-accounts', { email, fullName, clientId })
};
