/**
 * Genera Web/src/generatedLevelManifest.ts scansionando le sottocartelle
 * di Web/public/assets/levels/. Eseguire con:
 *   npx tsx Web/scripts/generate-level-manifest.ts
 */

import * as fs from "fs";
import * as path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const levelsDir = path.join(__dirname, "..", "public", "assets", "levels");
const outputFile = path.join(
  __dirname,
  "..",
  "src",
  "generatedLevelManifest.ts",
);

function capitalize(s: string): string {
  if (!s) return s;
  return s.charAt(0).toUpperCase() + s.slice(1);
}

/** Legge il nome dal file .mark (riga 2 se non contiene '|', altrimenti vuoto). */
function readLevelName(filePath: string): string {
  try {
    const content = fs.readFileSync(filePath, "utf-8");
    const lines = content.split("\n");
    if (lines.length >= 2 && !lines[1]!.includes("|")) {
      return lines[1]!.trim();
    }
  } catch {
    // file non leggibile: ignora
  }
  return "";
}

interface LevelEntry {
  id: number;
  name: string;
}

interface FolderEntry {
  folder: string;
  displayName: string;
  levels: LevelEntry[];
}

const entries: FolderEntry[] = [];

const subdirs = fs
  .readdirSync(levelsDir, { withFileTypes: true })
  .filter((d) => d.isDirectory())
  .map((d) => d.name)
  .sort();

console.info(`\n${"=".repeat(60)}`);
console.info(`  GENERATE-LEVEL-MANIFEST`);
console.info(`  Scansiona Web/public/assets/levels/ e genera il manifesto`);
console.info(`  dei livelli in generatedLevelManifest.ts`);
console.info(`${"=".repeat(60)}`);
console.info(`\n  ⚠️  Vengono inclusi SOLO file con nome numerico: es. 1.mark, 2.mark`);
console.info(`     File come TEST_GHOST.mark o draft.mark vengono ignorati.\n`);

for (const dir of subdirs) {
  const dirPath = path.join(levelsDir, dir);
  const markFiles = fs.readdirSync(dirPath).filter((f) => f.endsWith(".mark"));

  if (markFiles.length === 0) continue;

  const levels: LevelEntry[] = markFiles
    .map((f) => {
      const id = parseInt(path.basename(f, ".mark"), 10);
      if (isNaN(id)) return null;
      const name = readLevelName(path.join(dirPath, f));
      return { id, name };
    })
    .filter((e): e is LevelEntry => e !== null)
    .sort((a, b) => a.id - b.id);

  if (levels.length === 0) continue;

  entries.push({
    folder: dir,
    displayName: capitalize(dir),
    levels,
  });

  console.info(
    `  📁 ${dir}: ${levels.map((l) => `${l.id} "${l.name || "(senza nome)"}"`).join(", ")}`,
  );
}

const foldersCode = entries
  .map((e) => {
    const levelsStr = e.levels
      .map((l) => `      { id: ${l.id}, name: ${JSON.stringify(l.name)} }`)
      .join(",\n");
    return (
      `    { folder: ${JSON.stringify(e.folder)}, displayName: ${JSON.stringify(e.displayName)}, levels: [\n` +
      `${levelsStr}\n` +
      `    ] as const },`
    );
  })
  .join("\n");

const output = `// GENERATO AUTOMATICAMENTE — non modificare manualmente.
// Rigenera con: npx tsx Web/scripts/generate-level-manifest.ts

export interface LevelEntry {
    id: number;
    name: string;
}

export interface FolderManifest {
    folder: string;
    displayName: string;
    levels: readonly LevelEntry[];
}

export const LEVEL_FOLDERS: readonly FolderManifest[] = [
${foldersCode}
];
`;

fs.writeFileSync(outputFile, output, "utf-8");
console.info(
  `\n✅ Manifesto livelli scritto → ${path.relative(process.cwd(), outputFile)}`,
);
console.info(
  `   Cartelle: ${entries.map((e) => `${e.folder} (${e.levels.length} livelli)`).join(", ")}`,
);
