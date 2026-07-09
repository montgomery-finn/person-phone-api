using PersonPhone.Domain.Entities;

namespace PersonPhone.Domain.Interfaces;

public interface IRepository<T> where T : IEntity
{
    Task AddAsync(T entity);
    Task<T?> GetByIdAsync(Guid id);
    Task UpdateAsync(T entity);
}
