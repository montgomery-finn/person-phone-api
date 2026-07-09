using PersonPhone.Domain.Entities;

namespace PersonPhone.Domain.Interfaces;

public interface IPersonRepository : IRepository<Person>
{
    Task<IEnumerable<Person>> GetAllAsync();
    Task<bool> ExistsByCpfAsync(string cpf, Guid? excludingId = null);
}