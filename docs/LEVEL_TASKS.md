# LEVEL_TASKS.md — Tracker livelli Mark In DeDark

Stato di avanzamento della progettazione e implementazione dei 20 livelli.
Per le linee guida di design, narrativa, nemici e meccaniche: [`LEVEL_DESIGN_GUIDELINES.md`](./LEVEL_DESIGN_GUIDELINES.md).

---

## Come creare un livello

Ogni livello va creato nel formato ASCII con `level-editor.ts` e poi convertito in `.mark`.

**1. Leggi la guida completa (obbligatorio prima di iniziare):**

```
npx tsx Web/scripts/level-editor.ts --help
```

**2. Genera la griglia vuota con il pack corretto:**

```
npx tsx Web/scripts/level-editor.ts --new --pack=N > Web/public/assets/levels/N.ascii
```

**3. Disegna il livello** nel file `.ascii` generato — mappa + annotazioni secondo le specifiche in `--help`.

**4. Converti in `.mark`:**

```
npx tsx Web/scripts/ascii-to-mark.ts Web/public/assets/levels/N.ascii -o Web/public/assets/levels/N.mark
```

> **Importante:** conserva sempre entrambi i file — `N.ascii` è la sorgente editabile, `N.mark` è il formato di gioco. Non cancellare il `.ascii` dopo la conversione: è l'unico modo per riaprire e correggere il livello in futuro.

**5. Valida:**

```
npx tsx Web/scripts/validate-level.ts Web/public/assets/levels/N.mark
```

**6. Verifica il round-trip** (facoltativo ma consigliato — deve essere identico):

```
npx tsx Web/scripts/level-editor.ts Web/public/assets/levels/N.mark
```

> **Nota:** `level-editor.ts` legge solo file `.mark`, non `.ascii`. Usarlo su un `.ascii` produce output vuoto senza errori — non è un bug del file. Per verificare un `.ascii` bisogna sempre convertirlo prima (step 4) e poi visualizzare il `.mark` risultante.

**7. Registra il livello in `src/levels.ts`:**

Il file `Web/src/levels.ts` è il manifest dei livelli pubblicati — quelli che compaiono nella pagina di selezione. Ogni voce è commentata finché il livello non è pronto.

- Scegli il nome definitivo (può differire dal nome provvisorio in questo file)
- Decomenta la riga corrispondente in `Web/src/levels.ts` e scrivi il nome definitivo
- Se il nome cambia, aggiornalo anche nella tabella "Stato complessivo" qui sotto

---

## Vincoli dimensionali e strutturali obbligatori

Questi vincoli integrano le linee guida di `LEVEL_DESIGN_GUIDELINES.md` con misure concrete per ogni livello. Rispettarli è obbligatorio — un livello fuori dimensione o con trigger aggirabili è un bug di design.

### Bounding box per livello

La bounding box include il fill perimetrale ma non il vuoto (NonCella) esterno alla mappa.

| Livello | Bounding box (righe × colonne) | Celle floor max | Larghezza corridoi principali |
| ------- | ------------------------------ | --------------- | ----------------------------- |
| 1       | 9–11 × 20–28                   | 50–70           | 1–3 (ingresso/uscita: 1)      |
| 2       | 9–11 × 22–30                   | 50–75           | 1–3 (zona buia: 3–5)          |
| 3       | 11–13 × 25–33                  | 60–100          | 1–3                           |
| 4       | 13–15 × 28–36                  | 80–130          | 1–3                           |
| 5       | 13–15 × 30–38                  | 90–140          | 1–3                           |
| 6       | 13–16 × 30–38                  | 100–150         | 1–5 (stanza aperta)           |
| 7       | 14–17 × 32–40                  | 110–170         | 1–3                           |
| 8       | 13–16 × 28–36                  | 90–140          | 1–3                           |
| 9       | 14–17 × 30–38                  | 110–165         | 1–3                           |
| 10      | 13–15 × 28–35                  | 80–130          | 1–3 (arena: fino a 7×7)       |
| 11–16   | 15–20 × 33–42                  | 130–240         | 1–3                           |
| 17–20   | 18–22 × 38–44                  | 180–320         | 1–3                           |

### Regola trigger di testo — sintesi operativa

