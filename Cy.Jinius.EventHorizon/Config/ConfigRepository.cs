namespace Cy.Jinius.EventHorizon.Config
{
    using Cy.Jinius.EventHorizon.Write;

    public class ConfigRepository(WriteDbContext writeDbContext) : IConfigRepository
    {
        public bool IsRebuildingReadModel()
        {
            return writeDbContext.EventHorizonConfig.FirstOrDefault()?.IsRebuildingReadModel ?? false;
        }

        public async Task SetRebuildingReadModel(bool isRebuildingReadModel)
        {
            var config = writeDbContext.EventHorizonConfig.FirstOrDefault();

            if (config == null)
            {
                config = new ConfigEntity { IsRebuildingReadModel = isRebuildingReadModel };
                await writeDbContext.EventHorizonConfig.AddAsync(config);
            }
            else
            {
                config.IsRebuildingReadModel = isRebuildingReadModel;
                writeDbContext.EventHorizonConfig.Update(config);
            }

            await writeDbContext.SaveChangesAsync();
        }
    }
}
