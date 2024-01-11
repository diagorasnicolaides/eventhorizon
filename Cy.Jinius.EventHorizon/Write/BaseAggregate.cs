namespace Cy.Jinius.EventHorizon.Write
{
    public abstract class BaseAggregate
    {
        public Guid Id { get; set; }

        [NonSerialized]
        private readonly IList<object> _changes;

        public int Version { get; set; }

        public DateTime Created { get; set; }

        public string? CreatedBy { get; set; }

        public string? CreatedSource { get; set; }

        public DateTime LastModified { get; set; }

        public string? LastModifiedBy { get; set; }

        public string? LastModifiedSource { get; set; }

        protected BaseAggregate()
        {
            _changes = new List<object>();
            Version = 0;
            Id = Guid.NewGuid();
        }

        public IEnumerable<object> GetChanges() => _changes.AsReadOnly();
        public void ClearChanges() => _changes.Clear();
        public void EnqueueApply(object @event)
        {
            WhenEventArrives(@event);
            _changes.Add(@event);
        }

        public abstract void WhenEventArrives(object @event);
    }
}