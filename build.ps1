
$VersionSuffix = $env:APPVEYOR_BUILD_VERSION
$MyGetApiKey = $env:MYGET_API_KEY
$MyGetApiUrl = $env:MYGET_API_URL

function Execute ($cmd) 
{ 
    Invoke-Expression $cmd
    if ($LastExitCode -ne 0) { Exit 1 } 
}

$BuildCoreCommands = @(
    "dotnet build src/Dock.Model/Dock.Model.csproj -c Release -f netstandard2.0 --version-suffix -build$VersionSuffix",
    "dotnet build src/Dock.Model/Dock.Model.csproj -c Release -f net461 --version-suffix -build$VersionSuffix",
    "dotnet build src/Dock.Model.INPC/Dock.Model.INPC.csproj -c Release -f netstandard2.0 --version-suffix -build$VersionSuffix",
    "dotnet build src/Dock.Model.INPC/Dock.Model.INPC.csproj -c Release -f net461 --version-suffix -build$VersionSuffix",
    "dotnet build src/Dock.Model.ReactiveUI/Dock.Model.ReactiveUI.csproj -c Release -f netstandard2.0 --version-suffix -build$VersionSuffix",
    "dotnet build src/Dock.Model.ReactiveUI/Dock.Model.ReactiveUI.csproj -c Release -f net461 --version-suffix -build$VersionSuffix",
    "dotnet build src/Dock.Serializer/Dock.Serializer.csproj -c Release -f netstandard2.0 --version-suffix -build$VersionSuffix",
    "dotnet build src/Dock.Serializer/Dock.Serializer.csproj -c Release -f net461 --version-suffix -build$VersionSuffix",
    "dotnet build src/Dock.Avalonia/Dock.Avalonia.csproj -c Release -f netstandard2.0 --version-suffix -build$VersionSuffix",
    "dotnet build src/Dock.Avalonia/Dock.Avalonia.csproj -c Release -f net461 --version-suffix -build$VersionSuffix"
)

$BuildSamplesCommands = @(
    "dotnet build samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.0 --version-suffix -build$VersionSuffix",
    "dotnet build samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.1 --version-suffix -build$VersionSuffix",
    "dotnet build samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f net461 --version-suffix -build$VersionSuffix"
)

$PackCommands = @(
    "dotnet pack src/Dock.Model/Dock.Model.csproj -c Release --version-suffix -build$VersionSuffix",
    "dotnet pack src/Dock.Model.INPC/Dock.Model.INPC.csproj -c Release --version-suffix -build$VersionSuffix",
    "dotnet pack src/Dock.Model.ReactiveUI/Dock.Model.ReactiveUI.csproj -c Release --version-suffix -build$VersionSuffix",
    "dotnet pack src/Dock.Serializer/Dock.Serializer.csproj -c Release --version-suffix -build$VersionSuffix",
    "dotnet pack src/Dock.Avalonia/Dock.Avalonia.csproj -c Release --version-suffix -build$VersionSuffix"
)

$TestCommands = @(
    "dotnet test tests/Dock.Model.INPC.UnitTests/Dock.Model.INPC.UnitTests.csproj -c Release -f netcoreapp2.0",
    "dotnet test tests/Dock.Model.INPC.UnitTests/Dock.Model.INPC.UnitTests.csproj -c Release -f netcoreapp2.1",
    "dotnet test tests/Dock.Model.INPC.UnitTests/Dock.Model.INPC.UnitTests.csproj -c Release -f net461",
    "dotnet test tests/Dock.Model.UnitTests/Dock.Model.UnitTests.csproj -c Release -f netcoreapp2.0",
    "dotnet test tests/Dock.Model.UnitTests/Dock.Model.UnitTests.csproj -c Release -f netcoreapp2.1",
    "dotnet test tests/Dock.Model.UnitTests/Dock.Model.UnitTests.csproj -c Release -f net461",
    "dotnet test tests/Dock.Model.ReactiveUI.UnitTests/Dock.Model.ReactiveUI.UnitTests.csproj -c Release -f netcoreapp2.0",
    "dotnet test tests/Dock.Model.ReactiveUI.UnitTests/Dock.Model.ReactiveUI.UnitTests.csproj -c Release -f netcoreapp2.1",
    "dotnet test tests/Dock.Model.ReactiveUI.UnitTests/Dock.Model.ReactiveUI.UnitTests.csproj -c Release -f net461",
    "dotnet test tests/Dock.Serializer.UnitTests/Dock.Serializer.UnitTests.csproj -c Release -f netcoreapp2.0",
    "dotnet test tests/Dock.Serializer.UnitTests/Dock.Serializer.UnitTests.csproj -c Release -f netcoreapp2.1",
    "dotnet test tests/Dock.Serializer.UnitTests/Dock.Serializer.UnitTests.csproj -c Release -f net461",
    "dotnet test tests/Dock.Avalonia.UnitTests/Dock.Avalonia.UnitTests.csproj -c Release -f netcoreapp2.0",
    "dotnet test tests/Dock.Avalonia.UnitTests/Dock.Avalonia.UnitTests.csproj -c Release -f netcoreapp2.1",
    "dotnet test tests/Dock.Avalonia.UnitTests/Dock.Avalonia.UnitTests.csproj -c Release -f net461"
)

