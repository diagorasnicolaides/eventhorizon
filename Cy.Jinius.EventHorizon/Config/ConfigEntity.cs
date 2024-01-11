namespace Cy.Jinius.EventHorizon.Config
{
    public class ConfigEntity
    {
        public Guid Id { get; internal init; }

        public bool IsRebuildingReadModel { get; set; }

        public uint Version { get; set; }
    }
}
