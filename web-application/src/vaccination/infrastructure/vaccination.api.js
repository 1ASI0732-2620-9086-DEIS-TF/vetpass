import { http } from '../../shared/infrastructure/http';

export const vaccinationApi = {
  obtenerCartilla: (petId) => http.get(`/pets/${petId}/vaccination-card`),

  /** Registro de una dosis aplicada (US10). Las reglas las verifica la API. */
  registrarDosis: (petId, doseId, { applicationDate, batchCode, veterinarianId }) =>
    http.post(`/pets/${petId}/vaccination-card/doses/${doseId}/application`,
      { applicationDate, batchCode, veterinarianId })
};
