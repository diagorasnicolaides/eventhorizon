using Cy.Jinius.EventHorizon.Write;

namespace Cy.Jinius.EventHorizon.testapi.Models
{
    public class Person : BaseAggregate
    {
        public string? Name { get; set; }
        public Address? Address { get; set; }
        public override void WhenEventArrives(object @event)
        {
            switch (@event)
            {
                case NameAddedEvent nameAddedEvent:
                    Name = nameAddedEvent.Name;
                    break;
                case NameUpdatedEvent nameUpdatedEvent:
                    Name = nameUpdatedEvent.Name;
                    break;
                case AddressAddedEvent addressAddedEvent:
                    Address = new Address
                    {
                        Street = addressAddedEvent.Street,
                        City = addressAddedEvent.City
                    };
                    break;

            }
        }
    }
}
