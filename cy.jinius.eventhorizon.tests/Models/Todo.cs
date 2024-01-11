using cy.jinius.eventhorizon.tests.Models.Events;
using Cy.Jinius.EventHorizon.Write;

namespace cy.jinius.eventhorizon.tests.Models;

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
                Timestamps = new TodoTimestamps(startedEvent.StartedTime.AddHours(1), null);
                break;
            case TodoCompletedEvent completedEvent:
                Timestamps = new TodoTimestamps(Timestamps.StartedTime, completedEvent.CompletedTime);
                break;
        }
    }
}

