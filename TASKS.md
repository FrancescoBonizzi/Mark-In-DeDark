# Task

- Alcuni sono da eseguire, flaggali come fatti
- Altri sono di approfondimento, sono quelli che iniziano con "Task di approfondimento" in quel caso crea un nuovo task in un heading h2 a parte in cui descrivi come lo faresti, ma non implementarlo. Tutti gli altri task implementali subito invece.

---

## Resoconto notturno — decisioni e ragionamenti

Questa sezione viene aggiornata durante l'esecuzione dei task. Leggila domattina.

### Decisioni preliminari concordate

- **Nome livello nel .mark**: aggiunta riga 2 dedicata (solo il nome, testo plain) dopo `GameGridSerializerV4`. Il parser viene aggiornato per leggere e ignorarla in retrocompatibilità. Tutti i `.mark` esistenti vengono aggiornati con un nome inventato coerente.
- **Livelli da generare stanotte**: solo livelli 2–5 (Atto I). Livelli 6–20 rimandati.
- **Estetica menu**: dark amb ambra/arancione come le candele. Horror ma elegante.
- **Bug "tutto blu" LevelEditor**: già risolto, saltato.
- **Autonomia**: procedo con giudizio, ogni scelta non ovvia è documentata qui sotto.

### Log esecuzione task

**Timer rimosso** ✅ — `GameHud.ts`: rimosso `_timerEl` dal DOM e dalla logica `update`. Il parametro `elapsedMs` resta nella firma (prefissato `_`) per non rompere i call site esistenti in `gamebootstrap.ts`.

**Script manifest** ✅ — Rimosso `console.info()` vuoto iniziale (era il "logo inutile"). Aggiunte emoji nei log: 🗺️ 📁 ✅ per level-manifest, 🔍 🎨 ✅ per spritepack-manifest.

**Colore ingresso/uscita LevelEditor** ✅ — Cambiato `#00FFFF` (cyan brillante, testo bianco illeggibile) a `#006868` (teal scuro), stesso aspetto riconoscibile ma con contrasto adeguato per il testo.

**Goblin trigger LevelEditor** ✅ — Rinominato da `TriggerGoblin` a `Trappola Goblin ⚡` nel tab Terra (ground type 5), colore reso meno fluorescente. Aggiunto hint nel tab Nemici: "⚡ Per far spawnare un Goblin usa Trappola Goblin nel tab Terra".

**Nome livello nel .mark** ✅ — Aggiunta riga 2 (plain text, senza pipe) dopo `GameGridSerializerV4`. Retrocompatibilità: se la riga 2 contiene `|` è il vecchio formato, il parser gestisce entrambi. Modificati: `GridTypes.ts` (campo `levelName`), `GridLoader.ts`, `GridSerializer.ts`, `ascii-to-mark.ts` (flag `-n "Nome"`), `validate-level.ts`, `generate-level-manifest.ts` (legge nome dai file, lo include nel manifest), `generatedLevelManifest.ts` (tipo `LevelEntry` con `id+name`), `folderlevels.ts` (mostra nome nei bottoni), `gameover.ts` (usa `LevelEntry.name`). Tutti i .mark esistenti aggiornati con nomi inventati coerenti. LevelEditor: campo "Nome:" in toolbar, persiste in stato e nel file salvato.

**Bug texture pack LevelEditor** ✅ — Root cause: `buildToolbar` catturava `state` come parametro al momento della costruzione; dopo `state = result.state` (open file), i listener continuavano a mutare il vecchio oggetto. Fix: rimossi i direct mutations dai listener in `buildToolbar`, aggiunti callback (`onPackChanged`, `onDirtyChanged`, `onFilenameChanged`, `onLevelNameChanged`) che mutano il `state` nell'outer scope dove vive sempre il riferimento corretto.

**Pagine menu redesign** ✅ — Redesign completo con estetica Gothic Dungeon Crawler: sfondo quasi-nero, luce ambra pulsante, ember CSS animati. Logo PNG originale con glow candle. Menu principale: 3 bottoni con taglio obliquo (clip-path), angoli dorati all'hover. Selezione livelli: "default" = story card hero con occhi rossi animati in background (blink CSS), espansioni in griglia secondaria. Lista livelli: righe con numero/nome e accento arancione al hover. Blocco dispositivi touch: `DeviceGuard.ts` shared utility, schermata di errore elegante. `hasPhysicalKeyboard()` estratto dal `gamebootstrap.ts` nel servizio condiviso.

---

