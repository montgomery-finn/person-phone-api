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

    public Task<IEnumerable<Person>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Person>>(_people.Values.Where(p => p.IsActive));
    }

    public Task<Person?> GetByIdAsync(Guid id)
    {
        _people.TryGetValue(id, out var person);
        return Task.FromResult(person);
    }

    public Task UpdateAsync(Person person)
    {
        _people[person.Id] = person;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByCpfAsync(string cpf, Guid? excludingId = null)
    {
        var exists = _people.Values.Any(p => p.Id != excludingId && p.Cpf.Value == cpf);
        return Task.FromResult(exists);
    }
}