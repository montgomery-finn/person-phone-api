using System.Text.RegularExpressions;

namespace PersonPhone.Domain.ValueObjects;

public sealed record PhoneNumber
{
    private static readonly Regex NonDigits = new(@"\D", RegexOptions.Compiled);

    public string Value { get; }

    public PhoneNumber(string number)
    {
        if (!TryNormalize(number, out var digits, out var error))
            throw new ArgumentException(error, nameof(number));

        Value = digits;
    }

    public static bool IsValid(string? number) => TryNormalize(number, out _, out _);

    public override string ToString() => Value;

    private static bool TryNormalize(string? number, out string digits, out string? error)
    {
        digits = string.Empty;

        if (string.IsNullOrWhiteSpace(number))
        {
            error = "Number is required.";
            return false;
        }

        var normalized = NonDigits.Replace(number, string.Empty);

        if (normalized.Length < 10 || normalized.Length > 11)
        {
            error = "Number must be between 10 and 11 digits.";
            return false;
        }

        digits = normalized;
        error = null;
        return true;
    }
}
