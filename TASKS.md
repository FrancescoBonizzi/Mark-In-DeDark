# Task

- Alcuni sono da eseguire, flaggali come fatti
- Altri sono di approfondimento, sono quelli che iniziano con "Task di approfondimento" in quel caso crea un nuovo task in un heading h2 a parte in cui descrivi come lo faresti, ma non implementarlo. Tutti gli altri task implementali subito invece.

---

- [x] LevelEditor: quando salvo continuo ad avere l'errore: "• ERRORE: Riga 2: formato dimensioni non valido. Atteso "RIGHE|COLONNE"." Penso sia perché abbiamo cambiato il formato aggiungendo il titolo del gioco, adegua anche il validatore
- [x] LevelEditor: non trovo il pulsante per piazzare il trigger goblin
- [x] LevelEditor: imponi che i muri e le porte possano solo essere INTERNI alla cella mai esterni
- [x] Task di approfondimento: come posso far distinguere l'entrata dall'uscita? → vedi Approfondimento 5
- [x] Animazione uscita: nel vecchio gioco il player quando era sull'uscita con la chiave, partiva un'animazione in cui andava verso la porta e diventava più piccolo e faceva fadeout. Guarda il vecchio codice CSharp.
- [x] Quando si esce dal livello perché è completato, l'animazione va bene ma è scentrata rispetto alla texture della porta. L'ominio dovrebbe entrare centralmente nella porta
- [x] Completa i TASK DA FARE più complessi scritti qui sotto

---

## TASK DA FARE — Distinguere entrata da uscita

### Problema

Le due porte (Ingresso e Uscita) sono visivamente quasi identiche. Il colore diverso non basta a renderle intuitive. In più, l'uscita si apre nel momento in cui il giocatore raccoglie la chiave — chi ci passa sopra dopo trova una porta già aperta e non capisce perché ce l'ha la chiave.

**Parte A — Distinguere visivamente entrata e uscita**

**Luce colorata diversa** (già parzialmente fatto)
Intensificare la differenza: ingresso con luce calda (ambra/arancione), uscita con luce fredda (ciano), e aggiungere una pulsazione sulla porta dell'uscita quando è aperta.

- Pro: atmosfera, no asset nuovi
- Contro: meno intuitivo rispetto a un simbolo esplicito

---

**Parte B — La porta si apre "da sola" quando prendi la chiave**

Il problema: appena il giocatore raccoglie la chiave, `exit.setOpen()` chiama `_exitDoor.setOpen()` che apre la porta visivamente. Se il giocatore poi si avvicina all'uscita trova una porta già aperta e la connessione causale (chiave → porta) non è chiara.

Da fare entrambe:

**B1 — Porta rimane chiusa visivamente, si apre solo all'interazione**
Il giocatore, quando cammina sull'uscita con la chiave, vede la porta aprirsi in quel momento. La chiave "si usa" sul posto.

- Come implementare: non chiamare `_exitDoor.setOpen()` in `key.collect()`. Chiamarlo invece in `gamebootstrap.ts` quando il player entra nella collision rect dell'uscita con `key.isCollected === true`.
- Impatto minimo: 2-3 righe in `gamebootstrap.ts`, zero modifiche al sistema porte.
- Pro: connessione causale chiara (metti piede sulla porta, la porta si apre)
- Contro: il giocatore deve sapere dove andare

**B2 — Porta aperta, ma con chiave visibile sulla porta fino all'arrivo**
Finché il giocatore non arriva davanti alla porta, appare una chiave rotante sopra (stessa sprite della chiave attuale, ma rimpicciolita).

- Implementazione: sprite key overlay sull'uscita, rimosso quando il player entra nella collision.

---

## TASK DA FARE — HUD in-game: barra retrò in basso

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

## TASK DA FARE 2 — Popup testi narrativi: proposta di miglioramento

### Problema attuale

I popup appaiono una sola volta (fadeout volante verso l'alto) quando il giocatore calpesta una cella con `stringCode`. Una volta scomparsi non si possono rileggere.

### Analisi opzioni

**Sviluppo A — "Mostra quando sei sulla cella"**
Il testo rimane fisso (niente animazione) finché il giocatore è sulla cella trigger. Quando se ne va, il testo scompare.

**Sviluppo B — "Diario persistente"**
Ogni testo narrativo è memorizzato in un piccolo log accessibile con un tasto (es. `T` o `TAB`). In game appare un'icona discreta nell'angolo quando c'è un nuovo messaggio non letto (`✦`). Premendo il tasto si apre un overlay semi-trasparente con tutti i messaggi letti in ordine cronologico.

Fare entrambi.

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
