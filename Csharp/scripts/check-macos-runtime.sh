#!/usr/bin/env zsh
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT"

FAILURES=0
WARNINGS=0

pass() { echo "[PASS] $1"; }
warn() { echo "[WARN] $1"; WARNINGS=$((WARNINGS + 1)); }
fail() { echo "[FAIL] $1"; FAILURES=$((FAILURES + 1)); }

check_command() {
  local cmd="$1"
  if command -v "$cmd" >/dev/null 2>&1; then
    pass "Command available: $cmd"
  else
    fail "Command missing: $cmd"
  fi
}

check_file() {
  local path="$1"
  if [[ -e "$path" ]]; then
    pass "Found: $path"
  else
    fail "Missing: $path"
  fi
}

check_executable_file() {
  local path="$1"
  if [[ -x "$path" ]]; then
    pass "Executable: $path"
  elif [[ -e "$path" ]]; then
    warn "Not executable: $path"
  else
    fail "Missing: $path"
  fi
}

echo "== macOS Runtime Preflight =="

if [[ "$(uname -s)" == "Darwin" ]]; then
  pass "Running on macOS"
else
  warn "Not running on macOS (detected: $(uname -s))"
fi

check_command dotnet
check_command wine
check_command winetricks

if dotnet mgcb --help >/dev/null 2>&1; then
  pass "dotnet-mgcb available"
else
  fail "dotnet-mgcb unavailable (install with: dotnet tool install -g dotnet-mgcb --version 3.8.4.1)"
fi

MGFX_PREFIX="${MGFXC_WINE_PATH:-$HOME/.wine-mgfx}"
if [[ -n "${MGFXC_WINE_PATH:-}" ]]; then
  pass "MGFXC_WINE_PATH set: $MGFXC_WINE_PATH"
else
  warn "MGFXC_WINE_PATH not set, using default: $MGFX_PREFIX"
fi

if [[ -d "$MGFX_PREFIX" ]]; then
  pass "Wine prefix exists: $MGFX_PREFIX"
else
  fail "Wine prefix missing: $MGFX_PREFIX"
fi

if [[ -f "$MGFX_PREFIX/winetricks.log" ]]; then
  if grep -q "d3dcompiler_47" "$MGFX_PREFIX/winetricks.log"; then
    pass "winetricks log contains d3dcompiler_47"
  else
    warn "winetricks log missing d3dcompiler_47"
  fi

  if grep -q "dotnet8" "$MGFX_PREFIX/winetricks.log"; then
    pass "winetricks log contains dotnet8"
  else
    warn "winetricks log missing dotnet8"
  fi
else
  warn "No winetricks.log found in prefix"
fi

check_file "MarkInDeDark.Game/MarkInDeDark.Game.csproj"
check_file "MarkInDeDark.Game/Assets/ContentBuilder/ContentBuilder.DesktopGL.mgcb"
check_file "MarkInDeDark.Shadows/Content/Content.DesktopGL.mgcb"
check_executable_file "scripts/rebuild-content-macos.sh"
check_executable_file "scripts/run-desktop-macos.sh"

if dotnet restore MarkInDeDark.Game/MarkInDeDark.Game.csproj --disable-parallel -v minimal >/dev/null 2>&1; then
  pass "Desktop restore OK"
else
  fail "Desktop restore failed"
fi

if dotnet build MarkInDeDark.Game/MarkInDeDark.Game.csproj -v minimal >/dev/null 2>&1; then
  pass "Desktop build OK"
else
  fail "Desktop build failed"
fi

echo ""
echo "Summary: $FAILURES failure(s), $WARNINGS warning(s)"
if [[ $FAILURES -gt 0 ]]; then
  exit 1
fi
