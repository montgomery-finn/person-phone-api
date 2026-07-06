using PersonPhone.Domain.Entities;

namespace PersonPhone.Domain.UnitTests.Entities;

public class PersonTests
{
    [Fact]
    public void Constructor_WithValidData_SetsPropertiesAndActivates()
    {
        var name = "John Doe";
        var cpf = "11144477735";
        var birthDate = new DateTime(1990, 1, 1);

        var person = new Person(name, cpf, birthDate);

        Assert.NotEqual(Guid.Empty, person.Id);
        Assert.Equal(name, person.Name);
        Assert.Equal(cpf, person.Cpf.Value);
        Assert.Equal(birthDate, person.BirthDate);
        Assert.True(person.IsActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => new Person(name!, "11144477735", DateTime.Now));

        Assert.Equal("name", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCpf_ThrowsArgumentException(string? cpf)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => new Person("John Doe", cpf!, DateTime.Now));

        Assert.Equal("cpf", ex.ParamName);
    }

    [Fact]
    public void Update_WithValidData_UpdatesNameCpfAndBirthDate()
    {
        var person = new Person("John Doe", "11144477735", new DateTime(1990, 1, 1));
        var id = person.Id;

        var newName = "Jane Doe";
        var newCpf = "52998224725";
        var newBirthDate = new DateTime(1985, 5, 20);

        person.Update(newName, newCpf, newBirthDate);

        Assert.Equal(id, person.Id);
        Assert.Equal(newName, person.Name);
        Assert.Equal(newCpf, person.Cpf.Value);
        Assert.Equal(newBirthDate, person.BirthDate);
        Assert.True(person.IsActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var person = new Person("John Doe", "11144477735", DateTime.Now);

        var ex = Assert.Throws<ArgumentException>(
            () => person.Update(name!, "52998224725", DateTime.Now));

        Assert.Equal("name", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidCpf_ThrowsArgumentException(string? cpf)
    {
        var person = new Person("John Doe", "11144477735", DateTime.Now);

        var ex = Assert.Throws<ArgumentException>(
            () => person.Update("Jane Doe", cpf!, DateTime.Now));

        Assert.Equal("cpf", ex.ParamName);
    }

    [Fact]
    public void Update_OnInactivePerson_DoesNotReactivate()
    {
        var person = new Person("John Doe", "11144477735", DateTime.Now);
        person.Deactivate();

        person.Update("Jane Doe", "52998224725", DateTime.Now);

        Assert.False(person.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var person = new Person("John Doe", "11144477735", DateTime.Now);

        person.Deactivate();

        Assert.False(person.IsActive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_IsIdempotentAndDoesNotThrow()
    {
        var person = new Person("John Doe", "11144477735", DateTime.Now);

        person.Deactivate();
        person.Deactivate();

        Assert.False(person.IsActive);
    }
}
