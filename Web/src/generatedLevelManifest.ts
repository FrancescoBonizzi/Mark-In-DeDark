// GENERATO AUTOMATICAMENTE — non modificare manualmente.
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
    { folder: "ai", displayName: "Ai", levels: [
      { id: 1, name: "L'ingresso" },
      { id: 2, name: "Il buio" },
      { id: 3, name: "La prima creatura" },
      { id: 4, name: "Gli automi" },
      { id: 5, name: "I corridoi del rito" }
    ] as const },
    { folder: "default", displayName: "Default", levels: [
      { id: 1, name: "Primo livello" },
      { id: 2, name: "Secondo livello" },
      { id: 3, name: "Terzo livello" }
    ] as const },
    { folder: "demo", displayName: "Demo", levels: [
      { id: 1, name: "Demo - Il Giardino" },
      { id: 2, name: "Demo - Grotta di Pietra" },
      { id: 3, name: "Demo - Caverne del Buio" },
      { id: 4, name: "Demo - Antri Profondi" },
      { id: 5, name: "Demo - Il Tempio" },
      { id: 6, name: "Demo - Sale Oscure" },
      { id: 7, name: "Demo - Catacomba" },
      { id: 8, name: "Demo - Corridoi Erosi" }
    ] as const },
];
