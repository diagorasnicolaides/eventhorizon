using Cy.Jinius.EventHorizon.Config;
using Cy.Jinius.EventHorizon.Read;
using Cy.Jinius.EventHorizon.ReadRebuild;
using Cy.Jinius.EventHorizon.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cy.Jinius.EventHorizon;
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEventHorizonPostgres<TAggregate, TDbContext>(this IServiceCollection serviceCollection, IConfiguration configuration)
        where TAggregate : BaseAggregate
        where TDbContext : DbContext
    {
        var configurationSection = configuration.GetSection("EventHorizon");

        var eventHorizonConfiguration = configurationSection.Get<EventHorizonConfiguration>()!;

        serviceCollection.AddOptions<EventHorizonConfiguration>()
                            .Bind(configurationSection)
                            .ValidateDataAnnotations() // TODO: highlight in demo
                            .ValidateOnStart();

        serviceCollection.AddHealthChecks()
                         .AddNpgSql(eventHorizonConfiguration.ReadConnectionString, name: $"eh-{eventHorizonConfiguration.ReadSchema}")
                         .AddNpgSql(eventHorizonConfiguration.WriteConnectionString, name: $"eh-{eventHorizonConfiguration.WriteSchema}")
                         .AddCheck<ReadRebuildHealthCheck>("eh-readrebuild");


        serviceCollection.AddDbContext<TDbContext>(options =>
            options.UseNpgsql(eventHorizonConfiguration.ReadConnectionString, b =>
            {
                // b.MigrationsHistoryTable(section.MigrationsTable, section.MigrationsSchema);
                // b.MigrationsAssembly(typeof(IServiceCollectionExtensions).Assembly.FullName);
                b.EnableRetryOnFailure();
            }
            ));

        serviceCollection.AddDbContext<WriteDbContext>(options =>
            options.UseNpgsql(eventHorizonConfiguration.WriteConnectionString, b =>
            {
                // b.MigrationsHistoryTable(section.MigrationsTable, section.MigrationsSchema);
                // b.MigrationsAssembly(typeof(IServiceCollectionExtensions).Assembly.FullName);
                b.EnableRetryOnFailure();
            }
            ));

        serviceCollection.AddTransient<IReadRepository<TAggregate>, ReadRepository<TAggregate, TDbContext>>();
        serviceCollection.AddTransient<IWriteRepository<TAggregate>, WriteRepository<TAggregate>>();
        serviceCollection.AddTransient<IEventHorizonRepository<TAggregate>, EventHorizonRepository<TAggregate>>();

        return serviceCollection;
    }
}

