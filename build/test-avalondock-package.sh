#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/.."

project=tests/Dock.Avalonia.AvalonDock.PackageTests/Dock.Avalonia.AvalonDock.PackageTests.csproj
# A fresh cache and source mapping ensure every Dock dependency comes from this pack.
package_cache=$(mktemp -d "${TMPDIR:-/tmp}/dock-avalondock-packages.XXXXXX")
trap 'rm -rf "$package_cache"' EXIT

dotnet restore "$project" --configfile tests/Dock.Avalonia.AvalonDock.PackageTests/NuGet.Config --packages "$package_cache"
dotnet build "$project" -c Release -f net8.0 --no-restore
dotnet test "$project" -c Release -f net10.0 --no-restore
dotnet build "$project" -c Release -f net8.0 --no-restore -p:TestGeneratedCatalog=true
dotnet test "$project" -c Release -f net10.0 --no-restore -p:TestGeneratedCatalog=true