Fonte autorevole con pattern visivi: [VINCOLI INVIOLABILI §2 in LEVEL_DESIGN_GUIDELINES.md](./LEVEL_DESIGN_GUIDELINES.md#2-trigger-di-testo--solo-in-corridoi-obbligati-da-1-cella).

**Regola in una riga:** ogni `!` deve stare in un corridoio largo esattamente 1 cella. Se sopra O sotto (o a sx O dx) c'è floor → trigger aggirabile → BUG di design.

Se l'area è aperta (es. giardino livello 1–2), i trigger vanno nei colli di bottiglia di ingresso/uscita all'area, non nell'area stessa.

### Schema strutturale livello 1 — gold standard

Il livello 1 è il modello di riferimento per la struttura. Ogni livello lineare semplice deve avere questa forma:

```
[fill perimetrale]
[fill] [corridoio ingresso 1 cella] [area centrale] [corridoio uscita 1 cella] [fill]
[fill perimetrale]
```

Schematico ASCII (non in scala — mostra la struttura, non le misure esatte):

```
▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓
▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓
▓ ▓ ▓ ▓ ▓ · · · · · · · · · ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓
▓ @ ! · · · · · K · · · · · · · ! · ! · E ▓ ▓ ▓ ▓ ▓ ▓   ← spawn a sx, chiave centro, uscita dx
▓ ▓ ▓ ▓ ▓ · · · · · · · · · ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓
▓ ▓ ▓ ▓ ▓ · · · · · · · · · ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓
▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓
▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓ ▓
```

Lettura dello schema:

- **`@`** = spawn player, in corridoio ingresso (1 cella di altezza) → il player entra da sinistra
- **`!`** subito dopo `@` = primo trigger ("Mio padre...") — il player DEVE passarci sopra
- **Area centrale** = 5+ celle di altezza, abbastanza ampia — il giardino che "respira"
- **`K`** = chiave al centro dell'area aperta, visibile e raggiungibile
- **Corridoio uscita** (1 cella di altezza) = collo di bottiglia a destra
- **`!` `!`** nel corridoio uscita = trigger 2 e 3 ("Il nome di mio fratello..." e "Sotto di lui...") — obbligatori
- **`E`** = uscita, alla fine del corridoio, addossata al muro

Proporzioni reali indicative: ingresso corridor = 3–4 celle lunghe, area centrale = 5–7 celle alte × 12–16 celle larghe, uscita corridor = 4–5 celle lunghe.

---

## Note pratiche sul formato ASCII

Queste note integrano la guida di `--help` con problemi concreti incontrati durante la generazione.

**Struttura delle righe-cella (89 caratteri)**
Ogni riga-cella ha 89 posizioni (per 45 colonne): le posizioni pari `0, 2, 4, …, 88` sono le celle, le posizioni dispari `1, 3, 5, …, 87` sono i separatori verticali. Ogni carattere Unicode (inclusi `▓`, `·`, `─`, `i`, `o`) occupa esattamente 1 posizione nella stringa JavaScript — il conteggio è per caratteri, non per byte UTF-8. Righe compatte tipo `▓▓▓▓▓` funzionano: i `▓` nelle posizioni dispari vengono trattati come separatore assente (nessun muro).

**Righe muro e posizione del primo `─`**
Il contenuto di una riga muro inizia alla posizione 0 (dopo i 5 spazi di prefisso). La posizione 0 corrisponde alla colonna 0. Se il livello ha fill (`▓`) in colonna 0, scrivere `─` in posizione 0 aggiunge un muro BOTTOM sulla cella fill — innocuo visivamente ma tecnicamente impreciso. Per precisione, lasciare 2 spazi iniziali nel contenuto (`  ─ ─ ─ …`) in modo che col 0 (e il suo separatore) non ricevano il muro, e il primo `─` cada sulla colonna 1.

**Ingresso orizzontale senza MURI espliciti**
La guida avverte che per Ingresso/Uscita su lato LEFT/RIGHT in corridoi orizzontali bisognerebbe usare la sezione `MURI`. In pratica, scrivere `▓i·` nel corpo della riga-cella (fill + separatore `i` + floor) funziona correttamente: il convertitore assegna `RIGHT=5` (Ingresso) alla cella fill, che il gioco tratta come equivalente visivo di `LEFT=5` sulla cella floor. Non è necessario aggiungere voci MURI per questo caso.

**Muri obbligatori attorno al floor — regola critica**
Ogni transizione floor↔fill (e floor↔NonCella) richiede un separatore muro esplicito nel file `.ascii`. Senza di esso, il motore usa texture "da esterno" che producono artefatti visivi. Vedi VINCOLO §4 in `LEVEL_DESIGN_GUIDELINES.md` per la regola completa con esempi.

Riepilogo pratico per un livello con fill perimetrale e area floor interna:
- **Verticali `│`:** nelle righe-cella, inserire `│` nella posizione dispari tra fill e floor (es. `▓▓▓▓▓▓▓▓▓│· ·`)
- **Orizzontali `─`:** nelle righe-muro (prefisso 5 spazi), scrivere `─` alla posizione `c*2` per ogni colonna floor che confina con fill sopra o sotto (es. `               ─ ─ ─ ─ ─`)
- L'ingresso `i` e l'uscita `o` sostituiscono `│`/`─` dove serve una apertura con tipo speciale.

**Il validatore avverte "nessun pacco di candele"**
Per i livelli senza nemici (livelli 1 e 2) questa è la situazione corretta. L'avviso può essere ignorato.

---

## Checklist pre-consegna

Prima di segnare un livello come completato, verifica:

- [ ] `npx tsx Web/scripts/validate-level.ts <livello.mark>` non riporta errori
- [ ] `npm run build` passa senza errori
- [ ] Entrambi i file `N.ascii` e `N.mark` sono presenti in `Web/public/assets/levels/`
- [ ] Almeno 1 Giocatore, 1 Chiave, 1 Uscita, almeno 1 pacco di Candele
- [ ] Lo spawn del giocatore è distante dai nemici (≥ 3 celle Manhattan)
- [ ] I bordi della mappa sono chiusi (Riempimento o muri Occludente)
- [ ] Porte e uscite sono sempre addossate a un muro — mai flottanti nel vuoto
- [ ] I trigger di testo sono in passaggi obbligati (corridoi da 1 cella o soglie)
- [ ] I pacchi di candele vanno messi solo in livelli con nemici
- [ ] La meccanica nuova è introdotta in modo chiaro e non ambiguo
- [ ] Non ci sono due meccaniche nuove nello stesso livello
- [ ] Ci sono abbastanza brazier da orientare il giocatore (**solo per pack interni** — `pacco1` è all'aperto, i brazier non hanno senso)
- [ ] Nessuna zona buia è obbligatoria senza una candela disponibile vicina
- [ ] La difficoltà è coerente con il livello precedente (non salti bruschi)
- [ ] I testi narrativi sono in prima persona, tono Mark adolescente, max 3 per livello
- [ ] I testi non spiegano la meccanica direttamente — suggeriscono
- [ ] La riga in `Web/src/levels.ts` è decommentata con il nome definitivo

---

## Stato complessivo

| Livello | Nome provvisorio           | Atto | Status     |
| ------- | -------------------------- | ---- | ---------- |
| 1       | L'ingresso                 | I    | ✅ Fatto   |
| 2       | Il buio                    | I    | ⬜ Da fare |
| 3       | La prima creatura          | I    | ⬜ Da fare |
| 4       | Gli automi                 | I    | ⬜ Da fare |
| 5       | I corridoi del rito        | I    | ⬜ Da fare |
| 6       | I soldati di polvere       | II   | ⬜ Da fare |
| 7       | La sorveglianza            | II   | ⬜ Da fare |
| 8       | La rabbia cieca            | II   | ⬜ Da fare |
| 9       | I guardiani rotti          | II   | ⬜ Da fare |
| 10      | Il senso di colpa          | II   | ⬜ Da fare |
| 11      | Fermo nel caos             | III  | ⬜ Da fare |
| 12      | Il sabotaggio              | III  | ⬜ Da fare |
| 13      | La privazione              | III  | ⬜ Da fare |
| 14      | Il labirinto vero          | III  | ⬜ Da fare |
| 15      | L'odio                     | III  | ⬜ Da fare |
| 16      | Il cacciatore instancabile | III  | ⬜ Da fare |
| 17      | Tutto insieme              | IV   | ⬜ Da fare |
| 18      | La prova definitiva        | IV   | ⬜ Da fare |
| 19      | Il Ventre del Padre        | IV   | ⬜ Da fare |
| 20      | Il giardino vuoto          | IV   | ⬜ Da fare |

---

## Atto I — La discesa (Livelli 1–5)

---

### Livello 1 — "L'ingresso" ✅

`pacco1` (esterno) | Nessun nemico | Meccanica: movimento, chiave, uscita

**Bounding box:** 9–11 righe × 22–28 colonne — NON superare questi limiti. Il livello è piccolo per definizione.

**Struttura:** corridoio ingresso (1 cella alto, 3–4 celle lungo) → area giardino centrale (5–7 celle alte, 12–16 celle larghe) → corridoio uscita (1 cella alto, 4–5 celle lungo). I trigger vanno nei corridoi stretti, non nell'area aperta.

**Candele:** 0

**Mappa:** Corridoio unico orizzontale nel giardino esterno. Strettamente lineare: ingresso a sinistra con muro `Ingresso` (WallType=5), chiave visibile al centro dell'area aperta, uscita a destra con muro `Uscita` (WallType=6). Nessuna diramazione, nessun vicolo cieco. La scena è illuminata dal chiaro di luna — nessuna zona buia. Nessun brazier: siamo all'aperto.

L'area centrale può essere ampia (5–7 tile di altezza) per il respiro visivo, ma l'ingresso e l'uscita devono essere colli di bottiglia larghi 1 cella — è lì che vanno i trigger di testo. Il giocatore impara solo due cose: muoversi e raccogliere la chiave per aprire l'uscita.

_Testi (ogni riga = trigger separato):_

> "Mio padre mi ha stretto la mano troppo forte, come al solito." _(all'ingresso)_
> "Il nome di mio fratello è graffiato su questa porta." _(vicino all'uscita)_
> "Sotto di lui, altri nomi." _(passo successivo)_

---

### Livello 2 — "Il buio" ⬜

`pacco1` (esterno) | Nessun nemico | Meccanica: zona buia (`Buio`)

**Bounding box:** 9–11 righe × 24–30 colonne. Leggermente più lungo del livello 1, non più alto.

**Struttura:** corridoio ingresso (1 cella) → area illuminata → zona buia (3–5 celle larghe, 3–5 alte) → area illuminata → corridoio uscita (1 cella). I trigger si mettono nei due colli di bottiglia (ingresso zona buia, uscita zona buia) — non nell'area aperta.

**Candele:** nessuna — non ci sono nemici da distrarre e l'inventario si azzera tra i livelli: una candela raccolta qui non arriverebbe al livello 3.

**Mappa:** Corridoio unico nel giardino esterno con una sezione centrale buia obbligatoria — un angolo delle rovine dove la luna non arriva. Prima e dopo la zona buia (`░`) ci sono aree illuminate normalmente. Percorso lineare, lunghezza leggermente maggiore del livello 1. Nessun brazier. I confini della zona buia sono segnati architettonicamente — il giocatore vede dove inizia e finisce il buio.

Il giocatore scopre che alcune zone azzerano la visibilità. La candela in mano illumina l'area intorno a Mark: basta tenerla per attraversare la zona buia. Non si piazza nulla. Non c'è nessuna nuova azione da imparare.

_Testi (ogni riga = trigger separato):_

> "La luna non arriva qui." _(entrando nella zona buia)_
> "Piano." _(al centro della zona buia — 1 parola, il giocatore è concentrato)_

---

### Livello 3 — "La prima creatura" ⬜

`pacco2` (interno) | **1 Zombie** | Meccanica: piazzare la candela (SPAZIO), distrazione zombie

**Bounding box:** 11–13 righe × 25–33 colonne.

**Struttura:** stanza spawn (sicura, brazier) → collo di bottiglia (1 cella) → stanza con zombie. Il pack di candele è nel collo di bottiglia, appena prima dello zombie — il giocatore lo trova obbligatoriamente mentre avanza. Il trigger va nel corridoio di transizione, non nella stanza.

**Candele:** 3 (abbondanti — primo uso, il giocatore deve sentirsi al sicuro di sperimentare)

**Mappa:** Due stanze collegate da un corridoio stretto (collo di bottiglia 1 cella). Inizio nella prima stanza (zona sicura con brazier). Le candele sono posizionate nel corridoio stretto appena prima dello zombie. La soluzione è quasi inevitabile: il giocatore trova la candela, la piazza, lo zombie si avvicina alla fiamma.

Il primo nemico deve essere un momento didattico chiaro. Un testo suggerisce la meccanica in voce di Mark senza descriverla passo-passo.

_Testi (ogni riga = trigger separato):_

> "Ha i vestiti rituali." _(vedendo lo zombie per la prima volta)_
> "Segue la fiamma, non me." _(vicino alle candele — suggerisce la meccanica senza spiegarla)_

---

### Livello 4 — "Gli automi" ⬜

`pacco2` (interno) | **2 Zombie** | Meccanica: prima porta (CTRL) — escalazione della distrazione (non meccanica nuova)

**Bounding box:** 13–15 righe × 28–36 colonne.

**Struttura:** 3–4 stanze in sequenza, collegate da colli di bottiglia da 1 cella. La porta sta in uno di questi colli di bottiglia — non in mezzo a una stanza aperta. I trigger di testo vanno nei corridoi stretti, prima degli zombie e prima della porta.

**Candele:** 4

**Mappa:** 3–4 stanze disposte in sequenza con una leggera diramazione opzionale. La porta divide la mappa in due zone. Ancora prevalentemente lineare ma con un momento di scelta. Brazier all'ingresso della stanza con la porta e vicino all'uscita.

Due zombie che bloccano percorsi diversi. Il giocatore deve gestire le distrazioni in sequenza. La porta (CTRL) è presentata come elemento di separazione tra zone sicure e pericolose — il testo la descrive in voce, senza menzionare il tasto.

_Testi (ogni riga = trigger separato):_

> "Nessuno di loro ha trovato la strada fuori." _(nella stanza principale — passaggio obbligato)_
> "La porta obbedisce ancora." _(zona di pausa, vicino alla prima porta)_
> "Si apre, si chiude." _(passo successivo — stesso punto della porta)_

---

### Livello 5 — "I corridoi del rito" ⬜

`pacco2` (interno) | **2 Zombie** | Meccanica: prima trappola temporizzata

**Bounding box:** 13–15 righe × 30–38 colonne.

**Struttura:** percorso principale lineare con 1–2 diramazioni brevi (max 2 stanze ciascuna). Il percorso principale passa obbligatoriamente per la trappola temporizzata — è lì che si impara la meccanica. I trigger vanno nei colli di bottiglia prima della trappola.

**Candele:** 4

**Mappa:** 4–5 stanze con un paio di vicoli ciechi esplorabili (contengono candele extra). La struttura è ancora lineare nel percorso principale ma con rami secondari che invogliano l'esplorazione senza essere obbligatori. Brazier all'ingresso delle stanze principali e vicino alla trappola. La trappola temporizzata è visivamente ovvia — il giocatore la vede da lontano.

Zombie in combinazione con trappola temporizzata visibile. Il giocatore impara che alcune trappole si possono attraversare di corsa nel momento giusto.

_Testi (ogni riga = trigger separato):_

> "Quante scarpe ci sono passate sopra." _(nel corridoio principale — zona sicura)_
> "Quelle lame battono un ritmo." _(vicino alla trappola)_
> "Se non mi fermo, passo." _(stesso punto — il giocatore deve concentrarsi)_

---

## Atto II — Le profondità (Livelli 6–10)

> **Dal livello 6 in poi i testi non spiegano mai una meccanica direttamente. Solo suggerimenti indiretti.**

---

### Livello 6 — "I soldati di polvere" ⬜

`pacco3` (interno) | **1 Scheletro** | Meccanica: visuale lineare (line-of-sight)

**Bounding box:** 13–16 righe × 30–38 colonne.

**Struttura:** stanza aperta centrale (il line-of-sight è il punto) + corridoi laterali. La stanza aperta serve a rendere evidente la visuale dello scheletro — è l'eccezione alla regola delle stanze compatte, giustificata dalla meccanica. I trigger si mettono nei corridoi di accesso alla stanza, non nella stanza stessa.

**Candele:** 4

**Mappa:** Stanza aperta centrale con corridoi laterali. La forma favorisce la visuale lunga e rende evidente dove lo scheletro copre. 5–6 stanze totali, struttura a T o a croce. Brazier agli angoli — segnano i punti dove la visuale dello scheletro non arriva.

Il giocatore vede lo scheletro, vede la linea retta, capisce il pericolo prima di sbatterci. Primo incontro con un'architettura pensata attorno alla visuale.

_Testi (ogni riga = trigger separato):_

> "Pattugliano stanze vuote." _(entrando nella stanza)_
> "Guardano dritto." _(vicino allo scheletro — suggerisce la meccanica senza spiegarla)_
> "Solo dritto." _(passo successivo)_

---

### Livello 7 — "La sorveglianza" ⬜

`pacco3` (interno) | **2 Scheletri + 1 Zombie** | Meccanica: line-of-sight multipla, angoli ciechi

**Bounding box:** 14–17 righe × 32–40 colonne.

**Struttura:** 5–6 stanze con corridoi incrociati. I trigger (2 trigger totali, entrambi brevi) si posizionano in colli di bottiglia di transizione tra le stanze principali.

**Candele:** 4

**Mappa:** 5–6 stanze con architettura a corridoi incrociati. La mappa è pensata per creare angoli ciechi utili al giocatore e coperture incrociate per gli scheletri. Brazier negli angoli ciechi — guidano il giocatore verso le zone sicure.

Più scheletri che coprono percorsi incrociati. Il giocatore deve imparare a muoversi negli angoli ciechi sfruttando la geometria della mappa.

_Testi (ogni riga = trigger separato):_

> "Qui coprono tutti i corridoi." _(nel corridoio centrale)_
> "Quasi tutti." _(stesso punto — zona di transizione)_

---

### Livello 8 — "La rabbia cieca" ⬜

`pacco4` (interno) | **1 Taurus** | Meccanica: carica del Taurus, muri rompibili

**Bounding box:** 13–16 righe × 28–36 colonne.

**Struttura:** corridoio principale → stanza sicura (brazier) → corridoio sorvegliato (il player entra nel field-of-view del Taurus) → muro fragile → stanza post-sfondamento. Il trigger di anticipazione va nel corridoio sicuro prima del Taurus.

**Candele:** 3

**Mappa:** Corridoio principale con una stanza separata da un muro fragile (visivamente distinto). Il Taurus pattuglia dall'altra parte del muro. Il percorso obbligato passa per il corridoio sorvegliato: il giocatore entra nel campo visivo del Taurus, la carica parte, il muro crolla — momento spettacolare che insegna entrambe le meccaniche in un colpo solo. 4–5 stanze totali. Brazier nella stanza sicura iniziale e dopo il muro fragile.

_Testi (ogni riga = trigger separato):_

> "Qualcosa respira dall'altra parte del muro." _(prima del muro fragile — zona sicura, anticipazione)_
> "Non sa fermarsi." _(dopo il muro sfondato — player ancora in zona pericolosa, breve)_
> "Tanta forza, così poca utilità." _(zona sicura dopo il muro — player respira)_

---

### Livello 9 — "I guardiani rotti" ⬜

`pacco5` (interno) | **1 Taurus + 2 Zombie** | Meccanica: muri fragili strategici (unica meccanica nuova — vedi nota trappola)

**Bounding box:** 14–17 righe × 30–38 colonne.

**Struttura:** percorso principale con 1–2 diramazioni brevi. Il muro fragile strategico è sul percorso principale (obbligatorio). La TrappolaMorteIstantanea è solo in una diramazione opzionale. Trigger nel corridoio stretto prima del muro fragile.

**Candele:** 3

**Mappa:** 5–6 stanze con alcuni muri fragili posizionati strategicamente — il giocatore inizia a sfruttarli attivamente, guidando il Taurus verso una parete che sblocca un percorso utile. La prima `TrappolaMorteIstantanea` compare in una diramazione opzionale (mai sul percorso obbligato verso l'uscita) — viene introdotta visivamente, non didatticamente: il giocatore la vede e la schiva, senza che sia la meccanica insegnata da questo livello. Questo è il punto della tabella meccaniche "introdotta visivamente, non nascosta": la trappola letale non è la meccanica del livello, è solo la sua prima apparizione nell'ambiente. La mappa ha un percorso principale chiaro con 1–2 diramazioni brevi. Brazier vicino al muro fragile strategico e vicino all'uscita.

_Testi (ogni riga = trigger separato):_

> "Hanno costruito muri di carta." _(vicino al muro fragile strategico)_
> "Già prima di lui si vedeva dove cedevano." _(stesso punto)_

---

### Livello 10 — "Il senso di colpa" ⬜

`pacco5` (interno) | **1 Ghost** | Meccanica: fermarsi per sopravvivere

**Bounding box:** 13–15 righe × 28–35 colonne.

**Struttura:** corridoio di accesso stretto (1 cella) → arena centrale (fino a 7×7) → corridoio di uscita stretto (1 cella). La complessità è psicologica, non spaziale — la mappa è volutamente semplice. Il trigger "Non guardarlo." va nel corridoio di accesso, obbligatorio.

**Candele:** 3

**Mappa:** Stanza isolata grande, quasi una singola arena. Corridoi di accesso stretti. Livello deliberatamente compatto — la tensione è psicologica, non esplorativa. Brazier agli ingressi della stanza per illuminarla chiaramente.

Il giocatore entra, il Ghost si avvicina. Se si muove viene attaccato. Deve stare fermo abbastanza a lungo da far passare il Ghost. Il giocatore scopre la meccanica da solo — o morendo una volta, o osservando il Ghost fermarsi quando lui si ferma.

_Testi (ogni riga = trigger separato):_

> "Non guardarlo." _(entrando nella stanza — zona pericolosa, 2 parole)_
> "Ombre che un tempo erano ragazzi come me." _(dopo il Ghost, zona sicura — player respira)_

---

## Atto III — Il cuore (Livelli 11–16)

---

### Livello 11 — "Fermo nel caos" ⬜

`pacco6` (interno) | **1 Ghost + 2 Zombie** | Meccanica: Ghost in combinazione con pericoli che forzano il movimento

**Bounding box:** 15–18 righe × 33–40 colonne.

**Struttura:** 5–6 stanze, collegate da colli di bottiglia. L'unico trigger ("Non c'è risposta giusta.") va in un corridoio di transizione obbligatorio, non nella stanza centrale.

**Candele:** 4

**Mappa:** 5–6 stanze con corridoi sufficientemente larghi da avere sempre un angolo sicuro raggiungibile. Gli zombie pattugliano percorsi che si intersecano con l'arena del Ghost, ma la geometria offre sempre almeno una finestra di immobilità. Nessun vicolo cieco obbligato. Brazier agli angoli sicuri.

Il vero horror del Ghost: i zombie ti forzano a muoverti, il Ghost attacca se ti muovi. Il giocatore deve trovare finestre di immobilità tra i movimenti degli zombie.

_Testi (ogni riga = trigger separato):_

> "Non c'è risposta giusta." _(nella stanza centrale — 1 frase, il giocatore deve concentrarsi)_

---

### Livello 12 — "Il sabotaggio" ⬜

`pacco6` (interno) | **1 Bubu + 2 Zombie** | Meccanica: Bubu spegne le candele

**Bounding box:** 15–18 righe × 34–41 colonne.

**Struttura:** 6–7 stanze con colli di bottiglia tra ogni coppia. Trigger nei colli di bottiglia: il primo ("Quello blu non mi vuole morto.") prima di vedere il Bubu per la prima volta.

**Candele:** 6 (più del solito — il Bubu ne spegnerà alcune, il giocatore deve averne abbastanza)

**Mappa:** 6–7 stanze con aree semi-aperte. Le candele piazzabili sono sparse in posizioni che il Bubu pattuglia naturalmente. Il giocatore è costretto a pensare dove posarle. Brazier nelle zone sicure per compensare la perdita di candele.

Il Bubu non uccide — ruba le candele. Il giocatore scopre che la risorsa su cui si è affidato può scomparire. Prima volta che la gestione delle candele è un problema strategico.

_Testi (ogni riga = trigger separato):_

> "Quello blu non mi vuole morto." _(vedendo il Bubu)_
> "Mi vuole al buio." _(stesso punto)_
> "Come se la mia luce fosse un'offesa." _(dopo che il Bubu spegne una candela — 1 frase, lascia il peso)_

---

### Livello 13 — "La privazione" ⬜

`pacco6` (interno) | **2 Bubu + 2 Scheletri** | Meccanica: scarsità critica di risorse

**Bounding box:** 15–18 righe × 34–41 colonne.

**Struttura:** percorso principale leggibile + 1–2 diramazioni brevi (con le ultime candele disponibili). Trigger sui due pack candele: "Un'ultima candela." e "Devo farla bastare." sono trigger piazzati sulla cella `!` adiacente all'ultimo pack, in un corridoio stretto.

**Candele:** 2 (pochissime — è il punto del livello)

**Mappa:** 6–7 stanze con zone buie sparse. Le candele sono nascoste in posizioni scomode — il giocatore deve scegliere se rischiare l'esplorazione in diramazioni brevi (max 2 stanze). Percorso principale sempre leggibile. Brazier nelle zone di snodo per evitare che il giocatore si perda completamente.

Le candele sono poche e i Bubu pattugliano le zone dove potrebbero essere piazzate. Il giocatore deve scegliere quando e dove usarle.

_Testi (ogni riga = trigger separato):_

> "Un'ultima candela." _(sull'ultima candela disponibile — trigger posizionato sul pacco dell'ultima candela rimasta nella mappa, dopo che il giocatore ha già usato le altre)_
> "Devo farla bastare." _(passo successivo, stesso punto)_

---

### Livello 14 — "Il labirinto vero" ⬜

`pacco7` (interno) | **2 Zombie + 2 Scheletri + 1 Bubu** | Meccanica: mix completo senza Goblin

**Bounding box:** 16–20 righe × 35–42 colonne.

**Struttura:** percorso principale con 2–3 diramazioni brevi. Il percorso principale è sempre quello più ovvio (il più dritto). I trigger atmosferici ("Mio padre è davvero uscito di qua...") vanno in una zona di pausa: un corridoio largo 1 cella poco prima di un bivio.

**Candele:** 4

**Mappa:** 8–10 stanze con percorso principale leggibile e 2–3 diramazioni opzionali brevi (max 2 stanze ciascuna). Le diramazioni contengono candele extra o vicoli ciechi. Il percorso principale è sempre il più ovvio. Brazier agli incroci per guidare il giocatore. Zone buie presenti ma non obbligatorie sul percorso principale. Trappole letali solo nelle diramazioni, mai sul percorso obbligato.

La prima vera prova d'insieme. Nessun testo tutorial — solo atmosfera.

_Testi (ogni riga = trigger separato):_

> "Mio padre è davvero uscito di qua, o l'ha solo raccontato?" _(in una zona di pausa)_
> "Se sapeva cosa c'era qua sotto, è un mostro." _(stesso punto o passo successivo)_
> "Se non lo sapeva, è un bugiardo." _(stesso punto)_

---

### Livello 15 — "L'odio" ⬜

`pacco7` (interno) | **1 Goblin** (da solo) | Meccanica: inseguimento puro, nessuna distrazione possibile

**Bounding box:** 16–20 righe × 35–42 colonne.

**Struttura:** mappa ampia con corridoi larghi (2–3 celle) per dare spazio di manovra al Goblin. Anche con corridoi più larghi, i trigger si mettono comunque in restringimenti a 1 cella — es. soglie tra stanze, porte, angoli. Il Goblin è distante dallo spawn: il player ha qualche secondo prima che arrivi.

**Candele:** 2 (critiche — ogni candela piazzata accelera il Goblin)

**Mappa:** Mappa ampia e aperta, 8–9 stanze, corridoi larghi. Lo spazio è necessario perché il Goblin insegue sempre — il giocatore ha bisogno di margine per manovrare. Pochi vicoli ciechi, percorso principale leggibile. Brazier sparsi — il Goblin li ignora, servono solo al giocatore per orientarsi.

Il Goblin insegue sempre — non si può distrarre come lo zombie. Il giocatore deve trovare la chiave e uscire. Scoprirà da solo il legame tra candele e velocità del Goblin.

_Testi (ogni riga = trigger separato):_

> "Questo mi odia." _(vedendo il Goblin — 3 parole, il player comincia a correre)_
> "È la fiamma che lo nutre." _(qualche stanza dopo, zona di transizione — suggerisce il legame candele/velocità senza spiegarlo)_

---

### Livello 16 — "Il cacciatore instancabile" ⬜

`pacco8` (interno) | **1 Goblin + 2 Zombie** | Meccanica: Goblin in combinazione

**Bounding box:** 16–20 righe × 35–42 colonne.

**Struttura:** 8–9 stanze con corridoi più stretti del livello 15. Il trigger ("L'unica mossa vincente era non giocare.") va nel corridoio finale prima dell'uscita — 1 cella di larghezza, obbligatorio.

**Candele:** 2

**Mappa:** 8–9 stanze con corridoi più stretti del livello 15. Gli zombie occupano le vie di fuga principali. La mappa costringe a percorsi più contorti dove il Goblin guadagna terreno. Il giocatore deve usare i zombie come ostacoli passivi per rallentare il Goblin. Percorso principale sempre identificabile. Brazier vicino all'uscita.

_Testi (ogni riga = trigger separato):_

> "L'unica mossa vincente era non giocare." _(vicino all'uscita — 1 frase, pianta il seme del finale)_

---

## Atto IV — Il finale (Livelli 17–20)

---

### Livello 17 — "Tutto insieme" ⬜

`pacco8` (interno) | **2 Zombie + 2 Scheletri + 1 Bubu + 1 Taurus** (no Goblin) | Meccanica: orchestrazione completa

**Bounding box:** 18–21 righe × 38–43 colonne.

**Struttura:** 10–12 stanze, percorso principale chiaro, 2–3 diramazioni brevi. Trigger ("Questo è l'inferno della gente per bene." / "Tutti morti, tutti in riga.") in una zona di pausa — corridoio stretto prima della stanza centrale, obbligatorio.

**Candele:** 4

**Mappa:** 10–12 stanze con percorso principale chiaro e 2–3 diramazioni brevi. Brazier agli incroci principali — la complessità è nei nemici, non nella navigazione. Zone buie presenti ma non obbligatorie. Trappole letali solo nelle diramazioni.

Mix completo di nemici tranne il Goblin. La prova di maturità prima del finale.

_Testi (ogni riga = trigger separato):_

> "Questo è l'inferno della gente per bene." _(nella zona centrale)_
> "Tutti morti, tutti in riga." _(stesso punto)_

---

### Livello 18 — "La prova definitiva" ⬜

`pacco8` (interno) | **2 Zombie + 2 Scheletri + 1 Bubu + 1 Taurus + 1 Goblin** | Meccanica: tutto insieme

**Bounding box:** 18–22 righe × 39–44 colonne.

**Struttura:** percorso ad anello — il player può sempre fuggire in avanti. I trigger ("Sono tornato all'inizio." / "Solo che ora conosco il nome di ogni mostro.") vanno nella sezione di ingresso, in un corridoio stretto prima della mischia principale.

**Candele:** 3

**Mappa:** 10–12 stanze con percorso principale ad anello — il giocatore può sempre fuggire in avanti senza tornare indietro. 1–2 diramazioni brevi. Brazier ai punti di svolta dell'anello. Il Goblin insegue mentre gli altri bloccano, ma l'anello garantisce sempre una via avanti.

La combinazione più pericolosa.

_Testi (ogni riga = trigger separato):_

> "Sono tornato all'inizio." _(zona di ingresso, prima della mischia)_
> "Solo che ora conosco il nome di ogni mostro." _(stesso punto)_

---

### Livello 19 — "Il Ventre del Padre" ⬜

`pacco8` (interno) | **Mix completo** | Meccanica: massima pressione, architettura claustrofobica

**Bounding box:** 19–22 righe × 40–44 colonne.

**Struttura:** 10–12 stanze compresse, corridoi prevalentemente da 1 cella. La claustrofobia è strutturale. I trigger ("La chiave è pesantissima." sulla cella chiave, "Sa di piombo." nel corridoio adiacente, "Sono diventato quello che volevano." nel corridoio finale prima dell'uscita) sono tutti in posizioni obbligatorie — il corridoio 1-cella garantisce l'attivazione.

**Candele:** 2

**Mappa:** Mappa densa con corridoi stretti, 10–12 stanze molto compresse. Zone buie estese. La sensazione di oppressione è architettonica — i corridoi larghi del livello 18 qui spariscono. Il punto più claustrofobico del gioco. Percorso principale identificabile anche nella densità. Brazier solo all'ingresso e vicino all'uscita.

Narrativamente: Mark è nel cuore del labirinto, il punto più lontano dall'uscita.

_Testi (ogni riga = trigger separato):_

> "La chiave è pesantissima." _(sulla chiave)_
> "Sa di piombo." _(passo successivo)_
> "Sono diventato quello che volevano." _(vicino all'uscita — rassegnazione totale)_

---

### Livello 20 — "Il giardino vuoto" ⬜

`pacco8` | Mix finale + finale narrativo

> **Nota sul pack:** Il livello usa `pacco8` per tutta la durata. L'area "esterna" del finale (il giardino in cenere) è una sezione del livello con layout aperto che evoca l'esterno, ma usa le stesse texture `pacco8`. L'effetto di contrasto con il livello 1 è narrativo, non visivo. Alternativa futura: implementare supporto a due pack per livello.

**Bounding box:** 20–22 righe × 40–44 colonne.

**Struttura:** sezione densa (come livello 19) → passaggio di transizione (collo di bottiglia 1 cella) → area aperta finale. I trigger nel finale ("Il cielo è grigio." / "Il villaggio è in cenere." / "Ho passato l'inferno...") si mettono nel passaggio di transizione e nella prima cella dell'area aperta — non in mezzo all'area aperta dove il player potrebbe aggirarli. Il percorso verso l'uscita è rettilineo nell'area finale.

**Candele:** 2

**Mappa:** Due sezioni distinte. La prima (densa, corridoi stretti) è come il livello 19. La seconda (layout aperto, mura perimetrali lontane) evoca il giardino esterno del livello 1 — ora devastato. Il passaggio tra le due sezioni è fisicamente attraversabile. I 3 trigger finali sono posizionati nel collo di bottiglia di transizione e sul percorso rettilineo verso l'uscita.

_Testi (ogni riga = trigger separato):_

> "Il cielo è grigio." _(appena fuori)_
> "Il villaggio è in cenere." _(stesso punto)_
> "Ho passato l'inferno per un mondo che non esiste più." _(al centro dell'area — colpo finale)_
