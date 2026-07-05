using System.Collections.Concurrent;
using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Interfaces;

namespace PersonPhone.Infrastructure.Repositories;

public class InMemoryPersonRepository : IPersonRepository
{
    private static readonly ConcurrentDictionary<Guid, Person> _people = new ();

    public Task AddAsync(Person person)
    {
        _people[person.Id] = person;
        return Task.CompletedTask;
    }
}