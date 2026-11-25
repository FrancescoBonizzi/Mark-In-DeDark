/** Gameplay strings (Italian). */
export const GameTexts = {
    notEnoughCandles:  'Non hai candele',
    keyFound:          'Hai trovato una chiave...',
    closedDoor:        'Trova la chiave prima di uscire',
    goblinApproaching: 'Il goblin si sta avvicinando',
    loadingLevel:      'Accensione della candela…',
    levelLoadFailed:   'Ooops! Il livello non si è caricato.',
    gameOver:          'Game Over!',
    levelCompleted:    'Livello completato!',
    candlePackFound:   (n: number) => `Hai trovato ${n} ${n === 1 ? 'candela' : 'candele'}`,
} as const;
