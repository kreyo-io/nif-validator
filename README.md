# nif-validator

Reference implementation for validating Spanish tax identification numbers (DNI, NIE, CIF) in **.NET, TypeScript, and Python**.

Zero dependencies. Pure functions. Verified against real public CIFs of Spanish companies.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![CI](https://github.com/kreyo-io/nif-validator/workflows/CI/badge.svg)](https://github.com/kreyo-io/nif-validator/actions)
[![NuGet](https://img.shields.io/nuget/v/Kreyo.NifValidator?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Kreyo.NifValidator)
[![npm](https://img.shields.io/npm/v/@kreyo/nif-validator?logo=npm&label=npm)](https://www.npmjs.com/package/@kreyo/nif-validator)
[![PyPI](https://img.shields.io/pypi/v/kreyo-nif-validator?logo=pypi&label=PyPI)](https://pypi.org/project/kreyo-nif-validator/)

> 🇪🇸 **Para developers en español**: esta librería valida números de identificación fiscal españoles (DNI, NIE, CIF) en .NET, JavaScript/TypeScript y Python. Implementación de referencia, sin dependencias, con tests sobre CIFs reales de empresas españolas. **[Documentación completa en español →](README.es.md)**

---

## Why this library exists

Validating a Spanish NIF correctly is harder than it looks. The algorithm has three variants (DNI, NIE, CIF), the CIF check digit follows specific rules per organization type, and most snippets you find online are incomplete or wrong.

This is a clean reference implementation, kept identical across three languages, verified with the same fixtures, and tested against real public Spanish company CIFs (Banco Santander, BBVA, Inditex, Telefónica, Repsol, Iberdrola).

If you're building anything that touches Spanish tax IDs — invoicing software, accounting tools, KYC flows, e-commerce checkouts — this library is for you.

## Install

### .NET (multi-target: net8.0 + netstandard2.0)

```bash
dotnet add package Kreyo.NifValidator
```

### TypeScript / JavaScript

```bash
npm install @kreyo/nif-validator
# or
pnpm add @kreyo/nif-validator
# or
yarn add @kreyo/nif-validator
```

### Python (3.10+)

```bash
pip install kreyo-nif-validator
```

## Usage

### .NET

```csharp
using Kreyo.NifValidator;

NifValidator.IsValid("12345678Z");          // true
NifValidator.IsValid("X1234567L");          // true (NIE)
NifValidator.IsValid("A39000013");          // true (Banco Santander)
NifValidator.IsValid("12345678A");          // false (wrong control letter)

NifValidator.Normalize(" 12345678-z ");     // "12345678Z"
NifValidator.GetNifType("X1234567L");       // "NIE"
NifValidator.GetNifType("invalid");         // null

// K, L, M: natural-person NIFs assigned by AEAT. Format-only check (see below)
NifValidator.IsValid("K1234567L");          // true
NifValidator.IsValid("K12345674");          // false (verification character must be a letter)
```

### TypeScript

```ts
import { isValid, normalize, getNifType } from "@kreyo/nif-validator";

isValid("12345678Z");           // true
isValid("X1234567L");           // true (NIE)
isValid("A39000013");           // true (Banco Santander)
isValid("12345678A");           // false

normalize(" 12345678-z ");      // "12345678Z"
getNifType("X1234567L");        // "NIE"
getNifType("invalid");          // null

isValid("K1234567L");           // true (K/L/M: format-only check, see below)
isValid("K12345674");           // false (verification character must be a letter)
```

### Python

```python
from nif_validator import is_valid, normalize, get_nif_type, ValidationOptions

is_valid("12345678Z")           # True
is_valid("X1234567L")           # True (NIE)
is_valid("A39000013")           # True (Banco Santander)
is_valid("12345678A")           # False

normalize(" 12345678-z ")       # "12345678Z"
get_nif_type("X1234567L")       # "NIE"
get_nif_type("invalid")         # None

is_valid("K1234567L")           # True (K/L/M: format-only check, see below)
is_valid("K12345674")           # False (verification character must be a letter)
```

## What gets validated

| Type | Format | Example | Notes |
|------|--------|---------|-------|
| **DNI** | 8 digits + control letter | `12345678Z` | For Spanish citizens |
| **NIE** | X/Y/Z + 7 digits + letter | `X1234567L` | For foreign residents |
| **CIF** | Letter + 7 digits + letter or digit | `A39000013` | For companies and other legal entities |

CIF type letters and their control rules:

| Type | Meaning | Control |
|------|---------|---------|
| A | Sociedad Anónima | digit only |
| B | Sociedad Limitada | digit only |
| C | Sociedad Colectiva | digit or letter |
| D | Sociedad Comanditaria | digit or letter |
| E | Comunidad de Bienes | digit only |
| F | Cooperativa | digit or letter |
| G | Asociación | digit or letter |
| H | Comunidad de Propietarios | digit only |
| J | Sociedad Civil | digit or letter |
| N | No residente | letter only |
| P | Corporación local | letter only |
| Q | Organismo público | letter only |
| R | Religiosa | letter only |
| S | Órgano de la Administración | letter only |
| U | Unión Temporal de Empresas | digit or letter |
| V | Otros tipos | digit or letter |
| W | Establecimiento permanente extranjero | letter only |

### K, L, M: natural-person NIFs (format-only check)

K, L and M are **current** NIFs for natural persons, assigned by AEAT (Real Decreto 1065/2007, arts. 19.2 and 20.2):

| Letter | Who |
|--------|-----|
| K | Spaniards under 14 years old resident in Spain |
| L | Spaniards resident abroad |
| M | Foreigners without NIE |

The official format is the initial letter + 7 alphanumeric characters + 1 alphabetic verification character. **The algorithm for that verification character is not published** in the BOE or by AEAT, so this library does not invent one: it checks the format only (`[KLM][0-9A-Z]{7}[A-Z]`). `K1234567L` is accepted; `K12345674` is rejected because the verification character must be a letter. This means a K/L/M NIF with a wrong verification letter is accepted. `getNifType` / `get_nif_type` / `GetNifType` return `"KLM"` for these.

### Letter `T`

`T` is not supported. We found no official source that defines `T` as an initial letter of a NIF, so it is rejected. This is "unsupported / not verifiable", not "deprecated".

### The letter-vs-digit rule for CIF is a convention

The table above (which CIF types take a digit, a letter, or either) is a **market convention**, not an official rule. The legislation only speaks of "a control character" (Orden EHA/451/2008) and does not say whether it is a letter or a digit; a 2008 AEAT communication, which we could only find as a third-party transcription, mentions digit controls for some new letters (J, U, V) and letter controls for others (R, W), but there is no complete official table. [python-stdnum](https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/es/cif.py) accepts both for every type, and its author notes that the available information is conflicting. This library keeps the stricter convention; if you must never reject a real company, treat a CIF rejected only because of letter-vs-digit as a warning.

### Deprecated option `includeDeprecated`

`includeDeprecated` / `IncludeDeprecated` / `include_deprecated` is **deprecated and has no effect since 0.2.0**. It is still accepted so existing code keeps compiling and running. Before 0.2.0 it enabled K, L, M, T with the CIF algorithm; that behavior was based on a wrong claim (see the [changelog](CHANGELOG.md)).

### Sources

- [Real Decreto 1065/2007 (BOE-A-2007-15984)](https://www.boe.es/buscar/act.php?id=BOE-A-2007-15984): arts. 19-22, NIF composition
- [Orden EHA/451/2008 (BOE-A-2008-3580)](https://www.boe.es/buscar/act.php?id=BOE-A-2008-3580&tn=1): NIF of legal entities (does not mention K, L, M or T)
- [AEAT, NIF composition: natural persons](https://sede.agenciatributaria.gob.es/Sede/ayuda/manuales-videos-folletos/manuales-practicos/guia-practica-cumplimentacion-modelo-censal-036/anexos/anexo-01-solicitud-nif-documentacion-aportar/informacion-sobre-numero-identificacion-fiscal/composicion-nif/personas-fisicas.html)
- [AEAT, NIF composition: legal entities](https://sede.agenciatributaria.gob.es/Sede/ayuda/manuales-videos-folletos/manuales-practicos/guia-practica-cumplimentacion-modelo-censal-036/anexos/anexo-01-solicitud-nif-documentacion-aportar/informacion-sobre-numero-identificacion-fiscal/composicion-nif/personas-juridicas-entidades.html)

## What this library does NOT do

- **It does not check whether a NIF is registered with AEAT.** This is a syntactic and check-digit validation. A NIF can be mathematically valid and still not exist in the tax registry. To verify that a NIF is active, query [VIES](https://ec.europa.eu/taxation_customs/vies/) (for EU tax IDs) or AEAT's services directly.
- **It does not validate the company name or address.** Just the format of the identifier.
- **It does not detect "ghost" NIFs** (mathematically valid but known to be unused). That's a business-rule concern, not a format-validation one.

## API

All three implementations expose the same three functions:

| Function | Returns | Throws |
|----------|---------|--------|
| `isValid(nif, options?)` | `boolean` | never |
| `normalize(nif)` | canonical `string` | on invalid |
| `getNifType(nif)` | `"DNI"` \| `"NIE"` \| `"CIF"` \| `null` | never |

Whitespace (spaces, tabs, newlines) and hyphens are ignored everywhere. Input is case-insensitive.

## Algorithm details

For technical details on the validation algorithm (DNI letter calculation, NIE prefix mapping, CIF check digit/letter computation), see [docs/algorithm.md](docs/algorithm.md).

## Made by Kreyo

This library is part of [Kreyo](https://kreyo.io), a developer-first platform of APIs for Spanish electronic invoicing: VeriFactu, FacturaE, AI invoice extraction, and PDF generation.

If you're building software that needs Spanish tax compliance, you might also like:

- **[Kreyo VeriFactu](https://kreyo.io/verifactu)** — generate, sign, and submit AEAT registros in real time
- **[Kreyo FacturaE](https://kreyo.io/facturae)** — generate signed FacturaE 3.2.2 XML for B2G and B2B
- **[Kreyo Extract](https://kreyo.io/extract)** — extract structured data from received invoices with AI
- **[Kreyo PDF](https://kreyo.io/pdf)** — generate invoices and documents as PDFs

And the sibling open-source repos in the Kreyo toolkit:

- **[verifactu-hash-calculator](https://github.com/kreyo-io/verifactu-hash-calculator)** — compute the VeriFactu SHA-256 hash chain (huella) for AEAT records
- **[facturae-examples](https://github.com/kreyo-io/facturae-examples)** — real, XSD-valid FacturaE 3.2.2 XML invoice examples with their JSON inputs

## Contributing

Issues and PRs welcome. Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License

[MIT](LICENSE) © Kreyo
