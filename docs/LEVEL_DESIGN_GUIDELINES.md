# Linee Guida di Design — Mark In DeDark

Guida per la creazione di livelli coerenti con il tono, la narrativa e la progressione del gioco.
Per la struttura tecnica del file `.mark` vedi [`LEVEL_FORMAT.md`](./LEVEL_FORMAT.md).

---

## Indice

1. [La storia di Mark — L'eredità del vuoto](#1-la-storia-di-mark--leredità-del-vuoto)
2. [Meccaniche del gioco — introduzione progressiva](#2-meccaniche-del-gioco--introduzione-progressiva)
3. [I nemici — descrizione e placement](#3-i-nemici--descrizione-e-placement)
4. [Convenzioni di design spaziale](#4-convenzioni-di-design-spaziale)
5. [Progressione dei livelli — schema completo](#5-progressione-dei-livelli--schema-completo)
6. [Istruzioni per la generazione di livelli](#6-istruzioni-per-la-generazione-di-livelli)

---

## 1. La storia di Mark — L'eredità del vuoto

### Il rito

Nella comunità di Mark non si diventa uomini compiendo diciotto anni. Si diventa uomini uscendo vivi dal **Ventre del Padre** — il labirinto sotterraneo sotto le rovine del vecchio castello, che ogni giovane maschio del villaggio deve attraversare da solo quando il tempo arriva.

La notte della prova, il padre stringe la mano del figlio — sempre troppo forte — e gli consegna una candela. Una candela fatta con il grasso dei falliti precedenti. Se la luce si spegne, sei perduto: diventi parte del labirinto. Se esci, sei un uomo. Se non esci... dicono che fossi un codardo.

Mark è l'ultimo di una stirpe obbligata a scendere nel buio per _meritarsi il sole_.

### Chi è Mark

Mark è un ragazzo adolescente, spaventato, che non ha mai scelto di essere qui. Ha letto i nomi incisi sulle porte: alcuni li riconosce. Suo fratello. Suo zio. Amici del padre. Nessuno che conosce è mai tornato, eppure il rito continua da generazioni. La società che lo ha costruito era ossessionata dalla _tempra_ — la convinzione che la sofferenza forgi la forza, che il buio insegni chi sei davvero.

Mark non sa ancora cosa pensa di tutto questo. Sa solo che vuole uscire.

### I mostri come specchi

I nemici che Mark incontra nel labirinto non sono creature casuali. Sono le deformazioni di chi è entrato prima di lui e non è riuscito a uscire — o è uscito trasformato in qualcosa di irriconoscibile.

- **Zombie:** Quelli che hanno _obbedito_ troppo. Hanno perso la volontà scendendo i gradini del labirinto e hanno continuato a camminare, meccanicamente, senza mai chiedersi perché. Vagano ancora nei corridoi ripetendo i gesti del rito, senza nessuno che li osservi.
- **Scheletri:** Quelli che hanno cercato l'onore militare. Pattugliano stanze vuote con la stessa disciplina che avevano in vita, proteggendo un trono che non esiste più, in nome di un re che non c'è mai stato.
- **Taurus:** L'incarnazione della rabbia cieca. È quello che resta di un uomo quando gli togli tutto tranne la forza e il senso di minaccia. Non sa dove andare se non _dritto_, non conosce altra risposta che la carica. I muri che sfonda erano già fragili — erano fatti per cedere.
- **Goblin:** L'invidia pura. Odiano chi ha ancora una luce perché loro l'hanno persa per primi, o gliela hanno strappata. Sono veloci, accaniti, e diventano più veloci ogni volta che vedono qualcuno usare le proprie risorse. Non vogliono uccidere Mark: vogliono che rimanga al buio come loro.
- **Ghost:** Il senso di colpa. L'ombra che sussurra di arrendersi. Attacca solo chi si muove — chi ancora _lotta_. Se ti fermi, ti sorpassa. Il labirinto premia la resa.
- **Bubu:** Il rimpianto. Non vuole uccidere, vuole solo spegnere la luce. Come se la luce fosse un'offesa alla miseria di chi l'ha perduta. È il sabotatore silenzioso che trasforma ogni risorsa in un lusso che non puoi permetterti.

### La candela

La candela non è un oggetto di gioco. È la speranza di Mark — l'unica cosa che tiene il buio a distanza, costruita con i resti di chi ha fallito prima di lui. Piazzarla per distrarre un nemico è un atto che ha un peso: Mark sacrifica temporaneamente la sua sicurezza, la sua luce, per creare un'opportunità. Il labirinto è progettato per far spegnere quella candela. Ogni nemico, a modo suo, vuole il buio.

Perdere la candela nel buio non è solo un malus meccanico: è Mark solo, senza niente, che deve trovare la forza di continuare in un posto che non vuole che lui esca.

### Il colpo di scena finale

Veicolato al livello 20.

**"Il giardino vuoto"** (nichilismo puro)
Mark apre l'ultima porta e trova il villaggio in rovina. Cenere, silenzio, nessuno. Mentre lui era nel labirinto, il mondo che aveva costruito quel rito si è spento di indifferenza. Il rito era l'unica cosa che teneva in piedi quella civiltà — e senza vittime da consumare, non è rimasto niente. La sua forza non serve a nulla in un mondo che non esiste più.

> _"Il cielo è grigio. Il villaggio è in cenere. Non c'è nessuno ad attendermi, non c'è festa né orgoglio. Hanno tirato avanti questa farsa spietata finché non si sono estinti da soli. Mi sono salvato l'anima per un mondo che aveva già smesso di esistere."_

### Il tono dei testi narrativi

I testi sono pensieri di Mark — voce interiore, prima persona singolare. Il tono è quello di un adolescente lucido e disilluso, che scarta in fretta l'ingenuità e sviluppa un cinismo difensivo e nichilista sempre più marcato. Più scende, più realizza che l'intero sistema è una truffa crudele e insensata, e lui è solo carne da macello. Non è l'eroe: è l'ingranaggio che capisce l'ingranaggio.

**Regole per scrivere i testi:**

- **2–4 testi per livello** al massimo. Ogni testo deve avere un impatto.
- **Posizionarli prima dell'oggetto o del pericolo**, non sopra. Mark pensa mentre si avvicina.
- **Non spiegare la meccanica direttamente** — suggerire la soluzione in modo utilitaristico ("Sono attratti dalla luce... se lascio la candela lì sbraneranno quella invece di me").
- **I tutorial espliciti** (premi X per fare Y) sono accettabili solo nei livelli 1–3, ma sempre dal punto di vista cinico di Mark.
- **Testi di atmosfera** nelle zone di transizione: entrate di stanze buie, corridoi prima dei nemici, vicino all'uscita.
- **La critica al rito è implacabile**, decostruita senza retorica per puro istinto di conservazione. Mark osserva, e il giocatore capisce l'assurdità del dolore.

**Esempi di voce corretta:**

> _"Mio padre mi ha stretto la mano per l'ultima volta. 'Torna uomo o non tornare', ha detto."_
> _"Questa luce non serve a scaldare. Serve solo a farti vedere chiaramente cosa sta per ucciderti."_
> _"Marcia imperiale per mucchi di ossa polverose. Proteggono la polvere da altra polvere."_
> _"Ogni anno un ragazzino nuovo buttato qua sotto. Secoli di carne fresca. Questa non è una prova, è un mattatoio burocratico."_
> _"Questo è l'inferno della gente per bene. Tutti morti, tutti in riga."_

---

## 2. Meccaniche del gioco — introduzione progressiva

Ogni meccanica deve essere **introdotta una alla volta**, in un contesto dove il giocatore capisce cosa sta succedendo senza possibilità di confusione. Non introdurre mai due meccaniche nuove nello stesso livello.

### Mappa delle meccaniche e quando introdurle

| Meccanica                                    | Livello di introduzione | Note                                                          |
| -------------------------------------------- | ----------------------- | ------------------------------------------------------------- |
| Movimento del giocatore                      | 1                       | Implicita — il giocatore scopre da solo                       |
| Raccogliere la chiave                        | 1                       | Chiave visibile, percorso diretto                             |
| Aprire l'uscita con la chiave                | 1                       | Uscita vicina alla chiave nei primi livelli                   |
| Raccogliere le candele                       | 1                       | Candele sul percorso, testo esplicativo                       |
| Piazzare una candela (SPAZIO)                | 2                       | Tutorial esplicito con testo                                  |
| Zone buie (Buio)                             | 2                       | Zona buia piccola, candela necessaria per passare             |
| Aprire/chiudere porte (CTRL)                 | 3–4                     | Una singola porta, testo esplicativo                          |
| Nemico Zombie — patruglia                    | 3                       | Un singolo zombie, meccanica distrazione candela              |
| Distrazione con candela                      | 3                       | Insegnata con il primo zombie                                 |
| Trappola temporizzata (TrappolaTemporizzata) | 5                       | Una trappola ovvia, si può attraversare di corsa              |
| Nemico Scheletro — visuale lineare           | 6                       | Da solo in stanza aperta                                      |
| Muri rompibili (Fragile, WallType=2)         | 8                       | Prima mostrati come muri distinguibili, poi Taurus li abbatte |
| Nemico Taurus — carica                       | 8                       | Da solo, corridoio dove la carica è dimostrativa              |
| Trappola letale (TrappolaMorteIstantanea)    | 9                       | Introdotta visivamente, non nascosta                          |
| Nemico Ghost — stai fermo                    | 10                      | Da solo, stanza dove il giocatore DEVE fermarsi               |
| Nemico Bubu — spegne candele                 | 12                      | Con 1–2 zombie, per insegnare la scarsità                     |
| Nemico Goblin — inseguimento puro            | 15                      | **Da solo in un livello dedicato**                            |
| Mix completo nemici                          | 17+                     | Solo dopo che il giocatore conosce tutti                      |

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
- **Placement consigliato:** **introdurlo da solo in un livello dedicato** (livello 15), lontano dallo spawn. Solo nei livelli finali usarlo in combinazione. Non usarlo prima del livello 15.

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
- Il primo incontro con muri fragili (livello 8) deve avvenire con un Taurus che li sfonda in modo spettacolare
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

### Arco narrativo in quattro atti

| Atto                   | Livelli | Texture         | Atmosfera                                              |
| ---------------------- | ------- | --------------- | ------------------------------------------------------ |
| **I — La discesa**     | 1–5     | pacco1 → pacco2 | Rovine esterne, ingresso nel labirinto, prime creature |
| **II — Le profondità** | 6–10    | pacco3 → pacco5 | Dungeon vero, guardiani antichi, l'orrore si consolida |
| **III — Il cuore**     | 11–16   | pacco6 → pacco8 | Il punto più buio, scarsità totale, il cacciatore      |
| **IV — Il finale**     | 17–20   | pacco8 → pacco1 | Tutto insieme, risalita, il colpo di scena             |

---

### Schema per 20 livelli

#### Atto I — La discesa (Livelli 1–5)

---

**Livello 1 — "L'ingresso"**
`pacco1` | Nessun nemico | Meccanica: movimento, chiave, uscita, candele (raccogliere) | Candele: abbondanti

Layout quasi completamente illuminato da brazier. La chiave è visibile dall'inizio. Obiettivo: il giocatore impara i controlli senza pressione. Mappa lineare con 1–2 stanze aperte, ingresso con muro `Ingresso` (WallType=5).

_Testi:_

> "Mio padre mi ha stretto la mano così forte che mi ha fatto male. 'Torna uomo o non tornare', ha detto. Mi ha dato questa candela. Puzza di grasso rancido." _(all'ingresso)_
> "Il nome di mio fratello è graffiato su questa porta. Lo chiamarono codardo per non essere uscito. Non sapevano che di qua, il coraggio, serve solo a morire più tardi." _(vicino all'uscita)_

---

**Livello 2 — "Il buio"**
`pacco1` | Nessun nemico | Meccanica: piazzare candela (SPAZIO), zona buia (`Buio`) | Candele: abbondanti

Stesso ambiente esterno. Una stanza centrale ha una zona `Buio` obbligatoria. Il giocatore scopre che senza candela non vede. La soluzione è ovvia: piazza, illumina, attraversa. Tutorial esplicito.

_Testi:_

> "Questa luce non serve a scaldare. Serve solo a farti vedere chiaramente cosa sta per ucciderti. Finché dura." _(entrando nella zona buia)_
> "Piazzare la candela a terra illumina di più. Ti compri un po' di vista, al prezzo di restare al buio dopo. Ottimo patto." _(tutorial piazzamento)_

---

**Livello 3 — "La prima creatura"**
`pacco1/2` | **1 Zombie** | Meccanica: distrazione zombie con candela | Candele: abbondanti, vicino allo zombie

Il primo nemico deve essere un momento chiaro e didattico. Un singolo Zombie pattuglia un corridoio obbligato. Le candele sono posizionate appena prima di lui — il giocatore le trova naturalmente. Un testo suggerisce la meccanica senza svelarla del tutto.

_Testi:_

> "Quello strascica i piedi come zio Thomas. Ha i suoi vestiti. Il rito non ti uccide, ti svuota e ti lascia a pattugliare il nulla per sempre." _(vedendo lo zombie)_
> "Sono attratti dalla luce come falene morte. Se lascio la candela lì a terra, sbraneranno quella invece di me." _(vicino alle candele, prima dello zombie)_

---

**Livello 4 — "Gli automi"**
`pacco2` | **2–3 Zombie** | Meccanica: distrazione multipla, sequenza | Candele: sufficienti

Più zombie che bloccano percorsi diversi. Il giocatore deve gestire le distrazioni in sequenza. Introduce la prima **porta** (CTRL) come elemento di separazione tra zone.

_Testi:_

> "Ogni anno un ragazzino nuovo buttato qua sotto. Secoli di carne fresca. Questa non è una prova, è un mattatoio burocratico." _(nella stanza principale)_
> "Questa porta (CTRL) è l'unica linea vera tra me e loro. Speriamo che questi idioti non sappiano usare la maniglia." _(tutorial porta)_

---

**Livello 5 — "I corridoi del rito"**
`pacco2` | **2–3 Zombie** | Meccanica: prima **trappola temporizzata** | Candele: sufficienti

Zombie in combinazione con trappole temporizzate ovvie (visibili). Il giocatore impara che alcune trappole si possono attraversare di corsa. Mappa con più stanze, un paio di vicoli ciechi.

_Testi:_

> "I corridoi sono levigati. Li hanno percorsi in troppi prima di me." _(nel corridoio principale)_
> "Delle lame fasate a tempo. Una trappola pigra, stupida, ma efficace. Basta correre quando cede. Facile morire per un'esitazione." _(vicino alla trappola temporizzata)_

---

#### Atto II — Le profondità (Livelli 6–10)

---

**Livello 6 — "I soldati di polvere"**
`pacco3` | **1 Scheletro** (+ 1 Zombie opzionale) | Meccanica: visuale lineare (line-of-sight) | Candele: moderate

Stanza aperta dove la visuale è evidente. Il giocatore vede lo scheletro, vede la linea retta, capisce il pericolo prima di sbatterci. Primo incontro con un'architettura pensata per la visuale.

_Testi:_

> "Marcia imperiale per mucchi di ossa polverose. Proteggono la polvere da altra polvere. L'ordine perfetto non ha bisogno di vita." _(entrando nella stanza)_
> "Se entro nella loro linea visiva, caricano. Hanno barattato l'anima per la disciplina. Meglio passargli alle spalle mentre fissano il nulla." _(vicino allo scheletro)_

---

**Livello 7 — "La sorveglianza"**
`pacco3` | **2 Scheletri + 1–2 Zombie** | Meccanica: line-of-sight multipla, angoli ciechi | Candele: moderate

Più scheletri che coprono percorsi incrociati. Il giocatore deve imparare a muoversi negli angoli ciechi. Mappa a 4–5 stanze con architettura che valorizza il line-of-sight.

_Testi:_

> "Tutto questo posto è costruito per essere sorvegliato. Chi l'ha progettato non voleva che nessuno uscisse senza essere visto." _(nel corridoio centrale)_

---

**Livello 8 — "La rabbia cieca"**
`pacco4` | **1 Taurus** | Meccanica: carica del Taurus, **muri rompibili** | Candele: moderate

Il Taurus deve essere presentato in modo spettacolare. Il percorso obbligato passa per un corridoio con un muro fragile. Il Taurus pattuglia dall'altra parte. Quando il giocatore entra nel campo visivo, la carica sfonda il muro — momento "wow" che insegna entrambe le meccaniche.

_Testi:_

> "Carica e sfonda i muri a testate. La rabbia pura, senza direzione. Un bestione cieco che spacca la creta. Tutto muscoli, nessun pensiero." _(dopo il muro sfondato)_
> "Tanta forza, così poca utilità. Mi ricorda le urla di mio nonno quando le cose non andavano come diceva lui." _(nel corridoio del Taurus)_

---

**Livello 9 — "I guardiani rotti"**
`pacco4/5` | **1 Taurus + 2 Zombie** | Meccanica: muri fragili strategici, **trappola letale** | Candele: scarse

Il giocatore inizia a sfruttare i muri fragili a proprio vantaggio (fare in modo che il Taurus sfonda una parete utile). Prima trappola letale visibile, mai obbligatoria.

_Testi:_

> "Hanno costruito muri di carta sperando che chi è furioso non se ne accorga. Se mi faccio caricare verso quella parete, il mostro mi aprirà la strada." _(vicino al muro fragile strategico)_

---

**Livello 10 — "Il senso di colpa"**
`pacco5` | **1 Ghost** | Meccanica: fermarsi per sopravvivere | Candele: scarse

Stanza isolata con un Ghost. Il giocatore entra, il Ghost si avvicina. Se si muove viene attaccato. Deve stare fermo abbastanza a lungo da far passare il Ghost. Livello breve — la tensione è psicologica, non esplorativa.

_Testi:_

> "Quel fantasma fa solo una cosa: ti uccide se cerchi di lottare. Se ti arrendi e resti immobile, passa oltre. Il labirinto premia la sottomissione totale." _(entrando nella stanza del Ghost)_
> "Solo ombre che un tempo erano ragazzi come me, convinti di essere speciali." _(dopo aver superato il Ghost)_

---

#### Atto III — Il cuore (Livelli 11–16)

---

**Livello 11 — "Fermo nel caos"**
`pacco5/6` | **1 Ghost + 2 Zombie** | Meccanica: Ghost in combinazione con pericoli che forzano il movimento | Candele: scarse

Il vero horror del Ghost: i zombie ti forzano a muoverti, il Ghost attacca se ti muovi. Il giocatore deve trovare finestre di immobilità tra i movimenti degli zombie.

_Testi:_

> "Da una parte un morto che ti insegue, dall'altra uno spettro che ti uccide se scappi. La scelta dell'uomo libero: farsi sbranare fermo, o farsi sbranare correndo." _(nella stanza centrale)_

---

**Livello 12 — "Il sabotaggio"**
`pacco6` | **1 Bubu + 2 Zombie** | Meccanica: Bubu spegne le candele | Candele: poche, gestione attenta

Il Bubu non uccide — ruba le candele. Il giocatore scopre che la risorsa su cui si è affidato può scomparire. Prima volta che la gestione delle candele è un problema strategico.

_Testi:_

> "Quei cosi blu non ti attaccano alla gola, spengono le candele. Ti vogliono cieco e spaventato come loro. La solidarietà dei miserabili." _(vedendo il Bubu per la prima volta)_
> "Perché mi rubano la luce? Forse perché nel rito non è previsto che tu veda davvero dove stai andando. Devi solo obbedire al percorso." _(dopo che il Bubu spegne una candela)_

---

**Livello 13 — "La privazione"**
`pacco6/7` | **2 Bubu + 2 Scheletri** | Meccanica: scarsità critica di risorse | Candele: pochissime, nascoste

Le candele sono nascoste e poche. Il giocatore deve scegliere quando usarle. I Bubu pattugliano le zone dove le candele potrebbero essere piazzate.

_Testi:_

> "Se accendo l'ultima candela me la rubano subito. Se non l'accendo muoio inciampando nel buio. Abbiamo un fottuto senso dell'umorismo quaggiù." _(nel punto critico della mappa)_

---

**Livello 14 — "Il labirinto vero"**
`pacco7` | **Zombie + Scheletri + Bubu** | Meccanica: mix completo senza Goblin | Candele: pochissime

Mappa complessa con più stanze, trappole, e tutti i nemici introdotti finora. La prima vera prova d'insieme. Nessun testo tutorial — solo atmosfera.

_Testi:_

> "Mio padre è davvero uscito di qua o l'ha solo raccontato? Se sapeva cosa c'era qua sotto, è un mostro. Se non lo sapeva, è un bugiardo." _(in una zona di pausa)_

---

**Livello 15 — "L'odio"**
`pacco7/8` | **1 Goblin** (da solo) | Meccanica: inseguimento puro, nessuna distrazione possibile | Candele: scarse, ma usarle velocizza il Goblin

Un Goblin da solo in una mappa ampia. Il Goblin insegue sempre — non si può distrarre come lo zombie. Ogni candela usata lo velocizza. Il giocatore impara che deve trovare la chiave e uscire minimizzando l'uso di risorse.

_Testi:_

> "Quello coso è puro odio in forma di fame. Ogni candela che accendo lo eccita. Mi sta cacciando, implacabile. E gode nel farlo." _(all'inizio, vedendo il Goblin)_
> "Odia il fatto che io abbia ancora una luce, ma gliela lascio inseguire finché avrò fiato." _(durante l'inseguimento)_

---

**Livello 16 — "Il cacciatore instancabile"**
`pacco8` | **1 Goblin + 2–3 Zombie** | Meccanica: Goblin in combinazione | Candele: rarissime

Il Goblin non è più solo. I zombie bloccano le vie di fuga, il Goblin insegue. Il giocatore deve usare i zombie come ostacoli per rallentare il Goblin.

_Testi:_

> "Alla fine vince il labirinto. È solo una trita-carne. L'unica mossa vincente era non giocare." _(vicino all'uscita)_

---

#### Atto IV — Il finale (Livelli 17–20)

---

**Livello 17 — "Tutto insieme"**
`pacco8` | **Zombie + Scheletri + Bubu + Taurus** (no Goblin) | Meccanica: orchestrazione completa | Candele: rarissime

Mix completo di nemici tranne il Goblin. Mappa grande con più strade, trappole letali, zone buie. La prova di maturità prima del finale.

_Testi:_

> "Questo è l'inferno della gente per bene. Pieno di regole, divieti, turni di pattuglia e sacrifici inutili. Tutti morti, tutti in riga." _(nella zona centrale)_

---

**Livello 18 — "La prova definitiva"**
`pacco8` | **Mix completo + Goblin** | Meccanica: tutto insieme | Candele: 1–2 pacchi nascosti

La combinazione più pericolosa. Il Goblin insegue mentre gli altri bloccano le vie. Mappa ampia con abbastanza spazio per manovrare ma nessun margine di errore.

_Testi:_

> "L'ultima sfida. Il Goblin è l'odio puro per la vita. È quello che resta quando togli a un essere umano tutto tranne la volontà di sopraffare gli altri." _(vedendo il Goblin nel mix)_

---

**Livello 19 — "Il Ventre del Padre"**
`pacco8` | **Mix completo** | Meccanica: massima pressione, architettura claustrofobica | Candele: 1 pacco visibile, altri nascosti

Il punto più buio e claustrofobico del gioco. Mappa densa, corridoi stretti, zone buie estese. Narrativamente: Mark è nel cuore del labirinto, il punto più lontano dall'uscita.

_Testi:_

> "La chiave è pesantissima. Sembra fatta d'oro, ma sa di piombo." _(sulla chiave)_
> "L'ultima porta. Sono diventato la macchina spietata che volevano. Ho calpestato morti e ombre pregando solo per me stesso. Aprite. Avete vinto voi." _(vicino all'uscita)_

---

**Livello 20 — "Il giardino vuoto" / "Il trono"**
`pacco8` → `pacco1` | Mix finale | Il colpo di scena | Candele: scarse

La mappa inizia scura (pacco8) e si schiarisce verso l'uscita, dove la texture diventa pacco1 — le stesse rovine esterne del livello 1. L'uscita si apre su un'area pacco1 vuota.

Il designer sceglie uno dei due epiloghi (vedi Sezione 1):

**Variante A:**

> "Il cielo è grigio. Il villaggio è in cenere. Non c'è nessuno ad attendermi, non c'è festa né orgoglio. Hanno tirato avanti questa farsa spietata finché non si sono estinti da soli. Mi sono salvato l'anima per un mondo che aveva già smesso di esistere." _(oltre l'uscita)_

**Variante B:**

> "La luce del sole... no. Solo un'altra stanza. E sul trono c'è siede lui. Aspetta silenzioso che io prenda la sua corona di niente, per condannare il prossimo ragazzino. Non si esce dal labirinto. Ne diventi solo il fottuto padrone." _(oltre l'uscita)_

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
- [ ] I testi non spiegano la meccanica direttamente — suggeriscono
