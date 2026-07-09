using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Interfaces;

namespace PersonPhone.Infrastructure.Repositories;

public class InMemoryPhoneRepository : InMemoryRepository<Phone>, IPhoneRepository
{
    public Task<IEnumerable<Phone>> GetAllAsync(Guid? personId = null) =>
        GetActiveAsync(personId.HasValue ? p => p.PersonId == personId.Value : null);

    public Task<bool> ExistsByNumberAsync(Guid personId, string number, Guid? excludingId = null)
    {
        var exists = Entities.Values.Any(p => p.Id != excludingId && p.PersonId == personId && p.Number.Value == number);
        return Task.FromResult(exists);
    }
}
