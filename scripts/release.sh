#!/usr/bin/env bash
# Builds SimpleCompass and packages it into a mod.io-ready ZIP: the built DLL,
# manifest.json, and thumbnail.png, rooted under a SimpleCompass/ folder inside the
# ZIP so it can be dragged straight into a Mods directory.
#
# Usage: scripts/release.sh
# Upload the resulting ZIP manually at https://mod.io/g/timberborn.
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

mod_name="SimpleCompass"
out_zip="release/${mod_name}.zip"

echo "==> Running unit tests"
dotnet test tests/SimpleCompass.Tests.csproj -c Release

echo "==> Building mod"
dotnet build SimpleCompass.csproj -c Release

stage_dir="$(mktemp -d)"
trap 'rm -rf "$stage_dir"' EXIT

mod_dir="$stage_dir/$mod_name"
mkdir -p "$mod_dir"
cp "bin/Release/${mod_name}.dll" manifest.json thumbnail.png "$mod_dir/"

mkdir -p "$(dirname "$out_zip")"
out_zip_abs="$(cd "$(dirname "$out_zip")" && pwd)/$(basename "$out_zip")"
rm -f "$out_zip_abs"

echo "==> Packaging $out_zip"
(cd "$stage_dir" && zip -r -X "$out_zip_abs" "$mod_name" >/dev/null)

echo "==> Wrote $out_zip - upload this at https://mod.io/g/timberborn"
