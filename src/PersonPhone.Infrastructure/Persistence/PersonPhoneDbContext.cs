using Microsoft.EntityFrameworkCore;
using PersonPhone.Domain.Entities;

namespace PersonPhone.Infrastructure.Persistence;

public class PersonPhoneDbContext : DbContext
{
    public PersonPhoneDbContext(DbContextOptions<PersonPhoneDbContext> options) : base(options)
    {
    }

    public DbSet<Person> People => Set<Person>();
    public DbSet<Phone> Phones => Set<Phone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersonPhoneDbContext).Assembly);
    }
}
