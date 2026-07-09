using PersonPhone.Domain.Entities;

namespace PersonPhone.Domain.Interfaces;

public interface IPhoneRepository : IRepository<Phone>
{
    Task<IEnumerable<Phone>> GetAllAsync(Guid? personId = null);
    Task<bool> ExistsByNumberAsync(Guid personId, string number, Guid? excludingId = null);
}
