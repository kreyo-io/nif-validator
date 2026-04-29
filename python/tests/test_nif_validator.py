"""Tests for nif_validator."""

import pytest
from nif_validator import (
    is_valid,
    normalize,
    get_nif_type,
    ValidationOptions,
)


# ===== DNI =====


@pytest.mark.parametrize(
    "nif",
    ["12345678Z", "00000000T", "99999999R", "00000001R", "23456789D", "87654321X"],
)
def test_is_valid_dni(nif: str) -> None:
    assert is_valid(nif) is True


@pytest.mark.parametrize(
    "nif", ["12345678A", "1234567Z", "123456789Z", "ABCDEFGHI"]
)
def test_is_valid_dni_invalid(nif: str) -> None:
    assert is_valid(nif) is False


@pytest.mark.parametrize(
    "nif",
    [
        "12345678-Z",
        "12345678 Z",
        "12345678z",
        " 12345678Z ",
        "12345678\tZ",
        "12345678\nZ",
        "  12345678  Z  ",
    ],
)
def test_is_valid_dni_with_separators(nif: str) -> None:
    assert is_valid(nif) is True


# ===== NIE =====


@pytest.mark.parametrize("nif", ["X1234567L", "Y1234567X", "Z1234567R"])
def test_is_valid_nie(nif: str) -> None:
    assert is_valid(nif) is True


@pytest.mark.parametrize("nif", ["X1234567A", "X12A4567L", "W1234567L"])
def test_is_valid_nie_invalid(nif: str) -> None:
    assert is_valid(nif) is False


# ===== CIF =====


@pytest.mark.parametrize("nif", ["A12345674", "B12345674", "E12345674", "H12345674"])
def test_is_valid_cif_digit_only(nif: str) -> None:
    assert is_valid(nif) is True


@pytest.mark.parametrize(
    "nif", ["N1234567D", "P1234567D", "Q1234567D", "R1234567D", "S1234567D", "W1234567D"]
)
def test_is_valid_cif_letter_only(nif: str) -> None:
    assert is_valid(nif) is True


@pytest.mark.parametrize(
    "nif",
    [
        "C1234567D",
        "C12345674",
        "D1234567D",
        "D12345674",
        "F1234567D",
        "F12345674",
        "G1234567D",
        "G12345674",
        "J1234567D",
        "J12345674",
        "U1234567D",
        "U12345674",
        "V1234567D",
        "V12345674",
    ],
)
def test_is_valid_cif_both_accepted(nif: str) -> None:
    assert is_valid(nif) is True


@pytest.mark.parametrize(
    "nif", ["A12345670", "P1234567A", "A123456789", "I12345678", "O12345678"]
)
def test_is_valid_cif_invalid(nif: str) -> None:
    assert is_valid(nif) is False


# ===== Real public Spanish company CIFs =====


@pytest.mark.parametrize(
    "nif,company",
    [
        ("A39000013", "Banco Santander"),
        ("A48265169", "BBVA"),
        ("A15075062", "Inditex"),
        ("A82018474", "Telefónica de España"),
        ("A78374725", "Repsol"),
        ("A48010615", "Iberdrola"),
    ],
)
def test_is_valid_cif_real_companies(nif: str, company: str) -> None:
    assert is_valid(nif) is True, f"{company} CIF {nif} should be valid"


# ===== Deprecated K, L, M, T =====


@pytest.mark.parametrize("nif", ["K12345674", "L12345674", "M12345674", "T12345674"])
def test_deprecated_default_rejects(nif: str) -> None:
    assert is_valid(nif) is False


@pytest.mark.parametrize("nif", ["K12345674", "L12345674", "M12345674", "T12345674"])
def test_deprecated_opt_in_accepts(nif: str) -> None:
    assert is_valid(nif, ValidationOptions(include_deprecated=True)) is True


# ===== Edge cases =====


@pytest.mark.parametrize("nif", [None, "", "   ", "12345", "1234567890"])
def test_is_valid_edge_cases(nif: str | None) -> None:
    assert is_valid(nif) is False


# ===== Normalize =====


@pytest.mark.parametrize(
    "input_,expected",
    [
        ("12345678-z", "12345678Z"),
        (" 12345678 Z ", "12345678Z"),
        ("a-12345674", "A12345674"),
        ("X 1234567 L", "X1234567L"),
    ],
)
def test_normalize(input_: str, expected: str) -> None:
    assert normalize(input_) == expected


@pytest.mark.parametrize("input_", ["12345678A", "invalid", None, ""])
def test_normalize_invalid_raises(input_: str | None) -> None:
    with pytest.raises(ValueError):
        normalize(input_)


# ===== get_nif_type =====


@pytest.mark.parametrize(
    "input_,expected",
    [
        ("12345678Z", "DNI"),
        ("X1234567L", "NIE"),
        ("A12345674", "CIF"),
        ("invalid", None),
        (None, None),
    ],
)
def test_get_nif_type(input_: str | None, expected: str | None) -> None:
    assert get_nif_type(input_) == expected
