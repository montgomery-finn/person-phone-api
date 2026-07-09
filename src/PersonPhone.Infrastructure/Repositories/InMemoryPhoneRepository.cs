using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Interfaces;

namespace PersonPhone.Infrastructure.Repositories;

public class InMemoryPhoneRepository : InMemoryRepository<Phone>, IPhoneRepository
{
    public Task<IEnumerable<Phone>> GetAllAsync(Guid? personId = null) =>
        GetActiveAsync(personId.HasValue ? p => p.PersonId == personId.Value : null);
}
