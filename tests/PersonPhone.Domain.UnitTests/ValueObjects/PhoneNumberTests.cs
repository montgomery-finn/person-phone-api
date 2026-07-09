using PersonPhone.Domain.ValueObjects;

namespace PersonPhone.Domain.UnitTests.ValueObjects;

public class PhoneNumberTests
{
    [Fact]
    public void Constructor_WithValidUnmaskedMobileNumber_StoresDigitsOnly()
    {
        var phoneNumber = new PhoneNumber("11987654321");

        Assert.Equal("11987654321", phoneNumber.Value);
    }

    [Fact]
    public void Constructor_WithValidUnmaskedResidentialNumber_StoresDigitsOnly()
    {
        var phoneNumber = new PhoneNumber("1122334455");

        Assert.Equal("1122334455", phoneNumber.Value);
    }

    [Fact]
    public void Constructor_WithValidMaskedNumber_NormalizesToDigitsOnly()
    {
        var phoneNumber = new PhoneNumber("(11) 98765-4321");

        Assert.Equal("11987654321", phoneNumber.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullEmptyOrWhitespace_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => new PhoneNumber(value!));
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    public void Constructor_WithWrongDigitCount_ThrowsArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new PhoneNumber(value));
    }

    [Fact]
    public void Equals_WithMaskedAndUnmaskedSameValue_AreEqual()
    {
        var unmasked = new PhoneNumber("11987654321");
        var masked = new PhoneNumber("(11) 98765-4321");

        Assert.Equal(unmasked, masked);
        Assert.True(unmasked == masked);
    }

    [Fact]
    public void ToString_ReturnsUnmaskedDigits()
    {
        var phoneNumber = new PhoneNumber("(11) 98765-4321");

        Assert.Equal("11987654321", phoneNumber.ToString());
    }

    [Theory]
    [InlineData("11987654321")]
    [InlineData("1122334455")]
    [InlineData("(11) 98765-4321")]
    [InlineData("(11) 2233-4455")]
    public void IsValid_WithValidNumber_ReturnsTrue(string value)
    {
        Assert.True(PhoneNumber.IsValid(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    public void IsValid_WithInvalidNumber_ReturnsFalse(string? value)
    {
        Assert.False(PhoneNumber.IsValid(value));
    }
}
