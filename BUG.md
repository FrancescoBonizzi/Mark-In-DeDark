# Bug

Valutare le differenze rispetto alla versione CSharp originale e implementare le correzioni.

- [ ] La trappola continua a "muoversi", animarsi anche quando non c'è più nessuno sopra. Inoltre la texture della parte sotto alla copertura della trappola, non c'è, c'è solo la tile "SLOT". Per entrambe le trappole vanno usata la texture trap_breaking e trapHole. Guarda com'era la versione CSharp originale, cerca: Csharp/MarkInDeDark.Library/Objects/TimedBreakableTrap.cs e Csharp/MarkInDeDark.Library/Objects/InstantlyDieTrap.cs. Adegua il comportamento sia logico che grafico
      Plan:
  - Allineare il modello di trigger Web a quello C#: oggi `Web/src/objects/TriggerObject.ts` gestisce un solo character per chiamata e resetta male il ciclo `pressed/left`; il riferimento corretto e` `Csharp/MarkInDeDark.Library/Objects/TriggerObject.cs`. Va deciso se rendere il trigger multi-character come nel legacy o se mantenere il trigger stateless e far iterare i chiamanti. La raccomandazione e` portare la semantica C#: il trigger deve sapere chi c'e`sopra, aggiornarsi ogni frame e invocare`onTriggerLeft` solo quando l'area si svuota davvero.
  - Correggere `Web/src/objects/TimedBreakableTrap.ts`: in `Idle` non deve animare la cover, in `GoingToBreak` deve animarla e scuotere solo mentre qualcuno e`sopra, in`Breaking`deve mostrare`trap_breaking`, in `Broken`deve lasciare visibile il fondo`trapHole`. Oggi il fondo non esiste e si vede solo la tile base del terreno.
  - Introdurre anche per la trappola temporizzata una rappresentazione persistente del buco, come nel C# dove `TimedBreakableTrap` incapsula una `InstantlyDieTrap`. La raccomandazione e`riusare davvero`InstantlyDieTrap` come sotto-oggetto invece di duplicare logica e sprite.
  - Correggere `Web/src/objects/InstantlyDieTrap.ts`: il buco deve esistere sempre come sprite/animazione di base `Grounds/trapHole/*`, fermo sul primo frame quando inattivo e animato solo quando qualcuno cade, esattamente come nel legacy.
  - Verificare il layering in `groundContainer` e gli z-index per evitare che il fondo della trappola venga coperto dalla tile di terra o da decal non dovute.
  - Validazione: aggiungere/aggiornare un test Playwright di debug su un livello con entrambe le trappole, controllando tre stati distinti: inattiva, in breaking, rotta. Se serve, aggiungere log temporanei su transizioni di stato e rimuoverli a fine fix.
- [ ] Verifica che anche i mostri muoiano sulle trappole
      Plan:
  - Verificare il loop principale in `Web/src/pages/gamebootstrap.ts`: oggi le trappole vengono valutate solo con `trap.checkAndTrigger(player)`, quindi i mostri non passano mai nella pipeline dei trigger. Questo e` quasi certamente il root cause.
  - Allineare la lista degli `affectedCharacters` al comportamento C#: sia il player sia i nemici devono essere registrati sulle trappole, incluse le entita`spawnate dopo l'inizializzazione come i Goblin della`GoblinTriggerTrap`.
  - Decidere l'ordine di update per evitare race condition: raccomandazione `player.update/enemy.update` prima, poi valutazione trappole su tutti i character vivi, poi `trap.update`. In questo modo la collisione usa la posizione finale del frame.
  - Verificare che `Character.fallInTrap()` funzioni anche per i nemici senza rompere gli handler di morte standard e senza lasciare riferimenti vivi in AI/light manager.
  - Aggiungere un test di regressione che metta almeno uno zombie su una trappola istantanea e una temporizzata, e verifichi la transizione a `Dying/Dead`.
- [ ] Verifica che il dirty sia applicato correttamente e nel claso applicalo
      Plan:
  - Audit del path attuale: `Web/src/grid/GameGrid.ts` applica il dirty in `_assignDirtySprite()` durante la creazione celle e `Web/src/grid/GameCell.ts` lo disegna come overlay a z-index `Z_GROUND_DIRTY`. Va confrontato col comportamento C# per capire se manca solo qualche esclusione o se manca la specular suppression sulle celle trigger.
  - Verificare le esclusioni corrette: oggi le trappole impostano `cell.noDecals = true`, ma il trigger Web non marca un equivalente di `HasATriggerObject` del C#. Va controllato se questo produce differenze visuali su specular/glow o su celle speciali oltre alle trappole.
  - Verificare che il dirty sia applicato solo a celle walkable di terra e non a `Buio`, `Riempimento`, trappole, o altre celle che nel legacy non ricevono decal. La raccomandazione e`centralizzare il criterio in`GameCell`/`GameGrid` invece di spargere eccezioni.
  - Se il comportamento visivo differisce dal C#, aggiungere screenshot/debug capture su livelli campione con `DIRTY_PROBABILITY_PCT` alto per validare distribuzione, layering e compatibilita` con blood decal.
  - Se durante l'audit emerge che il dirty e` gia` corretto, aggiornare il task con esito e chiudere senza introdurre cambi superflui.
