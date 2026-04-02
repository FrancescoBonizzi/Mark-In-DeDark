# CLAUDE.md — Mark In DeDark

Letto all'inizio di ogni sessione prima di scrivere codice. Tu sei l'ingegnere software incaricato della migrazione da C# a TypeScript/PixiJS.

---

## Stato del progetto & Piano di Attacco

Siamo in una fase di bugfixing, la maggior parte del progetto è migrata.

Regole di bugfixing:

- Il funzionamento preciso della vecchia pipeline di rendering è qui, leggila prima di fare qualunque cosa: `migration-specs/RENDERING_PIPELINE_CSHARP.md`.
- Il funzionamento preciso della nuova pipeline di rendering è qui, , leggila prima di fare qualunque cosa: `migration-specs/RENDERING_PIPELINE_PIXIJS.md`.
- Per iterare in autonomia senza disturbarmi puoi inserire da solo dei debug console, e lanciare il test automatico `npx playwright test tests/debug-game.spec.ts`, questo fa girare il gioco sul livello 3 per 10 secondi e scrive tutto l'output della console in `debug/game-debug-output.txt`, così puoi fare tutto da solo e iterare a volontà. (attualmente punta al livello 8)

### Task 1

Il nemico "Skeleton" non c'è. Per esempio nel livello 8 dovrebbe esserci. Controlla che tutti i nemici siano stati implementati.

### Task 2

Non c'è l'ombra del player (che non è calcolata in realtime, è una semplice texture animata)

### Task 3

Rispetto al gioco originale, la versione PixiJs sembra molto più zommata. Nel gioco originale sembra tutto più "piccolo", come se fosse zoomed-out.

### Task 4

La texture dei pavimenti sono parecchio desaturate.

### Task 5

Tutte le luci: quelle del player, quelle dei braceri, quelle dei nemici, sembrano ampie il doppio rispetto all'originale. Considera che nel gioco originale la luce del player, per darti una proporzione, illumina fino a una cella oltre la sua. Quindi se è al centro di 9 celle, illumina solo quelle adiacenti, non di più.

### Task 6

Il player, posizionato sulla candela, dovrebbe avere un fioco effetto particellare che simula gli scoppiettii del fuoco sulla candela. Qui non c'è.

### Task 7

Quando sono appena sopra rispetto ad una porta appena aperta, non illumino la cella su cui sono. Come se la porta aperta in qualche modo occludesse la cella appena sopra.

### Task 8

Fai le scritte popup text di un 20% più grandi

### Task 9

Per aprire le porte utilizza il tasto CTRL e non più SPAZIO. SPAZIO lo usiamo solo per piazzare la candela. Aggiorna anche i testi popup/tutorial relativi.

### Task 10

Ho notato che intorno ad alcuni oggetti tipo la chiave e i braceri e le candele da raccogliere c'è un alone nero

### Task 11

Nei testi popup, quando c'è una candela sola scrivi hai raccolto 1 candela, non 1 candele (gestisci singolare plurale del testo)

### Task Bonus

Dopo massimo 5 minuti in cui gioco, il browser fa crashare la pagina perché "utilizza una quantità significativa di memoria"

---

## Protocollo di Risoluzione Task (Leggere prima di agire)

Per task complessi, applica questo workflow rigoroso, salvo diverse indicazioni:

1. **Analisi:** Leggi i file C# originali responsabili del comportamento atteso (in `progetto-riferimento/`).
2. **Ispezione:** Leggi l'attuale implementazione TypeScript/Shader.
3. **Documentazione (NO CODICE):** Aggiorna prima i file `migration-specs/PATTERN_COOKBOOK.md` o `ARCHITECTURE_MAP.md` spiegando il problema (es. differenze matematiche, coordinate invertite).
4. **Implementazione:** Inserisci sempre un breve commento in cima al file modificato spiegando _cosa_ e _perché_ stai cambiando rispetto al C#.

---

## Regole Invarianti (Hard Rules)

- **Shader:** Ogni shader deve avere sia WGSL (WebGPU) che GLSL (WebGL). Mai GLSL-only.
- **TypeScript:** Nessun tipo `any`. I campi opzionali nelle classi vanno dichiarati come `T | undefined` (non `?: T`) per via di `exactOptionalPropertyTypes`.
- **Game Lifecycle:** Richiama `destroyGame()` prima di ogni `initGame()`.
- **Routing:** Navigazione tra pagine tramite `window.location.hash = '#/route'` (evita import circolari con `router.ts`).
- **Rendering Debug:** Per problemi visivi, controlla sempre l'ordine di z-index di PixiJS prima di alterare la logica delle classi visive.
- **Build:** `npm run build` deve passare senza errori dopo la risoluzione di ogni singolo task.

