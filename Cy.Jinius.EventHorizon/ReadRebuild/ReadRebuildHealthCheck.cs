using Cy.Jinius.EventHorizon.Config;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cy.Jinius.EventHorizon.ReadRebuild;
internal class ReadRebuildHealthCheck : IHealthCheck
{
    private readonly IConfigRepository _repository;

    public ReadRebuildHealthCheck(IConfigRepository repository)
    {
        _repository = repository;
    }
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return _repository.IsRebuildingReadModel()
                                        ? Task.FromResult(HealthCheckResult.Unhealthy("Read model is being rebuilt"))
                                        : Task.FromResult(HealthCheckResult.Healthy("Read model is ready"));
    }
}
