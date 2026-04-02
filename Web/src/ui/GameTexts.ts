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
