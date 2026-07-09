using PersonPhone.Application.DTOs.Phone;

namespace PersonPhone.Application.Services.Phone;

public interface IPhoneService
{
    Task<PhoneResponse> CreateAsync(CreatePhoneRequest request);
    Task<IEnumerable<PhoneResponse>> GetAllAsync(Guid? personId = null);
    Task<PhoneResponse?> GetByIdAsync(Guid id);
    Task<PhoneResponse?> UpdateAsync(Guid id, UpdatePhoneRequest request);
    Task<bool> DeleteAsync(Guid id);
}
