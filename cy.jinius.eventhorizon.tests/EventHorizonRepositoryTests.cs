using cy.jinius.eventhorizon.tests.Models;
using cy.jinius.eventhorizon.tests.Models.Events;
using Cy.Jinius.EventHorizon;
using Cy.Jinius.EventHorizon.Config;
using Cy.Jinius.EventHorizon.Read;
using Cy.Jinius.EventHorizon.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace cy.jinius.eventhorizon.tests;

[Collection("NonParallelCollection")]
public class EventHorizonRepositoryTests : IDisposable
{
    private readonly EventHorizonRepository<Todo> _eventHorizonRepository;
    private readonly DateTime _startedDateTime;
    private readonly DateTime _completedDateTime;
    private readonly WriteDbContext _writeDbContext;
    private readonly TodoReadDbContext _readDbContext;
    private readonly ReadRepository<Todo, TodoReadDbContext> _readRepository;

    public EventHorizonRepositoryTests()
    {
        _writeDbContext = CreateWriteDbContext();
        _readDbContext = CreateReadDbContext();

        _readRepository = new ReadRepository<Todo, TodoReadDbContext>(_readDbContext);
        var writeRepository = new WriteRepository<Todo>(_writeDbContext);

        _startedDateTime = DateTime.UtcNow;
        _completedDateTime = DateTime.UtcNow.AddDays(1);

        _eventHorizonRepository = new EventHorizonRepository<Todo>(writeRepository, _readRepository);
    }

    #region CreateAsync tests

    [Fact]
    public async Task EventHorizonRepository_CreateAsync_NewTodoCreated()
    {
        // Arrange
        var todo = SetupNewTodo();

        // Act
        var result = await _eventHorizonRepository.CreateAsync(todo);

        // Assert
        Assert.Equal(_completedDateTime, result.Timestamps.CompletedTime);
        Assert.Equal(_startedDateTime, result.Timestamps.StartedTime);
        Assert.Equal("My todo", result.Detail);
    }

