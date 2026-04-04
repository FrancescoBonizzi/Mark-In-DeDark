# LEVEL_TASKS.md — Tracker livelli Mark In DeDark

Stato di avanzamento della progettazione e implementazione dei 20 livelli.
Per le linee guida di design, narrativa, nemici e meccaniche: [`LEVEL_DESIGN_GUIDELINES.md`](./LEVEL_DESIGN_GUIDELINES.md).

---

## Nota sui pack di texture

- **`pacco1`** — esterno: giardino al chiaro di luna, rovine all'aperto. Usato per i livelli iniziali e per il finale del livello 20.
- **`pacco2`** — interno castello: ben tenuto, pulito, pavimenti rifiniti, muri intonacati. L'aspetto più "nobile" degli interni.
- **`pacco3`** — interno castello: ancora decoroso, qualche segno di usura ma strutturalmente integro.
- **`pacco4`** — interno castello: degrado visibile, muri screpolati, atmosfera più pesante.
- **`pacco5`** — interno castello: trascurato, poco illuminato, strutture che cominciano a cedere.
- **`pacco6`** — interno castello: rozzo, quasi nessun elemento decorativo, pietra a vista.
- **`pacco7`** — interno castello: fatiscente, corridoi irregolari, pavimentazione assente o rotta.
- **`pacco8`** — interno castello: il livello più grezzo — pietra nuda, nessun pavimento, architettura primitiva o crollata. Il cuore buio del castello.

### Porte

Le porte si aprono **e si chiudono** con CTRL. Possono essere usate strategicamente: chiudere una porta dietro di sé per bloccare temporaneamente un nemico in inseguimento, o aprirla per indirizzare un Taurus verso un muro fragile.

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

**5. Valida:**

```
npx tsx Web/scripts/validate-level.ts Web/public/assets/levels/N.mark
```

**6. Verifica il round-trip** (facoltativo ma consigliato — deve essere identico):

```
npx tsx Web/scripts/level-editor.ts Web/public/assets/levels/N.mark
```

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

### Livello 1 — "L'ingresso" ⬜

`pacco1` (esterno) | Nessun nemico | Meccanica: movimento, chiave, uscita

**Mappa:** Corridoio unico orizzontale nel giardino esterno, abbastanza largo da respirare (7–9 tile di altezza). Strettamente lineare: ingresso a sinistra con muro `Ingresso` (WallType=5), chiave al centro, uscita a destra. Nessuna diramazione, nessun vicolo cieco. L'obiettivo è visibile quasi dall'inizio. La scena è illuminata dal chiaro di luna — nessuna zona buia.

Il giocatore impara solo due cose: muoversi e raccogliere la chiave per aprire l'uscita. Nient'altro.

_Testi (ogni riga = trigger separato):_

