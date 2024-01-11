using BenchmarkDotNet.Attributes;
using Cy.Jinius.EventHorizon.Benchmark.Models;
using Cy.Jinius.EventHorizon.Benchmark.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace Cy.Jinius.EventHorizon.Benchmark.Tests;

public class QueryTests : GlobalTestSetup
{
    [Params(1, 10, 100)]
    public int totalEvents { get; set; }

    private Guid _jiniusId, _martenId, _uniqueTestGuid;

    public override async Task GlobalSetupTest()
    {
        var todo = new Todo();
        var martenEvents = new List<TodoCreatedEvent>();
        _uniqueTestGuid = Guid.NewGuid();
        for (int i = 0; i < totalEvents; i++)
        {
            var todoCreated = new TodoCreatedEvent("Benchmark " + _uniqueTestGuid);
            todo.EnqueueApply(todoCreated);
            martenEvents.Add(todoCreated);
        }

        await _eventHorizonRepository.CreateAsync(todo);
        _jiniusId = todo.Id;

        _martenId = Guid.NewGuid();
        await using var session = _vanillaStore.LightweightSession();
        session.Events.StartStream<Models.Marten.Todo>(_martenId, martenEvents);
        await session.SaveChangesAsync();
    }

    [Benchmark]
    public async Task EventHorizon_Query()
    {
        await _eventHorizonRepository.Query().Where(a => a.Detail.Contains(_uniqueTestGuid.ToString())).ToListAsync();
    }

    [Benchmark]
    public async Task Marten_Query()
    {
        await using var session = _vanillaStore.QuerySession();
        await session.Query<Models.Marten.Todo>().Where(a => a.Detail.Contains(_uniqueTestGuid.ToString())).ToListAsync();
    }
}