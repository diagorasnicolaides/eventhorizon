using BenchmarkDotNet.Attributes;
using Cy.Jinius.EventHorizon.Benchmark.Models;
using Cy.Jinius.EventHorizon.Config;
using Cy.Jinius.EventHorizon.Read;
using Cy.Jinius.EventHorizon.Write;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cy.Jinius.EventHorizon.Benchmark;

[MinColumn]
[MaxColumn]
[MemoryDiagnoser(true)]
public abstract class GlobalTestSetup
{
    internal EventHorizonRepository<Todo> _eventHorizonRepository;
    internal DocumentStore _vanillaStore;

    [GlobalSetup]
    public async Task Setup()
    {
        //var builder = new ConfigurationBuilder();
        //builder.SetBasePath(Directory.GetCurrentDirectory())
        //       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // IConfiguration config = builder.Build();

        var writeDbContext = CreateWriteDbContext();
        var readDbContext = CreateReadDbContext();

        var readRepository = new ReadRepository<Todo, TodoReadDbContext>(readDbContext);
        var writeRepository = new WriteRepository<Todo>(writeDbContext);

        _eventHorizonRepository = new EventHorizonRepository<Todo>(writeRepository, readRepository);

        _vanillaStore = DocumentStore.For(opts =>
        {
            opts.Connection("PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'");
            opts.Projections.Snapshot<Models.Marten.Todo>(Marten.Events.Projections.SnapshotLifecycle.Inline);
        });

        await GlobalSetupTest();
    }

    public virtual async Task GlobalSetupTest()
    {

    }


    private static WriteDbContext CreateWriteDbContext()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseNpgsql("PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'")
            .Options;

        var context = new WriteDbContext(options, GetConfig());

        context.Database.EnsureCreated();

        return context;
    }

    private static TodoReadDbContext CreateReadDbContext()
    {
        var options = new DbContextOptionsBuilder<TodoReadDbContext>()
            .UseNpgsql("PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'")
            .Options;

        var context = new TodoReadDbContext(options, GetConfig());

        context.Database.EnsureCreated();

        return context;
    }

    private static IOptions<EventHorizonConfiguration> GetConfig()
    {
        return Options.Create(new EventHorizonConfiguration(
                "benchmark",
                "benchmark",
                "PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'",
                "PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'"
            ));
    }

    //[GlobalCleanup]
    //public void GlobalCleanup()
    //{
    //    using var scope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
    //    using var context = scope.ServiceProvider.GetRequiredService<WriteDbContext>();

    //    var allEvents = context.WriteEvents.ToList();
    //    allEvents.ForEach(e => context.Remove(e));
    //    context.SaveChanges();
    //}

}
