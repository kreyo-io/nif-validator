using System;
using System.Text;

namespace Kreyo.NifValidator;

/// <summary>
/// Validates Spanish tax identification numbers (DNI, NIE, CIF, and K/L/M natural-person NIFs).
/// Pure functions, no dependencies, thread-safe.
/// </summary>
public static class NifValidator
{
    private const string DniLetters = "TRWAGMYFPDXBNJZSQVHLCKE";
    private const string CifControlLetters = "JABCDEFGHI";

    // CIF (legal entities) control-character rules. This split is a market CONVENTION,
    // not an official rule: BOE/AEAT only say "a control character" (Orden EHA/451/2008)
    // without stating whether it is a letter or a digit. python-stdnum accepts both.
    //   Letter-only control:  P, Q, R, S, N, W
    //   Digit-only control:   A, B, E, H
    //   Both accepted:        C, D, F, G, J, U, V
    private const string CifLetterOnly = "PQRSNW";
    private const string CifDigitOnly = "ABEH";
    private const string CifBothAccepted = "CDFGJUV";

    /// <summary>
    /// Validates a Spanish NIF (DNI, NIE, CIF, or K/L/M).
    /// Whitespace and hyphens are ignored. Case-insensitive.
    /// </summary>
    /// <param name="nif">The NIF to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(string? nif) => IsValid(nif, default);

    /// <summary>
    /// Validates a Spanish NIF with options. The options currently have no effect.
    /// </summary>
    /// <param name="nif">The NIF to validate.</param>
    /// <param name="options">Validation options (ignored).</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(string? nif, NifValidatorOptions options)
    {
        var clean = Clean(nif);
        if (clean is null) return false;

        var first = clean[0];

        if (IsAsciiDigit(first))
            return ValidateDni(clean);

        if (first is 'X' or 'Y' or 'Z')
            return ValidateNie(clean);

        if (CifLetterOnly.IndexOf(first) >= 0
            || CifDigitOnly.IndexOf(first) >= 0
            || CifBothAccepted.IndexOf(first) >= 0)
            return ValidateCif(clean);

        if (first is 'K' or 'L' or 'M')
            return ValidateKlm(clean);

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
        if (clean is null || !IsValid(clean))
            throw new ArgumentException("Invalid NIF", nameof(nif));
        return clean;
    }

    /// <summary>
    /// Returns the type of a NIF: "DNI", "NIE", "CIF", "KLM" (natural-person NIF assigned
    /// by AEAT, initial letter K, L or M), or null if invalid.
    /// </summary>
    /// <param name="nif">The NIF to inspect.</param>
    /// <returns>"DNI", "NIE", "CIF", "KLM", or null.</returns>
    public static string? GetNifType(string? nif)
    {
        var clean = Clean(nif);
        if (clean is null) return null;

        var first = clean[0];

        if (IsAsciiDigit(first) && ValidateDni(clean))
            return "DNI";

        if (first is 'X' or 'Y' or 'Z' && ValidateNie(clean))
            return "NIE";

        if ((CifLetterOnly.IndexOf(first) >= 0
             || CifDigitOnly.IndexOf(first) >= 0
             || CifBothAccepted.IndexOf(first) >= 0)
            && ValidateCif(clean))
            return "CIF";

        if (first is 'K' or 'L' or 'M' && ValidateKlm(clean))
            return "KLM";

        return null;
    }

    private static string? Clean(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var sb = new StringBuilder(input!.Length);
        foreach (var c in input)
        {
            if (char.IsWhiteSpace(c) || c == '-') continue;
            // ASCII-only upper-casing: Unicode case mappings must not turn
            // non-ASCII letters into A-Z.
            sb.Append(c is >= 'a' and <= 'z' ? (char)(c - 32) : c);
        }

        var clean = sb.ToString();
        return clean.Length == 9 ? clean : null;
    }

    private static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';

    private static bool IsAsciiUpper(char c) => c is >= 'A' and <= 'Z';

    private static bool ValidateDni(string clean)
    {
        var number = 0;
        for (var i = 0; i < 8; i++)
        {
            if (!IsAsciiDigit(clean[i])) return false;
            number = number * 10 + (clean[i] - '0');
        }
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

    // K/L/M: NIF of a natural person assigned by AEAT (RD 1065/2007, arts. 19.2 and 20.2):
    // initial letter + 7 alphanumeric characters + alphabetic verification character.
    // The verification algorithm is not published in BOE/AEAT, so only the format is checked.
    private static bool ValidateKlm(string clean)
    {
        for (var i = 1; i <= 7; i++)
            if (!IsAsciiDigit(clean[i]) && !IsAsciiUpper(clean[i])) return false;
        return IsAsciiUpper(clean[8]);
    }

    private static bool ValidateCif(string clean)
    {
        var first = clean[0];

        // Strict ASCII digits only (int.TryParse would accept a leading sign, e.g. "A+1234567")
        for (var i = 1; i <= 7; i++)
            if (!IsAsciiDigit(clean[i])) return false;

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

        // Both accepted (CDFGJUV)
        return control == checkLetter || control == (char)('0' + checkDigit);
    }
}

/// <summary>
/// Options for NIF validation. Currently has no effective options.
/// </summary>
public readonly struct NifValidatorOptions
{
    /// <summary>
    /// Obsolete and ignored. K, L and M NIFs (natural persons, RD 1065/2007 arts. 19.2 and 20.2)
    /// are accepted by default, validating format only. Kept so existing callers still compile.
    /// </summary>
    [Obsolete("No effect since 0.2.0: K/L/M are valid NIF types and are always format-checked; T has no official basis and is rejected.")]
    public bool IncludeDeprecated { get; init; }
}
