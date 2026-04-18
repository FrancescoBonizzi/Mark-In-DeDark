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
    { folder: "default", displayName: "Default", levels: [
      { id: 1, name: "Il giardino" },
      { id: 2, name: "L'ingresso" },
      { id: 3, name: "La prima creatura" },
      { id: 4, name: "Gli automi" }
    ] as const },
    { folder: "demo", displayName: "Demo", levels: [
      { id: 1, name: "Il Giardino" },
      { id: 2, name: "Grotta di Pietra" },
      { id: 3, name: "Caverne del Buio" },
      { id: 4, name: "Antri Profondi" },
      { id: 5, name: "Il Tempio" },
      { id: 6, name: "Sale Oscure" },
      { id: 7, name: "Catacomba" },
      { id: 8, name: "Corridoi Erosi" }
    ] as const },
];
