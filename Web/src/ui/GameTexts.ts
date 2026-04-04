/** Gameplay strings (Italian) and numeric stringCode → text mapping. */
export const GameTexts = {
    notEnoughCandles:  'Non hai candele',
    keyFound:          'Hai trovato una chiave...',
    closedDoor:        'Tutto chiuso...',
    gameOver:          'Game Over!',
    levelCompleted:    'Livello completato!',
    candlePackFound:   (n: number) => `Hai trovato ${n} ${n === 1 ? 'candela' : 'candele'}`,
} as const;

/** Maps numeric stringCode keys (from .mark files) to Italian tutorial text. */
const STRING_CODE_MAP: Record<string, string> = {
    // Livello 1
    '100000000000000001': 'Mio padre mi ha stretto la mano troppo forte, come al solito.',
    '100000000000000101': 'Il nome di mio fratello è graffiato su questa porta.',
    '100000000000000102': 'Sotto di lui, altri nomi.',
    '100000000000000005': "'Torna uomo o non tornare.'",
    '100000000000000002': "La candela puzza di qualcosa che non voglio sapere cos'è.",
    '100000000000000003': 'Ci sono nomi graffiati sui muri. Ne riconosco qualcuno.',
    '100000000000000004': 'Il nome di mio fratello è graffiato su questa porta. Sotto di lui, altri nomi.',
    // Livello 3+
    '637284382845827427': 'Una porta... premi CTRL per aprirla o chiuderla',
    '637284382983500501': 'Una candela... raccoglila e premi SPAZIO per posizionarla',
    '637284384552007485': 'Non è rassicurante...',
    '637284386793405944': 'Magari dovrei provare con una candela',
    '637284387001107200': "Ce l'ho fatta!",
    '637284388013304946': 'Questo qualcosa... sembra comportarsi in modo strano',
};

export function getStringCodeText(code: string): string | undefined {
    return STRING_CODE_MAP[code];
}
