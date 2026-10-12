# Building the documentation

Run `./build-docs.sh` from a checkout with the SDKs listed in `global.json` and
the project target frameworks installed. The script restores the pinned DocFX
tool, builds the primary libraries for net8.0, and generates the complete site
under `_site`. It stops if restoring, compilation, metadata extraction, or site
generation fails.

`build/Documentation.proj` stages each library's own DLL, XML comments, portable
PDB, and dependency manifest under `artifacts/documentation/api`. DocFX reads
the compiled metadata, including properties and commands produced by source
generators. It uses the PDB's Source Link information for source links. This
avoids running project source generators with DocFX's embedded Roslyn version.
No generated source files are added to project source directories.

The primary Avalonia 12 and ReactiveUI packages supply the shared API reference.
The `.v11` and `.Reactive` package variants reuse those type and member names,
so they are not loaded a second time into the same reference catalog. The
System.Text.Json generator project remains a source input because its public
generator API refers to Roslyn itself.

After a successful full build, `dotnet docfx build docfx/docfx.json` refreshes
articles and rendered pages using the existing API metadata. That command alone
does not verify or update API metadata. A full build replaces the staging
directory, generated `docfx/api/*.yml` and `*.manifest` files, and `_site` so
retired APIs cannot remain in the output. It preserves `docfx/api/index.md`.

See [DocFX's assembly-input documentation](https://dotnet.github.io/docfx/docs/dotnet-api-docs.html#generate-from-assemblies)
for metadata and Source Link behavior.
