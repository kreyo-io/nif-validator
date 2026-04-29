import { describe, it, expect } from "vitest";
import { isValid, normalize, getNifType } from "../src/index.js";

describe("isValid - DNI", () => {
  it.each([
    "12345678Z",
    "00000000T",
    "99999999R",
    "00000001R",
    "23456789D",
    "87654321X",
  ])("validates %s as valid DNI", (nif) => {
    expect(isValid(nif)).toBe(true);
  });

  it.each(["12345678A", "1234567Z", "123456789Z", "ABCDEFGHI"])(
    "rejects %s as invalid DNI",
    (nif) => {
      expect(isValid(nif)).toBe(false);
    }
  );

  it.each([
    "12345678-Z",
    "12345678 Z",
    "12345678z",
    " 12345678Z ",
    "12345678\tZ",
    "12345678\nZ",
    "  12345678  Z  ",
  ])("normalizes separators in %s", (nif) => {
    expect(isValid(nif)).toBe(true);
  });
});

describe("isValid - NIE", () => {
  it.each(["X1234567L", "Y1234567X", "Z1234567R"])(
    "validates %s as valid NIE",
    (nif) => {
      expect(isValid(nif)).toBe(true);
    }
  );

  it.each(["X1234567A", "X12A4567L", "W1234567L"])(
    "rejects %s as invalid NIE",
    (nif) => {
      expect(isValid(nif)).toBe(false);
    }
  );
});

describe("isValid - CIF digit-only types (A, B, E, H)", () => {
  it.each(["A12345674", "B12345674", "E12345674", "H12345674"])(
    "validates %s",
    (nif) => {
      expect(isValid(nif)).toBe(true);
    }
  );
});

describe("isValid - CIF letter-only types (N, P, Q, R, S, W)", () => {
  it.each(["N1234567D", "P1234567D", "Q1234567D", "R1234567D", "S1234567D", "W1234567D"])(
    "validates %s",
    (nif) => {
      expect(isValid(nif)).toBe(true);
    }
  );
});

describe("isValid - CIF both-accepted types (C, D, F, G, J, U, V)", () => {
  it.each([
    "C1234567D",
    "C12345674",
    "D1234567D",
    "D12345674",
    "F1234567D",
    "F12345674",
    "G1234567D",
    "G12345674",
    "J1234567D",
    "J12345674",
    "U1234567D",
    "U12345674",
    "V1234567D",
    "V12345674",
  ])("validates %s", (nif) => {
    expect(isValid(nif)).toBe(true);
  });
});

describe("isValid - CIF invalid", () => {
  it.each(["A12345670", "P1234567A", "A123456789", "I12345678", "O12345678"])(
    "rejects %s",
    (nif) => {
      expect(isValid(nif)).toBe(false);
    }
  );
});

describe("isValid - real public Spanish company CIFs", () => {
  it.each([
    ["A39000013", "Banco Santander"],
    ["A48265169", "BBVA"],
    ["A15075062", "Inditex"],
    ["A82018474", "Telefónica de España"],
    ["A78374725", "Repsol"],
    ["A48010615", "Iberdrola"],
  ])("validates %s (%s)", (nif) => {
    expect(isValid(nif)).toBe(true);
  });
});

describe("isValid - deprecated K, L, M, T", () => {
  it.each(["K12345674", "L12345674", "M12345674", "T12345674"])(
    "rejects %s by default",
    (nif) => {
      expect(isValid(nif)).toBe(false);
    }
  );

  it.each(["K12345674", "L12345674", "M12345674", "T12345674"])(
    "accepts %s with includeDeprecated",
    (nif) => {
      expect(isValid(nif, { includeDeprecated: true })).toBe(true);
    }
  );
});

describe("isValid - edge cases", () => {
  it.each([null, undefined, "", "   ", "12345", "1234567890"])(
    "rejects %s",
    (nif) => {
      expect(isValid(nif as string | null | undefined)).toBe(false);
    }
  );
});

describe("normalize", () => {
  it.each([
    ["12345678-z", "12345678Z"],
    [" 12345678 Z ", "12345678Z"],
    ["a-12345674", "A12345674"],
    ["X 1234567 L", "X1234567L"],
  ])("normalizes %s to %s", (input, expected) => {
    expect(normalize(input)).toBe(expected);
  });

  it.each(["12345678A", "invalid", null, ""])("throws on %s", (input) => {
    expect(() => normalize(input as string | null)).toThrow();
  });
});

describe("getNifType", () => {
  it.each([
    ["12345678Z", "DNI"],
    ["X1234567L", "NIE"],
    ["A12345674", "CIF"],
    ["invalid", null],
    [null, null],
  ])("returns %s -> %s", (input, expected) => {
    expect(getNifType(input as string | null)).toBe(expected);
  });
});
