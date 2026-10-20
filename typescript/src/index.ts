/**
 * Validates Spanish tax identification numbers (DNI, NIE, CIF, and K/L/M natural-person NIFs).
 * Pure functions, no dependencies, tree-shakeable.
 */

const DNI_LETTERS = "TRWAGMYFPDXBNJZSQVHLCKE";
const CIF_CONTROL_LETTERS = "JABCDEFGHI";
// CIF letter/digit control split: market CONVENTION, not an official rule (BOE/AEAT only
// say "a control character"; python-stdnum accepts both).
const CIF_LETTER_ONLY = "PQRSNW";
const CIF_DIGIT_ONLY = "ABEH";
const CIF_BOTH_ACCEPTED = "CDFGJUV";
const KLM_BODY = /^[0-9A-Z]{7}$/;
const KLM_CONTROL = /^[A-Z]$/;

export interface NifValidatorOptions {
  /**
   * @deprecated No effect since 0.2.0. K, L and M NIFs (natural persons, RD 1065/2007
   * arts. 19.2 and 20.2) are accepted by default (format only); T has no official basis
   * and is rejected. Kept so existing callers still compile.
   */
  includeDeprecated?: boolean;
}

/** "KLM" = natural-person NIF assigned by AEAT (initial letter K, L or M). */
export type NifType = "DNI" | "NIE" | "CIF" | "KLM";

/**
 * Validates a Spanish NIF (DNI, NIE, CIF, or K/L/M).
 * Whitespace and hyphens are ignored. Case-insensitive.
 * The options argument is accepted for backward compatibility and ignored.
 */
export function isValid(
  nif: string | null | undefined,
  _options: NifValidatorOptions = {}
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
  if (isKlm(first)) {
    return validateKlm(clean);
  }
  return false;
}

/**
 * Returns the canonical form of a valid NIF (uppercase, no separators).
 * Throws if the NIF is invalid.
 */
export function normalize(nif: string | null | undefined): string {
  const clean = cleanInput(nif);
  if (clean === null || !isValid(clean)) {
    throw new Error(`Invalid NIF: ${nif}`);
  }
  return clean;
}

/**
 * Returns the type of a NIF: "DNI", "NIE", "CIF", "KLM", or null if invalid.
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
      CIF_BOTH_ACCEPTED.includes(first)) &&
    validateCif(clean)
  )
    return "CIF";
  if (isKlm(first) && validateKlm(clean)) return "KLM";
  return null;
}

// ----- Internal helpers -----

function cleanInput(input: string | null | undefined): string | null {
  if (input === null || input === undefined) return null;
  if (input.trim() === "") return null;

  let out = "";
  for (const c of input) {
    if (c === "-" || /\s/.test(c)) continue;
    // ASCII-only upper-casing: Unicode case mappings must not turn non-ASCII
    // letters into A-Z (e.g. "ſ" -> "S").
    out += c >= "a" && c <= "z" ? c.toUpperCase() : c;
  }
  return out.length === 9 ? out : null;
}

function isDigit(c: string): boolean {
  return c >= "0" && c <= "9";
}

function isKlm(c: string): boolean {
  return c === "K" || c === "L" || c === "M";
}

// K/L/M: NIF of a natural person assigned by AEAT (RD 1065/2007, arts. 19.2 and 20.2):
// initial letter + 7 alphanumeric characters + alphabetic verification character.
// The verification algorithm is not published in BOE/AEAT, so only the format is checked.
function validateKlm(clean: string): boolean {
  return KLM_BODY.test(clean.slice(1, 8)) && KLM_CONTROL.test(clean[8]);
}

function validateDni(clean: string): boolean {
  const body = clean.slice(0, 8);
  if (!/^[0-9]{8}$/.test(body)) return false;
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
  if (!/^[0-9]{7}$/.test(body)) return false;

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
  // Both accepted (CDFGJUV)
  return control === checkLetter || control === String(checkDigit);
}
