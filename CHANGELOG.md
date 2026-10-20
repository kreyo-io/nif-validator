# Changelog

## 0.2.0

Behavior change in all three packages (.NET `Kreyo.NifValidator`, npm `@kreyo/nif-validator`, PyPI `kreyo-nif-validator`). Since the packages are pre-1.0, this is a minor bump.

### Fixed: K, L, M were wrongly documented and handled

Earlier versions (and their docs) claimed that K, L, M and T were "deprecated by AEAT pre-2008 / by Orden EHA/451/2008". That is false: Orden EHA/451/2008 does not mention K, L, M or T, and AEAT still describes K, L and M as current NIFs of natural persons (Real Decreto 1065/2007, arts. 19.2 and 20.2). The previous code also validated them with the CIF algorithm, which no source supports, and the README example `K12345674` -> valid contradicted the BOE (the verification character must be a letter).

- **K, L, M are now accepted by default**, validating the official format only: initial letter + 7 alphanumeric characters + 1 letter. No checksum is applied, because the algorithm is not published.
- `K12345674` (numeric verification character) is now **rejected**, also with the old option.
- `getNifType` / `get_nif_type` / `GetNifType` return the new value `"KLM"` for these NIFs (previously they returned `"CIF"` when the deprecated option was used).
- **`T` is unsupported** (no official source defines it as an initial letter) and is rejected. It was previously accepted with the deprecated option.
- **`includeDeprecated` / `IncludeDeprecated` / `include_deprecated` is deprecated and ignored.** It is still accepted so existing callers do not break. Callers that relied on it to accept `T...` or CIF-algorithm `K/L/M...` values will see those inputs rejected.

### Fixed: strict ASCII digits

- .NET: the CIF body check used `int.TryParse`, which accepts a leading sign, so `A+1234567` passed the digit check. It now requires ASCII `0`-`9`.
- Python: `str.isdigit()` accepts non-ASCII digits (and `int()` could raise on some of them); replaced by an ASCII check.
- All three: upper-casing during normalization is ASCII-only, so non-ASCII letters can no longer turn into `A-Z`.

### Docs

- The CIF letter-versus-digit control rule is now documented as a market convention, not an official rule (BOE/AEAT only say "a control character"; python-stdnum accepts both).
- Replaced two broken reference links with verified BOE/AEAT sources.
