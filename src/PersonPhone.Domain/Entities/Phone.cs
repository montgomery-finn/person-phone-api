using PersonPhone.Domain.Enums;
using PersonPhone.Domain.ValueObjects;

namespace PersonPhone.Domain.Entities;

public class Phone : IEntity
{
    public Guid Id { get; private set; }
    public Guid PersonId { get; private set; }
    public PhoneType Type { get; private set; }
    public PhoneNumber Number { get; private set; }
    public bool IsActive { get; private set; }

    public Phone()
    {

    }

    public Phone(Guid personId, PhoneType type, string number)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("PersonId is required.", nameof(personId));

        if (!Enum.IsDefined(type))
            throw new ArgumentException("Type is invalid.", nameof(type));

        Id = Guid.NewGuid();
        PersonId = personId;
        Type = type;
        Number = new PhoneNumber(number);
        IsActive = true;
    }

    public void Update(PhoneType type, string number)
    {
        if (!Enum.IsDefined(type))
            throw new ArgumentException("Type is invalid.", nameof(type));

        Type = type;
        Number = new PhoneNumber(number);
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
