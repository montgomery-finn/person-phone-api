namespace PersonPhone.Domain.Entities;

public interface IEntity
{
    Guid Id { get; }
    bool IsActive { get; }
}
