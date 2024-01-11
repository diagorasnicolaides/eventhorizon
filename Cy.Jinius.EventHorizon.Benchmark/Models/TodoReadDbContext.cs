using Cy.Jinius.EventHorizon.Config;
using Cy.Jinius.EventHorizon.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;

namespace Cy.Jinius.EventHorizon.Benchmark.Models;

public class TodoReadDbContext(DbContextOptions<TodoReadDbContext> options, IOptions<EventHorizonConfiguration> configuration) : ReadDbContext<Todo, TodoReadDbContext>(options, configuration)
{
    protected override void BuildActionForChildren(OwnedNavigationBuilder<AggregateSnapshotEntity<Todo>, Todo> builder)
    {
        builder.OwnsOne(x => x.Timestamps);
    }
}

