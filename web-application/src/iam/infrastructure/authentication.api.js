import { http } from '../../shared/infrastructure/http';

export const authenticationApi = {
  signIn: (email, password) => http.post('/authentication/sign-in', { email, password }),
  me: () => http.get('/authentication/me')
};
