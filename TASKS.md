# Task

- Alcuni sono da eseguire, flaggali come fatti
- Altri sono di approfondimento, sono quelli che iniziano con "Task di approfondimento" in quel caso crea un nuovo task in un heading h2 a parte in cui descrivi come lo faresti, ma non implementarlo. Tutti gli altri task implementali subito invece.

---

---

## TASK DA FARE - Migliorie giocabilità

**1. Il giocatore non sa che deve raccogliere la chiave prima di arrivare all'uscita** ✅

Il gioco non lo spiega da nessuna parte. La chiave blocca l'uscita silenziosamente (probabilmente il player non esce anche se ci cammina sopra, senza feedback).

→ **Fix**: aggiungere feedback all'uscita quando il giocatore non ha la chiave: popup HUD "Trova la chiave prima di uscire". Una riga di codice in `Exit.ts` quando il player interagisce senza avere la chiave.

---

**2. Non è chiaro dove si "usa" la candela vs dove si "raccoglie"** ✅

Le candele del pacchetto si raccolgono calpestando, le singole candele si accendono con interazione. Per un nuovo giocatore può essere confuso.

→ **Fix**: il livello 1 (già fatto) deve spiegarlo col testo narrativo. Nessuna modifica al codice necessaria — è un problema di level design. -> descritto nelle LEVEL_DESIGN_GUIDELINES

---

**3. Il Ghost è praticamente impossibile da gestire se il giocatore non sa "stai fermo"** ✅

La meccanica è geniale ma non è comunicata. Il giocatore reagisce istintivamente scappando e muore.

→ **Fix**: testo narrativo nel livello dove appare il Ghost per la prima volta ("Immobile. Devo restare immobile."). -> descritto nelle LEVEL_DESIGN_GUIDELINES e aggiornato LEVEL_TASKS col livello 10

---

**4. Il Taurus distrugge muri ma non c'è nessun indizio visivo prima della carica** ✅

Il wind-up da 1 secondo con colore blu è sottile. Se il giocatore non sa che il Taurus carica, viene preso di sorpresa.

→ **Fix**: Il colore di windup ora pulsa rapidamente (~4Hz) tra blu scuro e blu brillante, con opacity che varia tra 0.25 e 0.80. Implementato in `ChargeDirectionAI.ts`.

---

**5. La velocità del giocatore rispetto ai nemici varia molto per livello — alcuni livelli sembrano "impossibili"** ✅

Il Goblin che accelera ogni volta che usi una candela può diventare velocissimo e irrecuperabile se il giocatore è abituato ad usarne molte.

→ **Fix**: aggiunto cambio di colore progressivo (bianco→arancione→rosso in base alle candele usate) e notifica HUD "Il goblin si sta avvicinando" quando la velocità supera la soglia di 500ms/cella. Implementato in `Goblin.ts` e `gamebootstrap.ts`.

## DA RIFLETTERE ANCORA

**Ghost — potrebbe avere un comportamento più teatrale**

Attualmente patruglia ignorando i muri e attacca solo se il giocatore si muove. L'idea è bella ma il Ghost "si comporta normalmente" tranne per la condizione di attacco. Manca teatralità.

**Proposta concreta:** aggiungere uno stato `Hovering` dove il Ghost si ferma brevemente (0.5 s) su una cella casuale vicina al giocatore prima di riprendere il percorso. Effetto: sembra che il Ghost "annusi" la presenza del player, creando tensione anche quando il player è fermo. Implementazione: aggiungere un check in `PatrolGhostAI.update()` — se il Ghost è entro N celle dal player (es. 4), probabilità X% ogni N frame di fare una pausa. Non cambia la meccanica letale ma aumenta la tensione percepita.

---

**Bubu — ha molto potenziale inespresso**

Attualmente: patruglia, cerca candele, le spegne. Non emette luce. È "invisibile nel buio".

**Proposta più ambiziosa (consigliata):** aggiungere un breve **suono di respiro** o fruscio ogni N secondi quando il Bubu è vicino al giocatore (es. entro 5 celle), senza che il Bubu cambi comportamento. Il player sente qualcosa vicino ma non lo vede. Effetto orror puro — aumenta enormemente la tensione senza modificare la meccanica. Implementazione: in `Bubu.update()`, calcola distanza Manhattan dal player → se sotto soglia → chiama `SoundManager.playIfNotPlaying('bubu_nearby')` con rate casuale (ogni 2-4 secondi). Serve solo un nuovo file audio di 0.5 secondi.

---

## Analisi approfondimento — Come implementerei le proposte "DA RIFLETTERE ANCORA"

### Ghost — stato Hovering per teatralità

**Dove:** `Web/src/ai/PatrolGhostAI.ts`

**Come:**
1. Aggiungere `enum GhostPhase { Patrol, Hovering }` e `_phase = GhostPhase.Patrol`.
2. In `update()`, dopo il movimento patrol standard: se `_phase === Patrol` e il Ghost è entro 4 celle dal player (distanza Manhattan su griglia), generare un numero random ogni 60 frame; se < 0.15 (15% di probabilità), passare a `Hovering` per 500ms.
3. In `Hovering`: non restituire alcun input di movimento (restituire `CharacterActions.Idle`), decrementare il timer. Alla scadenza, tornare a `Patrol`.
4. Nessun cambio alla condizione di attacco (che è in `GhostEnemy.ts`, non nell'AI).

**Impatto:** zero sul gameplay, alta teatralità. Il Ghost sembra "fiutare" la presenza del player.

### Bubu — suono di respiro di prossimità

**Dove:** `Web/src/characters/enemies/Bubu.ts`

**Come:**
1. Aggiungere un timer `_nearbyBreathTimer = 0` e una soglia random `_nextBreathAt = randomBetween(2000, 4000)`.
2. In `update()`: calcolare distanza Manhattan tra `this.currentCell` e `player.currentCell`. Se < 5 celle, incrementare il timer. Quando supera `_nextBreathAt`, chiamare `SoundManagerInstance.play('bubuNearby', '...path...')` e resettare il timer con nuovo valore random.
3. Se il player si allontana (distanza ≥ 5), resettare il timer a 0.
4. Serve solo il file audio `Web/public/assets/sounds/effects/BubuNearby.wav` (0.5s, volume basso).
