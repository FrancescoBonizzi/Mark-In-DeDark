import * as fs from "fs";
import * as path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const sourceFile = path.join(
  __dirname,
  "..",
  "..",
  "docs",
  "LEVEL_DESIGN_GUIDELINES.md",
);
const outputFile = path.join(
  __dirname,
  "..",
  "src",
  "generatedLevelDesignGuidelines.ts",
);

if (!fs.existsSync(sourceFile)) {
  throw new Error(
    "Missing required documentation file: docs/LEVEL_DESIGN_GUIDELINES.md. " +
      "The build fails intentionally to keep Level Editor guidelines in sync.",
  );
}

const markdown = fs.readFileSync(sourceFile, "utf-8");

const output =
  `// GENERATO AUTOMATICAMENTE — non modificare manualmente.\n` +
  `// Rigenera con: npx tsx scripts/generate-level-design-guidelines.ts\n\n` +
  `export const LEVEL_DESIGN_GUIDELINES_MD = ${JSON.stringify(markdown)};\n`;

fs.writeFileSync(outputFile, output, "utf-8");
console.info(
  `✅ Level design guidelines generated -> ${path.relative(process.cwd(), outputFile)}`,
);
