## macOS Setup

### Prerequisiti

1. Installa `wine` e `winetricks`.
2. Crea un prefix dedicato per MGFXC.
3. Installa nel prefix:
   - `d3dcompiler_47`
   - `dotnet8`

Comandi consigliati:

```bash
export WINEPREFIX="$HOME/.wine-mgfx"
export WINEARCH=win64
wineboot -i
winetricks -q d3dcompiler_47
winetricks -q dotnet8
```

### Variabile ambiente

MGCB/MGFXC su macOS usa Wine per compilare gli `.fx`.

```bash
export MGFXC_WINE_PATH="$HOME/.wine-mgfx"
```

## Script macOS

Nota: gli asset runtime sono sotto `MarkInDeDark.Game/Assets`.

### Solo rigenerazione content

```bash
./scripts/rebuild-content-macos.sh
```

Fa in ordine:

1. Rebuild content gioco (`ContentBuilder.DesktopGL.mgcb`)
2. Rebuild content Shadows (`Content.DesktopGL.mgcb`)
3. Clean del progetto Shadows (per riallineare le risorse embeddate)

### Solo build + run del progetto

```bash
./scripts/run-desktop-macos.sh
```

### Check ambiente/runtime

```bash
./scripts/check-macos-runtime.sh
```

## Note utili

1. Se macOS blocca Wine al primo avvio: `Impostazioni di Sistema > Privacy e Sicurezza > Apri comunque`.
2. Se ritorna un errore MGFX: rilancia lo script completo, non solo `dotnet run`.
