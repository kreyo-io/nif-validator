using System;
using System.Text;

namespace Kreyo.NifValidator;

/// <summary>
/// Validates Spanish tax identification numbers (DNI, NIE, CIF).
/// Pure functions, no dependencies, thread-safe.
/// </summary>
public static class NifValidator
{
    private const string DniLetters = "TRWAGMYFPDXBNJZSQVHLCKE";
    private const string CifControlLetters = "JABCDEFGHI";

    // CIF type rules:
    //   Letter-only control:  P, Q, R, S, N, W
    //   Digit-only control:   A, B, E, H
    //   Both accepted:        C, D, F, G, J, U, V
    //   Deprecated (pre-2008): K, L, M, T (only validated when options.IncludeDeprecated)
    private const string CifLetterOnly = "PQRSNW";
    private const string CifDigitOnly = "ABEH";
    private const string CifBothAccepted = "CDFGJUV";
    private const string CifDeprecated = "KLMT";

    /// <summary>
    /// Validates a Spanish NIF (DNI, NIE, or CIF).
    /// Whitespace and hyphens are ignored. Case-insensitive.
    /// </summary>
    /// <param name="nif">The NIF to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(string? nif) => IsValid(nif, default);

    /// <summary>
    /// Validates a Spanish NIF (DNI, NIE, or CIF) with options.
    /// </summary>
    /// <param name="nif">The NIF to validate.</param>
    /// <param name="options">Validation options.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(string? nif, NifValidatorOptions options)
    {
        var clean = Clean(nif);
        if (clean is null) return false;

        var first = clean[0];

        if (char.IsDigit(first))
            return ValidateDni(clean);

        if (first is 'X' or 'Y' or 'Z')
            return ValidateNie(clean);

        if (CifLetterOnly.IndexOf(first) >= 0
            || CifDigitOnly.IndexOf(first) >= 0
            || CifBothAccepted.IndexOf(first) >= 0)
            return ValidateCif(clean);

        if (CifDeprecated.IndexOf(first) >= 0)
            return options.IncludeDeprecated && ValidateCif(clean);

        return false;
    }

    /// <summary>
    /// Returns the normalized form of a valid NIF (uppercase, no separators).
    /// </summary>
    /// <param name="nif">The NIF to normalize.</param>
    /// <returns>The normalized NIF.</returns>
    /// <exception cref="ArgumentException">Thrown if the NIF is invalid.</exception>
    public static string Normalize(string? nif)
    {
        var clean = Clean(nif);
        if (clean is null || !IsValid(clean, new NifValidatorOptions { IncludeDeprecated = true }))
            throw new ArgumentException("Invalid NIF", nameof(nif));
        return clean;
    }

    /// <summary>
    /// Returns the type of a NIF: "DNI", "NIE", "CIF", or null if invalid.
    /// </summary>
    /// <param name="nif">The NIF to inspect.</param>
    /// <returns>"DNI", "NIE", "CIF", or null.</returns>
    public static string? GetNifType(string? nif)
    {
        var clean = Clean(nif);
        if (clean is null) return null;

        var first = clean[0];

        if (char.IsDigit(first) && ValidateDni(clean))
            return "DNI";

        if (first is 'X' or 'Y' or 'Z' && ValidateNie(clean))
            return "NIE";

        if ((CifLetterOnly.IndexOf(first) >= 0
             || CifDigitOnly.IndexOf(first) >= 0
             || CifBothAccepted.IndexOf(first) >= 0
             || CifDeprecated.IndexOf(first) >= 0)
            && ValidateCif(clean))
            return "CIF";

        return null;
    }

    private static string? Clean(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var sb = new StringBuilder(input!.Length);
        foreach (var c in input)
        {
            if (char.IsWhiteSpace(c) || c == '-') continue;
            sb.Append(char.ToUpperInvariant(c));
        }

        var clean = sb.ToString();
        return clean.Length == 9 ? clean : null;
    }

    private static bool ValidateDni(string clean)
    {
        if (!int.TryParse(clean.AsSpan(0, 8), out var number)) return false;
        var expected = DniLetters[number % 23];
        return clean[8] == expected;
    }

    private static bool ValidateNie(string clean)
    {
        var prefix = clean[0] switch
        {
            'X' => '0',
            'Y' => '1',
            'Z' => '2',
            _ => '\0'
        };
        if (prefix == '\0') return false;
        var asDni = prefix + clean.Substring(1);
        return ValidateDni(asDni);
    }

    private static bool ValidateCif(string clean)
    {
        var first = clean[0];

        // Digits 1-7 (positions 1..7 in the body)
        if (!int.TryParse(clean.AsSpan(1, 7), out _)) return false;

        var sumOdd = 0;   // positions 1, 3, 5, 7 (1-based) doubled and digit-summed
        var sumEven = 0;  // positions 2, 4, 6 (1-based) directly

        for (var i = 1; i <= 7; i++)
        {
            var digit = clean[i] - '0';
            if ((i & 1) == 1)
            {
                var doubled = digit * 2;
                sumOdd += doubled > 9 ? doubled - 9 : doubled;
            }
            else
            {
                sumEven += digit;
            }
        }

        var total = sumOdd + sumEven;
        var checkDigit = (10 - total % 10) % 10;
        var checkLetter = CifControlLetters[checkDigit];
        var control = clean[8];

        if (CifLetterOnly.IndexOf(first) >= 0)
            return control == checkLetter;

        if (CifDigitOnly.IndexOf(first) >= 0)
            return control == (char)('0' + checkDigit);

        // Both accepted (CDFGJUV) or deprecated (KLMT)
        return control == checkLetter || control == (char)('0' + checkDigit);
    }
}

/// <summary>
/// Options for NIF validation.
/// </summary>
public readonly struct NifValidatorOptions
{
    /// <summary>
    /// When true, validates legacy NIF types K, L, M, T (deprecated by AEAT pre-2008).
    /// Default is false.
    /// </summary>
    public bool IncludeDeprecated { get; init; }
}
