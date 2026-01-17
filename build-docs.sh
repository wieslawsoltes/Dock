#!/bin/bash
export IsDocFx=true
dotnet build -c Release 
dotnet tool restore
dotnet docfx docfx/docfx.json
rm -rf src/Dock.Model.ReactiveUI/Generated
