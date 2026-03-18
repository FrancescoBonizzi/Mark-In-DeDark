Contesto: sto migrando videogiochi da MonoGame (C#) a PixiJS (TypeScript).

## RIFERIMENTI

Ho già un progetto migrato manualmente che useremo come riferimento:

- Riferimento migrato: /progetto-riferimento/ (troverai due cartelle: CSharp che il progetto
  originale e Web che è il migrato perfettamente funzionante, fatto da me a mano)
- Progetto da migrare: Csharp (e tutti i progetti correlati nella stessa directory).
- Il migrato andrà in Web/
- Target tech stack della migrazione: lo stesso identico del progetto-riferimento (guarda il suo package.json)

## STRATEGY & PROJECT MANAGEMENT (Anti-Complexity Protocol)

Data la complessità del progetto Mark In DeDark, non procedere in modo lineare. Applica una gestione rigorosa per "moduli atomici":

1. Esplora entrambi i progetti e comprendi la struttura
2. Crea migration-specs/ARCHITECTURE_MAP.md
   (mappatura MonoGame → PixiJS di classi, pattern, lifecycle)
3. Crea migration-specs/PATTERN_COOKBOOK.md
   (pattern C# ricorrenti con equivalente TS estratto dal riferimento)
4. Crea CLAUDE.md con le regole di migrazione da seguire in ogni sessione
5. MIGRATION_PROGRESS.md: Come primo atto dopo l'approvazione delle specifiche, crea questo file. Deve contenere una Roadmap suddivisa in Task (es. Core, Systems, Entities, UI, Shaders). Ogni task deve avere uno stato: [Pending], [In Progress], [Completed], [Verified].
6. Bottom-Up Approach: Inizia la migrazione dalle classi senza dipendenze (Utilities, Math, Modelli dati). Procedi poi verso i sistemi (Input, Camera, Collisioni) e solo alla fine alla logica delle Entità e delle Scene.
7. Dependency-First: Prima di migrare una classe, verifica che tutte le sue dipendenze siano già presenti nel progetto Web. Se mancano, migra prima quelle o crea degli "stub" temporanei se strettamente necessario.
8. Deep Analysis vs. Lazy Translation: Non limitarti a tradurre la sintassi da C# a TS. Comprendi l'intento del codice originale. Se una funzione in C# è un workaround per un limite di MonoGame che in PixiJS non esiste, adattala seguendo la "filosofia Web" del progetto-riferimento.
9. Validation Checkpoints: Alla fine di ogni Task significativo, fermati. Riassumi cosa è stato fatto, segnala eventuali deviazioni dal riferimento e chiedi conferma prima di passare al task successivo.

Non iniziare la migrazione finché non approvo esplicitamente questi file di progetto.

Note finali:

- Penso che il primo step della migrazione sia quello di scaffoldare il progetto base con il game
  cycle e con la gestione degli asset (che non saranno più xnb ma direttamente i file)
- I file di spritesheet vanno rimappati sul modello del progetto-riferimento
- La base del ParticleSystem la puoi copiare pari pari dal progetto-riferimento
- Il Game.ts è il punto d'ingresso principale e secondo me ti fa capire molto della struttura
- La Camera del progetto di riferimento secondo me puoi copiarla pari pari
- CollisionSolver, ScoreRepository ecc idem
- La gestione delle Pages (GameOver, MainMenu, Score) le puoi copiare pari pari
- interaction/Controller pure
- Ricordati che tutta la base "tecnica" del gioco probabilmente sarà riutilizzabile al 99% per il
  gioco che stai per migrare, quindi non reinventare la ruota, copiala di peso nel caso. Considerale
  le tue primitive per lo sviluppo
- Nel progetto-riferimento/web/docs ci sono informazioni per te che devi tenere a mente e applicare quando necessario

Informazioni sul progetto as is:

- Troverai alcuni progetti che avevo usato per compilare multipiattaform (Windows Desktop, UWP,
  Android - fregatene, puoi eliminarli tranquillamente)
- Il gioco utilizza anche un LevelEditor che attualmente NON tocchiamo, è un'applicazione standalone che non ci interessa in questo momento
- Il gioco utilizza una libreria che si chiama MarkInDeDark.Shadows che è una libreria per generare ombre in tempo reale con alcuni shader, anche questa andrà tradotta. Puoi integrarla nello stesso progetto senza bisogno di separarla come ho fatto nella versione c#.
- Si utilizzano gli shader. In questo momento sono shader opengl, considera che dovrai tradurli in shader utilizzabili da pixijs che dalla versione 8 usa WebGPU quindi WGSL, informati tu comunque.
