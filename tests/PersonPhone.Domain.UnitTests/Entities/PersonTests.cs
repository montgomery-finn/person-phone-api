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
}
