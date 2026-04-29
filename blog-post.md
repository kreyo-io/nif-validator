---
title: "Validar un NIF español en .NET, JavaScript y Python — el algoritmo, los edge cases, y por qué copiar de Stack Overflow no basta"
date: 2026-04-30
author: Iván
description: "Implementación de referencia del algoritmo de validación de NIF/NIE/CIF en tres lenguajes, con los edge cases que la mayoría de snippets ignoran. Verificada con CIFs reales de empresas españolas."
tags: [validación, nif, cif, españa, opensource]
---

# Validar un NIF español en .NET, JavaScript y Python

Si alguna vez has tenido que validar un NIF en producción, conoces la sensación: parece sencillo, pero acabas con tres helpers distintos repartidos por la base de código, ninguno cubre los mismos casos, y el de la integración con el CRM rechaza CIFs perfectamente válidos de empresas reales.

El problema no es complicado matemáticamente. Es que el algoritmo tiene tres variantes (DNI, NIE, CIF), cada una con sus reglas, y la mayoría de snippets que circulan por Stack Overflow son incompletos, tienen bugs sutiles, o ignoran edge cases que en producción aparecen el primer día.

Hoy publicamos **[nif-validator](https://github.com/kreyo-io/nif-validator)**, una implementación de referencia en .NET, TypeScript y Python. Sin dependencias, con tests sobre CIFs reales públicos de Banco Santander, BBVA, Inditex, Telefónica, Repsol e Iberdrola, y con la misma API en los tres lenguajes.

Este post explica cómo funciona el algoritmo y qué cosas se suelen escapar.

## Qué se valida

Un NIF español tiene siempre 9 caracteres. Hay tres tipos:

| Tipo | Formato | Ejemplo | Para quién |
|------|---------|---------|-----------|
| DNI | 8 dígitos + letra | `12345678Z` | Ciudadanos españoles |
| NIE | X/Y/Z + 7 dígitos + letra | `X1234567L` | Residentes extranjeros |
| CIF | letra + 7 dígitos + dígito o letra | `A39000013` | Empresas y entidades |

A nivel de validación, lo que comprobamos es que el último carácter (la "letra de control" o "dígito de control") cuadra matemáticamente con los anteriores. Vamos por partes.

## DNI: el más simple

El algoritmo del DNI lleva décadas igual:

```
letra = "TRWAGMYFPDXBNJZSQVHLCKE"[número % 23]
```

Donde `número` es la parte numérica de 8 dígitos. Punto.

Por ejemplo, `12345678 mod 23 = 14`, y la posición 14 de la cadena (0-indexada) es `Z`. Por eso `12345678Z` es válido.

¿Qué se suele escapar aquí?

- **Ceros a la izquierda**: `00000001R` es perfectamente válido. Algunos parseadores lo tratan como inválido porque convierten primero a entero y pierden los ceros.
- **Whitespace inesperado**: si tu input viene de un formulario o de un CSV, igual te llega `"12345678\tZ"` o `"  12345678  Z  "`. Conviene normalizar todos los espacios en blanco, no solo `' '`.
- **Mayúsculas vs minúsculas**: `12345678z` es el mismo NIF que `12345678Z`. Normaliza a mayúscula.

```python
DNI_LETTERS = "TRWAGMYFPDXBNJZSQVHLCKE"

def validate_dni(clean: str) -> bool:
    body = clean[:8]
    if not body.isdigit():
        return False
    return clean[8] == DNI_LETTERS[int(body) % 23]
```

## NIE: un DNI con prefijo

Para extranjeros residentes, el primer carácter es una letra (X, Y o Z) que se mapea a un dígito y luego se aplica el algoritmo del DNI:

| Prefijo | Mapea a |
|---------|---------|
| X | 0 |
| Y | 1 |
| Z | 2 |

Así que `X1234567L` se convierte mentalmente en `01234567L`, y se valida como DNI: `01234567 mod 23 = 11`, y la posición 11 es `L`. Válido.

```python
def validate_nie(clean: str) -> bool:
    prefix = {"X": "0", "Y": "1", "Z": "2"}.get(clean[0])
    if prefix is None:
        return False
    return validate_dni(prefix + clean[1:])
```

## CIF: aquí es donde la mayoría se equivoca

El CIF es para empresas y entidades. La primera letra indica el tipo de organización, y la última puede ser un dígito o una letra dependiendo de qué letra sea la primera. Esto es lo que la gente suele saltarse.

### Cálculo del dígito de control

Para los 7 dígitos centrales:

1. **Posiciones impares** (1ª, 3ª, 5ª, 7ª): multiplica por 2; si el resultado es mayor que 9, súmale los dos dígitos (equivalente a restar 9). Suma todos.
2. **Posiciones pares** (2ª, 4ª, 6ª): suma directamente.
3. **Total** = suma impares + suma pares.
4. **Dígito de control** = `(10 - total mod 10) mod 10` → un dígito de 0 a 9.
5. **Letra de control** = `"JABCDEFGHI"[dígitoControl]` (J=0, A=1, ..., I=9).

### Las reglas según tipo de letra

Esto es lo crítico:

| Letra inicial | Significado | Control |
|--------------|-------------|---------|
| **N, P, Q, R, S, W** | Organismos, religiosas, no residentes | **Solo letra** |
| **A, B, E, H** | SA, SL, comunidades de bienes y propietarios | **Solo dígito** |
| **C, D, F, G, J, U, V** | Cooperativas, asociaciones, etc. | Letra **o** dígito |
| **K, L, M, T** | Tipos deprecados (pre-2008) | Solo si el cliente lo permite |
| **Otras** | Inválido | — |

Si tu validador trata `P1234567A` como válido (porque la letra "A" cuadra con el cálculo del control letter), te has equivocado: el tipo P solo acepta letra, pero la letra correcta para el body `1234567` es `D`, no `A`.

```python
CIF_CONTROL_LETTERS = "JABCDEFGHI"

def validate_cif(clean: str) -> bool:
    first = clean[0]
    body = clean[1:8]
    if not body.isdigit():
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
    check_letter = CIF_CONTROL_LETTERS[check_digit]
    control = clean[8]

    if first in "PQRSNW":  # solo letra
        return control == check_letter
    if first in "ABEH":  # solo dígito
        return control == str(check_digit)
    return control == check_letter or control == str(check_digit)
```

## El edge case de los tipos K, L, M, T

Estos tipos están deprecados desde la Orden EHA/451/2008. AEAT ya no los emite, pero **siguen circulando en bases de datos antiguas**, especialmente en software contable que migra históricos.

¿Los aceptas o los rechazas?

Nuestra postura: **rechazar por defecto, aceptar con flag explícito**.

```ts
isValid("K12345674");                              // false
isValid("K12345674", { includeDeprecated: true }); // true
```

Esto le da al consumidor el control. Si tu app es una integración nueva con producción moderna, déjalo en false. Si estás migrando datos legacy de una gestoría con 30 años de historial, ponlo a true.

## Lo que NO valida (y que mucha gente confunde)

Un NIF puede ser **matemáticamente válido y no existir en AEAT**. Esta librería valida el formato y el dígito de control, no el censo fiscal.

`00000001R` pasa la validación matemática pero no es un NIF real de nadie. `12345678Z` tampoco lo es, aunque sea válido formalmente.

Para verificar si un NIF está activo, hay dos opciones:

1. **VIES** (servicio de la Comisión Europea) para NIFs intracomunitarios. Si el NIF empieza con prefijo de país (FR, DE, IT, ES, etc.), VIES te dice si está dado de alta para operaciones intracomunitarias.
2. **Servicios de AEAT** directamente, lo cual implica gestionar certificado y trámite.

Mezclar ambas validaciones (formato + censo) en la misma función es un error de diseño común. Lo correcto es hacer la validación de formato siempre (es síncrona, sin red, instantánea) y la verificación de censo solo cuando realmente importe (al alta de cliente, en facturación, etc.) y con caché agresivo.

## Tests con empresas reales

Una de las cosas que más nos costó al validar nuestra implementación fue distinguir bugs en el algoritmo de fixtures incorrectos. Es muy fácil "verificar" tu validador con CIFs que tú mismo te has inventado y que pasan porque el algoritmo es consistente consigo mismo.

La forma honesta de validar el algoritmo es probarlo contra **CIFs públicos reales de empresas grandes**. Estos seis pasan validación correctamente:

```
A39000013  Banco Santander
A48265169  BBVA
A15075062  Inditex
A82018474  Telefónica de España
A78374725  Repsol
A48010615  Iberdrola
```

Si tu validador rechaza alguno de estos, tienes un bug. Si tu validador inventado en el equipo acepta `A39000014` (con dígito de control incorrecto) como válido, también.

## La librería

Hemos publicado nif-validator como **monorepo en GitHub** con implementaciones en .NET (multi-target net8.0 + netstandard2.0), TypeScript (ESM + CJS) y Python (3.10+). Las tres comparten exactamente la misma API, los mismos vectores de prueba y la misma semántica.

```bash
# .NET
dotnet add package Kreyo.NifValidator

# TypeScript
npm install @kreyo/nif-validator

# Python
pip install nif-validator
```

Repo: **[github.com/kreyo-io/nif-validator](https://github.com/kreyo-io/nif-validator)**

MIT, sin dependencias, contributors welcome.

## Por qué Kreyo publica esto gratis

Esto es parte de [Kreyo](https://kreyo.io), una plataforma developer-first de APIs para facturación electrónica española: **VeriFactu** (registros AEAT en tiempo real), **FacturaE** (XML 3.2.2 firmado para B2G y B2B), **Extract** (extracción de datos con IA) y **PDF**.

Si construyes software de facturación, va a tocarte validar NIFs. Y va a tocarte hacer cosas mucho más complicadas que validar NIFs: encadenar registros con SHA-256, firmar XML, hablar SOAP con AEAT con mTLS y certificado del cliente, mapear códigos de facturae 3.2.2... esas las hacemos nosotros y las cobramos.

Esta librería es nuestra forma de calentar motores: cosas pequeñas y útiles que cualquiera necesita, publicadas bien. La próxima semana sale el repo del cálculo de huella SHA-256 de VeriFactu — ese sí que es de los que faltan en el ecosistema.

Hasta entonces, si te ahorra una tarde de trabajo, dale ⭐ en GitHub y avisa a un compañero que también la necesite.
