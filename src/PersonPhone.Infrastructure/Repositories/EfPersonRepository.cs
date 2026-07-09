using Microsoft.EntityFrameworkCore;
using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Interfaces;
using PersonPhone.Domain.ValueObjects;
using PersonPhone.Infrastructure.Persistence;

namespace PersonPhone.Infrastructure.Repositories;

public class EfPersonRepository : IPersonRepository
{
    private readonly PersonPhoneDbContext _context;

    public EfPersonRepository(PersonPhoneDbContext context) => _context = context;

    public async Task AddAsync(Person entity)
    {
        await _context.People.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public Task<Person?> GetByIdAsync(Guid id) =>
        _context.People.FirstOrDefaultAsync(p => p.Id == id);

    public async Task UpdateAsync(Person entity)
    {
        _context.People.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Person>> GetAllAsync() =>
        await _context.People.Where(p => p.IsActive).ToListAsync();

    public Task<bool> ExistsByCpfAsync(string cpf, Guid? excludingId = null)
    {
        var value = new Cpf(cpf);
        return _context.People.AnyAsync(p => p.Id != excludingId && p.Cpf == value);
    }
}
