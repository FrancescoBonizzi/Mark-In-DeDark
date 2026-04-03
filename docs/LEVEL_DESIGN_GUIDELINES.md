# Linee Guida di Design — Mark In DeDark

Guida per la creazione di livelli coerenti con il tono e la progressione del gioco.
Per la struttura tecnica del file `.mark` vedi [`LEVEL_FORMAT.md`](./LEVEL_FORMAT.md).

---

## Indice

1. [I nemici — descrizione e placement](#1-i-nemici--descrizione-e-placement)
2. [La storia di Mark — contesto narrativo](#2-la-storia-di-mark--contesto-narrativo)
3. [Convenzioni di design](#3-convenzioni-di-design)
4. [Progressione della difficoltà tra livelli](#4-progressione-della-difficoltà-tra-livelli)
5. [Istruzioni per la generazione di livelli](#5-istruzioni-per-la-generazione-di-livelli)

---

## 1. I nemici — descrizione e placement

Per la documentazione completa dell'AI (stati, transizioni, parametri di velocità) vedi [`ENEMY_AI.md`](./ENEMY_AI.md).

### Zombie (ObjectType=5)

- **Personalità:** patrugliante lento e meccanico — va avanti e indietro sul suo percorso senza deviare
- **Pericolo:** basso se hai candele, letale se lo incontri nel buio o senza via di fuga
- **Spegne candele:** sì, al contatto
- **Vede il giocatore:** no (patrol puro, non reagisce)
- **Luce propria:** sì — aura rossa/gialla intorno a lui
- **Placement consigliato:** corridoi stretti, zone obbligate di passaggio, a guardia di chiavi/uscite

### Scheletro (ObjectType=6)

- **Personalità:** patrugliante intelligente con visuale lineare — se ti vede in linea retta (senza muri in mezzo) parte all'inseguimento
- **Pericolo:** medio — può sorprenderti da lontano, ma si perde facilmente girando un angolo
- **Spegne candele:** sì, se sono nel suo raggio di ricerca
- **Vede il giocatore:** sì, solo su linea orizzontale o verticale senza ostacoli
- **Luce propria:** sì — bianca in pattuglia, rossa in inseguimento
- **Placement consigliato:** stanze aperte, corridoi lunghi dove il LoS è efficace

### Goblin (ObjectType=7)

- **Personalità:** cacciatore instancabile — conosce sempre la posizione del giocatore e lo insegue
- **Pericolo:** alto e crescente — diventa più veloce ogni volta che il giocatore usa una candela
- **Spegne candele:** sì, le cerca attivamente
- **Vede il giocatore:** sempre (pathfinding globale)
- **Luce propria:** sì — aura viola
- **Placement consigliato:** livelli avanzati; mettere lontano dallo spawn iniziale per dare al giocatore tempo di orientarsi. Un solo Goblin è già una sfida enorme.

### Taurus (ObjectType=8)

- **Personalità:** guardiano lento che carica in linea retta — patruglia lentamente, ma se ti vede in linea retta parte in carica devastante
- **Pericolo:** alto se ti trova in un corridoio — difficile da evitare, la carica è molto veloce
- **Spegne candele:** no
- **Vede il giocatore:** sì, solo su linea orizzontale o verticale (come lo Scheletro)
- **Luce propria:** sì — aura arancione intensa
- **Placement consigliato:** a guardia di stanze aperte o corridoi larghi; il giocatore deve imparare a non attraversare la sua visuale

### Ghost (ObjectType=10)

- **Personalità:** fantasma — attraversa i muri, attacca solo se il giocatore si muove
- **Pericolo:** psicologico — se ti fermi sei al sicuro, ma fermarti in un dungeon pieno di altri nemici è difficile
- **Spegne candele:** sì, molto velocemente (250 ms)
- **Attraversa i muri:** sì — i muri non lo fermano
- **Luce propria:** sì — aura verde/rosa
- **Placement consigliato:** in stanze chiuse dove il giocatore deve muoversi, o a combinazione con altri nemici che lo forzano a muoversi

### Bubu (ObjectType=11)

- **Personalità:** patrugliante silenzioso il cui obiettivo primario è spegnere le candele del giocatore
- **Pericolo:** indiretto — non uccide direttamente ma toglie risorse vitali
- **Spegne candele:** sì, è il suo comportamento principale
- **Vede il giocatore:** reagisce alle candele, non al giocatore
- **Luce propria:** sì — aura blu
- **Placement consigliato:** in combinazione con altri nemici; usare per creare scarsità di risorse durante il livello

---

## 2. La storia di Mark — contesto narrativo

Ogni livello che crei deve essere coerente con la storia e il tono del gioco. Questo non è un dungeon fantasy qualsiasi — è il viaggio di Mark.

### Chi è Mark

Mark è un ragazzo adolescente che si è perso in un labirinto oscuro. Non sa come ci è finito, non sa quanto è grande, non sa cosa lo aspetta al prossimo angolo. Ha con sé solo una candela accesa. Ogni livello completato non è una vittoria definitiva: è solo un'altra zona del labirinto che ha attraversato. L'uscita di ogni livello porta a una nuova zona, sempre più pericolosa.

### La meccanica come metafora

- **La candela** non è solo un oggetto di gioco: è la speranza di Mark, la sua unica sicurezza in un posto oscuro. Spegnerla (o farla spegnere dai nemici) non è solo un malus meccanico — è Mark che si ritrova solo nel buio totale.
- **I nemici** non sono mostri generici: sono le presenze che popolano questo labirinto. Alcuni (Zombie) sembrano vagare senza scopo. Altri (Goblin, Ghost) sembrano accanirsi specificamente su Mark e sulla sua luce.
- **La chiave e l'uscita** sono la struttura di ogni livello: c'è sempre una via d'uscita, ma Mark deve trovarla — e i pericoli sono lì per renderlo difficile.

### Il tono dei livelli

Il gioco **non è un labirinto in senso stretto** (infiniti corridoi uguali). Può essere:
- Un dungeon sotterraneo con stanze distinte
- Un castello in rovina con grandi sale
- Un corridoio lungo e lineare con ostacoli
- Una serie di stanze interconnesse da passaggi

L'importante è che ogni livello abbia una **progressione spaziale chiara**: da dove Mark arriva (ingresso) a dove vuole andare (uscita), con ostacoli in mezzo. Il giocatore deve potersi orientare, anche se il percorso non è ovvio.

### Coerenza tra livelli consecutivi

- I livelli devono avere una **continuità narrativa implicita**: Mark sta andando sempre "più in fondo" al labirinto. I livelli successivi devono sembrare zone più oscure, più pericolose, più distanti dall'uscita.
- Il **texture pack** scelto dovrebbe riflettere questo: pacco1 (all'aperto? rovine esterne) → pacchi successivi (più cupi, più sotterranei).
- **Nota specifica su pacco1:** le sprite del texture pack 1 hanno un aspetto di ambiente esterno (rovine all'aperto, pietra chiara). È adatto per i primi livelli, dove Mark è ancora in una zona "di confine". I pacchi successivi diventano progressivamente più scuri e claustrofobici.

### Il tono dei testi narrativi

- **Non abusare dei testi** — ogni testo deve avere un impatto. 2–4 testi per livello è il range ideale.
- **Posizionare i tutorial prima dell'oggetto**, non sopra. Il giocatore deve vedere il testo mentre si avvicina, non mentre è già sopra la risorsa.
- **I testi di atmosfera** vanno su zone di transizione (entrate di stanze buie, corridoi prima dei nemici, vicino all'uscita).
- **Voce in prima persona** — Mark parla tra sé e sé. Il tono è quello di un adolescente spaventato ma determinato.

---

## 3. Convenzioni di design

### Struttura spaziale

- **Usa il Riempimento** per riempire le aree tra le stanze — crea lo spessore visivo dei muri
- **Stanze di varie dimensioni** — mix di stanze piccole (tension) e grandi (respiro) crea ritmo
- **Corridoi stretti** — ideali per Zombie e Taurus, dove il giocatore non può aggirarli facilmente
- **Stanze aperte** — ideali per Ghost e Scheletro, dove la visuale lineare è efficace

### Distribuzione delle risorse

- Distribuire le candele **sparsamente** — il gioco deve essere teso, non comodo
- La chiave dovrebbe richiedere di **esplorare** la mappa, non essere subito visibile
- L'uscita può essere **vicina allo spawn** ma difficile da raggiungere per via dei nemici

### Difficoltà progressiva

- **Livelli early (1–3):** pochi nemici (2–4 Zombie), candele abbondanti, layout semplice
- **Livelli medi (4–6):** mix di nemici, candele scarse, layout con più stanze
- **Livelli avanzati (7–8):** nemici pericolosi (Goblin, Ghost), candele rare, labirinto

### Illuminazione

- Usare `Lucina` (ID=12, Brazier) per marcare punti di riferimento importanti — uscita, stanze chiave
- Il `Buio` (GroundType=7) per creare zone di suspense dove il giocatore entra "alla cieca"
- Ogni nemico emette luce propria — il giocatore vede il nemico prima di essere attaccato

---

## 4. Progressione della difficoltà tra livelli

Quando crei un nuovo livello devi sempre sapere **quale livello lo precede e quale lo segue**. La difficoltà deve salire gradualmente — non fare salti bruschi.

### Schema di progressione consigliato (8 livelli)

| Livello | Atmosfera | Nemici consigliati | Candele | Complessità mappa | Testi |
|---|---|---|---|---|---|
| 1 | Ingresso — rovine esterne (pacco1) | Nessun nemico o 1–2 Zombie fermi | Abbondanti (3–4 pacchi) | Lineare, 1–2 stanze | Tutorial completo: candela, porta, movimento |
| 2 | Primo dungeon | 2–3 Zombie con waypoint | Sufficienti | 2–3 stanze con corridoio | Testo atmosfera |
| 3 | Dungeon con sorprese | 3–4 Zombie + 1 Scheletro | Moderate | 3–4 stanze, un vicolo cieco | Tutorial porta, testo mistero |
| 4 | Zona più oscura | 4–5 Zombie + 1–2 Scheletri | Scarse | 4+ stanze, percorso non ovvio | Testo suggerimento/paura |
| 5 | Pericolo reale | 3 Zombie + 2 Scheletri + 1 Taurus | Scarse | Mappa ampia con zone aperte | Testo nemico insolito |
| 6 | Tensione alta | Mix Zombie/Scheletri/Bubu | Pochissime | Mappa densa, trappole | — |
| 7 | Incubo | Zombie + Scheletri + Ghost | Rarissime | Mappa grande, zone buie | — |
| 8 | Finale | Mix completo + Goblin | 1–2 pacchi nascosti | Massima complessità | Testo finale |

### Regole di progressione

1. **Non introdurre un nuovo tipo di nemico senza che il giocatore abbia già "imparato" il precedente.** Zombie prima degli Scheletri, Scheletri prima del Taurus, Ghost e Goblin solo tardi.
2. **Le candele devono diventare progressivamente più rare** — ma mai impossibili. Un livello senza soluzione non è design, è un bug.
3. **La complessità spaziale cresce** — i primi livelli possono essere quasi lineari. I livelli avanzati hanno rami, vicoli ciechi, stanze segrete.
4. **Usare le trappole** (`TrappolaMorteIstantanea`) con parsimonia nei livelli early, più frequentemente dopo il livello 5.
5. **Il Goblin è il nemico definitivo** — introdurlo troppo presto rende il gioco frustrante. Livello 7–8 al massimo.

### Se stai generando un singolo livello

- Leggi i livelli adiacenti (precedente e successivo) per capire il contesto
- Confronta la quantità di nemici e candele con il livello precedente — il tuo deve essere leggermente più difficile
- Se non ci sono livelli successivi definiti, pensa al livello come al "più difficile finora"

---

## 5. Istruzioni per la generazione di livelli

La regola fondamentale per qualsiasi livello generato è:

- **Gestire attentamente la progressione della difficoltà e la dimensione del gioco.** Un livello generato non vive da solo: fa parte di una sequenza. Deve essere coerente con ciò che lo precede e preparare il terreno per ciò che segue. Vedi [Sezione 4](#4-progressione-della-difficoltà-tra-livelli) per i dettagli.

### Checklist pre-consegna

Prima di considerare un livello completo, verifica:

- [ ] `npx tsx scripts/validate-level.ts <livello.mark>` non riporta errori
- [ ] Almeno 1 Giocatore, 1 Chiave, 1 Uscita, almeno 1 pacco di Candele
- [ ] Lo spawn del giocatore è distante dai nemici (≥ 3 celle Manhattan)
- [ ] I bordi della mappa sono chiusi (Riempimento o muri Occludente)
- [ ] La difficoltà è coerente con il livello che lo precede
- [ ] I waypoint nemici sono ben formati e ogni gruppo ha l'indice 1
