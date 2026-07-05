using PersonPhone.Domain.Entities;

namespace PersonPhone.Domain.Interfaces;

public interface IPersonRepository
{
    Task AddAsync(Person person);
}