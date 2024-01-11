using BenchmarkDotNet.Attributes;
using Cy.Jinius.EventHorizon.Benchmark.Models;
using Cy.Jinius.EventHorizon.Benchmark.Models.Event;

namespace Cy.Jinius.EventHorizon.Benchmark.Tests;

public class GetAllEventsAsyncTests : GlobalTestSetup
{
    [Params(1, 10, 100)]
    public int totalEvents { get; set; }

    private Guid _jiniusId, _martenId;

    public override async Task GlobalSetupTest()
    {
        var todo = new Todo();
        var martenEvents = new List<TodoCreatedEvent>();
        for (int i = 0; i < totalEvents; i++)
        {
            var todoCreated = new TodoCreatedEvent("Benchmark");
            todo.EnqueueApply(todoCreated);
            martenEvents.Add(todoCreated);
        }

        await _eventHorizonRepository.CreateAsync(todo);
        _jiniusId = todo.Id;

        _martenId = Guid.NewGuid();
        await using var _session = _vanillaStore.LightweightSession();
        _session.Events.StartStream<Models.Marten.Todo>(_martenId, martenEvents);
        await _session.SaveChangesAsync();
    }

    [Benchmark]
    public async Task EventHorizon_GetAllEventsAsync()
    {
        await _eventHorizonRepository.GetAllEventsAsync(_jiniusId);
    }

    [Benchmark]
    public async Task Marten_FetchStreamAsync()
    {
        await using var session = _vanillaStore.QuerySession();
        await session.Events.FetchStreamAsync(_martenId);
    }
}