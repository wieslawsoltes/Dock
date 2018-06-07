[CmdletBinding()]
Param(
    [string]$ConfigFileName = "$pwd\build.xml"
)

$ConfigFileItem = Get-ChildItem $ConfigFileName
[xml]$Config = Get-Content -Path $ConfigFileItem.FullName

Write-Host "[Sources]"
$Projects = $Config.Build.Sources.Project
ForEach ($project in $Projects) {
    $Name = $project.Name
    $Path = $project.Path
    $Frameworks = $project.Frameworks.Framework
    Write-Host "Project: $Name, Path: $Path"
    ForEach ($framework in $Frameworks) {
        Write-Host "- Framework: $framework"
    }
}

Write-Host "[Tests]"
$Projects = $Config.Build.Tests.Project
ForEach ($project in $Projects) {
    $Name = $project.Name
    $Path = $project.Path
    $Frameworks = $project.Frameworks.Framework
    Write-Host "Project: $Name, Path: $Path"
    ForEach ($framework in $Frameworks) {
        Write-Host "- Framework: $framework"
    }
}

Write-Host "[Apps]"
$Projects = $Config.Build.Apps.Project
ForEach ($project in $Projects) {
    $Name = $project.Name
    $Path = $project.Path
    $Frameworks = $project.Frameworks.Framework
    $Runtimes = $project.Runtimes.Runtime
    Write-Host "Project: $Name, Path: $Path"
    ForEach ($framework in $Frameworks) {
        Write-Host "- Framework: $framework"
    }
    ForEach ($Runtime in $Runtimes) {
        Write-Host "- Runtime: $Runtime"
    }
}
