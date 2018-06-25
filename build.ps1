[CmdletBinding()]
Param(
    [string]$ConfigFileName = "$pwd\build.xml",
    [string]$Configuration = "Release",
    [string[]]$DisabledFrameworks,
    [string]$VersionSuffix = $null,
    [switch]$BuildSources,
    [switch]$TestSources,
    [switch]$PackSources,
    [switch]$BuildApps,
    [switch]$PublishApps,
    [switch]$ZipApps,
    [switch]$CopyRedist,
    [switch]$PushNuGet,
    [switch]$IsNugetRelease,
    [String]$Artifacts
)

$ConfigFileItem = Get-ChildItem $ConfigFileName
[xml]$Config = Get-Content -Path $ConfigFileItem.FullName

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

if (-Not ($VersionSuffix)) {
    if ($env:APPVEYOR_BUILD_VERSION) {
        $VersionSuffix = "-build" + $env:APPVEYOR_BUILD_VERSION
        $VersionSuffixParam = "\`"$VersionSuffix\`""
        Write-Host "AppVeyor override VersionSuffix: $VersionSuffix" -ForegroundColor Yellow
    } else {
        $VersionSuffixParam = "\`"\`""
    }
} else {
    $VersionSuffixParam = "\`"$VersionSuffix\`""
}

if (-Not ($PushNuGet))
{
    if($env:APPVEYOR_REPO_NAME -eq 'wieslawsoltes/Dock' -And $env:APPVEYOR_REPO_BRANCH -eq 'master') {
        $PushNuGet = $true
        Write-Host "AppVeyor override PushNuGet: $PushNuGet" -ForegroundColor Yellow
    }
}

if (-Not ($IsNugetRelease)) {
    if ($env:APPVEYOR_REPO_TAG -eq 'True' -And $env:APPVEYOR_REPO_TAG_NAME) {
        $IsNugetRelease = $true
        Write-Host "AppVeyor override IsNugetRelease: $IsNugetRelease" -ForegroundColor Yellow
    }
}

if ($env:APPVEYOR_PULL_REQUEST_TITLE) {
    $PushNuGet = $false
    $IsNugetRelease = $false
    $PublishApps = $false
    $CopyRedist = $false
    $ZipApps = $false
    Write-Host "Pull Request #" + $env:APPVEYOR_PULL_REQUEST_NUMBER
    Write-Host "AppVeyor override PushNuGet: $PushNuGet" -ForegroundColor Yellow
    Write-Host "AppVeyor override IsNugetRelease: $IsNugetRelease" -ForegroundColor Yellow
    Write-Host "AppVeyor override PublishApps: $PublishApps" -ForegroundColor Yellow
    Write-Host "AppVeyor override CopyRedist: $CopyRedist" -ForegroundColor Yellow
    Write-Host "AppVeyor override ZipApps: $ZipApps" -ForegroundColor Yellow
}

Write-Host "ConfigFileName: $ConfigFileName" -ForegroundColor White
Write-Host "Configuration: $Configuration" -ForegroundColor White
Write-Host "DisabledFrameworks: $DisabledFrameworks" -ForegroundColor White
Write-Host "VersionSuffix: $VersionSuffix" -ForegroundColor White
Write-Host "BuildSources: $BuildSources" -ForegroundColor White
Write-Host "TestSources: $TestSources" -ForegroundColor White
Write-Host "PackSources: $PackSources" -ForegroundColor White
Write-Host "BuildApps: $BuildApps" -ForegroundColor White
Write-Host "PublishApps: $PublishApps" -ForegroundColor White
Write-Host "ZipApps: $ZipApps" -ForegroundColor White
Write-Host "CopyRedist: $CopyRedist" -ForegroundColor White
Write-Host "PushNuGet: $PushNuGet" -ForegroundColor White
Write-Host "IsNugetRelease: $IsNugetRelease" -ForegroundColor White
Write-Host "Artifacts: $Artifacts" -ForegroundColor White

function Validate
{
    param()
    if ($LastExitCode -ne 0) { Exit 1 }
}

function Zip
{
    param($source, $destination)
    if(Test-Path $destination) { Remove-item $destination }
    Add-Type -assembly "System.IO.Compression.FileSystem"
    [IO.Compression.ZipFile]::CreateFromDirectory($source, $destination)
}

function Invoke-BuildSources
{
    param()
    $Projects = $Config.Build.Sources.Project
    ForEach ($project in $Projects) {
        $Name = $project.Name
        $Path = $project.Path
        $Frameworks = $project.Frameworks.Framework
        ForEach ($framework in $Frameworks) {
            if (-Not ($DisabledFrameworks -match $framework)) {
                Write-Host "Build: $Name, $Configuration, $framework" -ForegroundColor Cyan
                $args = @('build', "$pwd\$Path\$Name\$Name.csproj", '-c', $Configuration, '-f', $framework, '--version-suffix', $VersionSuffixParam)
                & dotnet $args
                Validate
            }
        }
    }
}

function Invoke-TestSources
{
    param()
    $Projects = $Config.Build.Tests.Project
    ForEach ($project in $Projects) {
        $Name = $project.Name
        $Path = $project.Path
        $Frameworks = $project.Frameworks.Framework
        ForEach ($framework in $Frameworks) {
            if (-Not ($DisabledFrameworks -match $framework)) {
                Write-Host "Test: $Name, $Configuration, $framework" -ForegroundColor Cyan
                $args = @('test', "$pwd\$Path\$Name\$Name.csproj", '-c', $Configuration, '-f', $framework)
                & dotnet $args
                Validate
            }
        }
    }
}

function Invoke-PackSources
{
    param()
    $Projects = $Config.Build.Sources.Project
    ForEach ($project in $Projects) {
        $Name = $project.Name
        $Path = $project.Path
        Write-Host "Pack: $Name, $Configuration" -ForegroundColor Cyan
        $args = @('pack', "$pwd\$Path\$Name\$Name.csproj", '-c', $Configuration, '--version-suffix', $VersionSuffixParam)
        & dotnet $args
        Validate
        if (Test-Path $Artifacts) {
            $files = Get-Item "$pwd\$Path\$Name\bin\AnyCPU\$Configuration\*.nupkg"
            ForEach ($file in $files) {
                Write-Host "Copy: $file" -ForegroundColor Cyan
                Copy-Item $file.FullName -Destination $Artifacts
            }
        }
    }
}

function Invoke-BuildApps
{
    param()
    $Projects = $Config.Build.Apps.Project
    ForEach ($project in $Projects) {
        $Name = $project.Name
        $Path = $project.Path
        $Frameworks = $project.Frameworks.Framework
        ForEach ($framework in $Frameworks) {
            if (-Not ($DisabledFrameworks -match $framework)) {
                Write-Host "Build: $Name, $Configuration, $framework" -ForegroundColor Cyan
                $args = @('build', "$pwd\$Path\$Name\$Name.csproj", '-c', $Configuration, '-f', $framework, '--version-suffix', $VersionSuffixParam)
                & dotnet $args
                Validate
            }
        }
    }
}

function Invoke-PublishApps
{
    param()
    $Projects = $Config.Build.Apps.Project
    ForEach ($project in $Projects) {
        $Name = $project.Name
        $Path = $project.Path
        $Frameworks = $project.Frameworks.Framework
        $Runtimes = $project.Runtimes.Runtime
        ForEach ($framework in $Frameworks) {
            ForEach ($runtime in $Runtimes) {
                if (-Not ($DisabledFrameworks -match $framework)) {
                    Write-Host "Publish: $Name, $Configuration, $framework, $runtime" -ForegroundColor Cyan
                    $args = @('publish', "$pwd\$Path\$Name\$Name.csproj", '-c', $Configuration, '-f', $framework, '-r', $runtime, '--version-suffix', $VersionSuffixParam)
                    & dotnet $args
                    Validate
                }
            }
        }
    }
}

function Invoke-CopyRedist
{
    param()
    $RedistVersion = "14.14.26405"
    $RedistPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Redist\MSVC\$RedistVersion\x64\Microsoft.VC141.CRT\"
    $RedistRuntime = "win7-x64"
    $Projects = $Config.Build.Apps.Project
    ForEach ($project in $Projects) {
        $Name = $project.Name
        $Path = $project.Path
        $Frameworks = $project.Frameworks.Framework
        $Runtimes = $project.Runtimes.Runtime
        ForEach ($framework in $Frameworks) {
            ForEach ($runtime in $Runtimes) {
                if ($runtime -eq $RedistRuntime) {
                    $RedistDest = "$pwd\$Path\$Name\bin\AnyCPU\$Configuration\$framework\$RedistRuntime\publish"
                    if(Test-Path -Path $RedistDest) {
                        Write-Host "CopyRedist: $RedistDest, runtime: $RedistRuntime, version: $RedistVersion" -ForegroundColor Cyan
                        Copy-Item "$RedistPath\msvcp140.dll" -Destination $RedistDest
                        Copy-Item "$RedistPath\vcruntime140.dll" -Destination $RedistDest
                    } else {
                        Write-Host "CopyRedist: Path does not exists: $RedistDest" -ForegroundColor Red
                    }
                }
            }
        }
    }
}

function Invoke-ZipApps
{
    param()
    $Projects = $Config.Build.Apps.Project
    ForEach ($project in $Projects) {
        $Name = $project.Name
        $Path = $project.Path
        $Frameworks = $project.Frameworks.Framework
        $Runtimes = $project.Runtimes.Runtime
        ForEach ($framework in $Frameworks) {
            ForEach ($runtime in $Runtimes) {
                if (-Not ($DisabledFrameworks -match $framework)) {
                    Write-Host "Zip: $Name, $Configuration, $framework, $runtime" -ForegroundColor Cyan
                    $source = "$pwd\$Path\$Name\bin\AnyCPU\$Configuration\$framework\$runtime\publish\"
                    $destination = "$Artifacts\$Name-$framework-$runtime.zip"
                    Zip $source $destination
                    Write-Host "Zip: $destination" -ForegroundColor Cyan
                }
            }
        }
    }
}

function Invoke-PushNuGet
{
    param()
    $Projects = $Config.Build.Sources.Project
    ForEach ($project in $Projects) {
        $Name = $project.Name
        $Path = $project.Path
        if($IsNugetRelease) {
            if ($env:NUGET_API_URL -And $env:NUGET_API_KEY) {
                Write-Host "Push NuGet: $Name, $Configuration" -ForegroundColor Cyan
                $args = @('nuget', 'push', "$pwd\$Path\$Name\bin\AnyCPU\$Configuration\*.nupkg", '-s', $env:NUGET_API_URL, '-k', $env:NUGET_API_KEY)
                & dotnet $args
                Validate
            }
        } else {
            if ($env:MYGET_API_URL -And $env:MYGET_API_KEY) {
                Write-Host "Push MyGet: $Name, $Configuration" -ForegroundColor Cyan
                $args = @('nuget', 'push', "$pwd\$Path\$Name\bin\AnyCPU\$Configuration\*.nupkg", '-s', $env:MYGET_API_URL, '-k', $env:MYGET_API_KEY)
                & dotnet $args
                Validate
            }
        }
    }
}

if($BuildSources) {
    Invoke-BuildSources
}

if($TestSources) {
    Invoke-TestSources
}

if($BuildApps) {
    Invoke-BuildApps
}

if($PublishApps) {
    Invoke-PublishApps
}

if($CopyRedist) {
    Invoke-CopyRedist
}

if($ZipApps) {
    Invoke-ZipApps
}

if($PackSources) {
    Invoke-PackSources
}

if($PushNuGet) {
    Invoke-PushNuGet
}
