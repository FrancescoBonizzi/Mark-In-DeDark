/**
 * Genera Web/src/generatedSpritepacks.ts scansionando i file
 * Web/public/assets/images/spriteSheets/Spritesheet-*.json.
 *
 * Per ogni "pacco" trovato nei nomi dei frame (es. Grounds/pacco2/cell1)
 * raccoglie le coordinate {ss, x, y} di:
 *   - celle Terra  (Grounds/paccoN/cellM)
 *   - decal        (Grounds/paccoN/Decals/decal_M)
 *   - muri speciali(Grounds/paccoN/SpecialWalls/wall_special_M)
 *   - riempimento  (Grounds/paccoN/Riempimento/riempimento_M)
 *
 * Rileva automaticamente il frame "placeholder" (coordinata usata come
 * fallback quando un pack non ha quella texture) come la coordinata
 * che appare più volte nei dati cells+decals+specwalls.
 *
 * Eseguire con:
 *   npx tsx Web/scripts/generate-spritepack-manifest.ts
 */

import * as fs from "fs";
import * as path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ssDir = path.join(
  __dirname,
  "..",
  "public",
  "assets",
  "images",
  "spriteSheets",
);
const outFile = path.join(__dirname, "..", "src", "generatedSpritepacks.ts");

// ---------------------------------------------------------------------------
// Lettura frame da tutti i JSON
// ---------------------------------------------------------------------------

interface FrameCoord {
  ss: number;
  x: number;
  y: number;
}

interface RawPackData {
  cells: Map<number, FrameCoord>;
  decals: Map<number, FrameCoord>;
  specwalls: Map<number, FrameCoord>;
  riemp: Map<number, FrameCoord>;
}

const packs = new Map<number, RawPackData>();

function getOrCreate(packId: number): RawPackData {
  if (!packs.has(packId)) {
    packs.set(packId, {
      cells: new Map(),
      decals: new Map(),
      specwalls: new Map(),
      riemp: new Map(),
    });
  }
  return packs.get(packId)!;
}

const ssFiles = fs
  .readdirSync(ssDir)
  .filter((f) => /^Spritesheet-\d+\.json$/.test(f))
  .sort();

for (const file of ssFiles) {
  const ssIdx = parseInt(file.match(/(\d+)/)![1]!, 10);
  const data = JSON.parse(fs.readFileSync(path.join(ssDir, file), "utf-8")) as {
    frames: Record<
      string,
      { frame: { x: number; y: number; w: number; h: number } }
    >;
  };

  for (const [name, val] of Object.entries(data.frames)) {
    const pm = name.match(/pacco(\d+)/);
    if (!pm) continue;
    const packId = parseInt(pm[1]!, 10);
    const coord: FrameCoord = { ss: ssIdx, x: val.frame.x, y: val.frame.y };
    const pd = getOrCreate(packId);

    let m: RegExpMatchArray | null;
    if ((m = name.match(/\/cell(\d+)$/)))
      pd.cells.set(parseInt(m[1]!, 10), coord);
    else if ((m = name.match(/\/Decals\/decal_(\d+)$/)))
      pd.decals.set(parseInt(m[1]!, 10), coord);
    else if ((m = name.match(/\/SpecialWalls\/wall_special_(\d+)$/)))
      pd.specwalls.set(parseInt(m[1]!, 10), coord);
    else if ((m = name.match(/\/Riempimento\/riempimento_(\d+)$/)))
      pd.riemp.set(parseInt(m[1]!, 10), coord);
  }
}

// ---------------------------------------------------------------------------
// Rilevamento automatico frame placeholder
// ---------------------------------------------------------------------------

const coordFreq = new Map<string, number>();

for (const pd of packs.values()) {
  for (const section of [pd.cells, pd.decals, pd.specwalls] as Map<
    number,
    FrameCoord
  >[]) {
    for (const c of section.values()) {
      const key = `${c.ss}:${c.x}:${c.y}`;
      coordFreq.set(key, (coordFreq.get(key) ?? 0) + 1);
    }
  }
}

// Il placeholder è la coordinata più frequente
const [placeholderKey] = [...coordFreq.entries()].sort(
  (a, b) => b[1] - a[1],
)[0]!;
const [pss, px, py] = placeholderKey.split(":").map(Number) as [
  number,
  number,
  number,
];
const placeholder: FrameCoord = { ss: pss, x: px, y: py };

console.log(
  `Placeholder rilevato: SS${pss} (${px},${py}) — appare ${coordFreq.get(placeholderKey)} volte`,
);

// ---------------------------------------------------------------------------
// Generazione output TypeScript
// ---------------------------------------------------------------------------

const packIds = [...packs.keys()].sort((a, b) => a - b);
console.log(`Pack trovati: ${packIds.join(", ")}`);

function coordStr(c: FrameCoord): string {
  return `{ss:${c.ss},x:${c.x},y:${c.y}}`;
}

function isPlaceholder(c: FrameCoord): boolean {
  return (
    c.ss === placeholder.ss && c.x === placeholder.x && c.y === placeholder.y
  );
}

function sectionCode(m: Map<number, FrameCoord>): string {
  const entries = [...m.entries()].sort((a, b) => a[0] - b[0]);
  return (
    "[" +
    entries
      .map(([, c]) => `{frame:${coordStr(c)},real:${!isPlaceholder(c)}}`)
      .join(",") +
    "]"
  );
}

// Riempimento: prendi dal primo pack che li ha (sono identici per tutti)
const riempSource = packs.get(packIds[0]!)!.riemp;
const riempCode =
  "[" +
  [...riempSource.entries()]
    .sort((a, b) => a[0] - b[0])
    .map(([, c]) => coordStr(c))
    .join(",") +
  "]";

const packsCode = packIds
  .map((id) => {
    const pd = packs.get(id)!;
    return `    ${id}: { cells:${sectionCode(pd.cells)}, decals:${sectionCode(pd.decals)}, specwalls:${sectionCode(pd.specwalls)} },`;
  })
  .join("\n");

const output = `// GENERATO AUTOMATICAMENTE — non modificare manualmente.
// Rigenera con: npx tsx Web/scripts/generate-spritepack-manifest.ts
// oppure tramite: npm run build

export interface FrameCoord { ss: number; x: number; y: number }
export interface PackFrame   { frame: FrameCoord; real: boolean }

export interface PackData {
    cells:     PackFrame[];  // cell1..N in ordine
    decals:    PackFrame[];  // decal_1..N in ordine
    specwalls: PackFrame[];  // wall_special_1..N in ordine
}

/** Frame placeholder — coordinata usata dai pack che non hanno la texture. */
export const PLACEHOLDER_FRAME: FrameCoord = ${coordStr(placeholder)};

/** Pack disponibili (rilevati dai nomi frame negli spritesheet). */
export const PACK_IDS: readonly number[] = [${packIds.join(", ")}] as const;

/** Frame dei 13 Riempimento — identici per tutti i pack. */
export const RIEMPIMENTO_FRAMES: FrameCoord[] = ${riempCode};

/** Dati per pack: celle, decal, muri speciali con flag real/placeholder. */
export const SPRITEPACK_MANIFEST: Readonly<Record<number, PackData>> = {
${packsCode}
};
`;

fs.writeFileSync(outFile, output, "utf-8");
console.info(`Manifesto SpriteSheet per level-editor scritto in ${outFile}`);
