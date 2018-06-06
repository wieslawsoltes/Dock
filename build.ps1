[CmdletBinding()]
Param(
    [string]$Configuration = "Release",
    [string]$VersionSuffix = $null,
    [string]$ApiKey = $null,
    [string]$ApiUrl = $null,
    [switch]$BuildCore,
    [switch]$TestCore,
    [switch]$PackCore,
    [switch]$BuildSamples,
    [switch]$PublishSamples,
    [switch]$PushNuGet,
    [switch]$CopyRedist
)

if ($VersionSuffix -eq $null) {
    if ($env:APPVEYOR_BUILD_VERSION) {
        $VersionSuffix = "-build" + $env:APPVEYOR_BUILD_VERSION
    }
    else {
        $VersionSuffix = ""
    }
}

if ($PushNuGet -eq $false)
{
    if($env:APPVEYOR_REPO_NAME -eq 'wieslawsoltes/Dock' -And $env:APPVEYOR_REPO_BRANCH -eq 'master') {
        if($env:APPVEYOR_REPO_TAG -eq 'True' -And $env:APPVEYOR_REPO_TAG_NAME) {
            $ApiKey = $env:NUGET_API_KEY
            $ApiUrl = $env:NUGET_API_URL
        }
        else {
            $ApiKey = $env:MYGET_API_KEY
            $ApiUrl = $env:MYGET_API_URL
        }
        $PushNuGet = $true
    }
}

if ($CopyRedist -eq $false) {
    if ($env:APPVEYOR -eq 'True') {
        $CopyRedist = $true
    }
}

$CoreProjects = @(
    "Dock.Model",
    "Dock.Model.INPC",
    "Dock.Model.ReactiveUI",
    "Dock.Model.Serializer",
    "Dock.Avalonia"
)

$CoreFrameworks = @(
    "netstandard2.0",
    "net461"
)

$TestProjects = @(
    "Dock.Model.UnitTests",
    "Dock.Model.INPC.UnitTests",
    "Dock.Model.ReactiveUI.UnitTests",
    "Dock.Serializer.UnitTests",
    "Dock.Avalonia.UnitTests"
)

$TestFrameworks = @(
    "netcoreapp2.0",
    "netcoreapp2.1",
    "net461"
)

$SamplesProjects = @(
    "AvaloniaDemo"
)

$SamplesFrameworks = @(
    "netcoreapp2.0",
    "netcoreapp2.1",
    "net461"
)

$SamplesRuntimes = @(
    "win7-x64",
    "ubuntu.14.04-x64",
    "debian.8-x64",
    "osx.10.12-x64"
)

function Execute($cmd) 
{ 
    Invoke-Expression $cmd
    if ($LastExitCode -ne 0) { Exit 1 } 
}

if ($BuildCore -eq $true) {
    ForEach ($project in $CoreProjects) {
        ForEach ($framework in $CoreFrameworks) {
            $cmd = "dotnet build src/$project/$project.csproj -c $Configuration -f $framework --version-suffix $VersionSuffix"
            Execute $cmd
        }
    }
}

if ($TestCore -eq $true) {
    ForEach ($project in $TestProjects) {
        ForEach ($framework in $TestFrameworks) {
            $cmd = "dotnet test tests/$project/$project.csproj -c $Configuration -f $framework"
            Execute $cmd
        }
    }
}

if ($PackCore -eq $true) {
    ForEach ($project in $CoreProjects) {
        $cmd = "dotnet pack src/$project/$project.csproj -c $Configuration --version-suffix $VersionSuffix"
        Execute $cmd
    }
}

if ($BuildSamples -eq $true) {
    ForEach ($project in $SamplesProjects) {
        ForEach ($framework in $SamplesFrameworks) {
            $cmd = "dotnet build samples/$project/$project.csproj -c $Configuration -f $framework --version-suffix $VersionSuffix"
            Execute $cmd
        }
    }
}

if ($PublishSamples -eq $true) {
    ForEach ($project in $SamplesProjects) {
        ForEach ($framework in $SamplesFrameworks) {
            ForEach ($runtime in $SamplesRuntimes) {
                $cmd = "dotnet publish samples/$project/$project.csproj -c $Configuration -f $framework -r $runtime --version-suffix $VersionSuffix"
                Execute $cmd
            }
        }
    }
}

if($CopyRedist -eq $true) {
    $RedistPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Redist\MSVC\14.14.26405\x64\Microsoft.VC141.CRT"
    $RedistDest = "$pwd\samples\AvaloniaDemo\bin\AnyCPU\Release"
    $RedistRuntime = "win7-x64"
    Copy-Item "$RedistPath\msvcp140.dll" "$RedistDest\netcoreapp2.0\$RedistRuntime\publish"
    Copy-Item "$RedistPath\vcruntime140.dll" "$RedistDest\netcoreapp2.0\$RedistRuntime\publish"
    Copy-Item "$RedistPath\msvcp140.dll" "$RedistDest\netcoreapp2.1\$RedistRuntime\publish"
    Copy-Item "$RedistPath\vcruntime140.dll" "$RedistDest\netcoreapp2.1\$RedistRuntime\publish"
}

if($PushNuGet -eq $true -And $ApiKey -ne $null -And $ApiUrl -ne $null) {
    ForEach ($project in $CoreProjects) {
        $cmd = "dotnet nuget push src\$project\bin\AnyCPU\$Configuration\*.nupkg -s $ApiUrl -k $ApiKey"
        Execute $cmd
    }
}
