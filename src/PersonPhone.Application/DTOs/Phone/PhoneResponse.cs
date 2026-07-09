using PersonPhone.Domain.Enums;

namespace PersonPhone.Application.DTOs.Phone;

public record PhoneResponse(Guid Id, Guid PersonId, PhoneType Type, string Number, bool IsActive);
