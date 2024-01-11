namespace Cy.Jinius.EventHorizon.Read;

using Cy.Jinius.EventHorizon.Config;
using Cy.Jinius.EventHorizon.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;

public abstract class ReadDbContext<TAggregate, TDbContext>(DbContextOptions<TDbContext> options, IOptions<EventHorizonConfiguration> configuration) : DbContext(options)
    where TAggregate : BaseAggregate
    where TDbContext : DbContext
{
    public DbSet<AggregateSnapshotEntity<TAggregate>> AggregateSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(configuration.Value.ReadSchema);

        modelBuilder.Entity<AggregateSnapshotEntity<TAggregate>>()
                .HasKey(b => b.Id);

        modelBuilder.Entity<AggregateSnapshotEntity<TAggregate>>()
                .Property(a => a.Version).IsConcurrencyToken();

        modelBuilder.Entity<AggregateSnapshotEntity<TAggregate>>().OwnsOne(a => a.Payload, builder =>
        {
            builder.ToJson();
            BuildActionForChildren(builder);
        });

    }

    protected abstract void BuildActionForChildren(OwnedNavigationBuilder<AggregateSnapshotEntity<TAggregate>, TAggregate> builder);
}