# Linee Guida di Design — Mark In DeDark

Guida per la creazione di livelli coerenti con il tono, la narrativa e la progressione del gioco.
Per la struttura tecnica del file `.mark` vedi [`LEVEL_FORMAT.md`](./LEVEL_FORMAT.md).

---

## Indice

1. [La storia di Mark — L'eredità del vuoto](#1-la-storia-di-mark--leredità-del-vuoto)
2. [Meccaniche del gioco — introduzione progressiva](#2-meccaniche-del-gioco--introduzione-progressiva)
3. [I nemici — descrizione e placement](#3-i-nemici--descrizione-e-placement)
4. [Convenzioni di design spaziale](#4-convenzioni-di-design-spaziale)
5. [Progressione dei livelli — principi e arco narrativo](#5-progressione-dei-livelli--principi-e-arco-narrativo)
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

- **2–3 testi per livello** al massimo. Ogni testo deve avere un impatto.
- **Posizionarli prima dell'oggetto o del pericolo**, non sopra. Mark pensa mentre si avvicina.
- **Non spiegare la meccanica direttamente** — suggerire in modo utilitaristico. "Sono attratti dalla luce. Se lascio la candela lì, inseguono quella." funziona meglio di "piazza la candela per distrarre il nemico".
- **I tutorial espliciti** (premi X per fare Y) sono accettabili solo nell'Atto I (livelli 1–5), sempre in voce. Dal livello 6 in poi: solo suggerimenti indiretti.
- **Lunghezza in funzione del pericolo** — questa è la regola più importante:
  - Vicino a un nemico attivo: **massimo 2 frasi brevi**. Il giocatore sta guardando il nemico, non lo schermo.
  - In zone di pausa o transizione: massimo 3 frasi.
  - Finale (livello 20): massimo 4–5 frasi — è l'unico momento in cui il giocatore si ferma davvero.
- **L'arco della voce si comprime** man mano che Mark scende. All'inizio è spaventato e concreto (sente, annusa, riconosce). Poi diventa freddo e osservativo. Alla fine è minimalista, amaro, quasi senza fiato. Le frasi si accorciano. I punti fermi aumentano.

**Evoluzione della voce attraverso i livelli:**

| Atto        | Tono                   | Esempio                                 |
| ----------- | ---------------------- | --------------------------------------- |
| I (1–5)     | Spaventato, sensoriale | _"La candela puzza di grasso rancido."_ |
| II (6–10)   | Osservativo, freddo    | _"Proteggono il nulla."_                |
| III (11–16) | Cinico, tagliente      | _"Se non l'accendo muoio al buio."_     |
| IV (17–20)  | Minimale, rassegnato   | _"Sa di piombo."_                       |

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
| Zone buie (Buio)                             | 2                       | Zona buia piccola, la candela in mano illumina — non serve piazzarla |
| Piazzare una candela (SPAZIO)                | 3                       | Introdotta col primo zombie come meccanica di distrazione     |
| Nemico Zombie — patruglia                    | 3                       | Un singolo zombie, meccanica distrazione candela              |
| Distrazione con candela                      | 3                       | Insegnata con il primo zombie                                 |
| Aprire/chiudere porte (CTRL)                 | 4                       | Una singola porta, testo esplicativo in voce                  |
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

## 5. Progressione dei livelli — principi e arco narrativo

### Principi di progressione

1. **Un nemico nuovo per livello** — non introdurre mai due tipi di nemici nuovi insieme.
2. **Ogni nemico nuovo va introdotto in isolamento** prima di essere combinato con altri. Il Goblin deve avere un livello tutto suo.
3. **Le candele diminuiscono progressivamente** — ma ogni livello deve essere risolvibile. Un livello senza soluzione non è design, è un bug.
4. **La struttura è sempre leggibile** — tutti i livelli hanno un percorso principale chiaro. I livelli avanzati aggiungono 2–3 diramazioni brevi (max 2–3 stanze ciascuna) con candele extra o vicoli ciechi. Mai più di 2 bivi contemporanei visibili. La complessità è nei nemici, non nella navigazione.
5. **Il buio non è mai obbligatorio** — si può sempre trovare un percorso con abbastanza luce. Il buio è una sfida opzionale o una scorciatoia.

### Arco narrativo in quattro atti

| Atto                   | Livelli | Texture         | Atmosfera                                              |
| ---------------------- | ------- | --------------- | ------------------------------------------------------ |
| **I — La discesa**     | 1–5     | pacco1 → pacco2 | Rovine esterne, ingresso nel labirinto, prime creature |
| **II — Le profondità** | 6–10    | pacco3 → pacco5 | Dungeon vero, guardiani antichi, l'orrore si consolida |
| **III — Il cuore**     | 11–16   | pacco6 → pacco8 | Il punto più buio, scarsità totale, il cacciatore      |
| **IV — Il finale**     | 17–20   | pacco8 → pacco1 | Tutto insieme, risalita, il colpo di scena             |

Per lo schema dettagliato livello-per-livello (testi, nemici, meccaniche) e lo stato di avanzamento: **[`LEVEL_TASKS.md`](./LEVEL_TASKS.md)**.

---

## 6. Istruzioni per la generazione di livelli

La regola fondamentale per qualsiasi livello generato è:

- **Ogni livello introduce una cosa sola** e la insegna bene. La complessità si accumula nel tempo, non in un singolo livello.
- **Il giocatore deve sempre sapere dove andare** anche senza aver trovato la chiave. La struttura della mappa deve essere leggibile.
- **Il buio non è mai l'unica via** — ci sono sempre percorsi illuminati, anche se più pericolosi.

### Registro nomi livelli (`src/levels.ts`)

Il file `Web/src/levels.ts` contiene il manifest dei livelli pubblicati — quelli che compaiono nella pagina di selezione del gioco. Ogni voce è commentata finché il livello non è pronto.

**Workflow per ogni livello:**

1. Sviluppa il livello seguendo questa guida
2. Scegli il nome definitivo (può differire dal nome provvisorio qui sotto)
3. Decomenta la riga corrispondente in `src/levels.ts` e scrivi il nome definitivo
4. Se il nome cambia rispetto a quello indicato in questa guida, aggiornalo anche qui

I nomi provvisori per ogni livello sono quelli indicati in grassetto nello [Schema per 20 livelli](#schema-per-20-livelli).

---

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
- [ ] La voce in `src/levels.ts` è decommentata con il nome definitivo del livello
