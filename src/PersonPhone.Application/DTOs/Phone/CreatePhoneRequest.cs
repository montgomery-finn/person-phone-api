using PersonPhone.Domain.Enums;

namespace PersonPhone.Application.DTOs.Phone;

public record CreatePhoneRequest(Guid PersonId, PhoneType Type, string Number);
