# Copies para promocionar nif-validator

Estos son los textos para los distintos canales. Adapta según contexto, no copies tal cual — el algoritmo de Reddit/LinkedIn detecta texto idéntico y baja alcance.

---

## r/spaindev

**Título sugerido**:
```
[OSS] nif-validator: validación de NIF/CIF/NIE en .NET, TS y Python con tests sobre CIFs reales (Santander, BBVA, Inditex...)
```

**Cuerpo**:
```
Hola a todos,

He publicado una librería para validar NIFs españoles que llevaba un tiempo escribiendo para Kreyo (la plataforma de APIs de facturación electrónica que estoy montando). La idea era tener una implementación de referencia limpia, sin dependencias, idéntica en los tres lenguajes que más usamos.

Lo que aporta vs lo que ya hay:

- Las tres variantes (DNI, NIE, CIF) con todas las reglas de control según tipo de letra de CIF (P/Q/R/S/N/W solo letra, A/B/E/H solo dígito, C/D/F/G/J/U/V ambos).
- Tests sobre CIFs públicos reales de empresas grandes (Banco Santander, BBVA, Inditex, Telefónica, Repsol, Iberdrola) — para que sepas que no es solo el algoritmo cuadrando consigo mismo.
- Manejo correcto de tipos deprecados K/L/M/T con flag `includeDeprecated` (off por defecto).
- Normaliza espacios (incluyendo tabs y saltos de línea), guiones, mayúsculas/minúsculas.
- Misma API en los tres lenguajes: isValid, normalize, getNifType.

Repo: https://github.com/kreyo-io/nif-validator

Está publicado en npm, NuGet y PyPI con MIT license.

Disclosure: Lo construyo yo y forma parte del ecosistema OSS de Kreyo, pero esto es genuinamente útil per se y no requiere usar Kreyo para nada. Si os ahorra una tarde de trabajo o evita el típico bug del CIF de empresa real que rechaza un validador roto, ya merece la pena.

Feedback bienvenido. Si encontráis algún caso que no maneja, abrid un issue.
```

---

## r/programacion

Mismo cuerpo pero con título un poco distinto:

**Título sugerido**:
```
Publiqué una librería OSS para validar NIF/CIF/NIE en .NET, TypeScript y Python (con tests sobre empresas reales)
```

---

## LinkedIn (post building-in-public)

Versión un poco más personal y con narrativa:

```
He publicado el primer repo OSS de Kreyo: nif-validator.

Validar un NIF español parece sencillo hasta que tu helper rechaza el CIF de tu cliente más grande. Y eso pasa porque la mayoría de snippets que circulan ignoran las reglas según tipo de letra del CIF (algunos tipos solo aceptan letra de control, otros solo dígito, otros ambos), o no normalizan bien el whitespace, o no manejan los tipos deprecados K/L/M/T que siguen vivos en bases de datos legacy.

He publicado una implementación de referencia en .NET, TypeScript y Python. Sin dependencias. Misma API en los tres lenguajes. Tests con CIFs reales de Banco Santander, BBVA, Inditex, Telefónica, Repsol e Iberdrola para verificar que el algoritmo funciona contra el mundo real, no solo contra fixtures que me he inventado yo.

Es la primera pieza de una serie de repos OSS que van a salir en las próximas semanas dentro de Kreyo. La próxima es probablemente la más útil del lote: la implementación de referencia del cálculo de huella SHA-256 de VeriFactu, con los vectores de prueba oficiales de AEAT. Esa sí que falta en el ecosistema.

Por qué publicamos esto gratis: porque cuando construyes software de facturación española, va a tocarte validar NIFs sí o sí, y un buen repo OSS te ahorra una tarde. Lo que cobramos en Kreyo son las cosas grandes — VeriFactu, FacturaE, extracción con IA, generación de PDFs.

Repo: github.com/kreyo-io/nif-validator
MIT, contributions welcome.

#OpenSource #Spain #DeveloperTools #VeriFactu #FacturacionElectronica
```

---

## X (Twitter)

Thread corto:

```
1/ Acabo de publicar el primer repo OSS de @kreyo_io: nif-validator.

Validación de NIF/NIE/CIF español en .NET, TypeScript y Python.

Sin dependencias. Misma API en los 3 lenguajes. MIT.

github.com/kreyo-io/nif-validator
```

```
2/ Por qué otro validador de NIF si Stack Overflow está lleno de snippets?

Porque la mayoría de snippets:
- Se saltan reglas del CIF según tipo de letra (P/Q/R/S/N/W solo letra, A/B/E/H solo dígito...)
- No normalizan whitespace
- Ignoran tipos deprecados K/L/M/T que siguen vivos en BBDD legacy
```

```
3/ La verificación más honesta: tests sobre CIFs PÚBLICOS de empresas reales.

Banco Santander, BBVA, Inditex, Telefónica, Repsol, Iberdrola — los 6 pasan.

Si tu validador inventado rechaza alguno o acepta uno con dígito incorrecto, tienes un bug.
```

```
4/ Esto es el warm-up de la serie OSS de Kreyo.

La próxima pieza es la que más falta hacía: implementación de referencia del cálculo de huella SHA-256 de VeriFactu con los vectores oficiales de AEAT.

Sale la semana que viene.
```

---

## Hacker News (cuando publiquemos verifactu-hash-calculator)

Para nif-validator NO recomiendo HN — el público internacional no entiende el problema y va a comentar "this is just a regex". Reservar HN para el repo del hash de VeriFactu, donde el ángulo "Spain's new real-time tax reporting system" sí tiene gancho internacional.

---

## Cuándo publicar cada cosa

**Mismo día (lunes ideal, 9:00-11:00 hora España)**:
1. Push del repo a GitHub
2. Publicar en npm, NuGet, PyPI
3. Publicar post en kreyo.io/blog
4. Submit a r/spaindev (mañana)
5. Post en LinkedIn (media mañana)
6. Thread en X (mediodía)

**Día después** (martes):
- Submit a r/programacion (no el mismo día que r/spaindev — los algoritmos detectan cross-post agresivo)
- Compartir el post en grupos relevantes de WhatsApp/Telegram de developers (si tienes alguno)

**Una semana después**:
- Si fue bien, post en kreyo.io/blog: "Una semana después de publicar nif-validator: stars, feedback, y cosas que aprendimos"
- Si fue regular, doblar esfuerzo en LinkedIn y empezar a preparar el siguiente repo

NO HACER:
- No abusar de los foros: 1 submit por sub, no más.
- No re-postear el mismo contenido en LinkedIn al cabo de 3 días.
- No mencionar Kreyo cada 5 minutos en los comentarios. Si alguien pregunta, contestas; si no, no fuerzas.
