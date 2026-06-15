export interface LevelInfo {
    id: number;
    name: string;
}

// Ogni voce va decommentata quando il livello è pronto.
// Il nome può essere diverso da quello provvisorio nelle linee guida —
// scrivere il nome definitivo al momento dello sviluppo.
// Aggiornare anche LEVEL_DESIGN_GUIDELINES.md se il nome cambia.
export const LEVELS: readonly LevelInfo[] = [
    { id: 1,  name: "L'ingresso" },
    // { id: 2,  name: "Il buio" },
    // { id: 3,  name: "La prima creatura" },
    // { id: 4,  name: "Gli automi" },
    // { id: 5,  name: "I corridoi del rito" },
    // { id: 6,  name: "I soldati di polvere" },
    // { id: 7,  name: "La sorveglianza" },
    // { id: 8,  name: "La rabbia cieca" },
    // { id: 9,  name: "I guardiani rotti" },
    // { id: 10, name: "Il senso di colpa" },
    // { id: 11, name: "Fermo nel caos" },
    // { id: 12, name: "Il sabotaggio" },
    // { id: 13, name: "La privazione" },
    // { id: 14, name: "Il labirinto vero" },
    // { id: 15, name: "L'odio" },
    // { id: 16, name: "Il cacciatore instancabile" },
    // { id: 17, name: "Tutto insieme" },
    // { id: 18, name: "La prova definitiva" },
    // { id: 19, name: "Il Ventre del Padre" },
    // { id: 20, name: "Il giardino vuoto" },
];

export function getLevelName(levelId: number): string {
    return LEVELS.find(l => l.id === levelId)?.name ?? `Livello ${levelId}`;
}
