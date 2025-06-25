# Contributing to Dock

Thank you for taking the time to contribute to Dock! This guide explains the basic workflow for submitting patches and the coding conventions used in the repository.

## Getting help

Before starting work on a feature or bug fix please open an issue so that we can discuss your proposal. For small fixes feel free to submit a pull request directly.

## Development workflow

1. Fork the repository and create a topic branch from `master`.
2. Make your changes in that branch and commit them with clear messages.
3. Push the branch to your fork and open a pull request against `master`.
4. Your contribution must be licensed under the MIT license.

## Code style

The project uses the settings in `.editorconfig`. In general:

- Use spaces with an indent size of four for C# code.
- Keep a newline at the end of each file.
- Follow the naming rules enforced by the configuration.

Run `dotnet format` if you have it installed to ensure consistent formatting.

## Running tests

Run `dotnet test` from the repository root to execute all unit tests. You can also use `./build.sh` (or `build.cmd` on Windows) to restore, build and test the entire solution.

## Branching model

The `master` branch contains the latest stable code. Feature work should happen in separate branches. Avoid committing directly to `master` in your fork.

We appreciate your interest in improving Dock and look forward to your pull requests!

