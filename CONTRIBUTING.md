# Contributing

Thanks for considering a contribution to nif-validator!

## How to contribute

- **Bug reports**: open an issue with a clear title, expected vs actual behavior, and a minimal reproduction (an input string and the language you're using).
- **Feature requests**: open an issue and describe the use case. We're conservative about adding features — this library is meant to stay small and focused on format validation.
- **Pull requests**: see below.

## Submitting a pull request

1. Fork the repo and create a feature branch.
2. Make your change in **all three implementations** (.NET, TypeScript, Python). They must stay in sync — same API surface, same behavior, same fixtures.
3. Add tests in all three languages.
4. Run the test suites locally:
   ```bash
   # .NET
   cd dotnet && dotnet test

   # TypeScript
   cd typescript && npm test

   # Python
   cd python && pytest
   ```
5. Update the README if you change the public API.
6. Open the PR with a clear description of what changed and why.

## Code style

- Keep functions pure. No I/O, no globals, no side effects.
- No external dependencies in the library code (test dependencies are fine).
- Match the existing style of each language. The .NET implementation uses idiomatic C# 12; the TypeScript one uses idiomatic TS with strict types; the Python one uses type hints and modern Python 3.10+.

## Algorithm changes

If you find a bug in the validation algorithm, please:

1. Open an issue first with a clear test case showing the expected behavior.
2. Reference the AEAT documentation or BOE source supporting your claim.
3. After discussion, submit a PR with the fix in all three languages and a regression test in each.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
