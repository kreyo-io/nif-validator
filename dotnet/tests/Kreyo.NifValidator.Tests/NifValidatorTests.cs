using Xunit;
using Kreyo.NifValidator;

namespace Kreyo.NifValidator.Tests;

public class NifValidatorTests
{
    // ===== DNI =====

    [Theory]
    [InlineData("12345678Z")]
    [InlineData("00000000T")]
    [InlineData("99999999R")]
    [InlineData("00000001R")]
    [InlineData("23456789D")]
    [InlineData("87654321X")]
    public void IsValid_Dni_Valid(string nif) =>
        Assert.True(NifValidator.IsValid(nif));

    [Theory]
    [InlineData("12345678A")]   // Wrong letter
    [InlineData("1234567Z")]    // Too short
    [InlineData("123456789Z")]  // Too long
    [InlineData("ABCDEFGHI")]   // All letters
    public void IsValid_Dni_Invalid(string nif) =>
        Assert.False(NifValidator.IsValid(nif));

    [Theory]
    [InlineData("12345678-Z")]
    [InlineData("12345678 Z")]
    [InlineData("12345678z")]
    [InlineData(" 12345678Z ")]
    [InlineData("12345678\tZ")]
    [InlineData("12345678\nZ")]
    [InlineData("  12345678  Z  ")]
    public void IsValid_Dni_WithSeparators(string nif) =>
        Assert.True(NifValidator.IsValid(nif));

    // ===== NIE =====

    [Theory]
    [InlineData("X1234567L")]   // X mapped to 01234567L
    [InlineData("Y1234567X")]   // Y mapped to 11234567X
    [InlineData("Z1234567R")]   // Z mapped to 21234567R
    public void IsValid_Nie_Valid(string nif) =>
        Assert.True(NifValidator.IsValid(nif));

    [Theory]
    [InlineData("X1234567A")]   // Wrong control letter
    [InlineData("X12A4567L")]   // Letter inside body
    [InlineData("W1234567L")]   // W is not a NIE prefix
    public void IsValid_Nie_Invalid(string nif) =>
        Assert.False(NifValidator.IsValid(nif));

    [Theory]
    [InlineData("X-1234567-L")]
    [InlineData("X 1234567 L")]
    [InlineData("x1234567l")]
    public void IsValid_Nie_WithSeparators(string nif) =>
        Assert.True(NifValidator.IsValid(nif));

    // ===== CIF — synthetic fixtures by type (body 1234567) =====

    [Theory]
    [InlineData("A12345674")]   // A: digit-only control (4)
    [InlineData("B12345674")]
    [InlineData("E12345674")]
    [InlineData("H12345674")]
    public void IsValid_Cif_DigitOnlyTypes(string nif) =>
        Assert.True(NifValidator.IsValid(nif));

    [Theory]
    [InlineData("N1234567D")]   // N: letter-only control
    [InlineData("P1234567D")]
    [InlineData("Q1234567D")]
    [InlineData("R1234567D")]
    [InlineData("S1234567D")]
    [InlineData("W1234567D")]
    public void IsValid_Cif_LetterOnlyTypes(string nif) =>
        Assert.True(NifValidator.IsValid(nif));

    [Theory]
    [InlineData("C1234567D")]   // C accepts letter
    [InlineData("C12345674")]   // C accepts digit
    [InlineData("D1234567D")]
    [InlineData("D12345674")]
    [InlineData("F1234567D")]
    [InlineData("F12345674")]
    [InlineData("G1234567D")]
    [InlineData("G12345674")]
    [InlineData("J1234567D")]
    [InlineData("J12345674")]
    [InlineData("U1234567D")]
    [InlineData("U12345674")]
    [InlineData("V1234567D")]
    [InlineData("V12345674")]
    public void IsValid_Cif_BothAcceptedTypes(string nif) =>
        Assert.True(NifValidator.IsValid(nif));

    [Theory]
    [InlineData("A12345670")]   // Wrong control digit
    [InlineData("P1234567A")]   // P requires letter, A is wrong letter
    [InlineData("A123456789")]  // Too long
    [InlineData("I12345678")]   // I is not a valid CIF type
    [InlineData("O12345678")]   // O is not a valid CIF type
    public void IsValid_Cif_Invalid(string nif) =>
        Assert.False(NifValidator.IsValid(nif));

    // ===== Real public Spanish company CIFs =====

    [Theory]
    [InlineData("A39000013")]   // Banco Santander
    [InlineData("A48265169")]   // BBVA
    [InlineData("A15075062")]   // Inditex
    [InlineData("A82018474")]   // Telefónica de España
    [InlineData("A78374725")]   // Repsol
    [InlineData("A48010615")]   // Iberdrola
    public void IsValid_Cif_RealPublicCompanies(string nif) =>
        Assert.True(NifValidator.IsValid(nif));

    // ===== Deprecated types K, L, M, T =====

    [Theory]
    [InlineData("K12345674")]
    [InlineData("L12345674")]
    [InlineData("M12345674")]
    [InlineData("T12345674")]
    public void IsValid_Cif_Deprecated_DefaultRejects(string nif) =>
        Assert.False(NifValidator.IsValid(nif));

    [Theory]
    [InlineData("K12345674")]
    [InlineData("L12345674")]
    [InlineData("M12345674")]
    [InlineData("T12345674")]
    public void IsValid_Cif_Deprecated_OptInAccepts(string nif) =>
        Assert.True(NifValidator.IsValid(
            nif,
            new NifValidatorOptions { IncludeDeprecated = true }));

    // ===== Edge cases =====

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("1234567890")]
    public void IsValid_EdgeCases_ReturnsFalse(string? nif) =>
        Assert.False(NifValidator.IsValid(nif));

    // ===== Normalize =====

    [Theory]
    [InlineData("12345678-z", "12345678Z")]
    [InlineData(" 12345678 Z ", "12345678Z")]
    [InlineData("a-12345674", "A12345674")]
    [InlineData("X 1234567 L", "X1234567L")]
    public void Normalize_ReturnsCanonical(string input, string expected) =>
        Assert.Equal(expected, NifValidator.Normalize(input));

    [Theory]
    [InlineData("12345678A")]
    [InlineData("invalid")]
    [InlineData(null)]
    [InlineData("")]
    public void Normalize_InvalidThrows(string? input) =>
        Assert.Throws<ArgumentException>(() => NifValidator.Normalize(input));

    // ===== GetNifType =====

    [Theory]
    [InlineData("12345678Z", "DNI")]
    [InlineData("X1234567L", "NIE")]
    [InlineData("A12345674", "CIF")]
    [InlineData("invalid", null)]
    [InlineData(null, null)]
    public void GetNifType_ReturnsCorrectType(string? input, string? expected) =>
        Assert.Equal(expected, NifValidator.GetNifType(input));
}
