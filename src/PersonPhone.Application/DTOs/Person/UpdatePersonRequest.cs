namespace PersonPhone.Application.DTOs.Person;

public record UpdatePersonRequest(string Name, string Cpf, DateTime BirthDate);
