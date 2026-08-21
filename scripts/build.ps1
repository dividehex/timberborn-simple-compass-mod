<#
.SYNOPSIS
  Build, test, and deploy Simple Compass into your Timberborn Mods folder.
.DESCRIPTION
  Tests run first and gate the deploy - nothing is built or copied if they fail.
.EXAMPLE
  ./scripts/build.ps1
.EXAMPLE
  ./scripts/build.ps1 -p:TimberbornDir="C:\Games\Timberborn" -p:TimberbornModsDir="D:\Mods"
#>
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    Write-Host "==> Running unit tests"
    dotnet test tests/SimpleCompass.Tests.csproj -c Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host "==> Building and deploying mod"
    dotnet build SimpleCompass.csproj -c Release @args
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host "==> Done - deployed to your Timberborn Mods/SimpleCompass folder."
}
finally {
    Pop-Location
}
