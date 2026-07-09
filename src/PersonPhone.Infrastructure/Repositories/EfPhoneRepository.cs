using Microsoft.EntityFrameworkCore;
using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Interfaces;
using PersonPhone.Domain.ValueObjects;
using PersonPhone.Infrastructure.Persistence;

namespace PersonPhone.Infrastructure.Repositories;

public class EfPhoneRepository : IPhoneRepository
{
    private readonly PersonPhoneDbContext _context;

    public EfPhoneRepository(PersonPhoneDbContext context) => _context = context;

    public async Task AddAsync(Phone entity)
    {
        await _context.Phones.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public Task<Phone?> GetByIdAsync(Guid id) =>
        _context.Phones.FirstOrDefaultAsync(p => p.Id == id);

    public async Task UpdateAsync(Phone entity)
    {
        _context.Phones.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Phone>> GetAllAsync(Guid? personId = null)
    {
        var query = _context.Phones.Where(p => p.IsActive);

        if (personId.HasValue)
            query = query.Where(p => p.PersonId == personId.Value);

        return await query.ToListAsync();
    }

    public Task<bool> ExistsByNumberAsync(Guid personId, string number, Guid? excludingId = null)
    {
        var value = new PhoneNumber(number);
        return _context.Phones.AnyAsync(p =>
            p.Id != excludingId && p.PersonId == personId && p.Number == value);
    }
}
