#!/usr/bin/env bash

dotnet build src/Dock.Model/Dock.Model.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet publish src/Dock.Model/Dock.Model.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Model.UnitTests/Dock.Model.UnitTests.csproj -c Release -f netcoreapp2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Model.UnitTests/Dock.Model.UnitTests.csproj -c Release -f netcoreapp2.1
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet build src/Dock.Model.INPC/Dock.Model.INPC.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet publish src/Dock.Model.INPC/Dock.Model.INPC.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Model.INPC.UnitTests/Dock.Model.INPC.UnitTests.csproj -c Release -f netcoreapp2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Model.INPC.UnitTests/Dock.Model.INPC.UnitTests.csproj -c Release -f netcoreapp2.1
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet build src/Dock.Model.ReactiveUI/Dock.Model.ReactiveUI.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet publish src/Dock.Model.ReactiveUI/Dock.Model.ReactiveUI.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Model.ReactiveUI.UnitTests/Dock.Model.ReactiveUI.UnitTests.csproj -c Release -f netcoreapp2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Model.ReactiveUI.UnitTests/Dock.Model.ReactiveUI.UnitTests.csproj -c Release -f netcoreapp2.1
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet build src/Dock.Serializer/Dock.Serializer.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet publish src/Dock.Serializer/Dock.Serializer.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Serializer.UnitTests/Dock.Serializer.UnitTests.csproj -c Release -f netcoreapp2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Serializer.UnitTests/Dock.Serializer.UnitTests.csproj -c Release -f netcoreapp2.1
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet build src/Dock.Avalonia/Dock.Avalonia.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet publish src/Dock.Avalonia/Dock.Avalonia.csproj -c Release -f netstandard2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Avalonia.UnitTests/Dock.Avalonia.UnitTests.csproj -c Release -f netcoreapp2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet test tests/Dock.Avalonia.UnitTests/Dock.Avalonia.UnitTests.csproj -c Release -f netcoreapp2.1
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet build samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.0
if [ $? -ne 0 ]; then
    exit 1
fi

dotnet build samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.1
if [ $? -ne 0 ]; then
    exit 1
fi
