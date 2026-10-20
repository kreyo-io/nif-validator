# nif-validator

Implementación de referencia para validar números de identificación fiscal españoles (DNI, NIE, CIF) en **.NET, TypeScript y Python**.

Sin dependencias. Funciones puras. Verificada con CIFs reales públicos de empresas españolas.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![CI](https://github.com/kreyo-io/nif-validator/workflows/CI/badge.svg)](https://github.com/kreyo-io/nif-validator/actions)
[![NuGet](https://img.shields.io/nuget/v/Kreyo.NifValidator?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Kreyo.NifValidator)
[![npm](https://img.shields.io/npm/v/@kreyo/nif-validator?logo=npm&label=npm)](https://www.npmjs.com/package/@kreyo/nif-validator)
[![PyPI](https://img.shields.io/pypi/v/kreyo-nif-validator?logo=pypi&label=PyPI)](https://pypi.org/project/kreyo-nif-validator/)

---

## Por qué existe esta librería

Validar un NIF español correctamente tiene más miga de la que parece. El algoritmo tiene tres variantes (DNI, NIE, CIF), el dígito de control del CIF sigue reglas específicas según el tipo de organización, y la mayoría de snippets que circulan por Stack Overflow están incompletos o tienen bugs sutiles.

Esto es una implementación de referencia, idéntica en los tres lenguajes, verificada con los mismos vectores, y testeada contra CIFs reales públicos de empresas españolas (Banco Santander, BBVA, Inditex, Telefónica, Repsol, Iberdrola).

Si construyes software que toca identificadores fiscales españoles — facturación, contabilidad, KYC, e-commerce — esta librería es para ti.

## Instalación

### .NET (multi-target: net8.0 + netstandard2.0)

```bash
dotnet add package Kreyo.NifValidator
```

### TypeScript / JavaScript

```bash
npm install @kreyo/nif-validator
```

### Python (3.10+)

```bash
pip install kreyo-nif-validator
```

## Uso

### .NET

```csharp
using Kreyo.NifValidator;

NifValidator.IsValid("12345678Z");          // true
NifValidator.IsValid("X1234567L");          // true (NIE)
NifValidator.IsValid("A39000013");          // true (Banco Santander)
NifValidator.IsValid("12345678A");          // false (letra de control incorrecta)

NifValidator.Normalize(" 12345678-z ");     // "12345678Z"
NifValidator.GetNifType("X1234567L");       // "NIE"
NifValidator.GetNifType("invalido");        // null

// K, L, M: NIF de persona física asignados por la AEAT. Solo se comprueba el formato (ver abajo)
NifValidator.IsValid("K1234567L");          // true
NifValidator.IsValid("K12345674");          // false (el carácter de verificación debe ser una letra)
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
getNifType("invalido");         // null

isValid("K1234567L");           // true (K/L/M: solo formato, ver abajo)
isValid("K12345674");           // false (el carácter de verificación debe ser una letra)
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
get_nif_type("invalido")        # None

is_valid("K1234567L")           # True (K/L/M: solo formato, ver abajo)
is_valid("K12345674")           # False (el carácter de verificación debe ser una letra)
```

## Qué se valida

| Tipo | Formato | Ejemplo | Notas |
|------|---------|---------|-------|
| **DNI** | 8 dígitos + letra de control | `12345678Z` | Ciudadanos españoles |
| **NIE** | X/Y/Z + 7 dígitos + letra | `X1234567L` | Residentes extranjeros |
| **CIF** | Letra + 7 dígitos + letra o dígito | `A39000013` | Empresas y otras entidades |

Letras de tipo CIF y sus reglas de control:

| Tipo | Significado | Control |
|------|-------------|---------|
| A | Sociedad Anónima | solo dígito |
| B | Sociedad Limitada | solo dígito |
| C | Sociedad Colectiva | dígito o letra |
| D | Sociedad Comanditaria | dígito o letra |
| E | Comunidad de Bienes | solo dígito |
| F | Cooperativa | dígito o letra |
| G | Asociación | dígito o letra |
| H | Comunidad de Propietarios | solo dígito |
| J | Sociedad Civil | dígito o letra |
| N | No residente | solo letra |
| P | Corporación local | solo letra |
| Q | Organismo público | solo letra |
| R | Religiosa | solo letra |
| S | Órgano de la Administración | solo letra |
| U | Unión Temporal de Empresas | dígito o letra |
| V | Otros tipos | dígito o letra |
| W | Establecimiento permanente extranjero | solo letra |

### K, L, M: NIF de persona física (solo se comprueba el formato)

K, L y M son NIF **vigentes** de persona física, asignados por la AEAT (Real Decreto 1065/2007, arts. 19.2 y 20.2):

| Letra | Quién |
|-------|-------|
| K | Españoles menores de 14 años residentes en España |
| L | Españoles residentes en el extranjero |
| M | Extranjeros sin NIE |

El formato oficial es letra inicial + 7 caracteres alfanuméricos + 1 carácter de verificación alfabético. **El algoritmo de ese carácter de verificación no está publicado** en el BOE ni por la AEAT, así que esta librería no se inventa uno: comprueba solo el formato (`[KLM][0-9A-Z]{7}[A-Z]`). `K1234567L` se acepta; `K12345674` se rechaza porque el carácter de verificación debe ser una letra. Consecuencia: un NIF K/L/M con la letra de verificación incorrecta se acepta. `getNifType` / `get_nif_type` / `GetNifType` devuelven `"KLM"` para estos NIF.

### La letra `T`

`T` no está soportada. No hemos encontrado ninguna fuente oficial que defina `T` como letra inicial de un NIF, así que se rechaza. Es "no soportada / no verificable", no "deprecada".

### La regla letra-vs-dígito del CIF es una convención

La tabla anterior (qué tipos de CIF llevan dígito, letra o cualquiera) es una **convención de mercado**, no una regla oficial. La normativa solo habla de "un carácter de control" (Orden EHA/451/2008) y no dice si es letra o dígito; una comunicación de la AEAT de 2008, que solo hemos podido localizar como transcripción de terceros, menciona control numérico para algunas letras nuevas (J, U, V) y alfabético para otras (R, W), pero no hay una tabla oficial completa. [python-stdnum](https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/es/cif.py) acepta ambos para todos los tipos, y su autor reconoce que la información disponible es contradictoria. Esta librería mantiene la convención más estricta; si no puedes permitirte rechazar una empresa real, trata un CIF rechazado solo por letra-vs-dígito como un aviso.

### Opción obsoleta `includeDeprecated`

`includeDeprecated` / `IncludeDeprecated` / `include_deprecated` está **obsoleta y no tiene efecto desde la 0.2.0**. Se sigue aceptando para que el código existente compile y funcione. Antes de la 0.2.0 activaba K, L, M, T con el algoritmo del CIF; ese comportamiento se basaba en una afirmación errónea (ver el [changelog](CHANGELOG.md)).

### Fuentes

- [Real Decreto 1065/2007 (BOE-A-2007-15984)](https://www.boe.es/buscar/act.php?id=BOE-A-2007-15984): arts. 19-22, composición del NIF
- [Orden EHA/451/2008 (BOE-A-2008-3580)](https://www.boe.es/buscar/act.php?id=BOE-A-2008-3580&tn=1): NIF de personas jurídicas (no menciona K, L, M ni T)
- [AEAT, composición del NIF: personas físicas](https://sede.agenciatributaria.gob.es/Sede/ayuda/manuales-videos-folletos/manuales-practicos/guia-practica-cumplimentacion-modelo-censal-036/anexos/anexo-01-solicitud-nif-documentacion-aportar/informacion-sobre-numero-identificacion-fiscal/composicion-nif/personas-fisicas.html)
- [AEAT, composición del NIF: personas jurídicas y entidades](https://sede.agenciatributaria.gob.es/Sede/ayuda/manuales-videos-folletos/manuales-practicos/guia-practica-cumplimentacion-modelo-censal-036/anexos/anexo-01-solicitud-nif-documentacion-aportar/informacion-sobre-numero-identificacion-fiscal/composicion-nif/personas-juridicas-entidades.html)

## Lo que esta librería NO hace

- **No comprueba si un NIF está dado de alta en AEAT.** Esta es una validación sintáctica y de dígito de control. Un NIF puede ser matemáticamente válido y no existir en el censo fiscal. Para verificar si un NIF está activo, consulta [VIES](https://ec.europa.eu/taxation_customs/vies/) (NIFs intracomunitarios) o los servicios de AEAT directamente.
- **No valida nombre ni dirección de la empresa.** Solo el formato del identificador.
- **No detecta NIFs "fantasma"** (matemáticamente válidos pero conocidos como no usados). Eso es responsabilidad de reglas de negocio, no de validación de formato.

## API

Las tres implementaciones exponen las mismas tres funciones:

| Función | Devuelve | Lanza |
|---------|----------|-------|
| `isValid(nif, options?)` | `boolean` | nunca |
| `normalize(nif)` | `string` canónico | si es inválido |
| `getNifType(nif)` | `"DNI"` \| `"NIE"` \| `"CIF"` \| `null` | nunca |

Espacios en blanco (espacios, tabs, saltos de línea) y guiones se ignoran. La entrada no distingue mayúsculas/minúsculas.

## Detalles del algoritmo

Para detalles técnicos del algoritmo de validación (cálculo de letra del DNI, mapeo de prefijo NIE, cálculo del dígito/letra de control del CIF), ver [docs/algorithm.md](docs/algorithm.md).

## Hecho por Kreyo

Esta librería es parte de [Kreyo](https://kreyo.io), una plataforma developer-first de APIs para facturación electrónica española: VeriFactu, FacturaE, extracción de facturas con IA y generación de PDFs.

Si construyes software que necesita compliance fiscal español, también te puede interesar:

- **[Kreyo VeriFactu](https://kreyo.io/verifactu)** — genera, firma y envía registros a AEAT en tiempo real
- **[Kreyo FacturaE](https://kreyo.io/facturae)** — genera XML FacturaE 3.2.2 firmado para B2G y B2B
- **[Kreyo Extract](https://kreyo.io/extract)** — extrae datos estructurados de facturas recibidas con IA
- **[Kreyo PDF](https://kreyo.io/pdf)** — genera facturas y documentos como PDF

Y los repos open-source hermanos del toolkit de Kreyo:

- **[verifactu-hash-calculator](https://github.com/kreyo-io/verifactu-hash-calculator)** — calcula la cadena de huella SHA-256 de VeriFactu para registros de la AEAT
- **[facturae-examples](https://github.com/kreyo-io/facturae-examples)** — ejemplos reales de XML FacturaE 3.2.2 válidos contra el XSD, con su JSON de entrada

## Contribuir

Issues y PRs son bienvenidos. Ver [CONTRIBUTING.md](CONTRIBUTING.md).

## Licencia

[MIT](LICENSE) © Kreyo
