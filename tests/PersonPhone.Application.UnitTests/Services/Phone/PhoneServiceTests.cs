using PersonPhone.Application.DTOs.Person;
using PersonPhone.Application.DTOs.Phone;
using PersonPhone.Application.Services.Person;
using PersonPhone.Application.Services.Phone;
using PersonPhone.Domain.Enums;
using PersonPhone.Infrastructure.Repositories;

namespace PersonPhone.Application.UnitTests.Services.Phone;

public class PhoneServiceTests
{
    private static PhoneService CreateService(
        out InMemoryPersonRepository personRepository, out InMemoryPhoneRepository phoneRepository)
    {
        personRepository = new InMemoryPersonRepository();
        phoneRepository = new InMemoryPhoneRepository();
        return new PhoneService(phoneRepository, personRepository);
    }

    private static async Task<Guid> CreatePersonAsync(InMemoryPersonRepository personRepository, string cpf)
    {
        var personService = new PersonService(personRepository);
        var person = await personService.CreateAsync(new CreatePersonRequest("John Doe", cpf, new DateTime(1990, 1, 1)));
        return person.Id;
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentPersonId_ThrowsArgumentException()
    {
        var service = CreateService(out _, out _);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(new CreatePhoneRequest(Guid.NewGuid(), PhoneType.Mobile, "11987654321")));

        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_WithDeactivatedPersonId_ThrowsArgumentException()
    {
        var service = CreateService(out var personRepository, out _);
        var personService = new PersonService(personRepository);
        var person = await personService.CreateAsync(
            new CreatePersonRequest("John Doe", "11144477735", new DateTime(1990, 1, 1)));
        await personService.DeleteAsync(person.Id);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(new CreatePhoneRequest(person.Id, PhoneType.Mobile, "11987654321")));
    }

    [Fact]
    public async Task CreateAsync_WithActivePersonId_CreatesPhoneSuccessfully()
    {
        var service = CreateService(out var personRepository, out _);
        var personId = await CreatePersonAsync(personRepository, "24977448316");

        var response = await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321"));

        Assert.Equal(personId, response.PersonId);
        Assert.Equal(PhoneType.Mobile, response.Type);
        Assert.Equal("11987654321", response.Number);
        Assert.True(response.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesTypeAndNumber()
    {
        var service = CreateService(out var personRepository, out _);
        var personId = await CreatePersonAsync(personRepository, "93387302711");
        var created = await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321"));

        var updated = await service.UpdateAsync(
            created.Id, new UpdatePhoneRequest(PhoneType.Residential, "1122334455"));

        Assert.NotNull(updated);
        Assert.Equal(personId, updated!.PersonId);
        Assert.Equal(PhoneType.Residential, updated.Type);
        Assert.Equal("1122334455", updated.Number);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsNull()
    {
        var service = CreateService(out _, out _);

        var result = await service.UpdateAsync(
            Guid.NewGuid(), new UpdatePhoneRequest(PhoneType.Residential, "1122334455"));

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_DeactivatesPhone()
    {
        var service = CreateService(out var personRepository, out _);
        var personId = await CreatePersonAsync(personRepository, "12470460875");
        var created = await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321"));

        var deleted = await service.DeleteAsync(created.Id);
        var afterDelete = await service.GetByIdAsync(created.Id);

        Assert.True(deleted);
        Assert.NotNull(afterDelete);
        Assert.False(afterDelete!.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ReturnsFalse()
    {
        var service = CreateService(out _, out _);

        var deleted = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
    }

    [Fact]
    public async Task UpdateAsync_OnDeletedPhone_ThrowsArgumentException()
    {
        var service = CreateService(out var personRepository, out _);
        var personId = await CreatePersonAsync(personRepository, "77593034674");
        var created = await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321"));
        await service.DeleteAsync(created.Id);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(created.Id, new UpdatePhoneRequest(PhoneType.Residential, "1122334455")));

        Assert.Equal("id", ex.ParamName);
    }

    [Fact]
    public async Task DeleteAsync_OnAlreadyDeletedPhone_ThrowsArgumentException()
    {
        var service = CreateService(out var personRepository, out _);
        var personId = await CreatePersonAsync(personRepository, "68445258699");
        var created = await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321"));
        await service.DeleteAsync(created.Id);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteAsync(created.Id));

        Assert.Equal("id", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateNumberForSamePerson_ThrowsArgumentException()
    {
        var service = CreateService(out var personRepository, out _);
        var personId = await CreatePersonAsync(personRepository, "10433218100");
        await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321"));

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Commercial, "(11) 98765-4321")));

        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_WithSameNumberForDifferentPerson_CreatesSuccessfully()
    {
        var service = CreateService(out var personRepository, out _);
        var personAId = await CreatePersonAsync(personRepository, "96001338914");
        var personBId = await CreatePersonAsync(personRepository, "08386379499");
        await service.CreateAsync(new CreatePhoneRequest(personAId, PhoneType.Mobile, "11987654321"));

        var response = await service.CreateAsync(new CreatePhoneRequest(personBId, PhoneType.Mobile, "11987654321"));

        Assert.Equal(personBId, response.PersonId);
        Assert.Equal("11987654321", response.Number);
    }

    [Fact]
    public async Task CreateAsync_WithNumberOfDeletedPhone_ThrowsArgumentException()
    {
        var service = CreateService(out var personRepository, out _);
        var personId = await CreatePersonAsync(personRepository, "02654235114");
        var created = await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321"));
        await service.DeleteAsync(created.Id);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321")));

        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public async Task UpdateAsync_WithNumberOfAnotherPhoneForSamePerson_ThrowsArgumentException()
    {
        var service = CreateService(out var personRepository, out _);
        var personId = await CreatePersonAsync(personRepository, "16155940789");
        var phoneA = await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321"));
        var phoneB = await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Commercial, "1122334455"));

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(phoneA.Id, new UpdatePhoneRequest(phoneA.Type, phoneB.Number)));

        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public async Task UpdateAsync_KeepingOwnNumber_UpdatesSuccessfully()
    {
        var service = CreateService(out var personRepository, out _);
        var personId = await CreatePersonAsync(personRepository, "81618495950");
        var created = await service.CreateAsync(new CreatePhoneRequest(personId, PhoneType.Mobile, "11987654321"));

        var updated = await service.UpdateAsync(
            created.Id, new UpdatePhoneRequest(PhoneType.Commercial, created.Number));

        Assert.NotNull(updated);
        Assert.Equal(PhoneType.Commercial, updated!.Type);
        Assert.Equal(created.Number, updated.Number);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByPersonIdAndExcludesInactive()
    {
        var service = CreateService(out var personRepository, out _);
        var personAId = await CreatePersonAsync(personRepository, "47098725110");
        var personBId = await CreatePersonAsync(personRepository, "50921727542");

        var phoneA1 = await service.CreateAsync(new CreatePhoneRequest(personAId, PhoneType.Mobile, "11987654321"));
        var phoneA2 = await service.CreateAsync(new CreatePhoneRequest(personAId, PhoneType.Commercial, "1122334455"));
        await service.CreateAsync(new CreatePhoneRequest(personBId, PhoneType.Residential, "1133445566"));

        await service.DeleteAsync(phoneA2.Id);

        var personAPhones = await service.GetAllAsync(personAId);

        Assert.Single(personAPhones);
        Assert.Equal(phoneA1.Id, personAPhones.Single().Id);
    }
}
