namespace Cy.Jinius.EventHorizon.ReadRebuild;
using Cy.Jinius.EventHorizon.Config;
using Cy.Jinius.EventHorizon.Read;
using Cy.Jinius.EventHorizon.Write;
using Microsoft.Extensions.Logging;

internal class ReadRebuilder<TAggregate> where TAggregate : BaseAggregate
{
    private readonly IConfigRepository _internalConfigRepository;
    private readonly IWriteRepository<TAggregate> _writeRepository;
    private readonly IReadRepository<TAggregate> _readRepository;
    private readonly ILogger<ReadRebuilder<TAggregate>> _logger;

    internal ReadRebuilder(IConfigRepository internalConfigRepository, IWriteRepository<TAggregate> writeRepository, IReadRepository<TAggregate> readRepository, ILogger<ReadRebuilder<TAggregate>> logger)
    {
        _internalConfigRepository = internalConfigRepository;
        _writeRepository = writeRepository;
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task RebuildReadModelAsync()
    {
        _logger.LogInformation("Rebuilding read model...");
        var isAlreadyRebuilding = _internalConfigRepository.IsRebuildingReadModel();

        if (isAlreadyRebuilding)
        {
            while (isAlreadyRebuilding)
            {
                _logger.LogInformation("Waiting for another pod to finish rebuilding the read model...");
                await Task.Delay(1000);
                isAlreadyRebuilding = _internalConfigRepository.IsRebuildingReadModel();
            }
            _logger.LogInformation("Other pod finished!");
        }
        else
        {
            _logger.LogInformation("Starting rebuild...");
            await _internalConfigRepository.SetRebuildingReadModel(true);

            _logger.LogInformation("Finding all streams...");
            var allStreamIds = await _writeRepository.GetAllStreamIdsAsync(); // TODO: should bring in batches

            _logger.LogInformation("Found {count} streams", allStreamIds.Count());

            foreach (var streamId in allStreamIds)
            {
                _logger.LogInformation("Deleting {streamId}...", streamId);
                await _readRepository.DeleteByIdAsync(streamId);

                _logger.LogInformation("Rebuilding {streamId}...", streamId);
                var aggregate = await _writeRepository.FindByIdAsync(streamId);
                await _readRepository.CreateAsync(aggregate);
                _logger.LogInformation("Rebuilt {streamId}!", streamId);
            }

            await _internalConfigRepository.SetRebuildingReadModel(false);
            _logger.LogInformation("Finished rebuild!");
        }

    }
}
