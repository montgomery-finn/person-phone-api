using PersonPhone.Application.DTOs.Person;
using PersonPhone.Domain.Interfaces;

namespace PersonPhone.Application.Services.Person;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;

    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<PersonResponse> CreateAsync(CreatePersonRequest request)
    {
        var (name, cpf, birthDate) = request;

        var person = new Domain.Entities.Person(name, cpf, birthDate);

        await _personRepository.AddAsync(person);

        return ToResponse(person);
    }

    public async Task<IEnumerable<PersonResponse>> GetAllAsync()
    {
        var people = await _personRepository.GetAllAsync();

        return people.Select(ToResponse);
    }

    public async Task<PersonResponse?> GetByIdAsync(Guid id)
    {
        var person = await _personRepository.GetByIdAsync(id);

        return person is null ? null : ToResponse(person);
    }

    private static PersonResponse ToResponse(Domain.Entities.Person person)
    {
        return new PersonResponse(person.Id, person.Name, person.Cpf.Value, person.BirthDate, person.IsActive);
    }
}
