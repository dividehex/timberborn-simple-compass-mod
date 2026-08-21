#!/usr/bin/env bash
# Removes local build output (bin/, obj/) for the mod and test projects.
# Does not touch your Timberborn Mods folder or workshop_data.json (your private
# Steam Workshop upload config) - both are left alone.
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

rm -rf bin obj tests/bin tests/obj

echo "==> Removed bin/ and obj/ (repo root and tests/)."