> "Mio padre mi ha stretto la mano troppo forte, come al solito." _(all'ingresso)_
> "'Torna uomo o non tornare.'" _(qualche passo dopo)_
> "La candela puzza di qualcosa che non voglio sapere cos'è." _(ancora all'ingresso — zona sicura)_
> "Il nome di mio fratello è graffiato su questa porta." _(vicino all'uscita)_
> "Sotto di lui, altri nomi." _(stesso trigger o secondo trigger vicino all'uscita)_
> "Ne riconosco qualcuno." _(il silenzio fa il resto)_

---

### Livello 2 — "Il buio" ⬜

`pacco1` (esterno) | Nessun nemico | Meccanica: zona buia (`Buio`)

**Mappa:** Corridoio unico nel giardino esterno con una sezione centrale buia obbligatoria — un angolo delle rovine dove la luna non arriva. Prima e dopo la zona buia ci sono aree illuminate normalmente. Percorso ancora lineare, lunghezza leggermente maggiore del livello 1.

Il giocatore scopre che alcune zone azzerano la visibilità anche con la candela in mano. Non c'è nessuna nuova azione da imparare: bisogna solo muoversi con cautela. Nessun testo tutorial esplicito.

_Testi (ogni riga = trigger separato):_

> "La luna non arriva qui." _(entrando nella zona buia)_
> "Piano." _(al centro della zona buia — 1 parola, il giocatore è concentrato)_

---

### Livello 3 — "La prima creatura" ⬜

`pacco1/2` (transizione esterno→interno) | **1 Zombie** | Meccanica: distrazione zombie con candela (primo uso di SPAZIO)

**Mappa:** Due stanze collegate da un corridoio stretto. L'inizio è ancora all'aperto (pacco1), poi si scende sotto (pacco2). Lo zombie pattuglia il corridoio interno obbligatorio. Le candele da piazzare sono posizionate appena prima dello zombie — il giocatore le trova naturalmente mentre avanza.

Il primo nemico deve essere un momento didattico chiaro. Un testo suggerisce la meccanica senza svelarla del tutto. Qui il giocatore impara per la prima volta che può posare la candela (SPAZIO) per distrarre i nemici.

_Testi (ogni riga = trigger separato):_

> "Ha i vestiti rituali." _(vedendo lo zombie)_
> "La cicatrice di zio Thomas sul braccio." _(stesso punto o passo successivo)_
> "Segue la luce." _(vicino alle candele — suggerisce la meccanica)_
> "Se lascio la candela lì, insegue quella." _(stesso punto o passo successivo)_

---

### Livello 4 — "Gli automi" ⬜

`pacco2` (interno) | **2–3 Zombie** | Meccanica: distrazione multipla, sequenza, prima **porta** (CTRL)

**Mappa:** 3–4 stanze disposte in sequenza con qualche leggera diramazione. La porta divide la mappa in due zone. Ancora prevalentemente lineare ma con un momento di scelta.

Più zombie che bloccano percorsi diversi. Il giocatore deve gestire le distrazioni in sequenza. La porta (CTRL) è presentata come elemento di separazione tra zone sicure e pericolose.

_Testi (ogni riga = trigger separato):_

> "Quanti nomi riconosco sui muri." _(nella stanza principale)_
> "Sono entrati tutti." _(stesso punto o passo successivo)_
> "Nessuno di loro ha trovato la strada fuori." _(stesso punto, lascia il peso)_
> "Una porta. CTRL per aprirla, CTRL per chiuderla — l'unica cosa che obbedisce ancora." _(tutorial porta — zona di pausa)_

---

### Livello 5 — "I corridoi del rito" ⬜

`pacco2` (interno) | **2–3 Zombie** | Meccanica: prima **trappola temporizzata**

**Mappa:** 4–5 stanze con un paio di vicoli ciechi esplorabili. La struttura è ancora lineare nel percorso principale ma con rami secondari che invogliano l'esplorazione senza essere obbligatori.

Zombie in combinazione con trappole temporizzate visibili. Il giocatore impara che alcune trappole si possono attraversare di corsa nel momento giusto.

_Testi (ogni riga = trigger separato):_

> "I corridoi sono levigati." _(nel corridoio principale — zona sicura)_
> "Quante scarpe ci sono passate sopra." _(stesso punto o passo successivo)_
> "Quelle lame battono un ritmo. Se non mi fermo, passo." _(vicino alla trappola — breve, il giocatore deve concentrarsi)_

---

## Atto II — Le profondità (Livelli 6–10)

---

### Livello 6 — "I soldati di polvere" ⬜

`pacco3` (interno) | **1 Scheletro** (+ 1 Zombie opzionale) | Meccanica: visuale lineare (line-of-sight)

**Mappa:** Stanza aperta centrale con corridoi laterali. La forma favorisce la visuale lunga e rende evidente dove lo scheletro copre. 5–6 stanze totali, struttura a T o a croce.

Il giocatore vede lo scheletro, vede la linea retta, capisce il pericolo prima di sbatterci. Primo incontro con un'architettura pensata attorno alla visuale.

_Testi (ogni riga = trigger separato):_

> "Pattugliano stanze vuote." _(entrando nella stanza)_
> "Proteggono il nulla." _(stesso punto)_
> "Guardano dritto. Solo dritto." _(vicino allo scheletro)_
> "Un angolo cieco è già abbastanza." _(stesso punto — suggerisce la meccanica)_

---

### Livello 7 — "La sorveglianza" ⬜

`pacco3` (interno) | **2 Scheletri + 1–2 Zombie** | Meccanica: line-of-sight multipla, angoli ciechi

**Mappa:** 5–6 stanze con architettura a corridoi incrociati. La mappa è pensata per creare angoli ciechi utili al giocatore e coperture incrociate per gli scheletri.

Più scheletri che coprono percorsi incrociati. Il giocatore deve imparare a muoversi negli angoli ciechi sfruttando la geometria della mappa.

_Testi (ogni riga = trigger separato):_

> "Qui coprono tutti i corridoi." _(nel corridoio centrale)_
> "Quasi tutti." _(stesso punto — zona di transizione)_

---

### Livello 8 — "La rabbia cieca" ⬜

`pacco4` (interno) | **1 Taurus** | Meccanica: carica del Taurus, **muri rompibili**

**Mappa:** Corridoio principale con una stanza separata da un muro fragile. Il Taurus pattuglia dall'altra parte del muro. La struttura guida il giocatore nel campo visivo del Taurus in modo quasi inevitabile. 4–5 stanze totali.

Il percorso obbligato passa per il corridoio sorvegliato dal Taurus. Quando il giocatore entra nel campo visivo, la carica sfonda il muro — momento "wow" che insegna entrambe le meccaniche in un colpo solo.

_Testi (ogni riga = trigger separato):_

> "Carica e sfonda i muri a testate." _(dopo il muro sfondato — player ancora in zona pericolosa)_
> "Tanta forza, così poca utilità." _(stesso punto)_

---

### Livello 9 — "I guardiani rotti" ⬜

`pacco4/5` (interno) | **1 Taurus + 2 Zombie** | Meccanica: muri fragili strategici, **trappola letale**

**Mappa:** 5–6 stanze con alcuni muri fragili posizionati strategicamente. La prima trappola letale è visibile in un punto non obbligatorio del percorso. La mappa offre più strade ma una sola davvero sicura.

Il giocatore inizia a sfruttare i muri fragili attivamente — fare in modo che il Taurus sfonda una parete che sblocca un percorso utile. Prima trappola letale visibile, mai obbligatoria.

_Testi (ogni riga = trigger separato):_

> "Hanno costruito muri di carta." _(vicino al muro fragile strategico)_
> "Già prima di lui si vedeva dove cedevano." _(stesso punto)_

---

### Livello 10 — "Il senso di colpa" ⬜

`pacco5` (interno) | **1 Ghost** | Meccanica: fermarsi per sopravvivere

**Mappa:** Stanza isolata grande, quasi una singola arena. Corridoi di accesso stretti. Livello deliberatamente compatto — la tensione è psicologica, non esplorativa.

Il giocatore entra, il Ghost si avvicina. Se si muove viene attaccato. Deve stare fermo abbastanza a lungo da far passare il Ghost.

_Testi (ogni riga = trigger separato):_

> "Se mi muovo, mi attacca." _(entrando nella stanza)_
> "Fermo." _(stesso punto — 1 parola, player deve concentrarsi)_
> "Ombre che un tempo erano ragazzi come me." _(dopo il Ghost, zona sicura — player respira)_

---

## Atto III — Il cuore (Livelli 11–16)

---

### Livello 11 — "Fermo nel caos" ⬜

`pacco5/6` (interno) | **1 Ghost + 2 Zombie** | Meccanica: Ghost in combinazione con pericoli che forzano il movimento

**Mappa:** 5–6 stanze con corridoi abbastanza stretti. Gli zombie pattugliano percorsi che si intersecano con l'arena del Ghost. La geometria crea situazioni dove il giocatore è forzato a scegliere il male minore.

Il vero horror del Ghost: i zombie ti forzano a muoverti, il Ghost attacca se ti muovi. Il giocatore deve trovare finestre di immobilità tra i movimenti degli zombie.

_Testi (ogni riga = trigger separato):_

> "Fermo, arriva lo zombie." _(nella stanza centrale)_
> "Mi muovo, arriva il fantasma." _(stesso punto)_
> "Non c'è risposta giusta." _(stesso punto — massima brevità)_

---

### Livello 12 — "Il sabotaggio" ⬜

`pacco6` (interno) | **1 Bubu + 2 Zombie** | Meccanica: Bubu spegne le candele

**Mappa:** 6–7 stanze con aree semi-aperte. Le candele piazzabili sono sparse in posizioni che il Bubu pattuglia naturalmente. Il giocatore è costretto a pensare dove posarle.

Il Bubu non uccide — ruba le candele. Il giocatore scopre che la risorsa su cui si è affidato può scomparire. Prima volta che la gestione delle candele è un problema strategico.

_Testi (ogni riga = trigger separato):_

> "Quello blu non mi vuole morto." _(vedendo il Bubu)_
> "Mi vuole al buio." _(stesso punto)_
> "Come se la mia luce fosse un'offesa." _(dopo che il Bubu spegne una candela — 1 frase, lascia il peso)_

---

### Livello 13 — "La privazione" ⬜

`pacco6/7` (interno) | **2 Bubu + 2 Scheletri** | Meccanica: scarsità critica di risorse

**Mappa:** 6–7 stanze con zone buie sparse. Le candele sono nascoste in posti scomodi. La mappa ha qualche diramazione che porta a vicoli ciechi dove le candele potrebbero essere nascoste — il giocatore deve scegliere se rischiare l'esplorazione.

Le candele sono poche e i Bubu pattugliano le zone dove potrebbero essere piazzate. Il giocatore deve scegliere quando e dove usarle.

_Testi (ogni riga = trigger separato):_

> "Un'ultima candela." _(nel punto critico)_
> "Devo farla bastare." _(stesso punto)_

---

### Livello 14 — "Il labirinto vero" ⬜

`pacco7` (interno) | **Zombie + Scheletri + Bubu** | Meccanica: mix completo senza Goblin

**Mappa:** Prima mappa davvero grande — 8–10 stanze con struttura non lineare, più percorsi verso l'uscita, zone buie. Trappole letali in punti visibili ma pericolosi. La mappa richiede esplorazione attiva.

La prima vera prova d'insieme. Nessun testo tutorial — solo atmosfera.

_Testi (ogni riga = trigger separato):_

> "Mio padre è davvero uscito di qua, o l'ha solo raccontato?" _(in una zona di pausa)_
> "Se sapeva cosa c'era qua sotto, è un mostro." _(stesso punto o passo successivo)_
> "Se non lo sapeva, è un bugiardo." _(stesso punto)_

---

### Livello 15 — "L'odio" ⬜

`pacco7/8` (interno) | **1 Goblin** (da solo) | Meccanica: inseguimento puro, nessuna distrazione possibile

**Mappa:** Mappa ampia e aperta, 8–9 stanze, corridoi larghi. Lo spazio è necessario perché il Goblin insegue sempre — il giocatore ha bisogno di margine per manovrare. Pochi vicoli ciechi, percorso principale leggibile.

Il Goblin insegue sempre — non si può distrarre come lo zombie. Ogni candela usata lo velocizza. Il giocatore deve trovare la chiave e uscire minimizzando l'uso di risorse.

_Testi (ogni riga = trigger separato — il player li scorre mentre comincia a correre):_

> "Non si distrae." _(vedendo il Goblin)_
> "Non si stanca." _(stesso punto)_
> "Ogni candela che accendo lo avvicina." _(stesso punto)_

---

### Livello 16 — "Il cacciatore instancabile" ⬜

`pacco8` (interno) | **1 Goblin + 2–3 Zombie** | Meccanica: Goblin in combinazione

**Mappa:** 8–9 stanze con corridoi più stretti del livello 15. Gli zombie occupano le vie di fuga principali. La mappa costringe a percorsi più contorti dove il Goblin guadagna terreno.

Il Goblin non è più solo. I zombie bloccano le vie di fuga, il Goblin insegue. Il giocatore deve usare i zombie come ostacoli passivi per rallentare il Goblin.

_Testi (ogni riga = trigger separato):_

> "L'unica mossa vincente era non giocare." _(vicino all'uscita — 1 frase, pianta il seme del finale)_

---

## Atto IV — Il finale (Livelli 17–20)

---

### Livello 17 — "Tutto insieme" ⬜

`pacco8` (interno) | **Zombie + Scheletri + Bubu + Taurus** (no Goblin) | Meccanica: orchestrazione completa

**Mappa:** Mappa grande, 10–12 stanze, più strade verso l'uscita. Zone buie estese, trappole letali. La struttura è labirintica ma non caotica — c'è sempre una via, ma trovarla richiede attenzione.

Mix completo di nemici tranne il Goblin. La prova di maturità prima del finale.

_Testi (ogni riga = trigger separato):_

> "Questo è l'inferno della gente per bene." _(nella zona centrale)_
> "Tutti morti, tutti in riga." _(stesso punto)_

---

### Livello 18 — "La prova definitiva" ⬜

`pacco8` (interno) | **Mix completo + Goblin** | Meccanica: tutto insieme

**Mappa:** Mappa ampia con spazio sufficiente per manovrare (10–12 stanze) ma senza margini di errore. La pressione è totale: il Goblin insegue mentre gli altri bloccano. La mappa deve avere abbastanza corridoi da permettere circuiti di fuga.

La combinazione più pericolosa. Nessun singolo percorso è sicuro.

_Testi (ogni riga = trigger separato):_

> "Sono tornato all'inizio." _(zona di ingresso, prima della mischia)_
> "Solo che ora conosco il nome di ogni mostro." _(stesso punto)_

---

### Livello 19 — "Il Ventre del Padre" ⬜

`pacco8` (interno) | **Mix completo** | Meccanica: massima pressione, architettura claustrofobica

**Mappa:** Mappa densa con corridoi stretti, 10–12 stanze molto compresse. Le zone buie sono estese. La sensazione di oppressione è architettonica — i corridoi larghi del livello 18 qui spariscono. Il punto più claustrofobico del gioco.

Narrativamente: Mark è nel cuore del labirinto, il punto più lontano dall'uscita.

_Testi (ogni riga = trigger separato):_

> "La chiave è pesantissima." _(sulla chiave)_
> "Sa di piombo." _(stesso punto)_
> "L'ultima porta." _(vicino all'uscita)_
> "Sono diventato quello che volevano." _(stesso punto — rassegnazione totale)_

---

### Livello 20 — "Il giardino vuoto / Il trono" ⬜

`pacco8` → `pacco1` | Mix finale | Il colpo di scena

**Mappa:** Due sezioni distinte. La prima (pacco8) è scura e densa come il livello 19. La seconda (pacco1) si apre sul giardino esterno al chiaro di luna — lo stesso ambiente del livello 1, ora in cenere. Contrasto netto: dai corridoi claustrofobici all'aperto desolato. Il passaggio tra i due pack è fisicamente attraversabile. 2 trigger posizionati nell'area esterna mentre il player avanza verso l'uscita finale.

_Testi (ogni riga = trigger separato):_

> "Il cielo è grigio." _(appena fuori)_
> "Il villaggio è in cenere." _(stesso punto)_
> "Ho passato l'inferno per un mondo che non esiste più." _(al centro dell'area — colpo finale)_
