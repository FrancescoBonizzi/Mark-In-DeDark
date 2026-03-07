#!/usr/bin/env zsh
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT"

echo "[1/2] Build desktop host..."
dotnet build MarkInDeDark.Game/MarkInDeDark.Game.csproj -v minimal

echo "[2/2] Run desktop host..."
dotnet run --project MarkInDeDark.Game/MarkInDeDark.Game.csproj
