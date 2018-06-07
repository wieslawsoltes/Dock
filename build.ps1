[CmdletBinding()]
Param(
    [string]$Configuration = "Release",
    [string[]]$DisabledFrameworks,
    [string]$VersionSuffix = $null,
    [switch]$BuildSources,
    [switch]$TestSources,
    [switch]$PackSources,
    [switch]$BuildSamples,
    [switch]$PublishSamples,
    [switch]$ZipSamples,
    [switch]$CopyRedist,
    [switch]$PushNuGet,
    [switch]$IsNugetRelease,
    [String]$Artifacts
)

$VersionSuffixParam = $null

if (-Not $Artifacts) {
    $Artifacts = "$pwd\artifacts"
} else {
    Remove-Item "$Artifacts\*.zip" -ErrorAction SilentlyContinue
    Remove-Item "$Artifacts\*.nupkg" -ErrorAction SilentlyContinue
}

if (-Not (Test-Path $Artifacts)) {
    New-Item -ItemType directory -Path $Artifacts
}

Write-Host "Artifacts: $Artifacts"

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

if ($env:APPVEYOR_PULL_REQUEST_TITLE) {
    $PushNuGet = $false
    $IsNugetRelease = $false
    $PublishSamples = $false
    $CopyRedist = $false
    $ZipSamples = $false
    Write-Host "Pull Request #" + $env:APPVEYOR_PULL_REQUEST_NUMBER
    Write-Host "AppVeyor override PushNuGet: $PushNuGet"
    Write-Host "AppVeyor override IsNugetRelease: $IsNugetRelease"
    Write-Host "AppVeyor override PublishSamples: $PublishSamples"
    Write-Host "AppVeyor override CopyRedist: $CopyRedist"
    Write-Host "AppVeyor override ZipSamples: $ZipSamples"
}

$SourceProjects = @(
    "Dock.Model",
    "Dock.Model.INPC",
    "Dock.Model.ReactiveUI",
    "Dock.Serializer",
    "Dock.Avalonia"
)

$SourceFrameworks = @(
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

$SampleFrameworks = @(
    "netcoreapp2.0",
    "netcoreapp2.1",
    "net461"
)

$SampleRuntimes = @(
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

function Zip($source, $destination)
{
    if(Test-Path $destination) { Remove-item $destination }
    Add-Type -assembly "System.IO.Compression.FileSystem"
    [IO.Compression.ZipFile]::CreateFromDirectory($source, $destination)
}

function Invoke-BuildSources
{
    ForEach ($project in $SourceProjects) {
        ForEach ($framework in $SourceFrameworks) {
            if (-Not ($DisabledFrameworks -match $framework)) {
                Write-Host "Build: $project, $Configuration, $framework"
                $cmd = "dotnet build src\$project\$project.csproj -c $Configuration -f $framework $VersionSuffixParam"
                Execute $cmd
            }
        }
    }
}

function Invoke-TestSources
{
    ForEach ($project in $TestProjects) {
        ForEach ($framework in $TestFrameworks) {
            if (-Not ($DisabledFrameworks -match $framework)) {
                Write-Host "Test: $project, $Configuration, $framework"
                $cmd = "dotnet test tests\$project\$project.csproj -c $Configuration -f $framework"
                Execute $cmd
            }
        }
    }
}

function Invoke-PackSources
{
    ForEach ($project in $SourceProjects) {
        $cmd = "dotnet pack src\$project\$project.csproj -c $Configuration $VersionSuffixParam"
        Execute $cmd
        if (Test-Path $Artifacts) {
            $files = Get-Item "$pwd\src\$project\bin\AnyCPU\$Configuration\*.nupkg"
            ForEach ($file in $files) {
                Write-Host "Copy: $file"
                Copy-Item $file.FullName -Destination $Artifacts
            }
        }
    }
}

function Invoke-BuildSamples
{
    ForEach ($project in $SamplesProjects) {
        ForEach ($framework in $SampleFrameworks) {
            if (-Not ($DisabledFrameworks -match $framework)) {
                Write-Host "Build: $project, $Configuration, $framework"
                $cmd = "dotnet build samples\$project\$project.csproj -c $Configuration -f $framework $VersionSuffixParam"
                Execute $cmd
            }
        }
    }
}

function Invoke-PublishSamples
{
    ForEach ($project in $SamplesProjects) {
        ForEach ($framework in $SampleFrameworks) {
            ForEach ($runtime in $SampleRuntimes) {
                if (-Not ($DisabledFrameworks -match $framework)) {
                    Write-Host "Publish: $project, $Configuration, $framework, $runtime"
                    $cmd = "dotnet publish samples\$project\$project.csproj -c $Configuration -f $framework -r $runtime $VersionSuffixParam"
                    Execute $cmd
                }
            }
        }
    }
}

function Invoke-CopyRedist
{
    $RedistVersion = "14.14.26405"
    $RedistPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Redist\MSVC\$RedistVersion\x64\Microsoft.VC141.CRT\"
    $RedistRuntime = "win7-x64"
    ForEach ($project in $SamplesProjects) {
        ForEach ($framework in $SampleFrameworks) {
            ForEach ($runtime in $SampleRuntimes) {
                if ($runtime -eq $RedistRuntime) {
                    $RedistDest = "$pwd\samples\$project\bin\AnyCPU\$Configuration\$framework\$RedistRuntime\publish"
                    if(Test-Path -Path $RedistDest) {
                        Write-Host "CopyRedist: $RedistDest, runtime: $RedistRuntime, version: $RedistVersion"
                        Copy-Item "$RedistPath\msvcp140.dll" -Destination $RedistDest
                        Copy-Item "$RedistPath\vcruntime140.dll" -Destination $RedistDest
                    } else {
                        Write-Host "CopyRedist: Path does not exists: $RedistDest"
                    }
                }
            }
        }
    }
}

function Invoke-ZipSamples
{
    ForEach ($project in $SamplesProjects) {
        ForEach ($framework in $SampleFrameworks) {
            ForEach ($runtime in $SampleRuntimes) {
                if (-Not ($DisabledFrameworks -match $framework)) {
                    Write-Host "Zip: $project, $Configuration, $framework, $runtime"
                    $source = "$pwd\samples\$project\bin\AnyCPU\$Configuration\$framework\$runtime\publish\"
                    $destination = "$Artifacts\$project-$framework-$runtime.zip"
                    Zip $source $destination
                    Write-Host "Zip: $destination"
                }
            }
        }
    }
}

function Invoke-PushNuGet
{
    ForEach ($project in $SourceProjects) {
        if($IsNugetRelease) {
            if ($env:NUGET_API_URL -And $env:NUGET_API_KEY) {
                Write-Host "Push NuGet: $project, $Configuration"
                $cmd = "dotnet nuget push $pwd\src\$project\bin\AnyCPU\$Configuration\*.nupkg -s $env:NUGET_API_URL -k $env:NUGET_API_KEY"
                Execute $cmd
            }
        } else {
            if ($env:MYGET_API_URL -And $env:MYGET_API_KEY) {
                Write-Host "Push MyGet: $project, $Configuration"
                $cmd = "dotnet nuget push $pwd\src\$project\bin\AnyCPU\$Configuration\*.nupkg -s $env:MYGET_API_URL -k $env:MYGET_API_KEY"
                Execute $cmd
            }
        }
    }
}

if($BuildSources) {
    Invoke-BuildSources
}

if($TestCoree) {
    Invoke-TestSources
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

if($ZipSamples) {
    Invoke-ZipSamples
}

if($PackSources) {
    Invoke-PackSources
}

if($PushNuGet) {
    Invoke-PushNuGet
}
