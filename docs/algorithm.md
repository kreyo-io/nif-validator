# Spanish NIF Validation Algorithm

This document describes the algorithm used by `nif-validator` to validate Spanish tax identification numbers. The implementations in .NET, TypeScript, and Python all follow this exact specification.

## Overview

Spanish tax IDs come in four flavors:

- **DNI** (Documento Nacional de Identidad): for Spanish citizens. 8 digits + 1 control letter.
- **NIE** (Número de Identidad de Extranjero): for foreign residents. X/Y/Z prefix + 7 digits + 1 control letter.
- **CIF** (Código de Identificación Fiscal): for companies and other legal entities. 1 type letter + 7 digits + 1 control character (digit or letter, depending on type).

- **K/L/M NIF** (natural persons without DNI/NIE, assigned by AEAT): 1 letter (K, L or M) + 7 alphanumeric characters + 1 alphabetic verification character. See [K, L, M](#k-l-m-natural-person-nifs).

All of them are 9 characters long.

## Normalization (preprocessing)

Before validation, input is normalized:

1. Trim leading/trailing whitespace.
2. Remove all whitespace (spaces, tabs, newlines, etc.) and hyphens (`-`).
3. Convert ASCII letters `a-z` to uppercase (other characters are left untouched, so non-ASCII letters never become `A-Z`).

Digit checks are strict ASCII `0`-`9`: signs (`+`), non-ASCII digits and similar are rejected.

If the result is not exactly 9 characters, validation fails immediately.

This means `"12345678-z"`, `"  12345678  Z  "`, and `"12345678\tz"` are all equivalent to `"12345678Z"`.

## DNI validation

The control letter is computed as:

```
letter = "TRWAGMYFPDXBNJZSQVHLCKE"[number % 23]
```

Where `number` is the 8-digit body interpreted as an integer.

The letter alphabet `TRWAGMYFPDXBNJZSQVHLCKE` is the standard, fixed table used for the DNI check letter. The DNI is valid if and only if the 9th character matches this computed letter.

**Examples:**
- `12345678` mod 23 = 14 → `Z` → valid DNI: `12345678Z`
- `00000000` mod 23 = 0 → `T` → valid DNI: `00000000T`
- `99999999` mod 23 = 1 → `R` → valid DNI: `99999999R`

## NIE validation

The first character maps to a digit, then the rest is validated as a DNI:

| Prefix | Maps to |
|--------|---------|
| X | 0 |
| Y | 1 |
| Z | 2 |

After mapping, apply the DNI algorithm to the resulting 9-character string.

**Examples:**
- `X1234567L` → `01234567L` → `01234567` mod 23 = 11 → `L` → valid
- `Y1234567X` → `11234567X` → `11234567` mod 23 = 5 → `X` → valid
- `Z1234567R` → `21234567R` → `21234567` mod 23 = 1 → `R` → valid

## CIF validation

CIF validation is more involved. The control character can be a digit, a letter, or either, depending on the type letter at position 0.

> **Convention, not an official rule.** The letter-vs-digit split below is a market convention. The legislation (Orden EHA/451/2008) only says the NIF of a legal entity ends with "a control character" and does not say whether it is a letter or a digit. [python-stdnum](https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/es/cif.py) accepts both for every type and its author acknowledges the available information is conflicting. This library keeps the stricter convention; it is not backed by an official table.

### Step 1: Compute the check digit

For the 7-digit body (positions 1-7 in 1-based indexing):

1. **Odd positions** (1st, 3rd, 5th, 7th — i.e. indices 0, 2, 4, 6 in 0-based):
   - Multiply by 2.
   - If the result is greater than 9, subtract 9 (equivalent to summing the digits).
   - Sum these values.

2. **Even positions** (2nd, 4th, 6th — i.e. indices 1, 3, 5 in 0-based):
   - Sum directly.

3. **Total** = sum of odd + sum of even.

4. **Check digit** = `(10 - total mod 10) mod 10` (a number between 0 and 9).

5. **Check letter** = `"JABCDEFGHI"[checkDigit]` (J=0, A=1, B=2, ..., I=9).

### Step 2: Match against control character

The 9th character must match according to the conventional type letter rules:

| Types | Control rule |
|-------|--------------|
| **N, P, Q, R, S, W** | Letter only (must equal check letter) |
| **A, B, E, H** | Digit only (must equal check digit as a string) |
| **C, D, F, G, J, U, V** | Either (must equal check letter OR check digit) |
| Other (including `T`) | Invalid, or handled separately (K/L/M, see below) |

## K, L, M: natural-person NIFs

K, L and M are **current** NIFs of natural persons assigned by AEAT (Real Decreto 1065/2007, arts. 19.2 and 20.2):

- **K**: Spaniards under 14 years old resident in Spain.
- **L**: Spaniards resident abroad.
- **M**: foreigners without NIE.

Official format: initial letter + 7 alphanumeric characters + 1 alphabetic verification character. The algorithm for the verification character is **not published** by the BOE or AEAT, so no checksum is computed. The validator checks the format only:

```
^[KLM][0-9A-Z]{7}[A-Z]$   (after normalization)
```

| Input | Result | Why |
|-------|--------|-----|
| `K1234567L`, `L1234567D`, `M1234567L` | valid | format matches |
| `KA23B5C7Z` | valid | body is alphanumeric |
| `K12345674` | invalid | verification character must be a letter |
| `K123456L`, `K1234567`, `K12345678Z` | invalid | wrong length |

Trade-off: because no checksum is applied, a K/L/M NIF with a mistyped verification letter is accepted. We prefer that over rejecting real NIFs with an invented algorithm.

### Worked example

CIF `A39000013` (Banco Santander):

- Type: `A` → digit-only control.
- Body: `3900001`.
- Odd positions (indices 0, 2, 4, 6): digits `3, 0, 0, 1`.
  - `3 * 2 = 6` → `6`
  - `0 * 2 = 0` → `0`
  - `0 * 2 = 0` → `0`
  - `1 * 2 = 2` → `2`
  - Sum: `8`.
- Even positions (indices 1, 3, 5): digits `9, 0, 0`.
  - Sum: `9`.
- Total: `8 + 9 = 17`.
- Check digit: `(10 - 17 mod 10) mod 10 = (10 - 7) mod 10 = 3`.
- Control character: `3`. Matches the actual control `3`. → **Valid.**

## Edge cases

- **Empty / null input**: invalid.
- **Wrong length** (after normalization): invalid.
- **Invalid type letter** (I, O, etc.): invalid.
- **Letters in the digit body**: invalid.
- **K, L, M**: accepted by format only (see above).
- **`T` as initial letter**: rejected. No official source defines it, so it is unsupported (not "deprecated").
- **`includeDeprecated` option**: deprecated, accepted and ignored since 0.2.0.

## What this algorithm does NOT do

- It does not check whether the NIF is registered with AEAT.
- It does not detect "ghost" NIFs (mathematically valid but unused, e.g. `00000001R`).
- It does not validate company names or addresses.

For runtime verification of whether a NIF is active, query VIES (for cross-border EU IDs) or AEAT services directly.

## References

- [Real Decreto 1065/2007, de 27 de julio (BOE-A-2007-15984)](https://www.boe.es/buscar/act.php?id=BOE-A-2007-15984): arts. 19.2 and 20.2 (K, L, M), art. 22 (legal entities)
- [Orden EHA/451/2008 (BOE-A-2008-3580)](https://www.boe.es/buscar/act.php?id=BOE-A-2008-3580&tn=1): NIF of legal entities; does not mention K, L, M or T
- [AEAT, NIF composition: natural persons](https://sede.agenciatributaria.gob.es/Sede/ayuda/manuales-videos-folletos/manuales-practicos/guia-practica-cumplimentacion-modelo-censal-036/anexos/anexo-01-solicitud-nif-documentacion-aportar/informacion-sobre-numero-identificacion-fiscal/composicion-nif/personas-fisicas.html)
- [AEAT, NIF composition: legal entities](https://sede.agenciatributaria.gob.es/Sede/ayuda/manuales-videos-folletos/manuales-practicos/guia-practica-cumplimentacion-modelo-censal-036/anexos/anexo-01-solicitud-nif-documentacion-aportar/informacion-sobre-numero-identificacion-fiscal/composicion-nif/personas-juridicas-entidades.html)
- [python-stdnum `es/cif.py`](https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/es/cif.py): accepts both letter and digit for every type
