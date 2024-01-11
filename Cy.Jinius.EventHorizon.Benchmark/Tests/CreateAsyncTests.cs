using BenchmarkDotNet.Attributes;
using Cy.Jinius.EventHorizon.Benchmark.Models;
using Cy.Jinius.EventHorizon.Benchmark.Models.Event;

namespace Cy.Jinius.EventHorizon.Benchmark.Tests;

public class CreateAsyncTests : GlobalTestSetup
{
    [Params(1)]
    public int totalEvents { get; set; }

    private Todo _createTodo;

    private List<TodoCreatedEvent> _vanillaEvents;

    [IterationSetup]
    public void IterationSetup()
    {
        _createTodo = new Todo();
        for (int i = 0; i < totalEvents; i++)
        {
            _createTodo.EnqueueApply(new TodoCreatedEvent("Benchmark"));
        }

        _vanillaEvents = [];
        for (int i = 0; i < totalEvents; i++)
        {
            _vanillaEvents.Add(new TodoCreatedEvent("Benchmark"));
        }
    }

    [Benchmark]
    public async Task EventHorizon_CreateAsync()
    {
        await _eventHorizonRepository.CreateAsync(_createTodo);
    }

    [Benchmark]
    public async Task Marten_StartStream()
    {
        await using var session = _vanillaStore.LightweightSession();
        session.Events.StartStream<Models.Marten.Todo>(Guid.NewGuid(), _vanillaEvents);
        await session.SaveChangesAsync();
    }
}