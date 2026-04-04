# LEVEL_TASKS.md — Tracker livelli Mark In DeDark

Stato di avanzamento della progettazione e implementazione dei 20 livelli.
Per le linee guida di design, narrativa, nemici e meccaniche: [`LEVEL_DESIGN_GUIDELINES.md`](./LEVEL_DESIGN_GUIDELINES.md).

---

## Nota sui file .mark esistenti

I file `.mark` con status **⬜ Da fare** nella tabella sono **placeholder da sovrascrivere** — possono contenere dati di test o livelli vecchi non più validi. Ignorarne il contenuto e rigenerare il livello secondo le specifiche in questo file.

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

**7. Registra il livello in `src/levels.ts`:**

Il file `Web/src/levels.ts` è il manifest dei livelli pubblicati — quelli che compaiono nella pagina di selezione. Ogni voce è commentata finché il livello non è pronto.

- Scegli il nome definitivo (può differire dal nome provvisorio in questo file)
- Decomenta la riga corrispondente in `Web/src/levels.ts` e scrivi il nome definitivo
- Se il nome cambia, aggiornalo anche nella tabella "Stato complessivo" qui sotto

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
- [ ] Ci sono abbastanza brazier da orientare il giocatore
- [ ] Nessuna zona buia è obbligatoria senza una candela disponibile vicina
- [ ] La difficoltà è coerente con il livello precedente (non salti bruschi)
- [ ] I testi narrativi sono in prima persona, tono Mark adolescente, max 3 per livello
- [ ] I testi non spiegano la meccanica direttamente — suggeriscono
- [ ] La riga in `Web/src/levels.ts` è decommentata con il nome definitivo

---

## Stato complessivo

| Livello | Nome provvisorio           | Atto | Status     |
| ------- | -------------------------- | ---- | ---------- |
| 1       | L'ingresso                 | I    | ⬜ Da fare |
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

**Candele:** 0

**Mappa:** Corridoio unico orizzontale nel giardino esterno, abbastanza largo da respirare (7–9 tile di altezza). Strettamente lineare: ingresso a sinistra con muro `Ingresso` (WallType=5), chiave al centro, uscita a destra. Nessuna diramazione, nessun vicolo cieco. L'obiettivo è visibile quasi dall'inizio. La scena è illuminata dal chiaro di luna — nessuna zona buia. Brazier vicino alla chiave e vicino all'uscita come punti di riferimento visivi.

Il giocatore impara solo due cose: muoversi e raccogliere la chiave per aprire l'uscita. Nient'altro.

_Testi (ogni riga = trigger separato):_

