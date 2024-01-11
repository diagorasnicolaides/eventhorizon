using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("cy.jinius.eventhorizon.tests")]
[assembly: InternalsVisibleTo("cy.jinius.eventhorizon.benchmark")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // for mocking

namespace Cy.Jinius.EventHorizon.Read;

using Cy.Jinius.EventHorizon.Write;
using Microsoft.EntityFrameworkCore;

internal class ReadRepository<TAggregate, TDbContext> : IReadRepository<TAggregate>
    where TAggregate : BaseAggregate
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public ReadRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
    public async Task<TAggregate> CreateAsync(TAggregate aggregate)
    {
        var entity = await FindByIdAsync(aggregate.Id);
        if (entity != null)
        {
            throw new InvalidOperationException($"Aggregate with ID {aggregate.Id} already exists");
        }

        var newAggregate = new AggregateSnapshotEntity<TAggregate>
        {
            Id = aggregate.Id,
            Payload = aggregate,
            Version = aggregate.Version
        };
        var result = await _dbContext.Set<AggregateSnapshotEntity<TAggregate>>().AddAsync(newAggregate);

        await _dbContext.SaveChangesAsync();

        return result.Entity.Payload!;
    }

    public async Task<TAggregate> UpdateAsync(TAggregate aggregate)
    {
        AggregateSnapshotEntity<TAggregate>? dbAggregate = await _dbContext.Set<AggregateSnapshotEntity<TAggregate>>().FindAsync(aggregate.Id)
                                                            ?? throw new InvalidOperationException($"ID {aggregate.Id} was not found to update");

        dbAggregate!.Payload = aggregate;
        var result = _dbContext.Set<AggregateSnapshotEntity<TAggregate>>().Update(dbAggregate);

        await _dbContext.SaveChangesAsync();

        return result.Entity.Payload!;
    }

    public async Task<TAggregate?> FindByIdAsync(Guid id)
    {
        // EF core FindAsync uses the cache which we'd like to avoid
        var entity = await _dbContext.Set<AggregateSnapshotEntity<TAggregate>>().Where(a => a.Id.Equals(id)).ToListAsync();
        return entity.Any() ? entity.First().Payload! : null;
    }

    public IQueryable<TAggregate> Query()
    {
        return _dbContext.Set<AggregateSnapshotEntity<TAggregate>>().Select(a => a.Payload).AsNoTracking();
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var entity = await _dbContext.Set<AggregateSnapshotEntity<TAggregate>>().FindAsync(id);
        if (entity != null)
        {
            _dbContext.Set<AggregateSnapshotEntity<TAggregate>>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
