namespace VetPass.API.Shared.Domain.Services;

/// <summary>
/// Source of the current date for the application layer.
///
/// The domain never reads the clock: it receives the date as a parameter, so
/// that its rules can be verified independently of when they run. This is the
/// single point where that date is obtained, and it is expressed in the time
/// zone of the clinic, because a dose applied at nine at night in Lima belongs
/// to that day and not to the next one in UTC.
/// </summary>
public interface IClinicClock
{
    DateOnly Today { get; }
}

public class ClinicClock(TimeProvider timeProvider) : IClinicClock
{
    private static readonly TimeZoneInfo ClinicTimeZone = ResolveTimeZone();

    public DateOnly Today => DateOnly.FromDateTime(
        TimeZoneInfo.ConvertTimeFromUtc(timeProvider.GetUtcNow().UtcDateTime, ClinicTimeZone));

    private static TimeZoneInfo ResolveTimeZone()
    {
        foreach (var id in new[] { "America/Lima", "SA Pacific Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                // Identifier not available on this platform; try the next one.
            }
        }

        return TimeZoneInfo.Utc;
    }
}
