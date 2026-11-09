import { http } from '../../shared/infrastructure/http';

export const patientsApi = {
  /** Búsqueda por nombre de mascota o de dueño, con sus filtros (US08). */
  search: ({ search, species, cardStatus } = {}) =>
    http.get('/pets', { params: { search: search || undefined, species, cardStatus } }),

  get: (petId) => http.get(`/pets/${petId}`),

  registerPet: (pet) => http.post('/pets', pet),

  listClients: () => http.get('/clients'),

  registerClient: (client) => http.post('/clients', client)
};
