# Linee Guida di Design — Mark In DeDark

Guida per la creazione di livelli coerenti con il tono, la narrativa e la progressione del gioco.
Per la struttura tecnica del file `.mark` vedi [`LEVEL_FORMAT.md`](./LEVEL_FORMAT.md).

---

## Indice

1. [La storia di Mark — arco narrativo](#1-la-storia-di-mark--arco-narrativo)
2. [Meccaniche del gioco — introduzione progressiva](#2-meccaniche-del-gioco--introduzione-progressiva)
3. [I nemici — descrizione e placement](#3-i-nemici--descrizione-e-placement)
4. [Convenzioni di design spaziale](#4-convenzioni-di-design-spaziale)
5. [Progressione dei livelli — schema completo](#5-progressione-dei-livelli--schema-completo)
6. [Istruzioni per la generazione di livelli](#6-istruzioni-per-la-generazione-di-livelli)

---

## 1. La storia di Mark — arco narrativo

### Chi è Mark

Mark è un ragazzo adolescente. Un pomeriggio di fine estate, esplorando le rovine di un vecchio castello abbandonato vicino al suo paese, il terreno cede sotto di lui. Cade in una serie di gallerie sotterranee che non esistono su nessuna mappa. Non sa dove si trova. Non sa quanto è profondo. Ha con sé solo una candela.

Ogni livello completato non è una vittoria definitiva: è solo un'altra zona che Mark riesce ad attraversare. L'uscita di ogni livello porta sempre più in profondità — o così sembra. Ma il labirinto ha una direzione. Mark lo sente. Da qualche parte, c'è una via d'uscita.

### La meccanica come metafora

- **La candela** è la speranza di Mark: l'unica cosa che tiene il buio a distanza. Piazzarla per distrarre un nemico è un atto di coraggio — Mark sacrifica temporaneamente la sua sicurezza per creare un'opportunità. Perderla nel buio non è solo un malus meccanico: è Mark solo, al buio, che deve trovare la forza di andare avanti.
- **I nemici** sono le presenze che abitano il labirinto. Alcuni (Zombie) sembrano vagare senza scopo come automi. Altri (Goblin, Ghost) sembrano voler specificamente spegnere la luce di Mark — come se il labirinto stesso non volesse che lui uscisse.
- **La chiave e l'uscita** sono la struttura di ogni livello: c'è sempre una via d'uscita. Mark deve crederci.

### L'arco narrativo — struttura circolare

Il gioco ha un arco circolare deliberato:

> Rovine all'aperto → sotto terra → cripte → cuore del labirinto → risalita → uscita

I **livelli iniziali** si ambientano in rovine esterne — mura crollate, cielo visibile attraverso le crepe, pietra chiara. Mark è appena caduto: è disorientato ma non ancora disperato.

I **livelli centrali** scendono sempre più in profondità: corridoi angusti, stanze senza finestre, muri neri di umidità. Qui si concentrano i nemici più pericolosi.

I **livelli finali** mostrano i segni della risalita: la pietra torna più chiara, c'è aria che circola. L'ultimo livello riporta Mark all'esterno — le stesse rovine del livello 1, ma viste dall'altra parte. L'uscita è la luce del sole.

Questa struttura deve riflettersi nelle texture pack usate:
- **pacco1** → rovine esterne, pietra chiara (livelli 1–2 e livello finale)
- **pacco2** → transizione verso l'interno (livello 3)
- **pacchi 3–7** → progressivamente più scuri e sotterranei
- **pacco8** → il cuore del labirinto, il punto più buio
- **pacco1 di ritorno** → l'uscita (livello finale)

### Il tono dei testi narrativi

I testi sono pensieri di Mark — voce interiore, prima persona singolare. Il tono è quello di un adolescente spaventato ma non arreso. Non è eroico, non è disperato: è umano.

**Regole per scrivere i testi:**
- **2–4 testi per livello** al massimo. Ogni testo deve avere un impatto.
- **Posizionarli prima dell'oggetto**, non sopra. Mark "pensa" mentre si avvicina, non dopo.
- **Non spiegare la meccanica direttamente** — suggerire, non istruire. "Forse quella luce potrebbe aiutarmi" è meglio di "premi SPAZIO per piazzare la candela".
- **I tutorial espliciti** (premi X per fare Y) sono accettabili solo nei primi 2–3 livelli.
- **Testi di atmosfera** nelle zone di transizione: entrata di stanze buie, corridoi prima dei nemici, vicino all'uscita.

**Esempi di voce corretta:**
> *"Non so quanto sono profondo. Ma c'è aria che circola da quella parte."*
> *"Quella cosa sta andando avanti e indietro. Se si distrae... potrei passare."*
> *"Ho piazzato la candela. Adesso corri, Mark."*
> *"Questo posto non vuole che io esca."*

---

## 2. Meccaniche del gioco — introduzione progressiva

Ogni meccanica deve essere **introdotta una alla volta**, in un contesto dove il giocatore capisce cosa sta succedendo senza possibilità di confusione. Non introdurre mai due meccaniche nuove nello stesso livello.

### Mappa delle meccaniche e quando introdurle

| Meccanica | Livello di introduzione | Note |
|---|---|---|
| Movimento del giocatore | 1 | Implicita — il giocatore scopre da solo |
| Raccogliere la chiave | 1 | Chiave visibile, percorso diretto |
| Aprire l'uscita con la chiave | 1 | Uscita vicina alla chiave nei primi livelli |
| Raccogliere le candele | 1 | Candele sul percorso, testo esplicativo |
| Piazzare una candela (SPAZIO) | 1–2 | Tutorial esplicito con testo |
| Zone buie (Buio) | 2 | Zona buia piccola, candela necessaria per passare |
| Aprire/chiudere porte (CTRL) | 2–3 | Una singola porta, testo esplicativo |
| Nemico Zombie — patruglia | 3 | Un singolo zombie, meccanica distrazione candela |
| Distrazione con candela | 3 | Insegnata con il primo zombie |
| Trappola tempora (TrappolaTemporizzata) | 4–5 | Una trappola ovvia, si può attraversare di corsa |
| Nemico Scheletro — visuale lineare | 5 | Da solo o con 1 zombie, stanza aperta |
| Muri rompibili (Fragile, WallType=2) | 6 | Prima mostrati come muri distinguibili, poi Taurus li abbatte |
| Nemico Taurus — carica | 6 | Da solo, corridoio dove la carica è dimostrativa |
| Trappola letale (TrappolaMorteIstantanea) | 6–7 | Introdotta visivamente, non nascosta |
| Nemico Ghost — stai fermo | 7 | Da solo, stanza dove il giocatore DEVE fermarsi |
| Nemico Bubu — spegne candele | 8 | In combinazione con 1–2 zombie, crea scarsità |
| Nemico Goblin — inseguimento puro | 9 | **Da solo in un livello dedicato** |
| Mix completo nemici | 10 | Solo dopo che il giocatore conosce tutti |

### Principio fondamentale

> **Non fare mai un livello dove la soluzione è "cerca nel buio".**
> Il giocatore deve sempre poter vedere abbastanza della mappa da capire dove andare. Il buio (zone `Buio`) è uno strumento di tensione, non un labirinto cieco. Usa brazier (`Lucina`) per illuminare i punti di riferimento chiave — chiave, uscita, bivi importanti.

---

## 3. I nemici — descrizione e placement

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
- **Placement consigliato:** **introdurlo da solo in un livello dedicato** (livello 9), lontano dallo spawn. Solo nel livello finale usarlo in combinazione. Non usarlo prima del livello 9.

---

## 4. Convenzioni di design spaziale

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

### Distribuzione delle risorse

- Le candele devono essere **abbondanti nei primi livelli** e diventare progressivamente più scarse — ma mai "impossibili"
- La chiave nei primi livelli può essere **visibile e relativamente vicina** — il challenge è il percorso, non la ricerca
- Nei livelli avanzati la chiave può richiedere esplorazione, ma sempre con indizi visivi (un brazier, una stanza speciale)
- L'uscita può essere vicina allo spawn ma difficile da raggiungere per via dei nemici

### Uso dei muri rompibili (WallType=2 — Fragile)

I muri fragili aggiungono una meccanica di ambiente dinamico. Regole:
- Mostrare i muri fragili con una **texture visivamente distinta** dal muro normale
- Il primo incontro con muri fragili (livello 6) deve avvenire con un Taurus che li sfonda in modo spettacolare
- I muri fragili possono anche creare **scorciatoie** che il giocatore scopre facendo passare il Taurus

### Trappole

- **TrappolaTemporizzata (GroundType=4):** trappola con timer, si può attraversare di corsa. Introdurla visivamente ovvia, non nascosta.
- **TrappolaMorteIstantanea (GroundType=3):** fossa/lama. Usarla con parsimonia. Nei livelli early è un ostacolo visivo (si vede e si evita). Nei livelli avanzati può essere usata per restringere le vie di fuga dai nemici.

---

## 5. Progressione dei livelli — schema completo

### Principi di progressione

1. **Un nemico nuovo per livello** — non introdurre mai due tipi di nemici nuovi insieme.
2. **Ogni nemico nuovo va introdotto in isolamento** prima di essere combinato con altri. Il Goblin deve avere un livello tutto suo.
3. **Le candele diminuiscono progressivamente** — ma ogni livello deve essere risolvibile. Un livello senza soluzione non è design, è un bug.
4. **La complessità spaziale cresce** — i primi livelli sono quasi lineari. I livelli avanzati hanno rami e scelte.
5. **Il buio non è mai obbligatorio** — si può sempre trovare un percorso con abbastanza luce. Il buio è una sfida opzionale o una scorciatoia.

### Schema per 10 livelli

| Livello | Texture | Atmosfera | Nemici | Meccanica introdotta | Candele | Mappa | Testi |
|---|---|---|---|---|---|---|---|
| **1** | pacco1 | Rovine esterne — Mark è appena caduto | Nessuno | Movimento, chiave, uscita, candela (raccogliere) | Abbondanti | Lineare, 1–2 stanze aperte | Tutorial completo, voce di Mark disorientata |
| **2** | pacco1 | Rovine esterne — più in profondità | Nessuno | Piazzare candela (SPAZIO), zona buia | Abbondanti | 2–3 stanze, una zona buia superabile | Tutorial piazzamento candela, testo atmosfera |
| **3** | pacco2 | Primo corridoio sotterraneo | **1 Zombie** | Distrazione zombie con candela | Sufficienti | Corridoio lineare con UN zombie che blocca | Tutorial distrazione, pensiero di Mark "quella cosa si distrae" |
| **4** | pacco2 | Dungeon con stanze | **2–3 Zombie + porte** | Porte (CTRL), più zombie da distrarre in sequenza | Sufficienti | 3–4 stanze con corridoi, 1–2 porte | Testo porta, testo atmosfera |
| **5** | pacco3 | Zona più ampia e buia | **2 Zombie + 1 Scheletro** | Visuale lineare (line-of-sight) | Moderate | Stanza aperta per il Scheletro, corridoi per gli Zombie | Testo "quella cosa mi guarda diversamente" |
| **6** | pacco4 | Zona dei guardiani | **1 Taurus** (+ 1–2 Zombie opzionali) | Carica del Taurus, muri rompibili | Moderate | Corridoio con muro fragile che il Taurus sfonda, stanza prima e dopo | Testo "che suono era quello?" sul muro rotto |
| **7** | pacco5/6 | Cripta — il punto più buio | **1 Ghost** (+ 1 Zombie opzionale) | Fermarsi per sopravvivere al Ghost | Scarse | Stanza isolata con Ghost, poi area più grande | Testo "se mi fermo... non mi attacca" |
| **8** | pacco6/7 | Catacombe inferiori | **2–3 Zombie + 1 Bubu** | Scarsità di risorse (Bubu spegne le candele) | Pochissime | Mappa densa, zone buie, 1–2 trappole | Nessun testo tutorial — solo atmosfera |
| **9** | pacco7/8 | Il cuore del labirinto | **1 Goblin** (da solo) | Inseguimento puro — il Goblin non si può distrarre | Scarse, nascoste | Mappa ampia con molte stanze per sfuggire al Goblin | Testo "non smette di inseguirmi" |
| **10** | pacco8 → pacco1 | Risalita — l'uscita è vicina | Mix completo (Zombie, Scheletro, Bubu, Goblin) | Tutto insieme — la prova finale | 1–2 pacchi nascosti | Mappa grande, inizia scura e poi diventa più chiara verso l'uscita | Testo finale: "Luce. Finalmente luce." |

### Note specifiche per ciascun livello

**Livello 1 — "La caduta"**
Mark è appena caduto. Layout quasi completamente illuminato da brazier. La chiave è visibile dall'inizio. L'obiettivo è che il giocatore impari i controlli e la struttura chiave→uscita senza pressione. Testo iniziale: un pensiero disorientato di Mark.

**Livello 2 — "Il buio"**
Stesso ambiente esterno (pacco1), ma una stanza ha una zona `Buio`. Il giocatore scopre che senza candela non vede dove mette i piedi. La soluzione è ovvia: piazza la candela, illumina, attraversa. Nessun nemico — solo la meccanica pura.

**Livello 3 — "La creatura"**
Il primo nemico deve essere un momento chiaro e didattico. Un singolo Zombie pattuglia un corridoio obbligato. Davanti allo Zombie ci sono candele raccoglibili (il giocatore le trova naturalmente). Un testo di Mark suggerisce: "Se si avvicina a quella luce... potrei passare." La soluzione è una: piazza la candela dall'altra parte, aspetta, passa.

**Livello 6 — "Il guardiano"**
Il Taurus deve essere presentato in modo spettacolare. Il percorso obbligato passa per un corridoio con un muro fragile. Il Taurus pattuglia dall'altra parte. Quando il giocatore entra nel campo visivo del Taurus, la carica sfonda il muro — momento "wow" che insegna entrambe le meccaniche simultaneamente.

**Livello 7 — "Il fantasma"**
Stanza isolata con un Ghost. Il giocatore entra, il Ghost si avvicina. Se si muove viene attaccato. Il giocatore deve stare fermo abbastanza a lungo da far passare il Ghost e poi raggiungere l'uscita della stanza. Livello relativamente corto — la tensione è psicologica, non da esplorazione.

**Livello 9 — "Il cacciatore"**
Un Goblin da solo in una mappa ampia. Il Goblin insegue sempre, diventa più veloce se il giocatore usa candele. Il giocatore impara che deve trovare la chiave E uscire SENZA usare troppe candele, perché ogni candela velocizza il Goblin. Mappa a più stanze per dare opzioni di fuga.

**Livello 10 — "La risalita"**
La mappa inizia con texture scure (pacco8) e si "schiarisce" verso l'uscita (pacco1 nella zona finale, o almeno texture più chiare). Non è necessario che sia il livello più grande — deve essere il più soddisfacente da completare. L'uscita è letteralmente la luce del sole. Testo finale: Mark ce l'ha fatta.

---

## 6. Istruzioni per la generazione di livelli

La regola fondamentale per qualsiasi livello generato è:

- **Ogni livello introduce una cosa sola** e la insegna bene. La complessità si accumula nel tempo, non in un singolo livello.
- **Il giocatore deve sempre sapere dove andare** anche senza aver trovato la chiave. La struttura della mappa deve essere leggibile.
- **Il buio non è mai l'unica via** — ci sono sempre percorsi illuminati, anche se più pericolosi.

### Prima di iniziare a costruire un livello

1. Leggi i livelli adiacenti (precedente e successivo) per capire il contesto
2. Identifica la **meccanica nuova** che questo livello introduce
3. Costruisci la mappa attorno a quella meccanica — non aggiungere complessità extra
4. Posiziona i brazier (`Lucina`) prima di tutto: segnano i punti di riferimento
5. Aggiungi i nemici e verifica che la distrazione/strategia sia leggibile

### Checklist pre-consegna

Prima di considerare un livello completo, verifica:

- [ ] `npx tsx scripts/validate-level.ts <livello.mark>` non riporta errori
- [ ] Almeno 1 Giocatore, 1 Chiave, 1 Uscita, almeno 1 pacco di Candele
- [ ] Lo spawn del giocatore è distante dai nemici (≥ 3 celle Manhattan)
- [ ] I bordi della mappa sono chiusi (Riempimento o muri Occludente)
- [ ] La meccanica nuova è introdotta in modo chiaro e non ambiguo
- [ ] Non ci sono due meccaniche nuove nello stesso livello
- [ ] Ci sono abbastanza brazier da orientare il giocatore
- [ ] Nessuna zona buia è obbligatoria senza una candela disponibile vicina
- [ ] La difficoltà è coerente con il livello precedente (non salti bruschi)
- [ ] I testi narrativi sono in prima persona, tono Mark adolescente, max 4 per livello
