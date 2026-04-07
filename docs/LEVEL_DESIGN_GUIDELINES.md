# Linee Guida di Design — Mark In DeDark

Guida per la creazione di livelli coerenti con il tono, la narrativa e la progressione del gioco.
Per la struttura tecnica del file `.mark` vedi [`LEVEL_FORMAT.md`](./LEVEL_FORMAT.md).

---

## VINCOLI INVIOLABILI — leggere PRIMA di qualsiasi altra cosa

Da verificare PRIMA di creare il livello — senza eccezioni.

### 1. DIMENSIONI — non superare la bounding box

Ogni livello ha una bounding box massima (vedi tabella in `LEVEL_TASKS.md`). Un livello fuori dimensione è un bug di design, non una questione estetica.

Il livello 1 è piccolo per definizione: **9–11 righe × 22–28 colonne**. Un giardino enorme non è "più immersivo" — è un errore.

### 2. TRIGGER DI TESTO — solo in corridoi obbligati da 1 cella

Il trigger `!` si attiva solo quando il player calpesta quella cella esatta. Se il player può aggirarla, il testo non parte mai.

**Regola di verifica:** prima di piazzare un `!`, controlla che la sua riga (o colonna) non abbia celle floor sia sopra che sotto (o sia a sinistra che a destra). Se ci sono → il trigger è aggirabile → **non metterlo lì**.

Pattern VALIDO (corridoio 1 cella):

```
▓ ▓ ▓ ▓ ▓ ▓ ▓
▓ · · ! · · ▓   ← fill sopra E sotto: il player DEVE passare su !
▓ ▓ ▓ ▓ ▓ ▓ ▓
```

Pattern INVALIDO (trigger aggirabile):

```
▓ · · · · · ▓
▓ · · ! · · ▓   ← il player passa sopra o sotto !
▓ · · · · · ▓
```

Pattern VALIDO per area aperta (giardino livello 1 — trigger nei colli di bottiglia):

```
▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓
▓ ▓ ▓ · · · · · · · · · ▓ ▓ ▓ ▓
▓ ! · · · · K · · · · · · ! E ▓   ← trigger a sx (ingresso) e dx (uscita): OBBLIGATI
▓ ▓ ▓ · · · · · · · · · ▓ ▓ ▓ ▓
▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓
```

L'area interna può essere ampia — i trigger stanno nei colli di bottiglia di ingresso/uscita.

### 3. LINEARITÀ — il grafo delle stanze è una catena

Se la spec dice "lineare" o "strettamente lineare":

- Il grafo è: `Spawn → A → B → … → Uscita`
- Nessuna stanza ha più di 2 aperture (ingresso + uscita)
- Zero bivi sul percorso principale

Se la spec prevede diramazioni opzionali: max 1 bivio visibile alla volta, ogni diramazione max 2–3 stanze, mai chiave o uscita nelle diramazioni.

### 4. MURI — obbligatori attorno a ogni cella floor

Ogni cella floor deve avere muri espliciti (`LEFT`, `RIGHT`, `TOP`, `BOTTOM`) ai bordi con fill o NonCella. **Non basta avere fill adiacente:** senza muri il motore di rendering usa texture "da esterno" che producono artefatti visivi.

**Regola in una riga:** ogni transizione floor↔fill (o floor↔NonCella) deve avere un muro sul lato corrispondente della cella floor.

Nel formato `.mark`:
- bordo sinistro dell'area floor: `LEFT=1` sulla cella floor di bordo
- bordo destro: `RIGHT=1`
- bordo superiore: `TOP=1`
- bordo inferiore: `BOTTOM=1`
- dove c'è un ingresso usa `WallType=5`, dove c'è un'uscita usa `WallType=6`

### 5. PRIMA IL GRAFO, POI IL LIVELLO