- [x] LevelEditor: il colore per il muro ingresso/uscita è troppo forte per il testo bianco, cambia uno dei due
- [x] LevelEditor: bug, se io apro un livello che un dato texture pack e cambio il texture pack, poi salvo o eseguo il gioco, non viene applicato il nuovo texture pack.
- [x] Script di generazione manifest (entrambi), migliora un po' i log, falli più carini con qualche emoji e rimuovi i logo inutili
- [x] LevelEditor: il trigger goblin non c'è tra i possibili oggetti da piazzare
- [x] LevelEditor: fenomeno paranormale (già risolto in precedenza, saltato)
- [x] LevelEditor: attualmente non abbiamo un nome per ogni livello. Lo dobbiamo aggiungere, lo aggiugiamo da qualche parte nel file .mark, magari come prima riga. Così uno può chiamare il livello come gli pare (uno così può chiamare il livello 1.mark, 2.mark ecc così vengono ordinati nel gioco) ma avere il nome che preferisce. Il nome deve essere specificabile nel level editor, deve finire nel file .mark e deve essere utilizzato alla creazione del manifest per popolare poi nella pagina dei livelli, il nome sul pulsante.
- [x] Rimuovi il tempo che scorre dalla schermata di gioco, non serve più a nulla
- [x] Task di approfondimento: aggiungimi qui in questo file un'ipotesi di miglioria della UI: in game per quanto riguarda dove mettiamo i testi (tipo il numero di candele). E vorrei anche aggiungere "non hai ancora trovato la chiave" o "hai una chiave per uscire". Non mi dispiace mettere tutto in basso in una barra stile giochi retrò
- [x] Task di approfondimento: aggiungimi qui in questo file un'ipotesi di miglioria della UI: revisione di come appaiono i testi in game, parlo dei testi popup che appaiono ogni tanto. Il problema è attualmente sono dei popup che appunti fanno fadeout volando in alto. Non è male, ma non mi piace che si vedano solo una tantum e poi non li puoi più leggere. Potremmo pensare di mostrarli sempre fissi quando cammini sulla cella di riferimento, ma mi chiedevo se secondo te ci fosse un modo migliore per mostrarli. Hai delle idee? Accetto ipotesi.
- [x] Task di approfondimento: aggiungi qui in questo file un'ipotesi (se ne hai una), se ci sono migliorie che posso fare al gioco per renderlo più giocabile, senza introdurre nuove texture, ma solo cambiando alcune piccole cose. Fai un'analisi e fammi delle proposte
- [x] Task di approfondimento: rileggiti il file docs/ENEMY_AI e valuta migliorie alle AI dei nemici. Ovviamente l'anima dei nemici va bene che resti così com'è, ma valuta se si può fare qualcosa di meglio o se alcuni nemici, boh, si possono rendere più idiomatici. Sono disposto ad usare a dare a bubu o ghost qualcosa di più coinvolgente, se ti vengono in mente idee.
- [x] Dobbiamo fare delle modifiche alle pagine del menù.
  - [x] Il gioco originale aveva un logo: Csharp/Assets/logo.png usalo in giro
  - [x] C'era anche uno sfondo per la pagina dei livelli
  - [x] Revisione pagine navigazione con frontend-design
  - [x] Blocco dispositivi touch-only / schermo piccolo
- [x] Genera livelli 2–5 in @LEVEL_TASKS

---

## Approfondimento 1 — HUD in-game: barra retrò in basso

### Proposta

Sostituire l'attuale HUD in cima (candele flottanti + popup volanti) con una **barra fissa in basso** a tutta larghezza, stile videogiochi retrò anni '90. La barra è sempre visibile, semi-trasparente su sfondo scuro.

**Layout proposto (da sinistra a destra):**

```
[ 🕯 3 candele ]          [ ⚠ Non hai trovato la chiave ]          [ LIVELLO 2 ]
```

oppure, se la chiave è raccolta:

```
[ 🕯 2 candele ]          [ 🗝 Hai la chiave — trovate l'uscita ]   [ LIVELLO 2 ]
```

**Dettaglio implementazione:**

