using Cy.Jinius.EventHorizon.Write;

namespace Cy.Jinius.EventHorizon
{
    public interface IEventHorizonRepository<TAggregate> where TAggregate : BaseAggregate
    {
        Task<TAggregate> CreateAsync(TAggregate aggregate, Action? action = null);

        Task<TAggregate> UpdateAsync(TAggregate aggregate, Action? action = null);

        Task<TAggregate?> FindByIdAsync(Guid id);

        Task<IEnumerable<object>> GetAllEventsAsync(Guid aggregateId);

        IQueryable<TAggregate> Query();
    }
}