namespace Cy.Jinius.EventHorizon.Write;

using Cy.Jinius.EventHorizon.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class WriteDbContext(DbContextOptions<WriteDbContext> options, IOptions<EventHorizonConfiguration> configuration) : DbContext(options)
{
    public DbSet<ConfigEntity> EventHorizonConfig { get; set; }
    public DbSet<WriteEventEntity> WriteEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(configuration.Value.WriteSchema);

        modelBuilder.Entity<WriteEventEntity>().HasKey(b => b.Id);
        modelBuilder.Entity<WriteEventEntity>().Property(b => b.Id).ValueGeneratedOnAdd();
        // modelBuilder.Entity<WriteEventEntity>().Property(a => a.Version).IsConcurrencyToken();

        modelBuilder.Entity<ConfigEntity>().HasKey(b => b.Id);
        modelBuilder.Entity<ConfigEntity>().Property(b => b.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<ConfigEntity>().Property(b => b.Version).IsRowVersion();
    }
}