/**
 * Validates Spanish tax identification numbers (DNI, NIE, CIF).
 * Pure functions, no dependencies, tree-shakeable.
 */

const DNI_LETTERS = "TRWAGMYFPDXBNJZSQVHLCKE";
const CIF_CONTROL_LETTERS = "JABCDEFGHI";
const CIF_LETTER_ONLY = "PQRSNW";
const CIF_DIGIT_ONLY = "ABEH";
const CIF_BOTH_ACCEPTED = "CDFGJUV";
const CIF_DEPRECATED = "KLMT";

export interface NifValidatorOptions {
  /**
   * When true, validates legacy NIF types K, L, M, T (deprecated by AEAT pre-2008).
   * Default: false.
   */
  includeDeprecated?: boolean;
}

export type NifType = "DNI" | "NIE" | "CIF";

/**
 * Validates a Spanish NIF (DNI, NIE, or CIF).
 * Whitespace and hyphens are ignored. Case-insensitive.
 */
export function isValid(
  nif: string | null | undefined,
  options: NifValidatorOptions = {}
): boolean {
  const clean = cleanInput(nif);
  if (clean === null) return false;

  const first = clean[0];

  if (isDigit(first)) {
    return validateDni(clean);
  }
  if (first === "X" || first === "Y" || first === "Z") {
    return validateNie(clean);
  }
  if (
    CIF_LETTER_ONLY.includes(first) ||
    CIF_DIGIT_ONLY.includes(first) ||
    CIF_BOTH_ACCEPTED.includes(first)
  ) {
    return validateCif(clean);
  }
  if (CIF_DEPRECATED.includes(first)) {
    return Boolean(options.includeDeprecated) && validateCif(clean);
  }
  return false;
}

/**
 * Returns the canonical form of a valid NIF (uppercase, no separators).
 * Throws if the NIF is invalid.
 */
export function normalize(nif: string | null | undefined): string {
  const clean = cleanInput(nif);
  if (clean === null || !isValid(clean, { includeDeprecated: true })) {
    throw new Error(`Invalid NIF: ${nif}`);
  }
  return clean;
}

/**
 * Returns the type of a NIF: "DNI", "NIE", "CIF", or null if invalid.
 */
export function getNifType(nif: string | null | undefined): NifType | null {
  const clean = cleanInput(nif);
  if (clean === null) return null;

  const first = clean[0];

  if (isDigit(first) && validateDni(clean)) return "DNI";
  if ((first === "X" || first === "Y" || first === "Z") && validateNie(clean))
    return "NIE";
  if (
    (CIF_LETTER_ONLY.includes(first) ||
      CIF_DIGIT_ONLY.includes(first) ||
      CIF_BOTH_ACCEPTED.includes(first) ||
      CIF_DEPRECATED.includes(first)) &&
    validateCif(clean)
  )
    return "CIF";
  return null;
}

// ----- Internal helpers -----

function cleanInput(input: string | null | undefined): string | null {
  if (input === null || input === undefined) return null;
  if (input.trim() === "") return null;

  let out = "";
  for (const c of input) {
    if (c === "-" || /\s/.test(c)) continue;
    out += c.toUpperCase();
  }
  return out.length === 9 ? out : null;
}

function isDigit(c: string): boolean {
  return c >= "0" && c <= "9";
}

function validateDni(clean: string): boolean {
  const body = clean.slice(0, 8);
  if (!/^\d{8}$/.test(body)) return false;
  const num = parseInt(body, 10);
  return clean[8] === DNI_LETTERS[num % 23];
}

function validateNie(clean: string): boolean {
  const map: Record<string, string> = { X: "0", Y: "1", Z: "2" };
  const prefix = map[clean[0]];
  if (!prefix) return false;
  return validateDni(prefix + clean.slice(1));
}

function validateCif(clean: string): boolean {
  const first = clean[0];
  const body = clean.slice(1, 8);
  if (!/^\d{7}$/.test(body)) return false;

  let sumOdd = 0;
  let sumEven = 0;
  for (let i = 0; i < 7; i++) {
    const digit = parseInt(body[i], 10);
    if (i % 2 === 0) {
      const doubled = digit * 2;
      sumOdd += doubled > 9 ? doubled - 9 : doubled;
    } else {
      sumEven += digit;
    }
  }

  const total = sumOdd + sumEven;
  const checkDigit = (10 - (total % 10)) % 10;
  const checkLetter = CIF_CONTROL_LETTERS[checkDigit];
  const control = clean[8];

  if (CIF_LETTER_ONLY.includes(first)) {
    return control === checkLetter;
  }
  if (CIF_DIGIT_ONLY.includes(first)) {
    return control === String(checkDigit);
  }
  // Both accepted (CDFGJUV) and deprecated (KLMT)
  return control === checkLetter || control === String(checkDigit);
}
