using BenchmarkDotNet.Attributes;
using Cy.Jinius.EventHorizon.Benchmark.Models;
using Cy.Jinius.EventHorizon.Benchmark.Models.Event;

namespace Cy.Jinius.EventHorizon.Benchmark.Tests;

public class UpdateAsyncTests : GlobalTestSetup
{
    [Params(1, 10, 100)]
    public int totalEvents { get; set; }

    private Guid _martenId;
    private Todo _eventHorizonTodo;

    [IterationSetup]
    public void IterationSetupTest()
    {
        _martenId = Guid.NewGuid();
        _eventHorizonTodo = new Todo();
        var martenEvents = new List<TodoCreatedEvent>();
        for (int i = 0; i < totalEvents; i++)
        {
            var todoCreated = new TodoCreatedEvent("Benchmark");
            _eventHorizonTodo.EnqueueApply(todoCreated);
            martenEvents.Add(todoCreated);
        }

        _eventHorizonRepository.CreateAsync(_eventHorizonTodo).Wait();

        using var _session = _vanillaStore.LightweightSession();
        _session.Events.StartStream<Models.Marten.Todo>(_martenId, martenEvents);
        _session.SaveChanges();
    }

    [Benchmark]
    public async Task EventHorizon_GetAllEventsAsync()
    {
        _eventHorizonTodo.EnqueueApply(new TodoStartedEvent(DateTime.UtcNow));
        await _eventHorizonRepository.UpdateAsync(_eventHorizonTodo);
    }

    [Benchmark]
    public async Task Marten_FetchStreamAsync()
    {
        await using var session = _vanillaStore.LightweightSession();
        await session.Events.AppendOptimistic(_martenId, new TodoStartedEvent(DateTime.UtcNow));
    }
}