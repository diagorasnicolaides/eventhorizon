using Cy.Jinius.EventHorizon.Write;

namespace Cy.Jinius.EventHorizon.Read;

public interface IReadRepository<TAggregate> where TAggregate : BaseAggregate
{
    Task<TAggregate> CreateAsync(TAggregate aggregate);
    Task<TAggregate> UpdateAsync(TAggregate aggregate);
    Task<TAggregate> FindByIdAsync(Guid id);
    Task DeleteByIdAsync(Guid id);
    IQueryable<TAggregate> Query();
}
