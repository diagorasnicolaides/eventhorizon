namespace Cy.Jinius.EventHorizon.Write;
public record class WriteEventEntity
{
    public Guid Id { get; internal init; }
    public Guid AggregateId { get; internal init; }
    public int Version { get; internal init; }
    public string? Payload { get; internal init; }
    public string? Type { get; internal init; }

    // Metadata
    public DateTime Created { get; internal set; }
    public string? CreatedBy { get; internal set; }
    public string? CreatedSource { get; internal set; }
}
