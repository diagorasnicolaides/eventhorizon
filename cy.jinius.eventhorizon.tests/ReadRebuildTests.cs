using cy.jinius.eventhorizon.tests.Models;
using cy.jinius.eventhorizon.tests.Models.Events;
using Cy.Jinius.EventHorizon.Config;
using Cy.Jinius.EventHorizon.Read;
using Cy.Jinius.EventHorizon.ReadRebuild;
using Cy.Jinius.EventHorizon.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace cy.jinius.eventhorizon.tests;

[Collection("NonParallelCollection")]
public class ReadRebuildTests : IDisposable
{
    private readonly WriteDbContext _writeDbContext;
    private readonly TodoReadDbContext _readDbContext;
    private readonly ReadRepository<Todo, TodoReadDbContext> _readRepository;
    private readonly WriteRepository<Todo> _writeRepository;
    private readonly ReadRebuilder<Todo> _readRebuilder;

    public ReadRebuildTests()
    {
        _writeDbContext = CreateWriteDbContext();
        _readDbContext = CreateReadDbContext();
        var configRepository = new ConfigRepository(_writeDbContext);

        _readRepository = new ReadRepository<Todo, TodoReadDbContext>(_readDbContext);
        _writeRepository = new WriteRepository<Todo>(_writeDbContext);

        _readRebuilder = new ReadRebuilder<Todo>(configRepository, _writeRepository, _readRepository, Mock.Of<ILogger<ReadRebuilder<Todo>>>());
    }

    [Fact]
    public async Task ReadRebuilder_RebuildReadModelAsync_Successful()
    {
        // Arrange
        var uniqueId = Guid.NewGuid();
        var todo1 = SetupNewTodo("Todo 1" + uniqueId, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1));
        var todo2 = SetupNewTodo("Todo 2" + uniqueId, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2));
        var todo3 = SetupNewTodo("Todo 3" + uniqueId, DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(3));

        await _writeRepository.CreateAsync(todo1);
        await _writeRepository.CreateAsync(todo2);
        await _writeRepository.CreateAsync(todo3);

        // Act
        await _readRebuilder.RebuildReadModelAsync();

        // Assert
        var results = await _readRepository.Query()
                                            .Where(r => r.Detail.Contains(uniqueId.ToString()))
                                            .OrderBy(r => r.Timestamps.StartedTime)
                                            .ToListAsync();
        Assert.Equal(3, results.Count);

        Assert.Equal(todo1.Detail, results[0].Detail);
        Assert.True(todo1.Timestamps.CompletedTime?.Ticks - results[0].Timestamps.CompletedTime?.Ticks <= 10);
        Assert.True(todo1.Timestamps.StartedTime?.Ticks - results[0].Timestamps.StartedTime?.Ticks <= 10);

        Assert.Equal(todo2.Detail, results[1].Detail);
        Assert.True(todo2.Timestamps.CompletedTime?.Ticks - results[1].Timestamps.CompletedTime?.Ticks <= 10);
        Assert.True(todo2.Timestamps.StartedTime?.Ticks - results[1].Timestamps.StartedTime?.Ticks <= 10);

        Assert.Equal(todo3.Detail, results[2].Detail);
        Assert.True(todo3.Timestamps.CompletedTime?.Ticks - results[2].Timestamps.CompletedTime?.Ticks <= 10);
        Assert.True(todo3.Timestamps.StartedTime?.Ticks - results[2].Timestamps.StartedTime?.Ticks <= 10);
    }


    #region Helper methods

    private Todo SetupNewTodo(string detail, DateTime startedTime, DateTime completedTime)
    {
        var todo = new Todo();
        var todoCreatedEvent = new TodoCreatedEvent(detail);
        var todoStartedEvent = new TodoStartedEvent(startedTime);
        var todoCompletedEvent = new TodoCompletedEvent(completedTime);

        todo.EnqueueApply(todoCreatedEvent);
        todo.EnqueueApply(todoStartedEvent);
        todo.EnqueueApply(todoCompletedEvent);

        return todo;
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

    public void Dispose()
    {
        var allEvents = _writeDbContext.WriteEvents.ToList();
        allEvents.ForEach(e => _writeDbContext.Remove(e));
        _writeDbContext.SaveChanges();

        var aggregateSnapshots = _readDbContext.AggregateSnapshots.AsNoTracking().ToList();
        aggregateSnapshots.ForEach(a => _readRepository.DeleteByIdAsync(a.Id).Wait());
    }

    #endregion
}
