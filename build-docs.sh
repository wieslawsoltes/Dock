#!/bin/bash
export IsDocFx=true
dotnet build -c Release 
dotnet tool restore
dotnet docfx docfx/docfx.json
rm -rf src/Dock.Model.ReactiveUI/Generated
rm -rf src/Dock.Model.ReactiveUI.Services/Generated
rm -rf src/Dock.Model.ReactiveUI.Services.Avalonia/Generated
