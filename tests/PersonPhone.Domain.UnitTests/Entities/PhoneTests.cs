using PersonPhone.Domain.Entities;
using PersonPhone.Domain.Enums;

namespace PersonPhone.Domain.UnitTests.Entities;

public class PhoneTests
{
    [Fact]
    public void Constructor_WithValidData_SetsPropertiesAndActivates()
    {
        var personId = Guid.NewGuid();
        var type = PhoneType.Mobile;
        var number = "11987654321";

        var phone = new Phone(personId, type, number);

        Assert.NotEqual(Guid.Empty, phone.Id);
        Assert.Equal(personId, phone.PersonId);
        Assert.Equal(type, phone.Type);
        Assert.Equal(number, phone.Number.Value);
        Assert.True(phone.IsActive);
    }

    [Fact]
    public void Constructor_WithEmptyPersonId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => new Phone(Guid.Empty, PhoneType.Mobile, "11987654321"));

        Assert.Equal("personId", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidType_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => new Phone(Guid.NewGuid(), (PhoneType)99, "11987654321"));

        Assert.Equal("type", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    public void Constructor_WithInvalidNumber_ThrowsArgumentException(string? number)
    {
        var ex = Assert.Throws<ArgumentException>(
            () => new Phone(Guid.NewGuid(), PhoneType.Mobile, number!));

        Assert.Equal("number", ex.ParamName);
    }

    [Fact]
    public void Update_WithValidData_UpdatesTypeAndNumber()
    {
        var phone = new Phone(Guid.NewGuid(), PhoneType.Mobile, "11987654321");
        var id = phone.Id;
        var personId = phone.PersonId;

        var newType = PhoneType.Residential;
        var newNumber = "1122334455";

        phone.Update(newType, newNumber);

        Assert.Equal(id, phone.Id);
        Assert.Equal(personId, phone.PersonId);
        Assert.Equal(newType, phone.Type);
        Assert.Equal(newNumber, phone.Number.Value);
        Assert.True(phone.IsActive);
    }

    [Fact]
    public void Update_WithInvalidType_ThrowsArgumentException()
    {
        var phone = new Phone(Guid.NewGuid(), PhoneType.Mobile, "11987654321");

        var ex = Assert.Throws<ArgumentException>(
            () => phone.Update((PhoneType)99, "1122334455"));

        Assert.Equal("type", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    public void Update_WithInvalidNumber_ThrowsArgumentException(string? number)
    {
        var phone = new Phone(Guid.NewGuid(), PhoneType.Mobile, "11987654321");

        var ex = Assert.Throws<ArgumentException>(
            () => phone.Update(PhoneType.Residential, number!));

        Assert.Equal("number", ex.ParamName);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var phone = new Phone(Guid.NewGuid(), PhoneType.Mobile, "11987654321");

        phone.Deactivate();

        Assert.False(phone.IsActive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_IsIdempotentAndDoesNotThrow()
    {
        var phone = new Phone(Guid.NewGuid(), PhoneType.Mobile, "11987654321");

        phone.Deactivate();
        phone.Deactivate();

        Assert.False(phone.IsActive);
    }
}
