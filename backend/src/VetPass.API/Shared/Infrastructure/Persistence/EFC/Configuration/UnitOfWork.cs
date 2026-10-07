using Microsoft.EntityFrameworkCore;
using Npgsql;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Domain.Services;

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;

public class UnitOfWork(VetPassDbContext context) : IUnitOfWork
{
    private const string UniqueViolation = "23505";

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException failure)
            when (failure.InnerException is PostgresException { SqlState: UniqueViolation })
        {
            // El dominio comprueba la unicidad antes de guardar; el índice es la
            // red de seguridad cuando dos solicitudes pasan esa comprobación a la
            // vez. El conflicto se informa en los términos del dominio.
            throw new UniqueConstraintViolationException();
        }
    }
}
