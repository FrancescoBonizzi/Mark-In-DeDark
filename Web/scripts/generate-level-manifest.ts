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

interface FolderEntry {
  folder: string;
  displayName: string;
  levels: number[];
}

const entries: FolderEntry[] = [];

const subdirs = fs
  .readdirSync(levelsDir, { withFileTypes: true })
  .filter((d) => d.isDirectory())
  .map((d) => d.name)
  .sort();

for (const dir of subdirs) {
  const dirPath = path.join(levelsDir, dir);
  const markFiles = fs.readdirSync(dirPath).filter((f) => f.endsWith(".mark"));

  if (markFiles.length === 0) continue;

  const levels = markFiles
    .map((f) => parseInt(path.basename(f, ".mark"), 10))
    .filter((n) => !isNaN(n))
    .sort((a, b) => a - b);

  if (levels.length === 0) continue;

  entries.push({
    folder: dir,
    displayName: capitalize(dir),
    levels,
  });
}

const foldersCode = entries
  .map(
    (e) =>
      `    { folder: ${JSON.stringify(e.folder)}, displayName: ${JSON.stringify(e.displayName)}, levels: [${e.levels.join(", ")}] as const },`,
  )
  .join("\n");

const output = `// GENERATO AUTOMATICAMENTE — non modificare manualmente.
// Rigenera con: npx tsx Web/scripts/generate-level-manifest.ts

export interface FolderManifest {
    folder: string;
    displayName: string;
    levels: readonly number[];
}

export const LEVEL_FOLDERS: readonly FolderManifest[] = [
${foldersCode}
];
`;

fs.writeFileSync(outputFile, output, "utf-8");
console.info(`Manifesto livelli .mark scritto in ${outputFile}`);
console.info(
  `Cartelle trovate: ${entries.map((e) => `${e.folder} (${e.levels.length} livelli)`).join(", ")}`,
);