$PublishCommands = @(
    "dotnet publish samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.0 -r win7-x64 --version-suffix -build$VersionSuffix",
    "dotnet publish samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.1 -r win7-x64 --version-suffix -build$VersionSuffix",
    "dotnet publish samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.0 -r ubuntu.14.04-x64 --version-suffix -build$VersionSuffix",
    "dotnet publish samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.1 -r ubuntu.14.04-x64 --version-suffix -build$VersionSuffix",
    "dotnet publish samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.0 -r debian.8-x64 --version-suffix -build$VersionSuffix",
    "dotnet publish samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.1 -r debian.8-x64 --version-suffix -build$VersionSuffix",
    "dotnet publish samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.0 -r osx.10.12-x64 --version-suffix -build$VersionSuffix",
    "dotnet publish samples/AvaloniaDemo/AvaloniaDemo.csproj -c Release -f netcoreapp2.1 -r osx.10.12-x64 --version-suffix -build$VersionSuffix"
)

ForEach ($cmd in $BuildCoreCommands) { Execute $cmd }
ForEach ($cmd in $BuildSamplesCommands) { Execute $cmd }
ForEach ($cmd in $PackCommands) { Execute $cmd }
ForEach ($cmd in $TestCommands) { Execute $cmd }
ForEach ($cmd in $PublishCommands) { Execute $cmd }

$RedistPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Redist\MSVC\14.14.26405\x64\Microsoft.VC141.CRT"
$RedistDest = "$pwd\samples\AvaloniaDemo\bin\AnyCPU\Release"
$RedistRuntime = "win7-x64"
Copy-Item "$RedistPath\msvcp140.dll" "$RedistDest\netcoreapp2.0\$RedistRuntime\publish"
Copy-Item "$RedistPath\vcruntime140.dll" "$RedistDest\netcoreapp2.0\$RedistRuntime\publish"
Copy-Item "$RedistPath\msvcp140.dll" "$RedistDest\netcoreapp2.1\$RedistRuntime\publish"
Copy-Item "$RedistPath\vcruntime140.dll" "$RedistDest\netcoreapp2.1\$RedistRuntime\publish"

$NuGetCommands = @(
    "dotnet nuget push src\Dock.Model\bin\AnyCPU\Release\*.nupkg -s $MyGetApiUrl -k $MyGetApiKey",
    "dotnet nuget push src\Dock.Model.INPC\bin\AnyCPU\Release\*.nupkg -s $MyGetApiUrl -k $MyGetApiKey",
    "dotnet nuget push src\Dock.Model.ReactiveUI\bin\AnyCPU\Release\*.nupkg -s $MyGetApiUrl -k $MyGetApiKey",
    "dotnet nuget push src\Dock.Serializer\bin\AnyCPU\Release\*.nupkg -s $MyGetApiUrl -k $MyGetApiKey",
    "dotnet nuget push src\Dock.Avalonia\bin\AnyCPU\Release\*.nupkg -s $MyGetApiUrl -k $MyGetApiKey"
)

if ($MyGetApiKey -And $MyGetApiUrl) {
    ForEach ($cmd in $NuGetCommands) { Execute $cmd }
}
