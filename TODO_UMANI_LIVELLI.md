# LEVEL_TASKS.md — Tracker livelli Mark In DeDark

Canovaccio per la progettazione e implementazione dei 20 livelli.
Per le linee guida di design, nemici e meccaniche: [`LEVEL_DESIGN_GUIDELINES.md`](./LEVEL_DESIGN_GUIDELINES.md).

### Livello 5 — "I corridoi del rito" ⬜

`pacco2` (interno) | **2 Zombie** | Meccanica: prima trappola temporizzata

**Bounding box:** 1315 righe × 3038 colonne
**Candele:** 4

Percorso principale lineare con 12 diramazioni brevi. La trappola temporizzata è sul percorso principale — visivamente ovvia.

_Testi (ogni riga = trigger separato):_

> "Quante scarpe ci sono passate sopra." _(nel corridoio principale)_
> "Quelle lame battono un ritmo." _(vicino alla trappola)_
> "Se non mi fermo, passo." _(stesso punto)_

### Livello 6 — "I soldati di polvere" ⬜

`pacco3` (interno) | **1 Scheletro** | Meccanica: visuale lineare (line-of-sight)

**Bounding box:** 1316 righe × 3038 colonne
**Candele:** 4

Stanza aperta centrale per rendere evidente la visuale dello scheletro. Corridoi laterali come percorso alternativo.

_Testi (ogni riga = trigger separato):_

> "Pattugliano stanze vuote." _(entrando nella stanza)_
> "Guardano dritto." _(vicino allo scheletro)_
> "Solo dritto." _(passo successivo)_

### Livello 7 — "La sorveglianza" ⬜

`pacco3` (interno) | **2 Scheletri + 1 Zombie** | Meccanica: line-of-sight multipla, angoli ciechi

**Bounding box:** 1417 righe × 3240 colonne
**Candele:** 4

56 stanze con corridoi incrociati. Angoli ciechi che il giocatore può sfruttare.

_Testi (ogni riga = trigger separato):_

> "Qui coprono tutti i corridoi." _(nel corridoio centrale)_
> "Quasi tutti." _(stesso punto)_

### Livello 8 — "La rabbia cieca" ⬜

`pacco4` (interno) | **1 Taurus** | Meccanica: carica del Taurus, muri rompibili

**Bounding box:** 1316 righe × 2836 colonne
**Candele:** 3

Corridoio principale con muro fragile visivamente distinto. Il Taurus pattuglia dall'altra parte — la carica e lo sfondamento sono il momento didattico centrale.

_Testi (ogni riga = trigger separato):_

> "Qualcosa respira dall'altra parte del muro." _(prima del muro fragile)_
> "Non sa fermarsi." _(dopo il muro sfondato)_
> "Tanta forza, così poca utilità." _(zona sicura dopo il muro)_

### Livello 9 — "I guardiani rotti" ⬜

`pacco5` (interno) | **1 Taurus + 2 Zombie** | Meccanica: muri fragili strategici

**Bounding box:** 1417 righe × 3038 colonne
**Candele:** 3

56 stanze. Muri fragili da usare strategicamente. La prima `TrappolaMorteIstantanea` compare in una diramazione opzionale — solo visiva, non insegnata.

_Testi (ogni riga = trigger separato):_

> "Hanno costruito muri di carta." _(vicino al muro fragile strategico)_
> "Già prima di lui si vedeva dove cedevano." _(stesso punto)_

### Livello 10 — "Il senso di colpa" ⬜

`pacco5` (interno) | **1 Ghost** | Meccanica: fermarsi per sopravvivere

**Bounding box:** 1315 righe × 2835 colonne
**Candele:** 3

Arena centrale ampia con corridoi di accesso stretti da 1 cella. Livello volutamente semplice — la tensione è psicologica.

_Testi (ogni riga = trigger separato):_

> "Immobile. Devo restare immobile." _(corridoio obbligato prima della stanza del Ghost)_
> "Non guardarlo." _(entrando nella stanza)_
> "Ombre che un tempo erano ragazzi come me." _(dopo il Ghost, zona sicura)_

### Livello 11 — "Fermo nel caos" ⬜

`pacco6` (interno) | **1 Ghost + 2 Zombie** | Meccanica: Ghost in combinazione con pericoli che forzano il movimento

**Bounding box:** 1518 righe × 3340 colonne
**Candele:** 4

56 stanze. Gli zombie forzano il movimento, il Ghost attacca se ci si muove. La geometria deve offrire sempre almeno una finestra di immobilità.

_Testi (ogni riga = trigger separato):_

> "Non c'è risposta giusta." _(nella stanza centrale)_

### Livello 12 — "Il sabotaggio" ⬜

`pacco6` (interno) | **1 Bubu + 2 Zombie** | Meccanica: Bubu spegne le candele

**Bounding box:** 1518 righe × 3441 colonne
**Candele:** 6

67 stanze. Candele sparse in posizioni che il Bubu pattuglia.