- [ ] Gli UnpassableObject non sembrano implementati. Implementali guardando qui: /Users/fbonizzi/Source/Mark-In-DeDark/Csharp/MarkInDeDark.Library/Objects/UnpassableObject.cs
      Plan:
  - Il bug e`confermato:`Web/src/grid/GameGrid.ts`raccoglie`OggettoNonPassabile1/2`come se fossero`enemyPlacements`, ma `Web/src/characters/enemies/EnemyFactory.ts` ignora quei tipi. Quindi oggi non vengono ne` renderizzati ne` resi bloccanti.
  - Introdurre oggetti ambientali dedicati lato Web, separati dai nemici, con parita` minima al C#: sprite centrato sulla cella e cella resa non attraversabile. La raccomandazione e` creare `UnpassableObject` e `PassableObject` anche nel Web, con rendering su `objectsContainer` e z-index coerente con gli altri oggetti ambientali.
  - Definire dove marcare il blocco di pathfinding/movimento: il riferimento C# imposta `gameCell.IsUnwalkable = true`. Nel Web oggi non esiste un flag equivalente su `GameCell`, quindi va aggiunto un attributo esplicito consumato da pathfinding e `isPassableWithAction`, invece di piegare impropriamente i muri.
  - Aggiornare bootstrap/factory per creare questi oggetti dai placements della griglia e mantenerli distinti dagli enemy placements.
  - Aggiungere test di regressione: rendering dell'oggetto corretto e impossibilita` per player/nemici di attraversare la cella occupata.
- [ ] La glow decal viene utilizzata?
      Plan:
  - L'audit iniziale dice che la texture glow non e`morta:`Web/src/pages/gamebootstrap.ts`carica`Effects/glow`, `Web/src/lighting/ShadowSystem.ts`la usa per i pass specular/lightmask, e`Web/src/objects/BrazierLit.ts` la usa anche come glow visivo sopra il braciere.
  - Verificare se il dubbio riguarda una texture presente ma mai visibile oppure una feature diversa rispetto al C#. La raccomandazione e`confrontare il path Web con`Csharp/MarkInDeDark.Library/Lighting/MarkLight.cs`e`Csharp/MarkInDeDark.Library/Objects/BrazierLit.cs`, non introdurre nuovo codice finche` non si dimostra una mancanza reale.
  - Se il problema e` solo di visibilita`, usare un test/debug level con specular attivo per confermare che il glow compaia nei pass corretti e che `glowScale` per livello sia applicato.
  - Se invece emerge che manca un uso specifico del glow su qualche entita` portata male, aprire un task secondario mirato indicando esattamente l'entita` mancante.
- [ ] La texture snoflake non viene utilizzata per far nevicare, man -> /Users/fbonizzi/Source/Mark-In-DeDark/Csharp/MarkInDeDark.Library/Particles/Snowing/SnowFLake.cs
      Plan:
  - Il gap e`confermato: nel Web esiste l'asset`Effects/snowflake`, ma non esiste nessun uso runtime o flag equivalente a `IsSnowing`; nel C# la neve viene attivata da `LevelCustomizations`per i livelli 1-2 e istanziata in`MarkInDeDarkGame`tramite`SnowingManager`.
  - Portare prima la configurazione: estendere `Web/src/grid/LevelCustomization.ts` con un flag `isSnowing` allineato al C# (`true` per livelli 1-2, `false` altrove).
  - Portare poi il runtime: introdurre un sistema particellare leggero dedicato alla neve, separato da `TorchFireParticleSystem`, che usi `Effects/snowflake` e segua le stesse regole di base del legacy: spawn graduale, velocita` verticale, lieve vento laterale, rimozione fuori schermo, gestione rispetto alla camera.
  - Collegare il sistema al bootstrap di gioco in modo che venga creato solo nei livelli con neve e venga aggiornato/distrutto insieme al resto delle risorse di scena.
  - Validazione: controllare che i livelli 1-2 nevichino e che gli altri restino invariati, senza introdurre regressioni di performance o layering sopra HUD/menu.
- [ ] LevelEditor: quando utilizzi il comando "Cancella" fai cancellare anche il testo aggiunto come testo visualizzabile in game
      Plan:
  - Il bug e`confermato: in`Web/src/pages/leveleditor/editorState.ts`, `setObjectType(..., null)`cancella solo`objectType`e`objectAdditionalData`, ma non azzera `stringCode`. Questo lascia il marker/testo di cella anche dopo il comando `Cancella`.
  - Decidere il comportamento esatto del tool `Cancella`: raccomandazione coerente con la richiesta utente, il comando deve ripulire l'intera cella sul piano "contenuto gameplay", quindi oggetto e testo visualizzabile in game.
  - Implementare il reset di `stringCode` nel ramo di cancellazione oggetti e verificare che il marker `A` sparisca subito dalla UI della cella.
  - Aggiornare il test Playwright del level editor (`Web/tests/debug-leveleditor.spec.ts`) aggiungendo il caso combinato: testo presente, selezione `Cancella`, marker assente e stringa non serializzata nel `.mark` salvato.
