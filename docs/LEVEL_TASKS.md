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

`pacco1` | Nessun nemico | Meccanica: movimento, chiave, uscita, candele (raccogliere) | Candele: abbondanti

Layout quasi completamente illuminato da brazier. La chiave è visibile dall'inizio. Obiettivo: il giocatore impara i controlli senza pressione. Mappa lineare con 1–2 stanze aperte, ingresso con muro `Ingresso` (WallType=5).

_Testi:_

> "Mio padre mi ha stretto la mano troppo forte, come al solito. 'Torna uomo o non tornare.' La candela puzza di qualcosa che non voglio sapere cos'è." _(all'ingresso — zona sicura)_
> "Il nome di mio fratello è graffiato su questa porta. Sotto di lui, altri nomi. Ne riconosco qualcuno." _(vicino all'uscita — il silenzio fa il resto)_

---

### Livello 2 — "Il buio" ⬜

`pacco1` | Nessun nemico | Meccanica: piazzare candela (SPAZIO), zona buia (`Buio`) | Candele: abbondanti

Stesso ambiente esterno. Una stanza centrale ha una zona `Buio` obbligatoria. Il giocatore scopre che senza candela non vede. La soluzione è ovvia: piazza, illumina, attraversa. Tutorial esplicito.

_Testi:_

> "Qui dentro la candela non basta. Il buio mangia la luce prima che arrivi da qualche parte." _(entrando nella zona buia)_
> "Se la lascio a terra, illumino più lontano. Resto senza luce — ma almeno vedo dove mettere i piedi." _(tutorial piazzamento — zona sicura)_

---

### Livello 3 — "La prima creatura" ⬜

`pacco1/2` | **1 Zombie** | Meccanica: distrazione zombie con candela | Candele: abbondanti, vicino allo zombie

Il primo nemico deve essere un momento chiaro e didattico. Un singolo Zombie pattuglia un corridoio obbligato. Le candele sono posizionate appena prima di lui — il giocatore le trova naturalmente. Un testo suggerisce la meccanica senza svelarla del tutto.

_Testi:_

> "Ha i vestiti rituali. La cicatrice di zio Thomas sul braccio." _(vedendo lo zombie — vicino al nemico, 2 frasi brevissime)_
> "Segue la luce. Se lascio la candela lì, insegue quella." _(vicino alle candele — suggerisce la meccanica)_

---

### Livello 4 — "Gli automi" ⬜

`pacco2` | **2–3 Zombie** | Meccanica: distrazione multipla, sequenza | Candele: sufficienti

Più zombie che bloccano percorsi diversi. Il giocatore deve gestire le distrazioni in sequenza. Introduce la prima **porta** (CTRL) come elemento di separazione tra zone.

_Testi:_

> "Quanti nomi riconosco sui muri. Sono entrati tutti. Nessuno di loro ha trovato la strada fuori." _(nella stanza principale — vicino agli zombie)_
> "Una porta. Si apre con CTRL — l'unica cosa che obbedisce ancora." _(tutorial porta — zona di pausa)_

---

### Livello 5 — "I corridoi del rito" ⬜

`pacco2` | **2–3 Zombie** | Meccanica: prima **trappola temporizzata** | Candele: sufficienti

Zombie in combinazione con trappole temporizzate ovvie (visibili). Il giocatore impara che alcune trappole si possono attraversare di corsa. Mappa con più stanze, un paio di vicoli ciechi.

_Testi:_

> "I corridoi sono levigati. Quante scarpe ci sono passate sopra." _(nel corridoio principale — zona sicura)_
> "Quelle lame battono un ritmo. Se non mi fermo, passo." _(vicino alla trappola — breve, il giocatore deve concentrarsi)_

---

## Atto II — Le profondità (Livelli 6–10)

---

### Livello 6 — "I soldati di polvere" ⬜

`pacco3` | **1 Scheletro** (+ 1 Zombie opzionale) | Meccanica: visuale lineare (line-of-sight) | Candele: moderate

Stanza aperta dove la visuale è evidente. Il giocatore vede lo scheletro, vede la linea retta, capisce il pericolo prima di sbatterci. Primo incontro con un'architettura pensata per la visuale.

_Testi:_

> "Pattugliano stanze vuote. Proteggono il nulla." _(entrando nella stanza — player vede ma non è ancora in pericolo)_
> "Guardano dritto. Solo dritto. Un angolo cieco è già abbastanza." _(vicino allo scheletro — suggerisce la meccanica, max 2 frasi)_

---

### Livello 7 — "La sorveglianza" ⬜

`pacco3` | **2 Scheletri + 1–2 Zombie** | Meccanica: line-of-sight multipla, angoli ciechi | Candele: moderate

Più scheletri che coprono percorsi incrociati. Il giocatore deve imparare a muoversi negli angoli ciechi. Mappa a 4–5 stanze con architettura che valorizza il line-of-sight.

_Testi:_

> "Qui coprono tutti i corridoi. Quasi tutti." _(nel corridoio centrale — zona di transizione)_

---

### Livello 8 — "La rabbia cieca" ⬜

`pacco4` | **1 Taurus** | Meccanica: carica del Taurus, **muri rompibili** | Candele: moderate

Il Taurus deve essere presentato in modo spettacolare. Il percorso obbligato passa per un corridoio con un muro fragile. Il Taurus pattuglia dall'altra parte. Quando il giocatore entra nel campo visivo, la carica sfonda il muro — momento "wow" che insegna entrambe le meccaniche.

_Testi:_

> "Carica e sfonda i muri a testate. Tanta forza, così poca utilità." _(dopo il muro sfondato — player è ancora in zona pericolosa, breve)_

---

### Livello 9 — "I guardiani rotti" ⬜

`pacco4/5` | **1 Taurus + 2 Zombie** | Meccanica: muri fragili strategici, **trappola letale** | Candele: scarse

Il giocatore inizia a sfruttare i muri fragili a proprio vantaggio (fare in modo che il Taurus sfonda una parete utile). Prima trappola letale visibile, mai obbligatoria.

_Testi:_

> "Hanno costruito muri di carta. Già prima di lui si vedeva dove cedevano." _(vicino al muro fragile strategico)_

---

### Livello 10 — "Il senso di colpa" ⬜

`pacco5` | **1 Ghost** | Meccanica: fermarsi per sopravvivere | Candele: scarse

Stanza isolata con un Ghost. Il giocatore entra, il Ghost si avvicina. Se si muove viene attaccato. Deve stare fermo abbastanza a lungo da far passare il Ghost. Livello breve — la tensione è psicologica, non esplorativa.

_Testi:_

> "Se mi muovo, mi attacca. Fermo." _(entrando nella stanza del Ghost — 2 frasi cortissime, player deve concentrarsi)_
> "Ombre che un tempo erano ragazzi come me." _(dopo il Ghost, zona sicura — 1 frase, player respira)_

---

## Atto III — Il cuore (Livelli 11–16)

---

### Livello 11 — "Fermo nel caos" ⬜

`pacco5/6` | **1 Ghost + 2 Zombie** | Meccanica: Ghost in combinazione con pericoli che forzano il movimento | Candele: scarse

Il vero horror del Ghost: i zombie ti forzano a muoverti, il Ghost attacca se ti muovi. Il giocatore deve trovare finestre di immobilità tra i movimenti degli zombie.

_Testi:_

> "Fermo, arriva lo zombie. Mi muovo, arriva il fantasma. Non c'è risposta giusta." _(nella stanza centrale — vicino a entrambi i nemici, massima brevità)_

---

### Livello 12 — "Il sabotaggio" ⬜

`pacco6` | **1 Bubu + 2 Zombie** | Meccanica: Bubu spegne le candele | Candele: poche, gestione attenta

Il Bubu non uccide — ruba le candele. Il giocatore scopre che la risorsa su cui si è affidato può scomparire. Prima volta che la gestione delle candele è un problema strategico.

_Testi:_

> "Quello blu non mi vuole morto. Mi vuole al buio." _(vedendo il Bubu — vicino al nemico, brevissimo)_
> "Come se la mia luce fosse un'offesa." _(dopo che il Bubu spegne una candela — 1 frase, lascia il peso)_

---

### Livello 13 — "La privazione" ⬜

`pacco6/7` | **2 Bubu + 2 Scheletri** | Meccanica: scarsità critica di risorse | Candele: pochissime, nascoste

Le candele sono nascoste e poche. Il giocatore deve scegliere quando usarle. I Bubu pattugliano le zone dove le candele potrebbero essere piazzate.

_Testi:_

> "Un'ultima candela. Devo farla bastare." _(nel punto critico — vicino ai nemici, 2 frasi brevissime)_

---

### Livello 14 — "Il labirinto vero" ⬜

`pacco7` | **Zombie + Scheletri + Bubu** | Meccanica: mix completo senza Goblin | Candele: pochissime

Mappa complessa con più stanze, trappole, e tutti i nemici introdotti finora. La prima vera prova d'insieme. Nessun testo tutorial — solo atmosfera.

_Testi:_

> "Mio padre è davvero uscito di qua o l'ha solo raccontato? Se sapeva cosa c'era qua sotto, è un mostro. Se non lo sapeva, è un bugiardo." _(in una zona di pausa)_

---

### Livello 15 — "L'odio" ⬜

`pacco7/8` | **1 Goblin** (da solo) | Meccanica: inseguimento puro, nessuna distrazione possibile | Candele: scarse, ma usarle velocizza il Goblin

Un Goblin da solo in una mappa ampia. Il Goblin insegue sempre — non si può distrarre come lo zombie. Ogni candela usata lo velocizza. Il giocatore impara che deve trovare la chiave e uscire minimizzando l'uso di risorse.

_Testi:_

> "Non si distrae. Non si stanca. Ogni candela che accendo lo avvicina." _(vedendo il Goblin — 3 frasi corte, il player le scorre mentre comincia a correre)_

---

### Livello 16 — "Il cacciatore instancabile" ⬜

`pacco8` | **1 Goblin + 2–3 Zombie** | Meccanica: Goblin in combinazione | Candele: rarissime

Il Goblin non è più solo. I zombie bloccano le vie di fuga, il Goblin insegue. Il giocatore deve usare i zombie come ostacoli per rallentare il Goblin.

_Testi:_

> "L'unica mossa vincente era non giocare." _(vicino all'uscita — 1 frase, pianta il seme del finale)_

---

## Atto IV — Il finale (Livelli 17–20)

---

### Livello 17 — "Tutto insieme" ⬜

`pacco8` | **Zombie + Scheletri + Bubu + Taurus** (no Goblin) | Meccanica: orchestrazione completa | Candele: rarissime

Mix completo di nemici tranne il Goblin. Mappa grande con più strade, trappole letali, zone buie. La prova di maturità prima del finale.

_Testi:_

> "Questo è l'inferno della gente per bene. Tutti morti, tutti in riga." _(nella zona centrale — zona di transizione, 2 frasi)_

---

### Livello 18 — "La prova definitiva" ⬜

`pacco8` | **Mix completo + Goblin** | Meccanica: tutto insieme | Candele: 1–2 pacchi nascosti

La combinazione più pericolosa. Il Goblin insegue mentre gli altri bloccano le vie. Mappa ampia con abbastanza spazio per manovrare ma nessun margine di errore.

_Testi:_

> "Sono tornato all'inizio. Solo che ora conosco il nome di ogni mostro." _(vedendo il mix — zona di ingresso, prima della mischia)_

---

### Livello 19 — "Il Ventre del Padre" ⬜

`pacco8` | **Mix completo** | Meccanica: massima pressione, architettura claustrofobica | Candele: 1 pacco visibile, altri nascosti

Il punto più buio e claustrofobico del gioco. Mappa densa, corridoi stretti, zone buie estese. Narrativamente: Mark è nel cuore del labirinto, il punto più lontano dall'uscita.

_Testi:_

> "La chiave è pesantissima. Sa di piombo." _(sulla chiave — player si ferma a raccoglierla)_
> "L'ultima porta. Sono diventato quello che volevano." _(vicino all'uscita — 2 frasi, rassegnazione totale)_

---

### Livello 20 — "Il giardino vuoto / Il trono" ⬜

`pacco8` → `pacco1` | Mix finale | Il colpo di scena | Candele: scarse

La mappa inizia scura (pacco8) e si schiarisce verso l'uscita, dove la texture diventa pacco1 — le stesse rovine esterne del livello 1. L'uscita si apre su un'area pacco1 vuota.

Il player cammina attraverso l'area esterna vuota. 2 trigger separati mentre avanza.

_Testi:_

> "Il cielo è grigio. Il villaggio è in cenere." _(appena fuori — il player vede le rovine)_
> "Ho passato l'inferno per un mondo che non esiste più." _(al centro dell'area — 1 frase, colpo finale)_
