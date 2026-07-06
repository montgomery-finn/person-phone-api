using System.Text.RegularExpressions;

namespace PersonPhone.Domain.ValueObjects;

public sealed record Cpf
{
    private static readonly Regex NonDigits = new(@"\D", RegexOptions.Compiled);

    public string Value { get; }

    public Cpf(string cpf)
    {
        if (!TryNormalize(cpf, out var digits, out var error))
            throw new ArgumentException(error, nameof(cpf));

        Value = digits;
    }

    public static bool IsValid(string? cpf) => TryNormalize(cpf, out _, out _);

    public override string ToString() => Value;

    private static bool TryNormalize(string? cpf, out string digits, out string? error)
    {
        digits = string.Empty;

        if (string.IsNullOrWhiteSpace(cpf))
        {
            error = "Cpf is required.";
            return false;
        }

        var normalized = NonDigits.Replace(cpf, string.Empty);

        if (normalized.Length != 11)
        {
            error = "Cpf must contain 11 digits.";
            return false;
        }

        if (IsRepeatedSequence(normalized) || !HasValidCheckDigits(normalized))
        {
            error = "Cpf is invalid.";
            return false;
        }

        digits = normalized;
        error = null;
        return true;
    }

    private static bool IsRepeatedSequence(string digits)
    {
        for (var i = 1; i < digits.Length; i++)
        {
            if (digits[i] != digits[0])
                return false;
        }

        return true;
    }

    private static bool HasValidCheckDigits(string digits)
    {
        var firstCheckDigit = CalculateCheckDigit(digits, 9, 10);
        if (firstCheckDigit != digits[9] - '0') // fazer um char - '0' converte o número em inteiro
            return false;

        var secondCheckDigit = CalculateCheckDigit(digits, 10, 11);
        return secondCheckDigit == digits[10] - '0';
    }

    /// <summary>
    /// Calcula os digitos verificadores do CPF (dois ultimos numeros)
    /// </summary>
    /// <param name="digits">todos os digitos do cpf</param>
    /// <param name="digitCount">se está calculando o penúltimo ou último</param>
    /// <param name="startingWeight">se o peso começa em 11 ou 10</param>
    /// <returns></returns>
    private static int CalculateCheckDigit(string digits, int digitCount, int startingWeight)
    {
        // Multiplique os dígitos pelos pesos decrescentes.
        // Some os produtos.
        // Calcule resto = soma % 11.
        // Se resto < 2, o dígito é 0.
        // Caso contrário, o dígito é 11 - resto.

        var sum = 0;
        for (var i = 0; i < digitCount; i++)
            sum += (digits[i] - '0') * (startingWeight - i);

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
