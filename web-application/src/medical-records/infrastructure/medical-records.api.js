import { http } from '../../shared/infrastructure/http';

export const medicalRecordsApi = {
  listarAtenciones: (petId) => http.get(`/pets/${petId}/visits`),

  registrarAtencion: (petId, atencion) => http.post(`/pets/${petId}/visits`, atencion),

  emitirReceta: (visitId, items) => http.post(`/visits/${visitId}/prescription`, { items })
};
