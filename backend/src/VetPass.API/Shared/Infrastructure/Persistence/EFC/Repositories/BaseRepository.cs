using Microsoft.EntityFrameworkCore;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Operations every repository shares. Each bounded context extends it with the
/// queries its own aggregates need.
/// </summary>
public abstract class BaseRepository<TEntity>(VetPassDbContext context) where TEntity : class
{
    protected readonly VetPassDbContext Context = context;

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => Context.Set<TEntity>().Update(entity);

    public void Remove(TEntity entity) => Context.Set<TEntity>().Remove(entity);
}
