using PersonPhone.Domain.ValueObjects;

namespace PersonPhone.Domain.Entities;

public class Person : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Cpf Cpf { get; private set; }
    public DateTime BirthDate { get; private set; }
    public bool IsActive { get; private set; }

    public Person()
    {

    }

    public Person(string name, string cpf, DateTime birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        Cpf = new Cpf(cpf);
        BirthDate = birthDate;
        IsActive = true;
    }

    public void Update(string name, string cpf, DateTime birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        Name = name;
        Cpf = new Cpf(cpf);
        BirthDate = birthDate;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
