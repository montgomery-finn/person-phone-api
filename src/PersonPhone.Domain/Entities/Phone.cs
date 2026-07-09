using PersonPhone.Domain.Enums;

namespace PersonPhone.Domain.Entities;

public class Phone : IEntity
{
    public Guid Id { get; private set; }
    public Guid PersonId { get; private set; }
    public PhoneType Type { get; private set; }
    public string Number { get; private set; }
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

        ValidateNumber(number);

        Id = Guid.NewGuid();
        PersonId = personId;
        Type = type;
        Number = number;
        IsActive = true;
    }

    public void Update(PhoneType type, string number)
    {
        if (!Enum.IsDefined(type))
            throw new ArgumentException("Type is invalid.", nameof(type));

        ValidateNumber(number);

        Type = type;
        Number = number;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static void ValidateNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number) || number.Length < 10 || number.Length > 11)
            throw new ArgumentException("Number must be between 10 and 11 characters.", nameof(number));
    }
}
