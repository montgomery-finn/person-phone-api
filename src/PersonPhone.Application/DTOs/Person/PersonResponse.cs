namespace PersonPhone.Application.DTOs.Person;

public record PersonResponse(Guid Id, string Name, string Cpf, DateTime BirthDate, bool IsActive);