---

## Tech Stack & Riferimenti

| Tool       | Version    | Note                                    |
| ---------- | ---------- | --------------------------------------- |
| Vite       | 7.x        | `base` = `/mark-in-dedark/`             |
| TypeScript | 5.7 strict | `exactOptionalPropertyTypes: true`      |
| PixiJS     | 8.6.x      | WebGPU primario, WebGL fallback         |
| Howler     | 2.2.x      | Audio; singleton `SoundManagerInstance` |
| Navigo     | 8.x        | Hash router (`{ hash: true }`)          |

- **Sorgente C# originale:** `progetto-riferimento/`
- **Migration Specs:** `migration-specs/ARCHITECTURE_MAP.md` (enum tables, oggetti non ancora portati), `PATTERN_COOKBOOK.md`.
- **Assets pubblici:** `Web/public/assets/` (spritesheets, sounds, levels)
- **Livelli:** `.mark` (testo pipe-delimited, parsati da `grid/GridLoader.ts`)

---

## Orchestrazione del Workflow

### 1. Modalità Pianificazione (Predefinita)

- Entra in modalità pianificazione per QUALSIASI attività non banale (3+ passaggi o decisioni architetturali)
- Se qualcosa va storto, FERMATI e pianifica di nuovo immediatamente
- Usa la modalità pianificazione anche per i passaggi di verifica, non solo per la costruzione
- Scrivi specifiche dettagliate in anticipo per ridurre l'ambiguità

### 2. Strategia dei Subagenti

- Usa i subagenti liberamente per mantenere pulita la finestra di contesto principale
- Delega ricerca, esplorazione e analisi parallela ai subagenti
- Per problemi complessi, usa più risorse computazionali tramite subagenti
- Un task per subagente per un'esecuzione focalizzata

### 3. Ciclo di Auto-Miglioramento

- Dopo QUALSIASI correzione da parte dell'utente: aggiorna tasks/lessons.md con il pattern
- Scrivi regole per te stesso che prevengano lo stesso errore
- Itera senza pietà su queste lezioni finché il tasso di errore diminuisce
- Rivedi le lezioni all'inizio della sessione per il progetto rilevante

### 4. Verifica Prima di Considerare Completato

- Non segnare mai un task come completato senza dimostrare che funziona
- Confronta il comportamento tra la versione principale e le tue modifiche quando rilevante
- Chiediti: "Un ingegnere senior approverebbe questo?"
- Esegui test, controlla i log, dimostra la correttezza

### 5. Pretendi Eleganza (Bilanciata)

- Per modifiche non banali: fermati e chiedi "esiste un modo più elegante?"
- Se una soluzione sembra un hack: "Sapendo tutto ciò che so ora, implementa la soluzione elegante"
- Salta questo passaggio per fix semplici e ovvi - evita l'over-engineering
- Metti in discussione il tuo lavoro prima di presentarlo

### 6. Risoluzione Autonoma dei Bug

- Quando ricevi un bug report: risolvilo e basta. Non chiedere guida passo passo
- Analizza log, errori, test falliti - poi risolvili
- Nessun bisogno di far cambiare contesto all'utente
- Risolvi i test CI falliti senza che ti venga detto come

## Gestione dei Task

1. Pianifica Prima: Scrivi il piano in tasks/todo.md con elementi verificabili
2. Verifica il Piano: Fai un check prima di iniziare l'implementazione
3. Traccia i Progressi: Segna gli elementi come completati man mano
4. Spiega le Modifiche: Fornisci un riepilogo ad alto livello a ogni step
5. Documenta i Risultati: Aggiungi una sezione di revisione in tasks/todo.md
6. Registra le Lezioni: Aggiorna tasks/lessons.md dopo le correzioni

## Principi Fondamentali

- Prima la Semplicità: Rendi ogni modifica il più semplice possibile. Impatta il minimo codice.
- Zero Pigrizia: Trova le cause radice. Niente soluzioni temporanee. Standard da sviluppatore senior.
- Impatto Minimo: Modifica solo ciò che è necessario. Nessun effetto collaterale o nuovi bug.
