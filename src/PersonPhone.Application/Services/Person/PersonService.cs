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

        var person = new Domain.Entities.Person(name, cpf, birthDate!.Value);

        if (await _personRepository.ExistsByCpfAsync(person.Cpf.Value))
            throw new ArgumentException("Cpf already registered.", nameof(request));

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

    public async Task<PersonResponse?> UpdateAsync(Guid id, UpdatePersonRequest request)
    {
        var person = await _personRepository.GetByIdAsync(id);

        if (person is null)
            return null;

        if (!person.IsActive)
            throw new ArgumentException("Person already deleted.", nameof(id));

        var (name, cpf, birthDate) = request;

        if (await _personRepository.ExistsByCpfAsync(new Domain.ValueObjects.Cpf(cpf).Value, excludingId: id))
            throw new ArgumentException("Cpf already registered.", nameof(request));

        person.Update(name, cpf, birthDate!.Value);

        await _personRepository.UpdateAsync(person);

        return ToResponse(person);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var person = await _personRepository.GetByIdAsync(id);

        if (person is null)
            return false;

        if (!person.IsActive)
            throw new ArgumentException("Person already deleted.", nameof(id));

        person.Deactivate();
        await _personRepository.UpdateAsync(person);

        return true;
    }

    private static PersonResponse ToResponse(Domain.Entities.Person person)
    {
        return new PersonResponse(person.Id, person.Name, person.Cpf.Value, person.BirthDate, person.IsActive);
    }
}
