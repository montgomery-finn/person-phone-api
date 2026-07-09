using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonPhone.Domain.Entities;
using PersonPhone.Domain.ValueObjects;

namespace PersonPhone.Infrastructure.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Cpf)
            .HasConversion(cpf => cpf.Value, value => new Cpf(value))
            .HasColumnName("Cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(p => p.Cpf).IsUnique();

        builder.Property(p => p.BirthDate).IsRequired();

        builder.Property(p => p.IsActive).IsRequired();
    }
}
