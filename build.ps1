[CmdletBinding()]
Param(
    [string]$Configuration = "Release",
    [string]$VersionSuffix = $null,
    [switch]$BuildCore,
    [switch]$TestCore,
    [switch]$PackCore,
    [switch]$BuildSamples,
    [switch]$PublishSamples,
    [switch]$CopyRedist,
    [switch]$PushNuGet,
    [switch]$IsNugetRelease
)

$VersionSuffixParam = $null

if (-Not ($VersionSuffix)) {
    if ($env:APPVEYOR_BUILD_VERSION) {
        $VersionSuffix = "-build" + $env:APPVEYOR_BUILD_VERSION
        $VersionSuffixParam = "--version-suffix " + $VersionSuffix
        Write-Host "AppVeyor override VersionSuffix: $VersionSuffix"
    }
} else {
    $VersionSuffixParam = "--version-suffix $VersionSuffix"
    Write-Host "VersionSuffix: $VersionSuffix"
}

if (-Not ($PushNuGet))
{
    if($env:APPVEYOR_REPO_NAME -eq 'wieslawsoltes/Dock' -And $env:APPVEYOR_REPO_BRANCH -eq 'master') {
        $PushNuGet = $true
        Write-Host "AppVeyor override PushNuGet: $PushNuGet"
    }
}

if (-Not ($IsNugetRelease)) {
    if ($env:APPVEYOR_REPO_TAG -eq 'True' -And $env:APPVEYOR_REPO_TAG_NAME) {
        $IsNugetRelease = $true
        Write-Host "AppVeyor override IsNugetRelease: $IsNugetRelease"
    }
}

if (-Not ($CopyRedist)) {
    if ($env:APPVEYOR -eq 'True') {
        $CopyRedist = $true
        Write-Host "AppVeyor override CopyRedist: $CopyRedist"
    }
}

if ($env:APPVEYOR_PULL_REQUEST_TITLE) {
    $PushNuGet = $false
    $IsNugetRelease = $false
    $PublishSamples = $false
    $CopyRedist = $false
    Write-Host "Pull Request #" + $env:APPVEYOR_PULL_REQUEST_NUMBER
    Write-Host "AppVeyor override PushNuGet: $PushNuGet"
    Write-Host "AppVeyor override IsNugetRelease: $IsNugetRelease"
    Write-Host "AppVeyor override PublishSamples: $PublishSamples"
    Write-Host "AppVeyor override CopyRedist: $CopyRedist"
}

$CoreProjects = @(
    "Dock.Model",
    "Dock.Model.INPC",
    "Dock.Model.ReactiveUI",
    "Dock.Serializer",
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
    Try {
        Invoke-Expression $cmd
        if ($LastExitCode -ne 0) { Exit 1 }
    }
    Catch {
        Write-Host "Invoke Expression failed." -ForegroundColor Red
        Exit 1
    }
}

function Invoke-BuildCore
{
    ForEach ($project in $CoreProjects) {
        ForEach ($framework in $CoreFrameworks) {
            Write-Host "Build: $project, $Configuration, $framework"
            $cmd = "dotnet build src/$project/$project.csproj -c $Configuration -f $framework $VersionSuffixParam"
            Execute $cmd
        }
    }
}

function Invoke-TestCore
{
    ForEach ($project in $TestProjects) {
        ForEach ($framework in $TestFrameworks) {
            Write-Host "Test: $project, $Configuration, $framework"
            $cmd = "dotnet test tests/$project/$project.csproj -c $Configuration -f $framework"
            Execute $cmd
        }
    }
}

function Invoke-PackCore
{
    ForEach ($project in $CoreProjects) {
        $cmd = "dotnet pack src/$project/$project.csproj -c $Configuration $VersionSuffixParam"
        Execute $cmd
    }
}

function Invoke-BuildSamples
{
    ForEach ($project in $SamplesProjects) {
        ForEach ($framework in $SamplesFrameworks) {
            Write-Host "Build: $project, $Configuration, $framework"
            $cmd = "dotnet build samples/$project/$project.csproj -c $Configuration -f $framework $VersionSuffixParam"
            Execute $cmd
        }
    }
}

function Invoke-PublishSamples
{
    ForEach ($project in $SamplesProjects) {
        ForEach ($framework in $SamplesFrameworks) {
            ForEach ($runtime in $SamplesRuntimes) {
                Write-Host "Publish: $project, $Configuration, $framework, $runtime"
                $cmd = "dotnet publish samples/$project/$project.csproj -c $Configuration -f $framework -r $runtime $VersionSuffixParam"
                Execute $cmd
            }
        }
    }
}

function Invoke-CopyRedist
{
    $RedistPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Redist\MSVC\14.14.26405\x64\"
    $RedistDest = "$pwd\samples\AvaloniaDemo\bin\AnyCPU\$Configuration"
    $RedistRuntime = "win7-x64"
    Write-Host "CopyRedist: $RedistDest, $RedistRuntime"
    Copy-Item "$RedistPath\msvcp140.dll" "$RedistDest\netcoreapp2.0\$RedistRuntime\publish"
    Copy-Item "$RedistPath\vcruntime140.dll" "$RedistDest\netcoreapp2.0\$RedistRuntime\publish"
    Copy-Item "$RedistPath\msvcp140.dll" "$RedistDest\netcoreapp2.1\$RedistRuntime\publish"
    Copy-Item "$RedistPath\vcruntime140.dll" "$RedistDest\netcoreapp2.1\$RedistRuntime\publish"
}

function Invoke-PushNuGet
{
    ForEach ($project in $CoreProjects) {
        if($IsNugetRelease) {
            if ($env:NUGET_API_URL -And $env:NUGET_API_KEY) {
                Write-Host "Push NuGet: $project, $Configuration"
                $cmd = "dotnet nuget push src/$project/bin/AnyCPU/$Configuration/*.nupkg -s $env:NUGET_API_URL -k $env:NUGET_API_KEY"
                Execute $cmd
            }
        } else {
            if ($env:MYGET_API_URL -And $env:MYGET_API_KEY) {
                Write-Host "Push MyGet: $project, $Configuration"
                $cmd = "dotnet nuget push src/$project/bin/AnyCPU/$Configuration/*.nupkg -s $env:MYGET_API_URL -k $env:MYGET_API_KEY"
                Execute $cmd
            }
        }
    }
}

if($BuildCore) {
    Invoke-BuildCore
}

if($TestCoree) {
    Invoke-TestCore
}

if($PackCore) {
    Invoke-PackCore
}

if($BuildSamples) {
    Invoke-BuildSamples
}

if($PublishSamples) {
    Invoke-PublishSamples
}

if($CopyRedist) {
    Invoke-CopyRedist
}

if($PushNuGet) {
    Invoke-PushNuGet
}
