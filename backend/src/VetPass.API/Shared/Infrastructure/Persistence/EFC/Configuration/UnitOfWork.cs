using VetPass.API.Shared.Domain.Services;

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;

public class UnitOfWork(VetPassDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
