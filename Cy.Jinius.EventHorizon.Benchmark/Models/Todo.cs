using Cy.Jinius.EventHorizon.Benchmark.Models.Event;
using Cy.Jinius.EventHorizon.Write;

namespace Cy.Jinius.EventHorizon.Benchmark.Models;

public class Todo : BaseAggregate
{
    public string Detail { get; set; }

    public TodoTimestamps Timestamps { get; set; }

    public Todo()
    {
        Timestamps = new TodoTimestamps(null, null);
    }

    public override void WhenEventArrives(object @event)
    {
        switch (@event)
        {
            case TodoCreatedEvent createdEvent:
                Detail = createdEvent.Detail;
                break;
            case TodoDetailUpdatedEvent updatedEvent:
                Detail = updatedEvent.Detail;
                break;
            case TodoStartedEvent startedEvent:
                Timestamps = new TodoTimestamps(startedEvent.StartedTime, null);
                break;
            case TodoCompletedEvent completedEvent:
                Timestamps = new TodoTimestamps(Timestamps.StartedTime, completedEvent.CompletedTime);
                break;
        }
    }
}

