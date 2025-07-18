# Repository Guidelines for Codex Agents

## Scope
These guidelines apply to the entire repository.

## Code Style
- Adhere to the rules in `.editorconfig`.
- Indent C# code with four spaces and keep a newline at the end of each file.
- Prefer `var` when the type is apparent and use language keywords instead of BCL types.
- Name static fields with the `s_` prefix and internal or private fields with the `_` prefix.

## Documentation
- Write documentation in Markdown.
- Use `#` headings with a single space after the hash.
- Place new docs under the `docs/` folder unless another location is more suitable.
- Enclose code samples in fenced blocks and specify the language where possible.

## Workflow
- Create feature branches from `master` when making changes.
- Commit messages should contain a short summary line (max 72 characters) followed by a blank line and an optional body.
- All contributions are licensed under the MIT license as noted in `LICENSE.TXT`.

## Testing
- Run `dotnet test` from the repository root before submitting changes.
- Running `dotnet format` is recommended to ensure consistent formatting.
- Use `./build.sh` (or `build.cmd` on Windows) to restore, build and test everything when needed.
