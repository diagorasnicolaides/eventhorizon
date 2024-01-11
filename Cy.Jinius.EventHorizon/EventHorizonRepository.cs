using Cy.Jinius.EventHorizon.Read;
using Cy.Jinius.EventHorizon.Write;
using System.Transactions;

namespace Cy.Jinius.EventHorizon;
internal class EventHorizonRepository<T>(IWriteRepository<T> writeRepository, IReadRepository<T> readRepository)
    : IEventHorizonRepository<T> where T : BaseAggregate
{
    public async Task<T> CreateAsync(T aggregate, Action? action = null)
    {
        using var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled);

        await writeRepository.CreateAsync(aggregate);
        var aggregateSnapshot = await readRepository.CreateAsync(aggregate);

        action?.Invoke();

        scope.Complete();

        return aggregateSnapshot;
    }

    public async Task<T> UpdateAsync(T aggregate, Action? action = null)
    {
        using var scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled);

        await writeRepository.UpdateAsync(aggregate);
        var aggregateSnapshot = await readRepository.UpdateAsync(aggregate);

        action?.Invoke();

        scope.Complete();

        return aggregateSnapshot;
    }

    public async Task<T?> FindByIdAsync(Guid id)
    {
        return await readRepository.FindByIdAsync(id);
    }

    public async Task<IEnumerable<object>> GetAllEventsAsync(Guid aggregateId)
    {
        return await writeRepository.GetAllEventsAsync(aggregateId);
    }

    public IQueryable<T> Query()
    {
        return readRepository.Query();
    }
}

