<#
.SYNOPSIS
  Removes local build output (bin/, obj/) for the mod and test projects.
.DESCRIPTION
  Does not touch your Timberborn Mods folder or workshop_data.json (your private
  Steam Workshop upload config) - both are left alone.
#>
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    foreach ($dir in 'bin', 'obj', 'tests/bin', 'tests/obj') {
        if (Test-Path $dir) {
            Remove-Item -Recurse -Force $dir
        }
    }
    Write-Host "==> Removed bin/ and obj/ (repo root and tests/)."
}
finally {
    Pop-Location
}
