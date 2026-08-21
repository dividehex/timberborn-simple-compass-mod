<#
.SYNOPSIS
  Builds SimpleCompass and packages it into a mod.io-ready ZIP.
.DESCRIPTION
  Packages the built DLL, manifest.json, and thumbnail.png, rooted under a
  SimpleCompass/ folder inside the ZIP so it can be dragged straight into a Mods
  directory. Upload the resulting ZIP manually at https://mod.io/g/timberborn.
.EXAMPLE
  ./scripts/release.ps1
#>
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    $modName = 'SimpleCompass'
    $outZip = "release/$modName.zip"

    Write-Host "==> Running unit tests"
    dotnet test tests/SimpleCompass.Tests.csproj -c Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host "==> Building mod"
    dotnet build SimpleCompass.csproj -c Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $stageDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
    $modDir = Join-Path $stageDir $modName
    New-Item -ItemType Directory -Path $modDir -Force | Out-Null
    try {
        Copy-Item "bin/Release/$modName.dll", 'manifest.json', 'thumbnail.png' -Destination $modDir

        New-Item -ItemType Directory -Path (Split-Path -Parent $outZip) -Force | Out-Null
        if (Test-Path $outZip) { Remove-Item $outZip -Force }

        Write-Host "==> Packaging $outZip"
        Compress-Archive -Path $modDir -DestinationPath $outZip

        Write-Host "==> Wrote $outZip - upload this at https://mod.io/g/timberborn"
    }
    finally {
        Remove-Item -Recurse -Force $stageDir
    }
}
finally {
    Pop-Location
}
