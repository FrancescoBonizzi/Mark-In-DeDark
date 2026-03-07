#!/usr/bin/env zsh
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT"

export MGFXC_WINE_PATH="${MGFXC_WINE_PATH:-$HOME/.wine-mgfx}"

echo "[0/3] Clean content intermediates..."
rm -rf MarkInDeDark.Desktop/Assets/ContentBuilder/obj
rm -rf penumbra/Content/obj/DesktopGL

echo "[1/3] Rebuild game content (DesktopGL)..."
cd MarkInDeDark.Desktop/Assets/ContentBuilder
dotnet mgcb --@=ContentBuilder.DesktopGL.mgcb /rebuild

cd "$REPO_ROOT"
echo "[2/3] Rebuild Penumbra content (DesktopGL)..."
cd penumbra/Content
dotnet mgcb --@=Content.DesktopGL.mgcb /rebuild

cd "$REPO_ROOT"
echo "[3/3] Clean Penumbra project to refresh embedded resources..."
dotnet clean penumbra/MonoGame.Penumbra.WindowsDX.csproj -c Debug

echo "Content rebuild completed."
