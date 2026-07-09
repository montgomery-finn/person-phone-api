using PersonPhone.Application.DTOs.Person;
using PersonPhone.Application.Services.Person;
using PersonPhone.Infrastructure.Repositories;

namespace PersonPhone.Application.UnitTests.Services.Person;

public class PersonServiceTests
{
    private static PersonService CreateService(out InMemoryPersonRepository repository)
    {
        repository = new InMemoryPersonRepository();
        return new PersonService(repository);
    }

    [Fact]
    public async Task CreateAsync_WithCpfOfActivePerson_ThrowsArgumentException()
    {
        var service = CreateService(out _);
        var cpf = "22334455628";
        await service.CreateAsync(new CreatePersonRequest("John Doe", cpf, new DateTime(1990, 1, 1)));

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(new CreatePersonRequest("Jane Doe", cpf, new DateTime(1991, 2, 2))));

        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_WithCpfOfDeactivatedPerson_ThrowsArgumentException()
    {
        var service = CreateService(out _);
        var cpf = "98765432100";
        var created = await service.CreateAsync(new CreatePersonRequest("John Doe", cpf, new DateTime(1990, 1, 1)));
        await service.DeleteAsync(created.Id);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(new CreatePersonRequest("Jane Doe", cpf, new DateTime(1991, 2, 2))));
    }

    [Fact]
    public async Task CreateAsync_WithNewCpf_CreatesPersonSuccessfully()
    {
        var service = CreateService(out _);
        var cpf = "12345678909";

        var response = await service.CreateAsync(new CreatePersonRequest("John Doe", cpf, new DateTime(1990, 1, 1)));

        Assert.Equal(cpf, response.Cpf);
        Assert.True(response.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_KeepingOwnCpf_UpdatesSuccessfully()
    {
        var service = CreateService(out _);
        var cpf = "77788899941";
        var created = await service.CreateAsync(new CreatePersonRequest("John Doe", cpf, new DateTime(1990, 1, 1)));

        var updated = await service.UpdateAsync(
            created.Id, new UpdatePersonRequest("John Updated", cpf, new DateTime(1991, 3, 3)));

        Assert.NotNull(updated);
        Assert.Equal("John Updated", updated!.Name);
        Assert.Equal(cpf, updated.Cpf);
        Assert.Equal(new DateTime(1991, 3, 3), updated.BirthDate);
    }

    [Fact]
    public async Task UpdateAsync_WithCpfOfAnotherPerson_ThrowsArgumentException()
    {
        var service = CreateService(out _);
        var cpfA = "13579246828";
        var cpfB = "24681357928";
        await service.CreateAsync(new CreatePersonRequest("Person A", cpfA, new DateTime(1990, 1, 1)));
        var personB = await service.CreateAsync(new CreatePersonRequest("Person B", cpfB, new DateTime(1992, 2, 2)));

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(
                personB.Id, new UpdatePersonRequest("Person B Updated", cpfA, new DateTime(1993, 3, 3))));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateCpf_DoesNotMutateOriginalPersonState()
    {
        var service = CreateService(out var repository);
        var cpfA = "10293847541";
        var cpfB = "44455566619";
        await service.CreateAsync(new CreatePersonRequest("Person A", cpfA, new DateTime(1990, 1, 1)));
        var personB = await service.CreateAsync(new CreatePersonRequest("Person B", cpfB, new DateTime(1992, 2, 2)));

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(
                personB.Id, new UpdatePersonRequest("Hacked Name", cpfA, new DateTime(2000, 1, 1))));

        var persisted = await repository.GetByIdAsync(personB.Id);

        Assert.NotNull(persisted);
        Assert.Equal("Person B", persisted!.Name);
        Assert.Equal(cpfB, persisted.Cpf.Value);
        Assert.Equal(new DateTime(1992, 2, 2), persisted.BirthDate);
    }

    [Fact]
    public async Task UpdateAsync_OnDeletedPerson_ThrowsArgumentException()
    {
        var service = CreateService(out _);
        var created = await service.CreateAsync(
            new CreatePersonRequest("John Doe", "60729845257", new DateTime(1990, 1, 1)));
        await service.DeleteAsync(created.Id);

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(
                created.Id, new UpdatePersonRequest("John Updated", "60729845257", new DateTime(1991, 3, 3))));

        Assert.Equal("id", ex.ParamName);
    }

    [Fact]
    public async Task DeleteAsync_OnAlreadyDeletedPerson_ThrowsArgumentException()
    {
        var service = CreateService(out _);
        var created = await service.CreateAsync(
            new CreatePersonRequest("John Doe", "39053344705", new DateTime(1990, 1, 1)));
        await service.DeleteAsync(created.Id);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteAsync(created.Id));

        Assert.Equal("id", ex.ParamName);
    }
}
