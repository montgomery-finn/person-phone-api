using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonPhone.Domain.Entities;
using PersonPhone.Domain.ValueObjects;

namespace PersonPhone.Infrastructure.Persistence.Configurations;

public class PhoneConfiguration : IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> builder)
    {
        builder.ToTable("Phones");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PersonId).IsRequired();

        // Person/Phone não têm navigation properties entre si (por design do domínio),
        // então a FK é declarada sem navegação. Restrict porque a app só faz soft-delete
        // (Deactivate()), nunca DELETE físico de Person.
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(p => p.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Salvo como string: mais legível no banco e resiliente a reordenação do enum.
        builder.Property(p => p.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Number)
            .HasConversion(number => number.Value, value => new PhoneNumber(value))
            .HasColumnName("Number")
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(p => new { p.PersonId, p.Number }).IsUnique();

        builder.Property(p => p.IsActive).IsRequired();
    }
}
