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

// Optional: include deprecated types K, L, M, T (pre-2008)
NifValidator.IsValid("K12345674", new NifValidatorOptions { IncludeDeprecated = true });
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

isValid("K12345674", { includeDeprecated: true });  // true
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

is_valid("K12345674", ValidationOptions(include_deprecated=True))  # True
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
| K, L, M, T | Deprecated by AEAT (pre-2008) | only with `includeDeprecated` |

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

## Contributing

Issues and PRs welcome. Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License

[MIT](LICENSE) © Kreyo
