namespace Cy.Jinius.EventHorizon.Write
{
    public interface IWriteRepository<TAggregate> where TAggregate : BaseAggregate
    {
        Task CreateAsync(TAggregate aggregate);

        Task UpdateAsync(TAggregate aggregate);

        Task<IEnumerable<WriteEventEntity>> GetAllEventsRawAsync(Guid aggregateId);

        Task<IEnumerable<object>> GetAllEventsAsync(Guid aggregateId);

        Task<IEnumerable<Guid>> GetAllStreamIdsAsync();

        Task<TAggregate> FindByIdAsync(Guid aggregateId);
    }
}