> "Mio padre mi ha stretto la mano troppo forte, come al solito." _(all'ingresso)_
> "Il nome di mio fratello è graffiato su questa porta." _(vicino all'uscita)_
> "Sotto di lui, altri nomi." _(passo successivo)_

---

### Livello 2 — "Il buio" ⬜

`pacco1` (esterno) | Nessun nemico | Meccanica: zona buia (`Buio`)

**Candele:** nessuna — non ci sono nemici da distrarre e l'inventario si azzera tra i livelli: una candela raccolta qui non arriverebbe al livello 3.

**Mappa:** Corridoio unico nel giardino esterno con una sezione centrale buia obbligatoria — un angolo delle rovine dove la luna non arriva. Prima e dopo la zona buia ci sono aree illuminate normalmente. Percorso ancora lineare, lunghezza leggermente maggiore del livello 1. Brazier prima e dopo la zona buia per segnare i confini — il giocatore capisce dove inizia e finisce il buio.

Il giocatore scopre che alcune zone azzerano la visibilità. La candela in mano illumina l'area intorno a Mark: basta tenerla per attraversare la zona buia. Non si piazza nulla. Non c'è nessuna nuova azione da imparare: bisogna solo muoversi con cautela.

_Testi (ogni riga = trigger separato):_

> "La luna non arriva qui." _(entrando nella zona buia)_
> "Piano." _(al centro della zona buia — 1 parola, il giocatore è concentrato)_

---

### Livello 3 — "La prima creatura" ⬜

`pacco2` (interno) | **1 Zombie** | Meccanica: piazzare la candela (SPAZIO), distrazione zombie

**Candele:** 3 (abbondanti — primo uso, il giocatore deve sentirsi al sicuro di sperimentare)

**Mappa:** Due stanze collegate da un corridoio stretto. Inizio nella prima stanza (zona sicura con brazier), poi si passa al corridoio interno dove pattuglia lo zombie. Le candele da raccogliere sono posizionate appena prima dello zombie — il giocatore le trova naturalmente mentre avanza. La soluzione è quasi inevitabile: il giocatore trova la candela, la piazza, lo zombie si avvicina alla fiamma.

Il primo nemico deve essere un momento didattico chiaro. Un testo suggerisce la meccanica in voce di Mark senza descriverla passo-passo.

_Testi (ogni riga = trigger separato):_

> "Ha i vestiti rituali." _(vedendo lo zombie per la prima volta)_
> "Segue la fiamma, non me." _(vicino alle candele — suggerisce la meccanica senza spiegarla)_

---

### Livello 4 — "Gli automi" ⬜

`pacco2` (interno) | **2 Zombie** | Meccanica: prima porta (CTRL) — escalazione della distrazione (non meccanica nuova)

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

**Candele:** 4

**Mappa:** 5–6 stanze con architettura a corridoi incrociati. La mappa è pensata per creare angoli ciechi utili al giocatore e coperture incrociate per gli scheletri. Brazier negli angoli ciechi — guidano il giocatore verso le zone sicure.

Più scheletri che coprono percorsi incrociati. Il giocatore deve imparare a muoversi negli angoli ciechi sfruttando la geometria della mappa.

_Testi (ogni riga = trigger separato):_

> "Qui coprono tutti i corridoi." _(nel corridoio centrale)_
> "Quasi tutti." _(stesso punto — zona di transizione)_

---

### Livello 8 — "La rabbia cieca" ⬜

`pacco4` (interno) | **1 Taurus** | Meccanica: carica del Taurus, muri rompibili

**Candele:** 3

**Mappa:** Corridoio principale con una stanza separata da un muro fragile (visivamente distinto). Il Taurus pattuglia dall'altra parte del muro. Il percorso obbligato passa per il corridoio sorvegliato: il giocatore entra nel campo visivo del Taurus, la carica parte, il muro crolla — momento spettacolare che insegna entrambe le meccaniche in un colpo solo. 4–5 stanze totali. Brazier nella stanza sicura iniziale e dopo il muro fragile.

_Testi (ogni riga = trigger separato):_

> "Qualcosa respira dall'altra parte del muro." _(prima del muro fragile — zona sicura, anticipazione)_
> "Non sa fermarsi." _(dopo il muro sfondato — player ancora in zona pericolosa, breve)_
> "Tanta forza, così poca utilità." _(zona sicura dopo il muro — player respira)_

---

### Livello 9 — "I guardiani rotti" ⬜

`pacco5` (interno) | **1 Taurus + 2 Zombie** | Meccanica: muri fragili strategici (unica meccanica nuova — vedi nota trappola)

**Candele:** 3

**Mappa:** 5–6 stanze con alcuni muri fragili posizionati strategicamente — il giocatore inizia a sfruttarli attivamente, guidando il Taurus verso una parete che sblocca un percorso utile. La prima `TrappolaMorteIstantanea` compare in una diramazione opzionale (mai sul percorso obbligato verso l'uscita) — viene introdotta visivamente, non didatticamente: il giocatore la vede e la schiva, senza che sia la meccanica insegnata da questo livello. Questo è il punto della tabella meccaniche "introdotta visivamente, non nascosta": la trappola letale non è la meccanica del livello, è solo la sua prima apparizione nell'ambiente. La mappa ha un percorso principale chiaro con 1–2 diramazioni brevi. Brazier vicino al muro fragile strategico e vicino all'uscita.

_Testi (ogni riga = trigger separato):_

> "Hanno costruito muri di carta." _(vicino al muro fragile strategico)_
> "Già prima di lui si vedeva dove cedevano." _(stesso punto)_

---

### Livello 10 — "Il senso di colpa" ⬜

`pacco5` (interno) | **1 Ghost** | Meccanica: fermarsi per sopravvivere

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

**Candele:** 4

**Mappa:** 5–6 stanze con corridoi sufficientemente larghi da avere sempre un angolo sicuro raggiungibile. Gli zombie pattugliano percorsi che si intersecano con l'arena del Ghost, ma la geometria offre sempre almeno una finestra di immobilità. Nessun vicolo cieco obbligato. Brazier agli angoli sicuri.

Il vero horror del Ghost: i zombie ti forzano a muoverti, il Ghost attacca se ti muovi. Il giocatore deve trovare finestre di immobilità tra i movimenti degli zombie.

_Testi (ogni riga = trigger separato):_

> "Non c'è risposta giusta." _(nella stanza centrale — 1 frase, il giocatore deve concentrarsi)_

---

### Livello 12 — "Il sabotaggio" ⬜

`pacco6` (interno) | **1 Bubu + 2 Zombie** | Meccanica: Bubu spegne le candele

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

**Candele:** 2 (pochissime — è il punto del livello)

**Mappa:** 6–7 stanze con zone buie sparse. Le candele sono nascoste in posizioni scomode — il giocatore deve scegliere se rischiare l'esplorazione in diramazioni brevi (max 2 stanze). Percorso principale sempre leggibile. Brazier nelle zone di snodo per evitare che il giocatore si perda completamente.

Le candele sono poche e i Bubu pattugliano le zone dove potrebbero essere piazzate. Il giocatore deve scegliere quando e dove usarle.

_Testi (ogni riga = trigger separato):_

> "Un'ultima candela." _(sull'ultima candela disponibile — trigger posizionato sul pacco dell'ultima candela rimasta nella mappa, dopo che il giocatore ha già usato le altre)_
> "Devo farla bastare." _(passo successivo, stesso punto)_

---

### Livello 14 — "Il labirinto vero" ⬜

`pacco7` (interno) | **2 Zombie + 2 Scheletri + 1 Bubu** | Meccanica: mix completo senza Goblin

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

**Candele:** 2 (critiche — ogni candela piazzata accelera il Goblin)

**Mappa:** Mappa ampia e aperta, 8–9 stanze, corridoi larghi. Lo spazio è necessario perché il Goblin insegue sempre — il giocatore ha bisogno di margine per manovrare. Pochi vicoli ciechi, percorso principale leggibile. Brazier sparsi — il Goblin li ignora, servono solo al giocatore per orientarsi.

Il Goblin insegue sempre — non si può distrarre come lo zombie. Il giocatore deve trovare la chiave e uscire. Scoprirà da solo il legame tra candele e velocità del Goblin.

_Testi (ogni riga = trigger separato):_

> "Questo mi odia." _(vedendo il Goblin — 3 parole, il player comincia a correre)_
> "È la fiamma che lo nutre." _(qualche stanza dopo, zona di transizione — suggerisce il legame candele/velocità senza spiegarlo)_

---

### Livello 16 — "Il cacciatore instancabile" ⬜

`pacco8` (interno) | **1 Goblin + 2 Zombie** | Meccanica: Goblin in combinazione

**Candele:** 2

**Mappa:** 8–9 stanze con corridoi più stretti del livello 15. Gli zombie occupano le vie di fuga principali. La mappa costringe a percorsi più contorti dove il Goblin guadagna terreno. Il giocatore deve usare i zombie come ostacoli passivi per rallentare il Goblin. Percorso principale sempre identificabile. Brazier vicino all'uscita.

_Testi (ogni riga = trigger separato):_

> "L'unica mossa vincente era non giocare." _(vicino all'uscita — 1 frase, pianta il seme del finale)_

---

## Atto IV — Il finale (Livelli 17–20)

---

### Livello 17 — "Tutto insieme" ⬜

`pacco8` (interno) | **2 Zombie + 2 Scheletri + 1 Bubu + 1 Taurus** (no Goblin) | Meccanica: orchestrazione completa

**Candele:** 4

**Mappa:** 10–12 stanze con percorso principale chiaro e 2–3 diramazioni brevi. Brazier agli incroci principali — la complessità è nei nemici, non nella navigazione. Zone buie presenti ma non obbligatorie. Trappole letali solo nelle diramazioni.

Mix completo di nemici tranne il Goblin. La prova di maturità prima del finale.

_Testi (ogni riga = trigger separato):_

> "Questo è l'inferno della gente per bene." _(nella zona centrale)_
> "Tutti morti, tutti in riga." _(stesso punto)_

---

### Livello 18 — "La prova definitiva" ⬜

`pacco8` (interno) | **2 Zombie + 2 Scheletri + 1 Bubu + 1 Taurus + 1 Goblin** | Meccanica: tutto insieme

**Candele:** 3

**Mappa:** 10–12 stanze con percorso principale ad anello — il giocatore può sempre fuggire in avanti senza tornare indietro. 1–2 diramazioni brevi. Brazier ai punti di svolta dell'anello. Il Goblin insegue mentre gli altri bloccano, ma l'anello garantisce sempre una via avanti.

La combinazione più pericolosa.

_Testi (ogni riga = trigger separato):_

> "Sono tornato all'inizio." _(zona di ingresso, prima della mischia)_
> "Solo che ora conosco il nome di ogni mostro." _(stesso punto)_

---

### Livello 19 — "Il Ventre del Padre" ⬜

`pacco8` (interno) | **Mix completo** | Meccanica: massima pressione, architettura claustrofobica

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

**Candele:** 2

**Mappa:** Due sezioni distinte. La prima (densa, corridoi stretti) è come il livello 19. La seconda (layout aperto, mura perimetrali lontane) evoca il giardino esterno del livello 1 — ora devastato. Il passaggio tra le due sezioni è fisicamente attraversabile. 2 trigger posizionati nell'area aperta mentre il player avanza verso l'uscita finale.

_Testi (ogni riga = trigger separato):_

> "Il cielo è grigio." _(appena fuori)_
> "Il villaggio è in cenere." _(stesso punto)_
> "Ho passato l'inferno per un mondo che non esiste più." _(al centro dell'area — colpo finale)_
