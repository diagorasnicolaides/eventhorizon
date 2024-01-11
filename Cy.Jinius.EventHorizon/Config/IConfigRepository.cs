namespace Cy.Jinius.EventHorizon.Config
{
    public interface IConfigRepository
    {
        bool IsRebuildingReadModel();

        Task SetRebuildingReadModel(bool isRebuildingReadModel);
    }
}
