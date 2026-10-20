"""
Validates Spanish tax identification numbers (DNI, NIE, CIF, and K/L/M natural-person NIFs).

Pure functions, no dependencies, type-hinted.
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import Literal, Optional

__all__ = ["is_valid", "normalize", "get_nif_type", "ValidationOptions", "NifType"]

_DNI_LETTERS = "TRWAGMYFPDXBNJZSQVHLCKE"
_CIF_CONTROL_LETTERS = "JABCDEFGHI"
# CIF letter/digit control split: market CONVENTION, not an official rule (BOE/AEAT only
# say "a control character"; python-stdnum accepts both).
_CIF_LETTER_ONLY = "PQRSNW"
_CIF_DIGIT_ONLY = "ABEH"
_CIF_BOTH_ACCEPTED = "CDFGJUV"
_KLM = "KLM"
_DIGITS = frozenset("0123456789")
_UPPER = frozenset("ABCDEFGHIJKLMNOPQRSTUVWXYZ")

# "KLM" = natural-person NIF assigned by AEAT (initial letter K, L or M).
NifType = Literal["DNI", "NIE", "CIF", "KLM"]


@dataclass(frozen=True)
class ValidationOptions:
    """Options for NIF validation."""

    include_deprecated: bool = False
    """Deprecated and ignored since 0.2.0.

    K, L and M NIFs (natural persons, RD 1065/2007 arts. 19.2 and 20.2) are accepted by
    default (format only); T has no official basis and is rejected. Kept so existing
    callers still work.
    """


_DEFAULT_OPTIONS = ValidationOptions()


def is_valid(
    nif: Optional[str],
    options: ValidationOptions = _DEFAULT_OPTIONS,
) -> bool:
    """
    Validates a Spanish NIF (DNI, NIE, CIF, or K/L/M).

    Whitespace and hyphens are ignored. Case-insensitive.
    The options argument is accepted for backward compatibility and ignored.
    """
    clean = _clean(nif)
    if clean is None:
        return False

    first = clean[0]

    if first in _DIGITS:
        return _validate_dni(clean)
    if first in "XYZ":
        return _validate_nie(clean)
    if first in _CIF_LETTER_ONLY + _CIF_DIGIT_ONLY + _CIF_BOTH_ACCEPTED:
        return _validate_cif(clean)
    if first in _KLM:
        return _validate_klm(clean)
    return False


def normalize(nif: Optional[str]) -> str:
    """
    Returns the canonical form of a valid NIF (uppercase, no separators).

    Raises ValueError if the NIF is invalid.
    """
    clean = _clean(nif)
    if clean is None or not is_valid(clean):
        raise ValueError(f"Invalid NIF: {nif!r}")
    return clean


def get_nif_type(nif: Optional[str]) -> Optional[NifType]:
    """Returns the type of a NIF: 'DNI', 'NIE', 'CIF', 'KLM', or None if invalid."""
    clean = _clean(nif)
    if clean is None:
        return None

    first = clean[0]

    if first in _DIGITS and _validate_dni(clean):
        return "DNI"
    if first in "XYZ" and _validate_nie(clean):
        return "NIE"
    if (
        first in _CIF_LETTER_ONLY + _CIF_DIGIT_ONLY + _CIF_BOTH_ACCEPTED
        and _validate_cif(clean)
    ):
        return "CIF"
    if first in _KLM and _validate_klm(clean):
        return "KLM"
    return None


# ----- Internal helpers -----


def _clean(s: Optional[str]) -> Optional[str]:
    if s is None or not s.strip():
        return None
    # ASCII-only upper-casing: Unicode case mappings must not turn non-ASCII
    # letters into A-Z (e.g. "ſ" -> "S") or change the length ("ß" -> "SS").
    out = "".join(
        c.upper() if "a" <= c <= "z" else c for c in s if not c.isspace() and c != "-"
    )
    return out if len(out) == 9 else None


def _validate_dni(clean: str) -> bool:
    body = clean[:8]
    # str.isdigit() accepts non-ASCII digits; stay strict.
    if not all(c in _DIGITS for c in body):
        return False
    return clean[8] == _DNI_LETTERS[int(body) % 23]


def _validate_nie(clean: str) -> bool:
    prefix = {"X": "0", "Y": "1", "Z": "2"}.get(clean[0])
    if prefix is None:
        return False
    return _validate_dni(prefix + clean[1:])


def _validate_klm(clean: str) -> bool:
    # K/L/M: NIF of a natural person assigned by AEAT (RD 1065/2007, arts. 19.2 and 20.2):
    # initial letter + 7 alphanumeric characters + alphabetic verification character.
    # The verification algorithm is not published in BOE/AEAT, so only the format is checked.
    return all(c in _DIGITS or c in _UPPER for c in clean[1:8]) and clean[8] in _UPPER


def _validate_cif(clean: str) -> bool:
    first = clean[0]
    body = clean[1:8]
    if not all(c in _DIGITS for c in body):
        return False

    sum_odd = 0
    sum_even = 0
    for i in range(7):
        digit = int(body[i])
        if i % 2 == 0:
            doubled = digit * 2
            sum_odd += doubled - 9 if doubled > 9 else doubled
        else:
            sum_even += digit

    total = sum_odd + sum_even
    check_digit = (10 - total % 10) % 10
    check_letter = _CIF_CONTROL_LETTERS[check_digit]
    control = clean[8]

    if first in _CIF_LETTER_ONLY:
        return control == check_letter
    if first in _CIF_DIGIT_ONLY:
        return control == str(check_digit)
    return control == check_letter or control == str(check_digit)
