using VetPass.API.Patients.Domain.Model.ValueObjects;

namespace VetPass.API.Vaccination.Domain.Services;

/// <summary>
/// Provides the schedule in force for a species, assembled from the template
/// stored in the platform.
/// </summary>
public interface IVaccinationScheduleProvider
{
    Task<VaccinationSchedule> GetForAsync(Species species, CancellationToken cancellationToken = default);
}
