using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("cy.jinius.eventhorizon.tests")]
[assembly: InternalsVisibleTo("cy.jinius.eventhorizon.benchmark")]

namespace Cy.Jinius.EventHorizon.Write;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

internal class WriteRepository<TAggregate>(WriteDbContext dbContext) : IWriteRepository<TAggregate> where TAggregate : BaseAggregate
{
    public async Task CreateAsync(TAggregate aggregate)
    {
        // TODO: we can remove this and rely on DB ID to conflict as a perf boost
        //var events = await dbContext.WriteEvents.Where(x => x.AggregateId == aggregate.Id).ToListAsync();
        //if (events.Any())
        //{
        //    throw new InvalidOperationException($"Aggregate with ID {aggregate.Id} already exists");
        //}

        await CreateOrUpdateAsync(aggregate);
    }

    public async Task UpdateAsync(TAggregate aggregate)
    {
        var events = await dbContext.WriteEvents.Where(x => x.AggregateId == aggregate.Id).ToListAsync();
        if (!events.Any())
        {
            throw new InvalidOperationException($"Aggregate with ID {aggregate.Id} doesn't exist");
        }

        await CreateOrUpdateAsync(aggregate);
    }

    public async Task<IEnumerable<WriteEventEntity>> GetAllEventsRawAsync(Guid aggregateId)
    {
        return await dbContext.WriteEvents
                                    .Where(x => x.AggregateId == aggregateId)
                                    .OrderBy(a => a.Version)
                                    .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetAllEventsAsync(Guid aggregateId)
    {
        return await dbContext.WriteEvents
                                    .Where(x => x.AggregateId == aggregateId)
                                    .OrderBy(a => a.Version)
                                    .Select(e => JsonSerializer.Deserialize(e.Payload!, Type.GetType(e.Type!)!, JsonSerializerOptions.Default)!)
                                    .ToListAsync();
    }

    public async Task<IEnumerable<Guid>> GetAllStreamIdsAsync()
    {
        return await dbContext.WriteEvents.Select(e => e.AggregateId).Distinct().ToListAsync();
    }

    public async Task<TAggregate> FindByIdAsync(Guid aggregateId)
    {
        var events = await dbContext.WriteEvents
                                    .Where(x => x.AggregateId == aggregateId)
                                    .OrderBy(a => a.Version)
                                    .ToListAsync();

        var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate))!;
        aggregate.Id = aggregateId;

        if (events is not null && events.Any())
        {
            foreach (var @event in events)
            {
                var payload = JsonSerializer.Deserialize(@event.Payload!, Type.GetType(@event.Type!)!);
                var method = aggregate.GetType().GetMethod("WhenEventArrives");
                method!.Invoke(aggregate, new object[] { payload! });
            }

            var firstEvent = events.First();
            var lastEvent = events.Last();
            aggregate.Version = lastEvent.Version;
            aggregate.Created = firstEvent.Created;
            aggregate.CreatedBy = firstEvent.CreatedBy;
            aggregate.CreatedSource = firstEvent.CreatedSource;
            aggregate.LastModified = lastEvent.Created;
            aggregate.LastModifiedBy = lastEvent.CreatedBy;
            aggregate.LastModifiedSource = lastEvent.CreatedSource;
        }

        return aggregate;
    }

    private async Task CreateOrUpdateAsync(TAggregate aggregate)
    {
        var events = aggregate.GetChanges().Select(x => new WriteEventEntity()
        {
            AggregateId = aggregate.Id,
            Payload = JsonSerializer.Serialize<object>(x),
            Type = x.GetType().AssemblyQualifiedName!,
            Version = ++aggregate.Version,
            Created = DateTime.UtcNow,
            CreatedBy = "MY_CREATED_BY", // TODO: platformcontext
            CreatedSource = "MY_SOURCE", // TODO: platformcontext
        });
        await dbContext.WriteEvents.AddRangeAsync(events);

        await dbContext.SaveChangesAsync();
        aggregate.ClearChanges();
    }
}
