# TODO

## Fase 1

- [x] Farlo funzionare su MacOs
  - [ ] L'audio non va
- [ ] Far funzionare il LevelEditor con Avalonia UI

## Fase 2 - Migrazione PixiJs

- Prompt per iniziare
  Contesto: sto migrando videogiochi da MonoGame (C#) a PixiJS (TypeScript).

Ho già un progetto migrato manualmente che useremo come riferimento:

- Riferimento migrato: /progetto-riferimento/ (troverai due cartelle: CSharp che il progetto
  originale e Web che è il migrato perfettamente funzionante, fatto da me a mano)
- Progetto da migrare: Csharp/Starfall/ (e tutti i progetti correlati nella stessa directory).
- Il migrato andrà in Web/

Target tech stack della migrazione:

- Lo stesso identico del progetto-riferimento (guarda il suo package.json)

Prima di scrivere qualsiasi codice di migrazione:

1. Esplora entrambi i progetti e comprendi la struttura
2. Crea migration-specs/ARCHITECTURE_MAP.md
   (mappatura MonoGame → PixiJS di classi, pattern, lifecycle)
3. Crea migration-specs/PATTERN_COOKBOOK.md
   (pattern C# ricorrenti con equivalente TS estratto dal riferimento)
4. Crea CLAUDE.md con le regole di migrazione da seguire in ogni sessione
5. Proponimeli per review e aspetta la mia approvazione

Non iniziare la migrazione finché non approvo esplicitamente i tre file.

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
- Troverai alcuni progetti che avevo usato per compilare multipiattaform (Windows Desktop, UWP,
  Android - fregatene)

## Fase 3 - Erogarlo sul mio sito

- [ ] Sistemare i livelli demo, e magari chiedere a Marco
- [ ] Fare in modo che funzioni online
- [ ] Post LinkedIn e tutto il resto
- [ ] Open source
