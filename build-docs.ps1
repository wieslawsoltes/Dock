$env:IsDocFx = 'true'
dotnet tool restore
dotnet build -c Release
dotnet docfx docfx/docfx.json
rm -rf src/Dock.Model.ReactiveUI/Generated
