namespace PersonPhone.Application.DTOs.Person;

public record CreatePersonRequest(string Name, string Cpf, DateTime BirthDate);
