using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Interfaces;

namespace PersonPhone.Infrastructure.Repositories;

public class InMemoryPersonRepository : InMemoryRepository<Person>, IPersonRepository
{
    public Task<IEnumerable<Person>> GetAllAsync() => GetActiveAsync();

    public Task<bool> ExistsByCpfAsync(string cpf, Guid? excludingId = null)
    {
        var exists = Entities.Values.Any(p => p.Id != excludingId && p.Cpf.Value == cpf);
        return Task.FromResult(exists);
    }
}