import { http } from '../../shared/infrastructure/http';

export const medicalRecordsApi = {
  listVisits: (petId) => http.get(`/pets/${petId}/visits`),

  registerVisit: (petId, visit) => http.post(`/pets/${petId}/visits`, visit),

  issuePrescription: (visitId, items) => http.post(`/visits/${visitId}/prescription`, { items })
};
