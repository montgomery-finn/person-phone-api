using PersonPhone.Application.DTOs.Person;

namespace PersonPhone.Application.Services.Person;

public interface IPersonService
{
    Task<PersonResponse> CreateAsync(CreatePersonRequest request);
    Task<IEnumerable<PersonResponse>> GetAllAsync();
    Task<PersonResponse?> GetByIdAsync(Guid id);
}
