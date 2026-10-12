#!/bin/bash
set -euo pipefail

cd -- "$(dirname -- "${BASH_SOURCE[0]}")"
dotnet tool restore
dotnet msbuild build/Documentation.proj -m -nologo
dotnet docfx docfx/docfx.json
