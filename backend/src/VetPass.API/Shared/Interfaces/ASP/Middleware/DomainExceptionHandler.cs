using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Vaccination.Domain.Services;

namespace VetPass.API.Shared.Interfaces.ASP.Middleware;

/// <summary>
/// Turns every rule the domain defends into an HTTP answer, so that the
/// controllers stay free of error handling and the codes are consistent across
/// the whole API.
///
/// The distinction between 400 and 422 is the one the technical stories set:
/// 400 for data that cannot form a valid object (TS02-E2), and 422 for data
/// that is well formed but breaks a rule of the vaccination schedule (TS03-E3).
/// </summary>
public class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException) return false;

        var status = exception switch
        {
            InvalidCredentialsException => StatusCodes.Status401Unauthorized,
            ForbiddenOperationException => StatusCodes.Status403Forbidden,
            ResourceNotFoundException => StatusCodes.Status404NotFound,
            DomainRuleViolationException => StatusCodes.Status422UnprocessableEntity,
            InvalidDomainDataException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        logger.LogInformation("Regla de dominio incumplida: {Code} — {Message}",
            domainException.Code, domainException.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = TitleFor(status),
            Detail = domainException.Message,
            Type = $"https://vetpass.pe/problems/{domainException.Code}",
            Instance = context.Request.Path
        };
        problem.Extensions["code"] = domainException.Code;

        AddRuleDetails(problem, exception);

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static string TitleFor(int status) => status switch
    {
        StatusCodes.Status401Unauthorized => "Credenciales inválidas",
        StatusCodes.Status403Forbidden => "Operación no permitida",
        StatusCodes.Status404NotFound => "Recurso no encontrado",
        StatusCodes.Status422UnprocessableEntity => "Regla del esquema incumplida",
        _ => "Datos inválidos"
    };

    /// <summary>
    /// The interface does not show a generic warning: it states the age the pet
    /// has, the one the vaccine demands and the earliest date on which the dose
    /// could be applied (section 4.6.3). Those values travel in the answer.
    /// </summary>
    private static void AddRuleDetails(ProblemDetails problem, Exception exception)
    {
        switch (exception)
        {
            case MinimumAgeNotReachedException age:
                problem.Extensions["ageInWeeks"] = age.AgeInWeeks;
                problem.Extensions["requiredWeeks"] = age.RequiredWeeks;
                problem.Extensions["earliestAdmissibleDate"] = age.EarliestAdmissibleDate.ToString("yyyy-MM-dd");
                break;
            case Patients.Domain.Model.Aggregates.ImplausibleBirthDateException birth:
                problem.Extensions["ageInYears"] = birth.AgeInYears;
                problem.Extensions["maximumAgeInYears"] = birth.MaximumAgeInYears;
                problem.Extensions["species"] = birth.Species.ToString();
                break;
            case MinimumIntervalNotMetException interval:
                problem.Extensions["elapsedWeeks"] = interval.ElapsedWeeks;
                problem.Extensions["requiredWeeks"] = interval.RequiredWeeks;
                problem.Extensions["earliestAdmissibleDate"] = interval.EarliestAdmissibleDate.ToString("yyyy-MM-dd");
                break;
        }
    }
}
