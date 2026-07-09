using PersonPhone.Application.DTOs.Phone;
using PersonPhone.Domain.Interfaces;

namespace PersonPhone.Application.Services.Phone;

public class PhoneService : IPhoneService
{
    private readonly IPhoneRepository _phoneRepository;
    private readonly IPersonRepository _personRepository;

    public PhoneService(IPhoneRepository phoneRepository, IPersonRepository personRepository)
    {
        _phoneRepository = phoneRepository;
        _personRepository = personRepository;
    }

    public async Task<PhoneResponse> CreateAsync(CreatePhoneRequest request)
    {
        var (personId, type, number) = request;

        var person = await _personRepository.GetByIdAsync(personId);

        if (person is null || !person.IsActive)
            throw new ArgumentException("Person not found or inactive.", nameof(request));

        var phone = new Domain.Entities.Phone(personId, type, number);

        if (await _phoneRepository.ExistsByNumberAsync(personId, phone.Number.Value))
            throw new ArgumentException("Phone number already registered for this person.", nameof(request));

        await _phoneRepository.AddAsync(phone);

        return ToResponse(phone);
    }

    public async Task<IEnumerable<PhoneResponse>> GetAllAsync(Guid? personId = null)
    {
        var phones = await _phoneRepository.GetAllAsync(personId);

        return phones.Select(ToResponse);
    }

    public async Task<PhoneResponse?> GetByIdAsync(Guid id)
    {
        var phone = await _phoneRepository.GetByIdAsync(id);

        return phone is null ? null : ToResponse(phone);
    }

    public async Task<PhoneResponse?> UpdateAsync(Guid id, UpdatePhoneRequest request)
    {
        var phone = await _phoneRepository.GetByIdAsync(id);

        if (phone is null)
            return null;

        if (!phone.IsActive)
            throw new ArgumentException("Phone already deleted.", nameof(id));

        var (type, number) = request;

        if (await _phoneRepository.ExistsByNumberAsync(
                phone.PersonId, new Domain.ValueObjects.PhoneNumber(number).Value, excludingId: id))
            throw new ArgumentException("Phone number already registered for this person.", nameof(request));

        phone.Update(type, number);

        await _phoneRepository.UpdateAsync(phone);

        return ToResponse(phone);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var phone = await _phoneRepository.GetByIdAsync(id);

        if (phone is null)
            return false;

        if (!phone.IsActive)
            throw new ArgumentException("Phone already deleted.", nameof(id));

        phone.Deactivate();
        await _phoneRepository.UpdateAsync(phone);

        return true;
    }

    private static PhoneResponse ToResponse(Domain.Entities.Phone phone)
    {
        return new PhoneResponse(phone.Id, phone.PersonId, phone.Type, phone.Number.Value, phone.IsActive);
    }
}
