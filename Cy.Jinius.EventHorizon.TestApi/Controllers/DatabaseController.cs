using Cy.Jinius.EventHorizon.testapi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cy.Jinius.EventHorizon.testapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly IEventHorizonRepository<Person> _repository;

        public DatabaseController(IEventHorizonRepository<Person> repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<Person?> Get(Guid id)
        {
            return await _repository.FindByIdAsync(id);
        }

        [HttpPost("{name}")]
        public async Task<Person> Post(string name)
        {
            var person = new Person();
            person.EnqueueApply(new NameAddedEvent(name));
            return await _repository.CreateAsync(person);
        }

        [HttpPut("{id}/address")]
        public async Task<Person> PutAddress(Guid id)
        {
            Person? person = await _repository.FindByIdAsync(id) ?? throw new ArgumentOutOfRangeException("Person not found" + id);
            person.EnqueueApply(new AddressAddedEvent("My Street", "My City"));
            return await _repository.UpdateAsync(person);
        }

        [HttpPut("{id}/{name}")]
        public async Task<Person> Put(Guid id, string name)
        {
            Person? person = await _repository.FindByIdAsync(id) ?? throw new ArgumentOutOfRangeException("Person not found" + id);
            person.EnqueueApply(new NameUpdatedEvent(name));
            return await _repository.UpdateAsync(person);
        }
    }
}