    [Fact]
    public async Task EventHorizonRepository_CreateAsync_AlreadyExistsException()
    {
        // Arrange
        var todo = SetupNewTodo();

        // Act
        await _eventHorizonRepository.CreateAsync(todo);

        // Assert
        await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await _eventHorizonRepository.CreateAsync(todo));
    }

    [Fact]
    public async Task EventHorizonRepository_CreateAsync_ActionSuccessfulTodoCreated()
    {
        // Arrange
        var todo = SetupNewTodo();
        var actionSuccessful = false;

        // Act
        var result = await _eventHorizonRepository.CreateAsync(todo, () => actionSuccessful = true);

        // Assert
        Assert.Equal(_completedDateTime, result.Timestamps.CompletedTime);
        Assert.Equal(_startedDateTime, result.Timestamps.StartedTime);
        Assert.Equal("My todo", result.Detail);
        Assert.True(actionSuccessful);
    }

    [Fact]
    public async Task EventHorizonRepository_CreateAsync_ReadFailsNoWrite()
    {
        // Arrange
        var todo = SetupNewTodo();
        await _readRepository.CreateAsync(todo); // this will cause the read action to fail

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _eventHorizonRepository.CreateAsync(todo));

        // Assert
        var events = await _eventHorizonRepository.GetAllEventsAsync(todo.Id);
        Assert.Empty(events);
    }

    [Fact]
    public async Task EventHorizonRepository_CreateAsync_ActionFailsNoWriteNoRead()
    {
        // Arrange
        var todo = SetupNewTodo();

        // Act
        await Assert.ThrowsAnyAsync<Exception>(async () => await _eventHorizonRepository.CreateAsync(todo, () => throw new Exception()));

        // Assert
        var events = await _eventHorizonRepository.GetAllEventsAsync(todo.Id);
        var foundTodo = await _eventHorizonRepository.FindByIdAsync(todo.Id);
        Assert.Empty(events);
        Assert.Null(foundTodo);
    }

    #endregion

    #region UpdateAsync tests

    [Fact]
    public async Task EventHorizonRepository_UpdateAsync_UpdateTodoSuccess()
    {
        // Arrange
        var todo = SetupNewTodo();
        todo = await _eventHorizonRepository.CreateAsync(todo);

        // Act
        var todoDetailUpdatedEvent = new TodoDetailUpdatedEvent("My todo is updated");
        todo.EnqueueApply(todoDetailUpdatedEvent);
        var result = await _eventHorizonRepository.UpdateAsync(todo);

        // Assert
        Assert.Equal("My todo is updated", result.Detail);
        var foundTodo = await _eventHorizonRepository.FindByIdAsync(todo.Id);
        Assert.Equal("My todo is updated", foundTodo?.Detail);
    }

    [Fact]
    public async Task EventHorizonRepository_UpdateAsync_UpdateNonExistentTodoThrows()
    {
        // Arrange
        var todo = SetupNewTodo();

        // Act and Assert
        await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await _eventHorizonRepository.UpdateAsync(todo));
    }

    [Fact]
    public async Task EventHorizonRepository_UpdateAsync_ActionFailsNoWriteNoReadUpdate()
    {
        // Arrange
        var todo = SetupNewTodo();
        await _eventHorizonRepository.CreateAsync(todo);

        var todoDetailUpdatedEvent = new TodoDetailUpdatedEvent("My todo is updated");
        todo.EnqueueApply(todoDetailUpdatedEvent);

        // Act
        await Assert.ThrowsAnyAsync<Exception>(async () => await _eventHorizonRepository.UpdateAsync(todo, () => throw new Exception()));

        // Assert
        var events = await _eventHorizonRepository.GetAllEventsAsync(todo.Id);
        var foundTodo = await _eventHorizonRepository.FindByIdAsync(todo.Id);
        Assert.Equal(3, events.Count());
        Assert.DoesNotContain(events, e => e.GetType().Equals(todoDetailUpdatedEvent.GetType()));
        Assert.Equal("My todo", foundTodo?.Detail);
    }

    #endregion

    #region FindByIdAsync tests

    [Fact]
    public async Task EventHorizonRepository_FindByIdAsync_NullIfDoesNotExist()
    {
        // Arrange
        var todo = SetupNewTodo();
        await _eventHorizonRepository.CreateAsync(todo);

        // Act
        var result = await _eventHorizonRepository.FindByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EventHorizonRepository_FindByIdAsync_SuccessfullyFound()
    {
        // Arrange
        var todo = SetupNewTodo();
        await _eventHorizonRepository.CreateAsync(todo);

        // Act
        var result = await _eventHorizonRepository.FindByIdAsync(todo.Id);

        // Assert
        Assert.True(_startedDateTime.Ticks - result?.Timestamps.StartedTime?.Ticks <= 10);
        Assert.True(_completedDateTime.Ticks - result?.Timestamps.CompletedTime?.Ticks <= 10);
        Assert.Equal("My todo", result?.Detail);
    }

    #endregion


    [Fact]
    public async Task EventHorizonRepository_GetAllEventsAsync_FindsAllEvents()
    {
        // Arrange
        var todo = SetupNewTodo();
        todo = await _eventHorizonRepository.CreateAsync(todo);

        // Act
        var result = await _eventHorizonRepository.GetAllEventsAsync(todo.Id);

        // Assert
        Assert.Equal(3, result.Count());

        // First event
        var firstEventResult = (TodoCreatedEvent)result.ElementAt(0);
        Assert.Equal("My todo", firstEventResult.Detail);

        // Second event
        var secondEventResult = (TodoStartedEvent)result.ElementAt(1);
        Assert.Equal(_startedDateTime, secondEventResult.StartedTime);

        // Third event
        var thirdEventResult = (TodoCompletedEvent)result.ElementAt(2);
        Assert.Equal(_completedDateTime, thirdEventResult.CompletedTime);
    }

    [Fact]
    public async Task EventHorizonRepository_Query_SearchDetailFindsResult()
    {
        // Arrange
        var todo = new Todo();
        var randomForUniqueness = Guid.NewGuid();
        var todoCreatedEvent = new TodoCreatedEvent("My unique todo " + randomForUniqueness);
        todo.EnqueueApply(todoCreatedEvent);
        await _eventHorizonRepository.CreateAsync(todo);

        // Act
        var result = await _eventHorizonRepository.Query().Where(t => t.Detail.Contains(randomForUniqueness.ToString())).ToListAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("My unique todo " + randomForUniqueness, result.First().Detail);
    }

    [Fact]
    public async Task EventHorizonRepository_FindByIdAsyncThenUpdateAsync_ReadAndWriteSuccessfullyUpdated()
    {
        // Arrange
        // 1. Create a TODO
        var todo = SetupNewTodo();
        await _eventHorizonRepository.CreateAsync(todo);

        // 2. Find the created TODO
        var foundTodo = await _eventHorizonRepository.FindByIdAsync(todo.Id);

        // 3. Update the created TODO
        var todoDetailUpdatedEvent = new TodoDetailUpdatedEvent("My todo is updated");
        foundTodo?.EnqueueApply(todoDetailUpdatedEvent);
        await _eventHorizonRepository.UpdateAsync(foundTodo!);

        // Act
        var foundUpdatedTodo = await _eventHorizonRepository.FindByIdAsync(todo.Id);
        var allEvents = await _eventHorizonRepository.GetAllEventsAsync(todo.Id);

        // Assert
        // 1. Assert read model has update detail
        Assert.Equal("My todo is updated", foundUpdatedTodo?.Detail);

        // 2. Assert write model has updated event
        Assert.Equal(4, allEvents.Count());
        Assert.Equal(todoDetailUpdatedEvent.GetType(), allEvents.Last().GetType());
    }

    #region Helper methods

    private Todo SetupNewTodo()
    {
        var todo = new Todo();
        var todoCreatedEvent = new TodoCreatedEvent("My todo");
        var todoStartedEvent = new TodoStartedEvent(_startedDateTime);
        var todoCompletedEvent = new TodoCompletedEvent(_completedDateTime);

        todo.EnqueueApply(todoCreatedEvent);
        todo.EnqueueApply(todoStartedEvent);
        todo.EnqueueApply(todoCompletedEvent);

        return todo;
    }

    private static WriteDbContext CreateWriteDbContext()
    {
        var config = GetConfig();
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseNpgsql(config.Value.WriteConnectionString)
            .Options;

        var context = new WriteDbContext(options, GetConfig());

        context.Database.EnsureCreated();

        return context;
    }

    private static TodoReadDbContext CreateReadDbContext()
    {
        var config = GetConfig();
        var options = new DbContextOptionsBuilder<TodoReadDbContext>()
            .UseNpgsql(config.Value.ReadConnectionString)
            .Options;

        var context = new TodoReadDbContext(options, config);

        context.Database.EnsureCreated();

        return context;
    }

    private static IOptions<EventHorizonConfiguration> GetConfig()
    {
        return Options.Create(new EventHorizonConfiguration(
                "public",
                "public",
                "PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'",
                "PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'"
            ));
    }

    public void Dispose()
    {
        //var allEvents = _writeDbContext.WriteEvents.ToList();
        //allEvents.ForEach(e => _writeDbContext.Remove(e));
        //_writeDbContext.SaveChanges();

        //var aggregateSnapshots = _readDbContext.AggregateSnapshots.AsNoTracking().ToList();
        //aggregateSnapshots.ForEach(a => _readRepository.DeleteByIdAsync(a.Id).Wait());
        //_readDbContext.SaveChanges();
    }

    #endregion
}
