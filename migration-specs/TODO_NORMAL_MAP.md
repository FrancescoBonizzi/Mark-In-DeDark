Cosa ti consiglio concretamente

1. Isola il problema in uno scaffold minimale
   Invece di far debuggare l'intera app, crea un test case minimo: una sola mesh, una sola normal map, un singolo shader. Fallo funzionare lì prima. Questo riduce drasticamente il contesto che Claude deve gestire e rende il feedback visivo immediato.
2. Usa Claude come "traduttore riga per riga" invece che come "risolutore"
   Cambia il prompt da "fixa la normal map" a qualcosa come:

"Ecco lo shader HLSL originale che funziona in DirectX. Ecco il mio shader GLSL/PixiJS attuale. Fai una comparazione riga per riga e segnala ogni differenza semantica: sistemi di coordinate, convenzioni sulle normali (Y-up vs Y-down), spazi (tangent space vs world space), formato dei vertex attribute."

Questo funziona molto meglio perché Claude eccelle nel confronto strutturato e nell'analisi delle differenze, molto più che nel "ragionare su cosa potrebbe essere rotto." 3. Dai a Claude una checklist delle differenze note DirectX → WebGL
Metti nel prompt (o in un file CLAUDE.md) le differenze chiave che contano per il rendering:

DirectX usa Y-down per le texture, WebGL Y-up → le normal map potrebbero avere il canale green invertito
DirectX usa matrici row-major, GLSL column-major
DirectX clip space Z va da 0 a 1, WebGL da -1 a 1
Il winding order dei triangoli potrebbe essere invertito (causando backface culling → schermo nero)
PixiJS ha la sua pipeline di rendering e i suoi uniform che possono sovrascrivere o conflittuare con shader custom

Questo dà a Claude un "albero decisionale" invece di lasciarlo vagare. 4. Approccio incrementale con validazione visiva
Per ogni step di rendering, fai fare a Claude un output di debug visivo: renderizza solo le normali come colore (rgb = xyz), renderizza solo la diffuse, renderizza solo il risultato del dot product luce-normale... Se un passaggio è sbagliato, lo vedi subito e dai a Claude un feedback preciso ("le normali appaiono tutte blu uniforme" → il tangent space non è calcolato).

Gemini ha detto
Passare da DirectX a WebGL/PixiJS è un'impresa notevole e scontrarsi con il "black screen" (lo schermo nero) quando si toccano gli shader o il rendering pipeline è un rito di passaggio classico. Capisco perfettamente la tua frustrazione: quando un LLM non riesce a "vedere" l'output visivo e cerca di indovinare la matematica di un problema grafico, inizia a riscrivere codice a caso, rompendo anche il rendering di base e peggiorando la situazione.

L'approccio del tuo amico e la tua idea di prendere il controllo manuale sono due reazioni molto sensate. Analizziamo come procedere.

L'approccio Agentico del tuo amico: Ha senso per te?

Quello che sta facendo il tuo amico (usando un pattern Multi-Agente con Planner, Architect, Implementer e Reviewer) è una delle strategie migliori per lo sviluppo di nuove funzionalità o per la migrazione in blocco di moduli standard.

Tuttavia, applicarlo al tuo caso specifico e dentro Claude Code presenta due criticità:

Claude Code non è nato per questo: È un tool da riga di comando (CLI) ottimizzato per il loop rapido "chiedi-esegui-leggi file". Configurare un'architettura multi-agente rigida e parallela al suo interno non è immediato come scriversi uno script custom in Python con framework dedicati (come LangChain o CrewAI).

È l'approccio sbagliato per i bug grafici: Non hai bisogno di un "Plan Architect" per capire perché una normal map non si applica. Hai un problema di basso livello (matematica vettoriale, shader o data buffer). Un'orchestra di agenti che non "vede" lo schermo nero continuerà a passarsi la palla proponendo fix matematicamente plausibili ma visivamente errati.

La Strategia Consigliata: "Micro-Agentica" + Isolamento Manuale

È arrivato il momento di cambiare radicalmente il modo in cui chiedi aiuto all'AI. Smetti di chiederle di "sistemare il rendering" e inizia a usarla come un Consulente Tecnico e Strumento di Debug, tenendo tu le redini.

Ecco il piano d'azione:

1. Ferma l'AI dal fare "Refactoring"

Quando un'AI pensa per 20 minuti su un problema di rendering, sta probabilmente cambiando l'intera architettura della pipeline. Dalle istruzioni molto stringenti.

Prompt consigliato: "Agisci come un Senior WebGL Engineer. NON riscrivere l'intero sistema. Analizza il mio fragment/vertex shader attuale e indicami solo dove la matematica potrebbe fallire nel calcolo delle tangenti o della luce."

2. Riduci al minimo (Isolamento)

In grafica, se c'è schermo nero, un dot product sta dando un valore negativo o un colore sta venendo moltiplicato per zero. Chiedi a Claude di aiutarti a creare shader di debug.

Chiedigli di fare il render esclusivamente delle coordinate UV a schermo (spesso le UV si rompono passando da C# a JS).

Chiedigli di fare il render della Normal Map direttamente come output RGB crudo, saltando il calcolo della luce. Se vedi i colori blu/viola della normal map, sai che la texture è caricata. Se è nero, il problema è il caricamento della texture in PixiJS, non lo shader.

3. Fai attenzione alle trappole DirectX vs WebGL

Essendo una conversione da DirectX a WebGL, il problema al 90% risiede in una di queste differenze architetturali. Usa queste informazioni per "guidare" i ragionamenti di Claude:

Sistema di Coordinate: DirectX è solitamente Left-Handed, mentre WebGL è Right-Handed. L'asse Z è invertito.

Coordinate UV (V-Flip): In DirectX l'origine (0,0) delle texture è in alto a sinistra. In WebGL/OpenGL è spesso in basso a sinistra. Le normal map potrebbero avere il canale Verde (asse Y) invertito (Y- vs Y+).

Matrici: L'ordine di moltiplicazione nei vertex shader cambia (Vector _ Matrix in HLSL vs Matrix _ Vector in GLSL).

4. Dai "Occhi" all'AI usando strumenti esterni (Fondamentale)

L'AI fallisce perché non vede i buffer della GPU.

Usa un tool come Spector.js (un'estensione per il browser fantastica per WebGL).

Cattura un frame del tuo canvas WebGL.

Controlla i "Vertex Attributes" e i "Uniforms". La texture della normal map è davvero lì? L'attributo delle tangenti (a_tangent) è popolato da numeri o da zeri?

Passa questi dati testuali a Claude: "Ho ispezionato il frame con Spector.js. La texture della normal map è bindata correttamente, ma l'attributo a_tangent è un array di zeri. Come risolvo la generazione delle tangenti in PixiJS?" Quando dai questi dati specifici, l'AI diventa uno strumento incredibilmente preciso e smette di allucinare soluzioni.

In sintesi: sì, ha senso che tu ti metta "a mano" a risolvere il problema, ma puoi farlo affiancato dall'AI. Usa Claude per farti scrivere mini-test visivi e per chiarire dubbi sulle API di PixiJS, ma sii tu l'orchestratore del debugging.
