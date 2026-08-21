#!/usr/bin/env bash
# Build, test, and deploy Simple Compass into your Timberborn Mods folder.
# Tests run first and gate the deploy - nothing is built or copied if they fail.
#
# Usage:
#   scripts/build.sh
#   scripts/build.sh -p:TimberbornDir=/path/to/Timberborn -p:TimberbornModsDir=/path/to/Mods
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

echo "==> Running unit tests"
dotnet test tests/SimpleCompass.Tests.csproj -c Release

echo "==> Building and deploying mod"
dotnet build SimpleCompass.csproj -c Release "$@"

echo "==> Done - deployed to your Timberborn Mods/SimpleCompass folder."