Non iniziare mai a costruire il livello senza aver prima scritto il grafo delle stanze. Vedi il workflow obbligatorio in [Sezione 6](#6-workflow-obbligatorio-per-la-generazione).

### 6. Alcuni livelli d'esempio

Vedi:

- Web/public/assets/levels/demo/1.mark
- Web/public/assets/levels/demo/2.mark
- Web/public/assets/levels/demo/4.mark

Puoi aprirli nel Level Editor visuale o leggerli direttamente come file `.mark`.

---

## Indice

0. [VINCOLI INVIOLABILI](#vincoli-inviolabili--leggere-prima-di-qualsiasi-altra-cosa) ← **leggere per primo**
1. [Meccaniche del gioco — introduzione progressiva](#1-meccaniche-del-gioco--introduzione-progressiva)
2. [I nemici — descrizione e placement](#2-i-nemici--descrizione-e-placement)
3. [Convenzioni di design spaziale](#3-convenzioni-di-design-spaziale)
4. [Progressione dei livelli — texture e struttura](#4-progressione-dei-livelli--texture-e-struttura)
5. [Workflow per la creazione di un livello](#5-workflow-per-la-creazione-di-un-livello) ← **da seguire sempre**

---

## 1. Meccaniche del gioco — introduzione progressiva

Ogni meccanica deve essere **introdotta una alla volta**, in un contesto dove il giocatore capisce cosa sta succedendo senza possibilità di confusione. Non introdurre mai due meccaniche nuove nello stesso livello.

### Mappa delle meccaniche e quando introdurle

| Meccanica                                    | Livello di introduzione | Note                                                                 |
| -------------------------------------------- | ----------------------- | -------------------------------------------------------------------- |
| Movimento del giocatore                      | 1                       | Implicita — il giocatore scopre da solo                              |
| Raccogliere la chiave                        | 1                       | Chiave visibile, percorso diretto                                    |
| Aprire l'uscita con la chiave                | 1                       | Uscita vicina alla chiave nei primi livelli                          |
| Raccogliere le candele                       | 1                       | Candele sul percorso, testo esplicativo                              |
| Zone buie (Buio)                             | 2                       | Zona buia piccola, la candela in mano illumina — non serve piazzarla |
| Piazzare una candela (SPAZIO)                | 3                       | Introdotta col primo zombie come meccanica di distrazione            |
| Nemico Zombie — patruglia                    | 3                       | Un singolo zombie, meccanica distrazione candela                     |
| Distrazione con candela                      | 3                       | Insegnata con il primo zombie                                        |
| Aprire/chiudere porte (CTRL)                 | 4                       | Una singola porta, testo esplicativo in voce                         |
| Trappola temporizzata (TrappolaTemporizzata) | 5                       | Una trappola ovvia, si può attraversare di corsa                     |
| Nemico Scheletro — visuale lineare           | 6                       | Da solo in stanza aperta                                             |
| Muri rompibili (Fragile, WallType=2)         | 8                       | Prima mostrati come muri distinguibili, poi Taurus li abbatte        |
| Nemico Taurus — carica                       | 8                       | Da solo, corridoio dove la carica è dimostrativa                     |
| Trappola letale (TrappolaMorteIstantanea)    | 9                       | Introdotta visivamente, non nascosta                                 |
| Nemico Ghost — stai fermo                    | 10                      | Da solo, stanza dove il giocatore DEVE fermarsi                      |
| Nemico Bubu — spegne candele                 | 12                      | Con 1–2 zombie, per insegnare la scarsità                            |
| Nemico Goblin — inseguimento puro            | 15                      | **Da solo in un livello dedicato**                                   |
| Mix completo nemici                          | 17+                     | Solo dopo che il giocatore conosce tutti                             |

### Principio fondamentale

> **Non fare mai un livello dove la soluzione è "cerca nel buio".**
> Il giocatore deve sempre poter vedere abbastanza della mappa da capire dove andare. Il buio (zone `Buio`) è uno strumento di tensione, non un labirinto cieco. Usa brazier (`Lucina`) per illuminare i punti di riferimento chiave — chiave, uscita, bivi importanti.

### La candela in mano è inesauribile

Il giocatore ha **sempre** una candela accesa in mano che illumina il suo intorno. Questa luce è **inesauribile** e non si spegne mai da sola. Le candele raccoglibili nella mappa servono esclusivamente a essere **piazzate a terra** per distrarre i nemici (o illuminare zone fisse). Non servono per sopravvivere al buio: il giocatore può attraversare qualsiasi zona buia finché ha la candela in mano, il che è sempre. Questa distinzione è fondamentale: il game over per "buio" arriva solo se un nemico spegne tutte le candele che il giocatore ha in inventario **e poi** spegne anche quella in mano.

### Pacchetti candela vs candela singola — distinzione per il giocatore

Nel gioco esistono due tipi di oggetti "candela" con interazioni diverse:

- **Pacchetti candela (`CandlePack`):** oggetti sul pavimento che si raccolgono automaticamente **camminandoci sopra**. Danno N candele all'inventario del giocatore. Visivamente sono un gruppo di candele impacchettate.
- **Candele singole (`Candle`):** candele già posizionate nel livello (accese o spente). Si interagisce con esse premendo **CTRL** mentre ci si è accanto.

Questa distinzione è **contro-intuitiva** per un nuovo giocatore: l'interazione con CTRL funziona per le porte, e il giocatore potrebbe non capire che i pacchetti si raccolgono solo camminandoci sopra.

**Linea guida per il level design (livello 1):** il primo livello deve comunicare questa distinzione via testo narrativo. Il testo deve apparire _prima_ del primo pacchetto candela (es. "Le candele sul pavimento... le prendo senza fermarmi.") e, se il livello contiene anche candele singole, un secondo trigger prima di esse (es. "Questa è già accesa. Ctrl.").

### Ogni livello è autonomo — nessun carry-over

**L'inventario del giocatore si azzera a ogni livello.** Le candele raccolte in un livello non vengono portate in quello successivo: ogni livello inizia sempre con inventario vuoto. Di conseguenza, ogni livello deve essere progettato come un sistema chiuso: le candele presenti nella mappa sono esattamente quelle disponibili per quel livello, né di più né di meno. Non ha senso lasciare candele "di riserva per dopo" — o servono in quel livello, o non devono esserci. Questa regola vale per qualsiasi risorsa raccoglibile futura.

---

## 2. I nemici — descrizione e placement

Per la documentazione completa dell'AI (stati, transizioni, parametri di velocità) vedi [`ENEMY_AI.md`](./ENEMY_AI.md).

### Zombie (ObjectType=5) — il primo nemico

- **Personalità:** patrugliante lento e meccanico — va avanti e indietro senza deviare
- **La meccanica che insegna:** la distrazione con la candela. Lo zombie si avvicina alla candela piazzata, dando al giocatore il tempo di passare. Questa è la meccanica cardine del gioco.
- **Pericolo:** basso con candele, letale al buio o senza via di fuga
- **Spegne candele:** sì, al contatto
- **Vede il giocatore:** no (patrol puro)
- **Luce propria:** sì — aura rossa/gialla
- **Placement consigliato:** corridoi stretti, zone obbligate. Il primo zombie va messo in un corridoio dove c'è esattamente una soluzione ovvia (la candela). Zombi successivi in configurazioni più complesse.

### Scheletro (ObjectType=6) — il cacciatore

- **Personalità:** patrugliante con visuale lineare — ti insegue se ti vede in linea retta
- **La meccanica che insegna:** il line-of-sight. Il giocatore impara a non attraversare la visuale del nemico.
- **Pericolo:** medio — sorpresa da lontano, ma si perde girando un angolo
- **Spegne candele:** sì, se sono nel suo raggio di ricerca
- **Vede il giocatore:** sì, su linea orizzontale o verticale senza ostacoli
- **Luce propria:** sì — bianca in pattuglia, rossa in inseguimento
- **Placement consigliato:** stanze aperte, corridoi lunghi. La prima introduzione deve avvenire in una stanza dove la visuale è evidente — il giocatore vede lo scheletro, vede la linea retta, capisce il pericolo.

### Taurus (ObjectType=8) — il caricatore

- **Personalità:** guardiano lento che carica in linea retta quando ti vede. **Abbatte i muri rompibili (Fragile)** nella sua traiettoria.
- **La meccanica che insegna:** evitare la linea di carica + i muri rompibili. Un Taurus che sfonda un muro è un momento spettacolare e insegna entrambe le cose.
- **Pericolo:** alto in corridoi — la carica è molto veloce e devastante
- **Spegne candele:** no
- **Vede il giocatore:** sì, solo su linea orizzontale o verticale
- **Luce propria:** sì — aura arancione intensa
- **Placement consigliato:** a guardia di corridoi larghi o stanze con muri rompibili. Il livello di introduzione deve avere almeno un muro fragile che il Taurus può sfondare per mostrare la meccanica.

### Ghost (ObjectType=10) — il fantasma

- **Personalità:** attraversa i muri, attacca solo se il giocatore si muove
- **La meccanica che insegna:** fermarsi. Il giocatore deve imparare che in alcuni frangenti NON muoversi è la risposta giusta — una meccanica contro-intuitiva che va introdotta in isolamento.
- **Pericolo:** psicologico — fermarti in mezzo ad altri nemici è il vero pericolo
- **Spegne candele:** sì, molto velocemente
- **Attraversa i muri:** sì
- **Luce propria:** sì — aura verde/rosa
- **Placement consigliato:** il livello di introduzione deve avere un Ghost in una stanza dove il giocatore è solo con lui. Solo dopo, combinarlo con altri nemici che forzano il movimento.
- **Comunicare la meccanica "stai fermo":** il Ghost attacca solo se il giocatore si muove — ma questa è una meccanica completamente contro-intuitiva. Il giocatore reagisce istintivamente scappando e muore. Il testo narrativo nel livello di introduzione (livello 10) deve suggerirla _prima_ che il Ghost si avvicini, in un corridoio obbligato: `"Immobile. Devo restare immobile."` — una frase sola, breve, in prima persona. Questo è sufficiente a far intuire al giocatore che fermarsi è una risposta valida.

### Bubu (ObjectType=11) — il sabotatore

- **Personalità:** patrugliante che ha come obiettivo primario spegnere le candele del giocatore
- **La meccanica che insegna:** la scarsità di risorse. Il Bubu non uccide direttamente ma toglie le candele — la risorsa su cui il giocatore si è fino ad ora affidato.
- **Pericolo:** indiretto ma devastante in combinazione
- **Spegne candele:** sì, è il suo comportamento principale
- **Luce propria:** sì — aura blu
- **Placement consigliato:** in combinazione con Zombie o Scheletri, mai da solo nei primi incontri. Usare per creare livelli dove le candele devono essere usate con parsimonia.

### Goblin (ObjectType=7) — il cacciatore definitivo

- **Personalità:** conosce sempre la posizione del giocatore e lo insegue. Diventa più veloce ogni volta che il giocatore usa una candela.
- **La meccanica che insegna:** la pressione pura. Non c'è una "soluzione" al Goblin — va aggirato, confuso con le stanze, sfruttando la mappa. Questo è il nemico che mette alla prova tutto ciò che il giocatore ha imparato.
- **Pericolo:** il più alto del gioco. Un Goblin da solo riempie un livello di tensione.
- **Spegne candele:** sì, le cerca attivamente
- **Luce propria:** sì — aura viola
- **Placement consigliato:** **introdurlo da solo in un livello dedicato** (livello 15), lontano dallo spawn. Solo nei livelli finali usarlo in combinazione. Non usarlo prima del livello 15.

---

## 3. Convenzioni di design spaziale

### Filosofia: leggibilità prima di tutto

Un buon livello di Mark In DeDark non è un labirinto cieco. Il giocatore deve sempre poter capire la struttura dello spazio anche con la luce limitata. Questo si ottiene con:

- **Brazier (`Lucina`, ID=12)** sui punti di riferimento importanti: ingresso stanza chiave, vicinanza all'uscita, incroci principali. Il giocatore orienta il suo percorso mentale sulle luci fisse.
- **Stanze distinte** con transizioni chiare (porte, soglie strette). Il giocatore deve poter costruire una mappa mentale del livello.
- **Zone buie (`Buio`, GroundType=7) delimitate** — non usare il buio su metà mappa. Il buio deve essere una zona attraversabile, non un oblio infinito.

### Struttura spaziale

- **Usa il Riempimento** per dare spessore visivo ai muri e separare le stanze
- **Stanze di dimensioni miste** — stanze piccole (tensione) alternate a stanze grandi (respiro) creano ritmo
- **Corridoi stretti** — ideali per Zombie e Taurus, dove il giocatore non può aggirarli facilmente
- **Stanze aperte** — ideali per Ghost e Scheletro, dove la visuale lineare è efficace
- **Vicoli ciechi** — utili nei livelli medi per creare tensione, ma mai come percorso obbligato verso la chiave

### Definizione operativa di "percorso lineare"

Quando le specifiche di un livello dicono "lineare" o "strettamente lineare" significa:

- Il grafo delle stanze è una sequenza senza bivi: Spawn → A → B → … → Uscita
- In ogni punto del percorso principale il player ha esattamente **una** direzione di avanzamento (più eventualmente tornare indietro)
- Nessuna stanza ha più di **2 aperture** verso altre stanze (un ingresso, un'uscita)
- Se il livello prevede diramazioni opzionali: max 1 bivio visibile contemporaneamente; ogni diramazione ha max 2–3 stanze e non contiene chiave né uscita

**Come ottenere linearità:** ogni stanza deve avere al massimo 1 uscita che porta avanti e 1 che porta indietro. Usa il fill per murare tutte le aperture in eccesso. Non lasciare mai una stanza aperta su più di due lati.

### Colli di bottiglia obbligatori tra stanze

Ogni transizione tra una stanza e la successiva deve passare per un corridoio "collo di bottiglia":

- **Larghezza: esattamente 1 cella** (orizzontale o verticale)
- **Lunghezza: almeno 2 celle consecutive**

Questo corridoio è l'unico punto garantito in cui il player passerà. È il luogo corretto per i trigger di testo e per i pack di candele che devono essere trovati prima del pericolo successivo.

### Regola dei trigger di testo (`!`) — OBBLIGATORIA

Vedi [VINCOLI INVIOLABILI §2](#2-trigger-di-testo--solo-in-corridoi-obbligati-da-1-cella) per i pattern validi/invalidi.

Regola di auto-verifica rapida: prima di mettere un `!`, verifica che la riga (o colonna) in cui si trova non abbia celle floor sia sopra che sotto (o sia a sinistra che a destra). Se ci sono → trigger aggirabile → sposta o restringi il corridoio.

### Densità fill (regola obbligatoria)

Almeno il **50% delle celle** nella bounding box del livello deve essere `▓` (fill) o NonCella (spazio vuoto). Lo spazio calpestabile è la minoranza — il livello è scavato nella roccia, non costruito nel vuoto.

Eccezione: `pacco1` (esterno) usa muri perimetrali invece del fill come confine. Anche lì, però, il pavimento calpestabile non deve riempire l'intera bounding box — ci devono essere ostacoli, muri interni e zone non calpestabili.

### Dimensioni della bounding box per livello

La griglia è fissa **25 righe × 45 colonne**. Non usare l'intera griglia — lasciare sempre almeno 3 righe e 3 colonne di spazio vuoto (NonCella) su ogni bordo.

La tabella delle bounding box per livello è in `LEVEL_TASKS.md` (sezione "Vincoli dimensionali e strutturali obbligatori") — è quella la fonte autorevole. Ogni spec di livello in `LEVEL_TASKS.md` riporta anche la bounding box specifica.

**Vincolo duro generale:** per i livelli 1 e 2, la bounding box non deve superare 11 righe × 28 colonne. Un livello 1 grande quanto un livello 10 è un errore di design.

### Distribuzione delle risorse

- Le candele devono essere **abbondanti nei primi livelli** e diventare progressivamente più scarse — ma mai "impossibili"
- La chiave nei primi livelli può essere **visibile e relativamente vicina** — il challenge è il percorso, non la ricerca
- Nei livelli avanzati la chiave può richiedere esplorazione, ma sempre con indizi visivi (un brazier, una stanza speciale)
- L'uscita può essere vicina allo spawn ma difficile da raggiungere per via dei nemici

> **Regola fondamentale sulle candele raccoglibili:** non mettere mai pacchi di candele in zone prive di nemici. Le candele si usano esclusivamente per distrarre i nemici: piazzarle in un corridoio vuoto non ha senso dal punto di vista del gameplay. Ogni pacco di candele deve trovarsi in prossimità di un nemico o di una zona che richiede distrazione. Se un'area non ha nemici, non ha candele.

### Porte — ingresso, uscita e uso strategico

Le porte si aprono **e si chiudono** con CTRL. Possono essere usate strategicamente: chiudere una porta dietro di sé blocca temporaneamente un nemico in inseguimento; aprire una porta davanti al Taurus lo indirizza verso un muro fragile. Questa doppia utilità (blocco difensivo / guida del Taurus) è la meccanica core della porta — va introdotta con un testo che la suggerisca in voce di Mark senza nominarla esplicitamente.

#### Posizionamento fisico delle porte

Le porte di ingresso (spawn del giocatore) e di uscita devono sempre essere **addossate a un muro**. Non possono mai galleggiare al centro della mappa perché rappresentano aperture fisiche in una parete: una porta in mezzo allo spazio aperto non ha senso architettonico e rompe l'immersione.

**Regole:**

- La porta di ingresso e quella di uscita devono avere almeno un lato su un muro (`Occludente` o `Riempimento`).
- Le porte si mettono tipicamente come interruzione in una parete: una fila di muri con la porta al centro.
- Non posizionare mai una porta in mezzo a una stanza aperta, a un corridoio largo, o isolata senza contesto architettonico.
- Se visivamente la porta sembra "uscire dal nulla", il placement è sbagliato — riposizionala.

#### Ingresso e uscita devono essere sempre visivamente marcati

**Regola obbligatoria:** il giocatore deve capire subito da dove viene e dove deve andare. Per questo:

- **Spawn del giocatore (ObjectType=1):** deve sempre trovarsi in un corridoio stretto (1 cella di larghezza) che dia chiaramente l'idea di "sei entrato da lì", oppure avere una parete con muro tipo `Ingresso` (WallType=5) alle spalle — quella che il giocatore ha appena attraversato. Lo spawn in mezzo a una stanza aperta, senza contesto architettonico di provenienza, è vietato.

- **Uscita (ObjectType=3):** deve **sempre** avere una porta tipo `Uscita` (WallType=6) sulla cella adiacente, dalla parte verso cui si esce. Il gioco fa visualizzare questa porta come chiusa finché il giocatore non raccoglie la chiave, e aperta dopo. Senza la porta visiva, il giocatore non sa dove sia l'uscita, e il livello diventa illeggibile. Un'uscita senza porta è un bug di design.

In sintesi: **ingresso e uscita non sono mai celle anonime** — sono sempre riconoscibili a colpo d'occhio grazie alla porta che li accompagna.

### Uso dei muri rompibili (WallType=2 — Fragile)

I muri fragili aggiungono una meccanica di ambiente dinamico. Regole:

- Mostrare i muri fragili con una **texture visivamente distinta** dal muro normale
- Il primo incontro con muri fragili (livello 8) deve avvenire con un Taurus che li sfonda in modo spettacolare
- I muri fragili possono anche creare **scorciatoie** che il giocatore scopre facendo passare il Taurus

### Trappole

- **TrappolaTemporizzata (GroundType=4):** trappola con timer, si può attraversare di corsa. Introdurla visivamente ovvia, non nascosta.
- **TrappolaMorteIstantanea (GroundType=3):** fossa/lama. Usarla con parsimonia. Nei livelli early è un ostacolo visivo (si vede e si evita). Nei livelli avanzati può essere usata per restringere le vie di fuga dai nemici.

---

## 4. Progressione dei livelli — texture e struttura

### Principi di progressione

1. **Un nemico nuovo per livello** — non introdurre mai due tipi di nemici nuovi insieme.
2. **Ogni nemico nuovo va introdotto in isolamento** prima di essere combinato con altri. Il Goblin deve avere un livello tutto suo.
3. **Le candele diminuiscono progressivamente** — ma ogni livello deve essere risolvibile. Un livello senza soluzione non è design, è un bug.
4. **La struttura è sempre leggibile** — tutti i livelli hanno un percorso principale chiaro. I livelli avanzati aggiungono 2–3 diramazioni brevi (max 2–3 stanze ciascuna) con candele extra o vicoli ciechi. Mai più di 2 bivi contemporanei visibili. La complessità è nei nemici, non nella navigazione.
5. **Il buio non è mai obbligatorio** — si può sempre trovare un percorso con abbastanza luce. Il buio è una sfida opzionale o una scorciatoia.

### Struttura per atti

| Atto           | Livelli | Texture         |
| -------------- | ------- | --------------- |
| I — La discesa | 1–5     | pacco1 → pacco2 |
| II — Profondità| 6–10    | pacco3 → pacco5 |
| III — Il cuore | 11–16   | pacco6 → pacco8 |
| IV — Finale    | 17–20   | pacco8 → pacco1 |

### Pack di texture — descrizione visiva

Ogni livello usa un singolo pack. La transizione tra pack avviene tra un livello e l'altro, mai all'interno dello stesso livello.

- **`pacco1`** — esterno: giardino al chiaro di luna, rovine all'aperto. Usato per i livelli iniziali (Atto I) e per il finale narrativo del livello 20. **Non mettere mai brazier (`Lucina`)** — siamo all'aperto, i bracieri chiusi non hanno senso visivamente. Il pavimento è neve.
- **`pacco2`** — interno castello: ben tenuto, pulito, pavimenti rifiniti, muri intonacati. L'aspetto più "nobile" degli interni.
- **`pacco3`** — interno castello: ancora decoroso, qualche segno di usura ma strutturalmente integro.
- **`pacco4`** — interno castello: degrado visibile, muri screpolati, atmosfera più pesante.
- **`pacco5`** — interno castello: trascurato, poco illuminato, strutture che cominciano a cedere.
- **`pacco6`** — interno castello: rozzo, quasi nessun elemento decorativo, pietra a vista.
- **`pacco7`** — interno castello: fatiscente, corridoi irregolari, pavimentazione assente o rotta.
- **`pacco8`** — interno castello: il livello più grezzo — pietra nuda, nessun pavimento, architettura primitiva o crollata. Il cuore buio del castello.

Per lo schema dettagliato livello-per-livello (testi, nemici, meccaniche) e lo stato di avanzamento: **[`LEVEL_TASKS.md`](./LEVEL_TASKS.md)**.

---

## 5. Workflow per la creazione di un livello

### FASE 1 — Pianifica il grafo delle stanze

Prima di aprire il Level Editor, scrivi il grafo in forma testuale. Questo ti forza a verificare i vincoli mentre il design è ancora modificabile.

Esempio:
```
Spawn (corridoio 1×4) → Area principale (7×14) → Corridoio uscita (1×5) → Uscita
Trigger 1: corridoio spawn cella 2  ← obbligato
Trigger 2: corridoio uscita cella 2 ← obbligato
Chiave: centro area principale
Bounding box stimata: 10 righe × 24 colonne
```

### FASE 2 — Verifica i vincoli

Spunta ogni voce. Se una fallisce, torna alla Fase 1.

- [ ] Bounding box rispetta i limiti della tabella in `LEVEL_TASKS.md`?
- [ ] Celle floor < 50% della bounding box?
- [ ] Grafo lineare (o max 1 bivio per livelli con diramazioni)?
- [ ] Ogni trigger `!` è in un corridoio largo esattamente 1 cella?
- [ ] La meccanica nuova introdotta è esattamente una sola?

### FASE 3 — Costruisci nel Level Editor

Apri il Level Editor (`/level-editor`), crea il livello partendo dal fill e "scavando" le stanze. Non aggiungere stanze finché quelle già presenti non rispettano il grafo pianificato.

### FASE 4 — Valida il file

Esegui il validatore:
```
npx tsx Web/scripts/validate-level.ts Web/public/assets/levels/folder/N.mark
```

- **Per ogni `!`:** verifica che la riga (o colonna) non abbia floor sia sopra che sotto. Se sì, il trigger è aggirabile → sposta o restringi il corridoio.
- **Verifica il grafo:** ogni stanza ha max 2 aperture (ingresso + uscita).

Per le spec dettagliate di ogni livello: **[`LEVEL_TASKS.md`](./LEVEL_TASKS.md)**.
