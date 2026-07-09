using PersonPhone.Domain.Enums;

namespace PersonPhone.Application.DTOs.Phone;

public record UpdatePhoneRequest(PhoneType Type, string Number);
