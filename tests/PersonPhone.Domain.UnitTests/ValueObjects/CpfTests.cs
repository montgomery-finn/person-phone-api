using PersonPhone.Domain.ValueObjects;

namespace PersonPhone.Domain.UnitTests.ValueObjects;

public class CpfTests
{
    [Fact]
    public void Constructor_WithValidUnmaskedCpf_StoresDigitsOnly()
    {
        var cpf = new Cpf("11144477735");

        Assert.Equal("11144477735", cpf.Value);
    }

    [Fact]
    public void Constructor_WithValidMaskedCpf_NormalizesToDigitsOnly()
    {
        var cpf = new Cpf("111.444.777-35");

        Assert.Equal("11144477735", cpf.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithNullEmptyOrWhitespace_ThrowsArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => new Cpf(value!));
    }

    [Theory]
    [InlineData("111444777")]
    [InlineData("111444777350")]
    public void Constructor_WithWrongDigitCount_ThrowsArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new Cpf(value));
    }

    [Fact]
    public void Constructor_WithInvalidCheckDigit_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Cpf("11144477736"));
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    public void Constructor_WithRepeatedDigitsSequence_ThrowsArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new Cpf(value));
    }

    [Fact]
    public void Equals_WithMaskedAndUnmaskedSameValue_AreEqual()
    {
        var unmasked = new Cpf("11144477735");
        var masked = new Cpf("111.444.777-35");

        Assert.Equal(unmasked, masked);
        Assert.True(unmasked == masked);
    }

    [Fact]
    public void ToString_ReturnsUnmaskedDigits()
    {
        var cpf = new Cpf("111.444.777-35");

        Assert.Equal("11144477735", cpf.ToString());
    }

    [Theory]
    [InlineData("11144477735")]
    [InlineData("111.444.777-35")]
    public void IsValid_WithValidCpf_ReturnsTrue(string value)
    {
        Assert.True(Cpf.IsValid(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("111444777")]
    [InlineData("111444777350")]
    [InlineData("11144477736")]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    public void IsValid_WithInvalidCpf_ReturnsFalse(string? value)
    {
        Assert.False(Cpf.IsValid(value));
    }
}
