using Cy.Jinius.EventHorizon.Write;

namespace Cy.Jinius.EventHorizon.Read;

public class AggregateSnapshotEntity<TAggregate> where TAggregate : BaseAggregate
{
    public Guid Id { get; set; }

    public TAggregate? Payload { get; set; }

    public int Version { get; set; }
}
