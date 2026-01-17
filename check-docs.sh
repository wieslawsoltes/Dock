#!/bin/bash
set -e
dotnet tool restore
dotnet docfx docfx/docfx.json
