# CLAUDE.md — Mark In DeDark

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

- **Sorgente C# originale:** `Csharp/`
- **Migration Specs:** `migration-specs/ARCHITECTURE_MAP.md` (enum tables, oggetti non ancora portati), `PATTERN_COOKBOOK.md`.
- **Assets pubblici:** `Web/public/assets/` (spritesheets, sounds, levels)
- **Livelli:** `.mark` (testo pipe-delimited, parsati da `grid/GridLoader.ts`)
- **progetto-riferimento**: `/progetto-riferimento` contiene un altro gioco molto più semplice di questo che ho convertito manualmente da C# a Typescript/PixiJs (trovi entrambe le versioni). Usalo come riferimento per le parti di conversione a PixiJs, ma considera che non c'è nulla, in quel progetto, di relativo a rendering avanzato come shader, normal map, ecc.

---

## Workflow

- Per ogni bug: analizza prima il C# originale in `/Csharp`. Usa `progetto-riferimento/` se proprio hai dei dubbi per passare da CSharp a TS per le parti a certe conversioni in PixiJs. Non modificare senza aver letto la parte `/Csharp` originale.
- Risolvi autonomamente: analizza log, esegui il test Playwright, itera senza chiedere guida passo passo.
- Non segnare un task come completato senza che `npm run build` passi e il test Playwright confermi il comportamento.
- Dopo correzioni dell'utente: aggiorna `tasks/lessons.md` con il pattern per prevenire la ricorrenza.
- Impatto minimo: modifica solo ciò che serve. Niente refactor non richiesti, niente astrazioni speculative.
