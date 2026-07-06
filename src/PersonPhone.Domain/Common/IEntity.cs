namespace PersonPhone.Domain.Common;

public interface IEntity
{
    Guid Id { get; }
    bool IsActive { get; }
}
