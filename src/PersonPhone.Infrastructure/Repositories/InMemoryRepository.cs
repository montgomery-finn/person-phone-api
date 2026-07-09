using System.Collections.Concurrent;
using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Interfaces;

namespace PersonPhone.Infrastructure.Repositories;

public abstract class InMemoryRepository<T> : IRepository<T> where T : class, IEntity
{
    protected static readonly ConcurrentDictionary<Guid, T> Entities = new();

    public virtual Task AddAsync(T entity)
    {
        Entities[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public virtual Task<T?> GetByIdAsync(Guid id)
    {
        Entities.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public virtual Task UpdateAsync(T entity)
    {
        Entities[entity.Id] = entity;
        return Task.CompletedTask;
    }

    protected Task<IEnumerable<T>> GetActiveAsync(Func<T, bool>? predicate = null)
    {
        IEnumerable<T> query = Entities.Values.Where(e => e.IsActive);

        if (predicate is not null)
            query = query.Where(predicate);

        return Task.FromResult(query);
    }
}
