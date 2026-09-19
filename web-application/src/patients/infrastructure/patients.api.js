import { http } from '../../shared/infrastructure/http';

export const patientsApi = {
  /** Búsqueda por nombre de mascota o de dueño, con sus filtros (US08). */
  buscar: ({ search, species, cardStatus } = {}) =>
    http.get('/pets', { params: { search: search || undefined, species, cardStatus } }),

  obtener: (petId) => http.get(`/pets/${petId}`),

  registrarMascota: (mascota) => http.post('/pets', mascota),

  listarClientes: () => http.get('/clients'),

  registrarCliente: (cliente) => http.post('/clients', cliente)
};