_Testi (ogni riga = trigger separato):_

> "Quello blu non mi vuole morto." _(vedendo il Bubu)_
> "Mi vuole al buio." _(stesso punto)_
> "Come se la mia luce fosse un'offesa." _(dopo che il Bubu spegne una candela)_

### Livello 13 — "La privazione" ⬜

`pacco6` (interno) | **2 Bubu + 2 Scheletri** | Meccanica: scarsità critica di risorse

**Bounding box:** 1518 righe × 3441 colonne
**Candele:** 2

67 stanze con zone buie sparse. Le ultime candele sono in posizioni scomode.

_Testi (ogni riga = trigger separato):_

> "Un'ultima candela." _(sull'ultima candela disponibile)_
> "Devo farla bastare." _(passo successivo)_

### Livello 14 — "Il labirinto vero" ⬜

`pacco7` (interno) | **2 Zombie + 2 Scheletri + 1 Bubu** | Meccanica: mix completo senza Goblin

**Bounding box:** 1620 righe × 3542 colonne
**Candele:** 4

810 stanze. Prima vera prova d'insieme. Nessun testo tutorial.

_Testi (ogni riga = trigger separato):_

> "Mio padre è davvero uscito di qua, o l'ha solo raccontato?" _(in una zona di pausa)_
> "Se sapeva cosa c'era qua sotto, è un mostro." _(stesso punto)_
> "Se non lo sapeva, è un bugiardo." _(stesso punto)_

### Livello 15 — "L'odio" ⬜

`pacco7` (interno) | **1 Goblin** (da solo) | Meccanica: inseguimento puro

**Bounding box:** 1620 righe × 3542 colonne
**Candele:** 2

Mappa ampia con corridoi larghi. Il Goblin è distante dallo spawn: il player ha qualche secondo di vantaggio. Ogni candela piazzata lo accelera.

_Testi (ogni riga = trigger separato):_

> "Questo mi odia." _(vedendo il Goblin)_
> "È la fiamma che lo nutre." _(qualche stanza dopo)_

### Livello 16 — "Il cacciatore instancabile" ⬜

`pacco8` (interno) | **1 Goblin + 2 Zombie** | Meccanica: Goblin in combinazione

**Bounding box:** 1620 righe × 3542 colonne
**Candele:** 2

89 stanze con corridoi più stretti del livello 15. Gli zombie occupano le vie di fuga.

_Testi (ogni riga = trigger separato):_

> "L'unica mossa vincente era non giocare." _(vicino all'uscita)_

## Atto IV — Il finale (Livelli 1720)

### Livello 17 — "Tutto insieme" ⬜

`pacco8` (interno) | **2 Zombie + 2 Scheletri + 1 Bubu + 1 Taurus** | Meccanica: orchestrazione completa

**Bounding box:** 1821 righe × 3843 colonne
**Candele:** 4

1012 stanze. Mix completo senza Goblin — prova di maturità prima del finale.

_Testi (ogni riga = trigger separato):_

> "Questo è l'inferno della gente per bene." _(nella zona centrale)_
> "Tutti morti, tutti in riga." _(stesso punto)_

### Livello 18 — "La prova definitiva" ⬜

`pacco8` (interno) | **2 Zombie + 2 Scheletri + 1 Bubu + 1 Taurus + 1 Goblin** | Meccanica: tutto insieme

**Bounding box:** 1822 righe × 3944 colonne
**Candele:** 3

1012 stanze con percorso ad anello — il player può sempre fuggire in avanti.

_Testi (ogni riga = trigger separato):_

> "Sono tornato all'inizio." _(zona di ingresso, prima della mischia)_
> "Solo che ora conosco il nome di ogni mostro." _(stesso punto)_

### Livello 19 — "Il Ventre del Padre" ⬜

`pacco8` (interno) | **Mix completo** | Meccanica: massima pressione, architettura claustrofobica

**Bounding box:** 1922 righe × 4044 colonne
**Candele:** 2

1012 stanze compresse, corridoi prevalentemente da 1 cella. Zone buie estese.

_Testi (ogni riga = trigger separato):_

> "La chiave è pesantissima." _(sulla chiave)_
> "Sa di piombo." _(passo successivo)_
> "Sono diventato quello che volevano." _(vicino all'uscita)_

### Livello 20 — "Il giardino vuoto" ⬜

`pacco8` | **Mix finale** | Finale narrativo

**Bounding box:** 2022 righe × 4044 colonne
**Candele:** 2

Sezione densa (come livello 19) → passaggio stretto → area aperta finale che evoca il giardino del livello 1, devastato.

> **Nota:** L'area finale usa `pacco8` — il contrasto con il livello 1 è narrativo, non visivo.

_Testi (ogni riga = trigger separato):_

> "Il cielo è grigio." _(appena fuori)_
> "Il villaggio è in cenere." _(stesso punto)_
> "Ho passato l'inferno per un mondo che non esiste più." _(al centro dell'area — colpo finale)_