1. Nella `GameHud`, aggiungi `_keyEl: HTMLElement` (stato chiave) e `_levelEl: HTMLElement` (nome livello).
2. Il metodo `update()` riceve anche `hasKey: boolean` → aggiorna il testo e il colore (bianco/ambra se ha la chiave, rosso se non ce l'ha).
3. Il gioco passa il nome del livello al `GameHud` alla creazione (dalla `GridBundle.levelName`).
4. CSS: `#game-hud { position: fixed; bottom: 0; left: 0; right: 0; background: rgba(0,0,0,0.7); border-top: 1px solid #6b3a0a; padding: 6px 16px; display: flex; justify-content: space-between; }`.
5. I popup rimangono ma salgono dal basso (sopra la barra) invece di partire dall'alto.

**Impatto:** modifica di `GameHud.ts`, CSS `GameHud.css` (o stile inline), e `gamebootstrap.ts` per passare `hasKey` all'`update()`.

---

## Approfondimento 2 — Popup testi narrativi: proposta di miglioramento

### Problema attuale

I popup appaiono una sola volta (fadeout volante verso l'alto) quando il giocatore calpesta una cella con `stringCode`. Una volta scomparsi non si possono rileggere.

### Analisi opzioni

**Opzione A — "Mostra quando sei sulla cella" (idea originale dell'utente)**
Il testo rimane fisso (niente animazione) finché il giocatore è sulla cella trigger. Quando se ne va, il testo scompare.
- Pro: sempre leggibile se ci torni
- Contro: rovina la narrativa atmosferica — diventa un "cartello" meccanico, perde il senso di ricordo/pensiero spontaneo

**Opzione B — "Diario persistente" (mia proposta preferita)**
Ogni testo narrativo è memorizzato in un piccolo log accessibile con un tasto (es. `T` o `TAB`). In game appare un'icona discreta nell'angolo quando c'è un nuovo messaggio non letto (`✦`). Premendo il tasto si apre un overlay semi-trasparente con tutti i messaggi letti in ordine cronologico.
- Pro: atmosfera mantenuta (il primo incontro è sempre il popup volante), niente perso, si può rileggere a piacimento
- Contro: aggiunge un overlay UI e un altro tasto da ricordare

**Opzione C — "Popup con persistenza breve" (compromesso)**
Il popup vola in alto ma invece di sparire completamente, rimane nell'angolo in miniatura (piccolo, semi-opaco) per 30 secondi. Si fa click per rileggere o scompare da solo.
- Pro: elegante, non invade
- Contro: complesso da implementare bene su canvas

**Opzione D — "Prima apparizione + ripetizione audio breve"**
Il testo appare la prima volta come ora. Quando il giocatore torna sulla cella, il testo riappare senza animazione per 2 secondi (reminder silenzioso). Non si "accumula" più volte.
- Pro: semplice, poco invasivo, funziona per testi importanti
- Contro: il giocatore deve tornare sulla cella specifica

**Raccomandazione:** Opzione B (Diario) per i livelli con molto testo narrativo (dall'Atto II in poi), abbinata all'Opzione D come fallback per tutti i livelli. Si implementa in due fasi: prima D (basso costo), poi B se vuoi aggiungere profondità narrativa.

**Implementazione minima (Opzione D):**
- `TriggerObject` tiene un flag `_hasTriggered: boolean`
- Alla prima attivazione: popup normale (animazione volante)
- Alle attivazioni successive: popup breve (2 s, niente animazione, opacità 0.6)
- Nessuna UI aggiuntiva richiesta

---

## Approfondimento 3 — Migliorie giocabilità senza nuove texture

### Analisi

Ho analizzato il gameplay sulla base del codice esistente. Queste sono le frizioni più probabili per un nuovo giocatore, e come risolverle con cambiamenti minimi.

---

**1. Il giocatore non sa che deve raccogliere la chiave prima di arrivare all'uscita**

Il gioco non lo spiega da nessuna parte. La chiave blocca l'uscita silenziosamente (probabilmente il player non esce anche se ci cammina sopra, senza feedback).

→ **Fix**: aggiungere feedback all'uscita quando il giocatore non ha la chiave: un suono breve + popup HUD "Trova la chiave prima di uscire". Una riga di codice in `Exit.ts` quando il player interagisce senza avere la chiave.

---

**2. Non è chiaro dove si "usa" la candela vs dove si "raccoglie"**

Le candele del pacchetto si raccolgono calpestando, le singole candele si accendono con interazione. Per un nuovo giocatore può essere confuso.

→ **Fix**: il livello 1 (già fatto) deve spiegarlo col testo narrativo. Nessuna modifica al codice necessaria — è un problema di level design.

---

**3. Il Ghost è praticamente impossibile da gestire se il giocatore non sa "stai fermo"**

La meccanica è geniale ma non è comunicata. Il giocatore reagisce istintivamente scappando e muore.

→ **Fix**: testo narrativo nel livello dove appare il Ghost per la prima volta ("Immobile. Devo restare immobile."). Già nella linea guida, assicurarsi che il livello 10 (Ghost) abbia un trigger di testo obbligatorio che spiega questo.

---

**4. Il Taurus distrugge muri ma non c'è nessun indizio visivo prima della carica**

Il wind-up da 1 secondo con colore blu è sottile. Se il giocatore non sa che il Taurus carica, viene preso di sorpresa.

→ **Fix**: aggiungere un suono di "allerta" più marcato durante il wind-up (già esiste `"TaurusStartingCharge"` — verificare che sia abbastanza udibile). Il colore blu può essere reso più pulsante (animazione CSS sull'entità, no texture).

---

**5. Buio totale = confusione spaziale**

Quando le candele finiscono o il giocatore è nel buio (GroundType 7), non è chiaro dove si trova rispetto ai muri.

→ **Fix**: il player emette sempre una luce ambientale minima (raggio 0.5-1 cella, intensità molto bassa) che non illumina il testo ma permette di vedere i muri immediatamente adiacenti. Modifica di `Player.ts` nella configurazione luce.

---

**6. La velocità del giocatore rispetto ai nemici varia molto per livello — alcuni livelli sembrano "impossibili"**

Il Goblin che accelera ogni volta che usi una candela può diventare velocissimo e irrecuperabile se il giocatore è abituato ad usarne molte.

→ **Fix**: aggiungere un cap visivo: quando il Goblin supera una certa velocità soglia, mostrare un indicatore "il goblin si sta avvicinando" nella HUD. Non cambia la meccanica, solo aumenta la leggibilità.

---

## Approfondimento 4 — Analisi e migliorie AI nemici

### Valutazione corrente

Leggendo `ENEMY_AI.md`, le AI esistenti sono solide e idiomatiche per il genere. I problemi non sono di correttezza ma di **leggibilità del comportamento** per il giocatore e di **potenziale inespresso** di alcuni nemici.

---

**Zombie — OK com'è**
Semplice e prevedibile: perfetto come primo nemico. L'unica miglioria sarebbe un suono distinto quando il player entra nel suo "campo visivo" (in realtà nessuno), ma è fuori scope.

---

**Ghost — potrebbe avere un comportamento più teatrale**

Attualmente patruglia ignorando i muri e attacca solo se il giocatore si muove. L'idea è bella ma il Ghost "si comporta normalmente" tranne per la condizione di attacco. Manca teatralità.

**Proposta concreta:** aggiungere uno stato `Hovering` dove il Ghost si ferma brevemente (0.5 s) su una cella casuale vicina al giocatore prima di riprendere il percorso. Effetto: sembra che il Ghost "annusi" la presenza del player, creando tensione anche quando il player è fermo. Implementazione: aggiungere un check in `PatrolGhostAI.update()` — se il Ghost è entro N celle dal player (es. 4), probabilità X% ogni N frame di fare una pausa. Non cambia la meccanica letale ma aumenta la tensione percepita.

---

**Skeleton — quasi perfetto**

La fase di Incredulity (guarda a destra e sinistra per 3 secondi) è già molto evocativa. L'unica cosa che manca: quando lo Skeleton torna a pattugliare dopo aver perso il giocatore, riparte dal waypoint più vicino invece di continuare dal punto in cui si trovava. Questo è un limite del `PatrolAI` che resetta l'indice al waypoint più vicino. Non è un bug ma può sembrare strano visivamente.

**Proposta**: nessuna modifica necessaria — l'AI è già di alto livello per il genere.

---

**Goblin — meccanica di escalation molto efficace, ma manca un segnale**

Il Goblin diventa sempre più veloce con le candele usate, ma il player non ha feedback visivo di questo. Dopo 6 candele il Goblin è a velocità massima e sembra "impossibile".

**Proposta concreta:** aggiungere un cambio di colore progressivo al Goblin come fa lo Skeleton in modalità rabbia — ma graduale: a 0 candele usate colore base, a 3 un arancione, a 6 rosso pieno. Solo una riga in `Goblin.update()` per aggiornare il tint del corpo. Nessuna texture nuova.

---

**Taurus — OK com'è, ma il wind-up è sottile**

Come scritto in Approfondimento 3 punto 4: suono più marcato durante il wind-up. Il comportamento è già idiomatico e memorabile.

---

**Bubu — ha molto potenziale inespresso**

Attualmente: patruglia, cerca candele, le spegne. Non emette luce. È "invisibile nel buio".

**Proposta più ambiziosa (consigliata):** aggiungere un breve **suono di respiro** o fruscio ogni N secondi quando il Bubu è vicino al giocatore (es. entro 5 celle), senza che il Bubu cambi comportamento. Il player sente qualcosa vicino ma non lo vede. Effetto orror puro — aumenta enormemente la tensione senza modificare la meccanica. Implementazione: in `Bubu.update()`, calcola distanza Manhattan dal player → se sotto soglia → chiama `SoundManager.playIfNotPlaying('bubu_nearby')` con rate casuale (ogni 2-4 secondi). Serve solo un nuovo file audio di 0.5 secondi.

---

**Proposta trasversale — "allarme nemico" audivisivo**

Quando un nemico rileva il player (Skeleton entra in Following, Goblin viene spawned dal trigger), aggiungere un suono di allerta breve e distinto per tipo nemico. Questo aiuta il player a capire cosa sta succedendo anche quando il nemico è fuori campo visivo. Probabilmente già parzialmente implementato per Taurus/Skeleton — verificare se i sound asset esistono.

