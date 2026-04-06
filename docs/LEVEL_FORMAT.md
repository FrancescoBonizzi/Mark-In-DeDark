# Formato Livelli `.mark` — Mark In DeDark

Struttura tecnica del file `.mark`. Per le linee guida di design, narrativa e convenzioni vedi [`LEVEL_DESIGN_GUIDELINES.md`](./LEVEL_DESIGN_GUIDELINES.md).

I file vanno generati in `Web/public/assets/levels`

---

## Indice

- [Formato Livelli `.mark` — Mark In DeDark](#formato-livelli-mark--mark-in-dedark)
  - [Indice](#indice)
  - [1. Struttura generale del file](#1-struttura-generale-del-file)
  - [2. Intestazione del livello](#2-intestazione-del-livello)
    - [Dimensioni (Riga 2)](#dimensioni-riga-2)
    - [Configurazione ambiente (Riga 3)](#configurazione-ambiente-riga-3)
  - [3. Formato di ogni cella](#3-formato-di-ogni-cella)
  - [4. Tipi di terreno (GroundType)](#4-tipi-di-terreno-groundtype)
    - [Cella non giocabile](#cella-non-giocabile)
    - [Pavimenti calpestabili (Terra)](#pavimenti-calpestabili-terra)
    - [Pavimenti speciali](#pavimenti-speciali)
    - [Riempimento (sfondo non calpestabile)](#riempimento-sfondo-non-calpestabile)
  - [5. Tipi di oggetto (ObjectType)](#5-tipi-di-oggetto-objecttype)
    - [Oggetti essenziali](#oggetti-essenziali)
    - [Oggetti raccoglibili](#oggetti-raccoglibili)
    - [Nemici](#nemici)
    - [Oggetti ambientali](#oggetti-ambientali)
  - [6. Dati aggiuntivi dell'oggetto](#6-dati-aggiuntivi-delloggetto)
    - [Candele (ObjectType=4)](#candele-objecttype4)
    - [Nemici con waypoint (ObjectType=5,6,7,8,10,11)](#nemici-con-waypoint-objecttype56781011)
    - [Tutti gli altri oggetti](#tutti-gli-altri-oggetti)
  - [7. Tipi di muro (WallType)](#7-tipi-di-muro-walltype)
    - [Tabella dei tipi](#tabella-dei-tipi)
    - [Come costruire le stanze](#come-costruire-le-stanze)
    - [Porta / Ingresso / Uscita](#porta--ingresso--uscita)
  - [8. Texture pack (envTexturePackId)](#8-texture-pack-envtexturepackid)
  - [9. Testi e narrativa (stringCode)](#9-testi-e-narrativa-stringcode)
    - [Come funziona](#come-funziona)
    - [Codici disponibili](#codici-disponibili)
    - [Come usare i testi in un livello](#come-usare-i-testi-in-un-livello)
  - [10. Waypoint dei nemici](#10-waypoint-dei-nemici)
    - [Formato](#formato)
    - [Regole](#regole)
    - [Esempio da livello 7](#esempio-da-livello-7)
  - [11. Regole di validazione](#11-regole-di-validazione)
    - [Obbligatorio](#obbligatorio)
    - [Fortemente consigliato](#fortemente-consigliato)
    - [Coerenza dei waypoint](#coerenza-dei-waypoint)
    - [Confini mappa](#confini-mappa)
  - [12. Esempi di righe commentate](#12-esempi-di-righe-commentate)
  - [13. Template livello minimo](#13-template-livello-minimo)

---

## 1. Struttura generale del file

Un file `.mark` è testo UTF-8 con separatore `|` (pipe). La struttura è sempre:

```
GameGridSerializerV4
RIGHE|COLONNE
ENV_TEXTURE_PACK_ID|DIRTY_PROBABILITY_PCT
ROW|COL|GROUND|OBJECT|OBJ_DATA|TOP|RIGHT|BOTTOM|LEFT|STRING_CODE
ROW|COL|GROUND|OBJECT|OBJ_DATA|TOP|RIGHT|BOTTOM|LEFT|STRING_CODE
...
```

- **Riga 1** — stringa fissa `GameGridSerializerV4` (versione serializzatore)
- **Riga 2** — dimensioni mappa (sempre `25|45`)
- **Riga 3** — configurazione ambiente
- **Righe 4+** — una riga per ogni cella non vuota della griglia

> **Nota importante:** nel file le celle sono ordinate per `ROW, COL`. Ma nel codice la griglia è indicizzata `grid[col][row]`. Il parser inverte automaticamente. Non devi preoccupartene: scrivi sempre `row|col` nel file.

> **Solo le celle non-nulla** devono essere scritte. Le celle non elencate sono trattate come null (non esistenti / bordo mappa). Le celle che esistono ma non hanno oggetti o muri possono comunque essere omesse se il terreno è `NonCella` (0).

---

## 2. Intestazione del livello

### Dimensioni (Riga 2)

```
25|45
```

Sempre fisso: **25 righe × 45 colonne**. Non modificabile per compatibilità con il serializzatore.

- Righe: indici 0–24
- Colonne: indici 0–44

### Configurazione ambiente (Riga 3)

```
ENV_TEXTURE_PACK_ID|DIRTY_PROBABILITY_PCT
```

| Campo                   | Tipo        | Descrizione                                                                                                                        |
| ----------------------- | ----------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `ENV_TEXTURE_PACK_ID`   | int (1–8)   | Quale set di texture usare per pavimenti e muri (vedi [Sezione 8](#8-texture-pack-envtexturepackid))                               |
| `DIRTY_PROBABILITY_PCT` | int (0–100) | Percentuale di celle di pavimento a cui viene applicata una texture "sporca" (macchie, ruggine). `0` = pulito, `10` = molto sporco |

Esempio:

```
3|0     → texture pack 3, pavimento pulito
6|10    → texture pack 6, 10% di celle sporche
```

---

## 3. Formato di ogni cella

```
ROW|COL|GROUND|OBJECT|OBJ_DATA|TOP|RIGHT|BOTTOM|LEFT|STRING_CODE
```

| Campo         | Posizione | Tipo            | Obbligatorio | Descrizione                                                                     |
| ------------- | --------- | --------------- | ------------ | ------------------------------------------------------------------------------- |
| `ROW`         | 0         | int 0–24        | Sì           | Riga della cella                                                                |
| `COL`         | 1         | int 0–44        | Sì           | Colonna della cella                                                             |
| `GROUND`      | 2         | int             | Sì           | Tipo di terreno (vedi [Sezione 4](#4-tipi-di-terreno-groundtype))               |
| `OBJECT`      | 3         | int \| vuoto    | No           | Tipo di oggetto nella cella (vedi [Sezione 5](#5-tipi-di-oggetto-objecttype))   |
| `OBJ_DATA`    | 4         | string \| vuoto | No           | Dati aggiuntivi dell'oggetto (vedi [Sezione 6](#6-dati-aggiuntivi-delloggetto)) |
| `TOP`         | 5         | int             | Sì           | Muro superiore (vedi [Sezione 7](#7-tipi-di-muro-walltype))                     |
| `RIGHT`       | 6         | int             | Sì           | Muro destro                                                                     |
| `BOTTOM`      | 7         | int             | Sì           | Muro inferiore                                                                  |
| `LEFT`        | 8         | int             | Sì           | Muro sinistro                                                                   |
| `STRING_CODE` | 9         | string \| vuoto | No           | Codice testo narrativo (vedi [Sezione 9](#9-testi-e-narrativa-stringcode))      |

I campi vuoti si scrivono semplicemente lasciando due pipe adiacenti `||`.

**Esempio completo:**

```
12|18|1|5|Z-3-1|0|0|0|0|
```

→ Riga 12, Colonna 18, Pavimento Terra1, Nemico Zombie, waypoint "Z-3-1", nessun muro

---

## 4. Tipi di terreno (GroundType)

Il terreno definisce la texture del pavimento e il comportamento della cella. Ogni cella ha **uno** di questi valori.

### Cella non giocabile

| ID  | Nome       | Descrizione visiva                                                                                                |
| --- | ---------- | ----------------------------------------------------------------------------------------------------------------- |
| `0` | `NonCella` | Cella vuota — non renderizzata, non calpestabile. Usare per tutti i bordi e le aree esterne alla mappa giocabile. |

### Pavimenti calpestabili (Terra)

Tutti i tipi Terra sono pavimenti percorribili dal giocatore e dai nemici. La differenza è **solo visiva** (ogni tipo usa una texture diversa del texture pack scelto):

| ID   | Nome      | Texture usata | Note                                       |
| ---- | --------- | ------------- | ------------------------------------------ |
| `1`  | `Terra1`  | `cell1`       | Pavimento principale, il più comune        |
| `6`  | `Terra2`  | `cell2`       | Variante — usare per alternare visivamente |
| `9`  | `Terra3`  | `cell3`       | Variante                                   |
| `10` | `Terra4`  | `cell4`       | Disponibile in tutti i texture pack        |
| `11` | `Terra5`  | `cell5`       | Disponibile in tutti i texture pack        |
| `12` | `Terra6`  | `cell6`       | Dipende dal texture pack                   |
| `13` | `Terra7`  | `cell7`       | Dipende dal texture pack                   |
| `14` | `Terra8`  | `cell8`       | Dipende dal texture pack                   |
| `15` | `Terra9`  | `cell9`       | Dipende dal texture pack                   |
| `16` | `Terra10` | `cell10`      | Dipende dal texture pack                   |

> **Consiglio:** usa `Terra1` (1) come pavimento principale e le varianti per creare pattern visivi, bordi stanza, o highlight di zone importanti.

### Pavimenti speciali

| ID  | Nome                      | Descrizione              | Comportamento                                                                                                                        |
| --- | ------------------------- | ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------ |
| `7` | `Buio`                    | Cella completamente buia | Calpestabile, ma non illuminata. Il giocatore la vede solo con una candela accesa nelle vicinanze. Usa per stanze "nel buio totale". |
| `3` | `TrappolaMorteIstantanea` | Fossa / trappola letale  | Il giocatore muore istantaneamente camminandoci sopra. Texture: pozzo o lama.                                                        |
| `4` | `TrappolaTemporizzata`    | Trappola con timer       | Si attiva dopo un ritardo. Può essere attraversata di corsa.                                                                         |
| `5` | `TriggerGoblin`           | Trigger nascosto         | Calpestando questa cella si può scatenare un evento (attualmente legacy, non usato nei livelli esistenti).                           |

### Riempimento (sfondo non calpestabile)

Questi tipi producono texture di sfondo decorative — **non calpestabili**, usate per riempire le aree non giocabili attorno alle stanze, creare pareti spesse, bordi visivi.

| ID   | Nome            | Texture usata    |
| ---- | --------------- | ---------------- |
| `8`  | `Riempimento1`  | `riempimento_1`  |
| `17` | `Riempimento2`  | `riempimento_2`  |
| `18` | `Riempimento3`  | `riempimento_3`  |
| `19` | `Riempimento4`  | `riempimento_4`  |
| `20` | `Riempimento5`  | `riempimento_5`  |
| `21` | `Riempimento6`  | `riempimento_6`  |
| `22` | `Riempimento7`  | `riempimento_7`  |
| `23` | `Riempimento8`  | `riempimento_8`  |
| `24` | `Riempimento9`  | `riempimento_9`  |
| `25` | `Riempimento10` | `riempimento_10` |
| `26` | `Riempimento11` | `riempimento_11` |
| `27` | `Riempimento12` | `riempimento_12` |
| `28` | `Riempimento13` | `riempimento_13` |

> **Consiglio:** `Riempimento1` (8) è la scelta standard per lo sfondo. Le varianti cambiano solo la texture del rock/mattone.

---

## 5. Tipi di oggetto (ObjectType)

Una cella può contenere **zero o un oggetto**. Il campo OBJECT si lascia vuoto (`||`) per nessun oggetto.

### Oggetti essenziali

| ID  | Nome        | Descrizione                              | Regole                                                                                                 |
| --- | ----------- | ---------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| `1` | `Giocatore` | **Spawn point del giocatore**            | Obbligatorio — esattamente 1 per livello. Mettere su una cella di pavimento libera, lontana da nemici. |
| `2` | `Chiave`    | **Chiave** — raccoglibile dal giocatore  | Obbligatorio: 1 per livello. Senza chiave l'uscita non si apre.                                        |
| `3` | `Uscita`    | **Porta di uscita** — richiede la chiave | Obbligatorio — almeno 1 per livello. Il giocatore deve raccogliere la chiave per sbloccarla.           |

### Oggetti raccoglibili

| ID  | Nome      | Descrizione                                     | Dati aggiuntivi                                                                                      |
| --- | --------- | ----------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `4` | `Candele` | **Pacco di candele** — risorsa vitale del gioco | Numero di candele nel pacco (es: `3`). Default: 1. Vedi [Sezione 6](#6-dati-aggiuntivi-delloggetto). |

> Le candele sono la meccanica centrale del gioco: i nemici vengono attratti dalle candele e vanno a cercare di spegnerle, in quel momento il giocatore ha la possibilità di superarli senza farsi uccidere. Distribuirle strategicamente crea tensione.

### Nemici

| ID   | Nome        | Descrizione breve                                            | AI                         |
| ---- | ----------- | ------------------------------------------------------------ | -------------------------- |
| `5`  | `Zombie`    | Nemico lento che patruglia                                   | PatrolAI                   |
| `6`  | `Scheletro` | Cacciatore con visuale lineare                               | PatrolHunterLineOfSightAI  |
| `7`  | `Goblin`    | Inseguitore veloce che si accelera                           | PlayerHunterAI             |
| `8`  | `Taurus`    | Colosso che carica in linea retta (e rompe i muri rompibili) | PatrolChargeAI             |
| `10` | `Ghost`     | Fantasma — attraversa i muri                                 | PatrolGhostAI              |
| `11` | `Bubu`      | Patrugliante che spegne le candele                           | PatrolAI (searchesCandles) |

Per la descrizione completa dei comportamenti AI vedi [`ENEMY_AI.md`](./ENEMY_AI.md). Per le linee guida di placement vedi [`LEVEL_DESIGN_GUIDELINES.md`](./LEVEL_DESIGN_GUIDELINES.md).

### Oggetti ambientali

| ID   | Nome                   | Descrizione                                                                                                                    | Passabile                    |
| ---- | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------ | ---------------------------- |
| `12` | `Lucina` _(Brazier)_   | **Torcia / braciere ambientale** — sorgente di luce statica con fiamma animata. Non è un nemico nonostante il nome nel codice. | No                           |
| `13` | `OggettoNonPassabile1` | Ostacolo decorativo solido (es: statua, botte, colonna)                                                                        | No — blocca il passaggio     |
| `14` | `OggettoNonPassabile2` | Ostacolo decorativo solido alternativo                                                                                         | No — blocca il passaggio     |
| `15` | `OggettoPassabile1`    | Elemento decorativo attraversabile (es: tappeto, erba)                                                                         | Sì — non blocca il passaggio |

> `Lucina` (ID 12) è il modo per piazzare torce ambientali che illuminano permanentemente una zona. Ideale per corridoi chiave, ingressi, stanze speciali.

---

## 6. Dati aggiuntivi dell'oggetto

Il campo `OBJ_DATA` è una stringa opzionale che varia per tipo di oggetto:

### Candele (ObjectType=4)

```
NUMERO_CANDELE
```

Intero che specifica quante candele contiene il pacco. Se vuoto, default = 1.

```
5|10|1|4|3|0|0|0|0|    → Pacco di 3 candele alla posizione (5, 10)
5|11|1|4||0|0|0|0|     → Pacco di 1 candela (default)
```

### Nemici con waypoint (ObjectType=5,6,7,8,10,11)

```
ZONA-SEQUENZA_ID
```

Identifica il gruppo di patruglia e la posizione del nemico nel percorso. Vedi [Sezione 10](#10-waypoint-dei-nemici) per il dettaglio.

```
7|27|1|5|Z-1-2|0|0|0|0|   → Zombie, gruppo "Z-1", posizione 2 nel percorso
14|8|1|6|S-1-1|0|0|0|0|   → Scheletro, gruppo "S-1", posizione 1
```

### Tutti gli altri oggetti

Campo vuoto `||` — nessun dato aggiuntivo necessario.

---

## 7. Tipi di muro (WallType)

Ogni cella ha **4 muri** (top, right, bottom, left). Un muro appartiene alla cella che lo "possiede":

- **TOP** = muro sul bordo superiore della cella
- **RIGHT** = muro sul bordo destro
- **BOTTOM** = muro sul bordo inferiore
- **LEFT** = muro sul bordo sinistro

> **Regola importante:** i muri definiscono i bordi delle stanze. Un muro LEFT su cella (5,3) e un muro RIGHT su cella (5,2) definiscono lo stesso confine fisico visto da due lati opposti — ma sono gestiti separatamente. Per creare un muro solido tra due celle adiacenti, di solito si usa il muro su **una sola** delle due celle.

### Tabella dei tipi

| ID   | Nome         | Descrizione                                                           | Blocca passaggio   | Visivo       |
| ---- | ------------ | --------------------------------------------------------------------- | ------------------ | ------------ |
| `0`  | `Niente`     | Nessun muro — bordo aperto                                            | No                 | —            |
| `1`  | `Occludente` | **Muro solido principale** — mattoni/pietra                           | Sì                 | Sì           |
| `2`  | `Fragile`    | Muro rompibile (abbattibile)                                          | Sì (finché intero) | Sì           |
| `3`  | `Porta`      | Porta apribile                                                        | Sì (se chiusa)     | Sì — animata |
| `4`  | `Muro1`      | Muro decorativo variante                                              | Sì                 | Sì           |
| `5`  | `Ingresso`   | **Ingresso principale livello** (muro con porta grande, lato entrata) | Sì                 | Sì           |
| `6`  | `Uscita`     | **Uscita principale livello** (muro con porta grande, lato uscita)    | Sì                 | Sì           |
| `7`  | `Decal1`     | Decorazione visiva su muro (macchia, muschio, crepa)                  | No                 | Sì           |
| `8`  | `Decal2`     | Decorazione visiva variante                                           | No                 | Sì           |
| `9`  | `Decal3`     | Decorazione visiva variante                                           | No                 | Sì           |
| `10` | `Decal4`     | Decorazione visiva variante                                           | No                 | Sì           |
| `11` | `Decal5`     | Decorazione visiva variante                                           | No                 | Sì           |
| `12` | `Muro2`      | Muro decorativo variante 2                                            | Sì                 | Sì           |
| `13` | `Muro3`      | Muro decorativo variante 3                                            | Sì                 | Sì           |
| `14` | `Muro4`      | Muro decorativo variante 4                                            | Sì                 | Sì           |
| `15` | `Muro5`      | Muro decorativo variante 5                                            | Sì                 | Sì           |
| `16` | `Decal6`     | Decorazione visiva variante                                           | No                 | Sì           |
| `17` | `Decal7`     | Decorazione visiva variante                                           | No                 | Sì           |
| `18` | `Decal8`     | Decorazione visiva variante                                           | No                 | Sì           |

### Come costruire le stanze

Per creare una stanza rettangolare con muri `Occludente` (1):

- Il bordo **superiore** della stanza: cella con `TOP=1`
- Il bordo **inferiore**: cella con `BOTTOM=1`
- Il bordo **sinistro**: cella con `LEFT=1`
- Il bordo **destro**: cella con `RIGHT=1`

```
# Cella nell'angolo in alto a sinistra:
5|3|1|||1|0|0|1|    → TOP=1 e LEFT=1

# Cella sul bordo superiore (non angolo):
5|5|1|||1|0|0|0|    → solo TOP=1

# Cella interna (nessun muro):
6|5|1|||0|0|0|0|    → nessun muro
```

### Porta / Ingresso / Uscita

- **Ingresso** (WallType=5): usa `TOP=5` sulla cella di ingresso livello per corridoi verticali (entrata dall'alto). Per corridoi **orizzontali** (entrata da sinistra), usa `LEFT=5` sulla prima cella di pavimento. In alternativa `RIGHT=5` sulla cella fill adiacente è visivamente equivalente.
- **Uscita** (WallType=6): usa `RIGHT=6` sull'ultima cella di pavimento per corridoi orizzontali (uscita a destra), oppure `TOP=6` per corridoi verticali. Abbinare sempre a un oggetto `Uscita` (ObjectType=3) sulla stessa cella.
- L'oggetto `Uscita` (ObjectType=3) è ciò che rende il livello completabile — il muro Uscita/Ingresso è solo visivo (la "cornice" della porta). Sono cose distinte: senza l'oggetto il livello non finisce, senza il muro visivo non si vede la porta.

---

## 8. Texture pack (envTexturePackId)

Il valore `envTexturePackId` (riga 3 del file) seleziona il set di texture grafiche per pavimenti, muri, decorazioni e porte del livello. Corrisponde alla cartella `Grounds/paccoN` nello spritesheet `Spritesheet-0.json`.

| ID  | Pack     | Texture pavimento disponibili | Caratteristiche                                                                                                                                                             |
| --- | -------- | ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `1` | `pacco1` | cell1–cell4                   | Set completo: muri random, pilastri, decals 1–5, porte animate, maindoor entrata/uscita. **Aspetto esterno/rovine all'aperto** — pietra chiara, adatto per i primi livelli. |
| `2` | `pacco2` | cell1–cell4                   | Set completo: muri random, pilastri, decals 1–8, porte animate, maindoor con variante exit                                                                                  |
| `3` | `pacco3` | cell4–cell5                   | Set ridotto: decals 2–3, special walls 3–5                                                                                                                                  |
| `4` | `pacco4` | cell4–cell5                   | Set ridotto                                                                                                                                                                 |
| `5` | `pacco5` | cell4–cell5                   | Set ridotto: special walls 3–5                                                                                                                                              |
| `6` | `pacco6` | cell4–cell5                   | Set ridotto: decals 3, special walls 3–5                                                                                                                                    |
| `7` | `pacco7` | cell4–cell5                   | Set ridotto: special walls 3–5                                                                                                                                              |
| `8` | `pacco8` | cell4–cell5                   | Set ridotto: decals 1–5, special walls 1–5                                                                                                                                  |

> **Raccomandazione per i livelli early:** usare `pacco1` o `pacco2` per la massima varietà visiva. I pacchi 3–8 sono adatti per livelli avanzati con un'estetica più scura e uniforme.

**Mappatura dei livelli esistenti:**

| Livello | Pack   | Dirty% | Note                      |
| ------- | ------ | ------ | ------------------------- |
| 1       | pacco1 | 0%     | —                         |
| 2       | pacco2 | 0%     | —                         |
| 3       | pacco3 | 0%     | —                         |
| 4       | pacco3 | 0%     | —                         |
| 5       | pacco4 | 0%     | —                         |
| 6       | pacco5 | 0%     | —                         |
| 7       | pacco7 | 0%     | —                         |
| 8       | pacco6 | 10%    | Sporco per atmosfera cupa |

---

## 9. Testi e narrativa (stringCode)

Il campo `STRING_CODE` (nono campo di ogni riga cella) permette di associare un **testo narrativo** a qualsiasi cella della mappa. Quando il giocatore entra in quella cella, il testo viene mostrato a schermo come pensiero/voce interiore di Mark.

### Come funziona

1. Aggiungi il codice numerico nel campo `STRING_CODE` di una cella
2. Il codice deve essere registrato in `Web/src/ui/GameTexts.ts` nella mappa `STRING_CODE_MAP`
3. Quando il giocatore calpesta quella cella, il testo appare automaticamente

### Codici disponibili

Questi sono i testi già definiti nel gioco (mappati in `GameTexts.ts`):

| Codice               | Testo                                                         | Dove usato | Momento consigliato                            |
| -------------------- | ------------------------------------------------------------- | ---------- | ---------------------------------------------- |
| `637284382845827427` | _"Una porta... premi CTRL per aprirla o chiuderla"_           | Livello 3  | Prima porta che il giocatore incontra          |
| `637284382983500501` | _"Una candela... raccoglila e premi SPAZIO per posizionarla"_ | Livello 3  | Prima candela raccoglibile                     |
| `637284384552007485` | _"Non è rassicurante..."_                                     | Livello 3  | Atmosfera — zona pericolosa o buia             |
| `637284386793405944` | _"Magari dovrei provare con una candela"_                     | Livello 4  | Suggerimento — zona buia davanti a un ostacolo |
| `637284387001107200` | _"Ce l'ho fatta!"_                                            | Livello 4  | Zona di arrivo/vittoria vicino all'uscita      |
| `637284388013304946` | _"Questo qualcosa... sembra comportarsi in modo strano"_      | Livello 5  | Primo incontro con un nemico insolito          |

> **Per aggiungere nuovi testi** occorre aggiungere una entry in `Web/src/ui/GameTexts.ts` nella mappa `STRING_CODE_MAP` con un ID numerico univoco (timestamp-like) e la stringa italiana.

### Come usare i testi in un livello

I testi vengono scritti sul **campo `STRING_CODE`** di una normale cella di pavimento — non richiedono un `OBJECT` particolare:

```
# Cella con solo testo, nessun oggetto
4|18|1|0||0|0|0|0|637284382983500501
# ↑ Pavimento Terra1, OBJECT=0 (nessun oggetto), STRING_CODE=testo candela

# Cella con testo su terreno puro (senza nemmeno OBJECT)
19|21|1|0||0|0|0|0|637284386793405944
```

---

## 10. Waypoint dei nemici

I nemici `Zombie`, `Scheletro`, `Goblin`, `Taurus`, `Ghost` e `Bubu` possono seguire un percorso di patruglia definito da waypoint. Il sistema funziona raccogliendo tutte le celle del livello che condividono lo stesso codice waypoint.

### Formato

```
PREFISSO-GRUPPO-INDICE
```

| Parte      | Descrizione                                                           |
| ---------- | --------------------------------------------------------------------- |
| `PREFISSO` | `Z` (gruppo Zombie/nemico generico) o `S` (gruppo Scheletro/speciale) |
| `GRUPPO`   | Numero che identifica il gruppo di patruglia (1, 2, 3, ...)           |
| `INDICE`   | Posizione del waypoint nel percorso (1, 2, 3, 4, ...)                 |

**Esempio:** `Z-1-1`, `Z-1-2`, `Z-1-3`, `Z-1-4` → 4 waypoint per il gruppo di patruglia "Z-1". Il nemico con `OBJ_DATA=Z-1-2` appartiene a questo gruppo e inizia dal waypoint 2, poi pattuglia in sequenza tutti gli altri waypoint del gruppo.

### Regole

1. **Un nemico = un waypoint.** Ogni riga del livello con un nemico che ha waypoint è sia il nemico che il waypoint (stessa cella).
2. **I waypoint di uno stesso gruppo sono le celle degli altri nemici del gruppo.** Se metti 4 Zombie con `Z-1-1`, `Z-1-2`, `Z-1-3`, `Z-1-4`, i 4 Zombie pattugliano tra le rispettive posizioni iniziali in sequenza.
3. **Gruppi diversi non si influenzano.** `Z-1-*` e `Z-2-*` sono percorsi indipendenti.
4. **Nemici senza waypoint** (campo vuoto) pattugliano in modo casuale o restano fermi.

### Esempio da livello 7

```
7|27|1|5|Z-1-2|0|0|0|0|    → Zombie, posizione (7,27), waypoint Z-1-2
9|27|1|5|Z-1-1|0|0|0|0|    → Zombie, posizione (9,27), waypoint Z-1-1
7|29|1|5|Z-1-3|0|0|0|0|    → Zombie, posizione (7,29), waypoint Z-1-3
9|29|1|5|Z-1-4|0|0|0|0|    → Zombie, posizione (9,29), waypoint Z-1-4
```

Questi 4 Zombie formano un gruppo che patruglia tra i 4 angoli di un'area 2×2.

---

## 11. Regole di validazione

### Obbligatorio

- **Esattamente 1** cella con `OBJECT=1` (Giocatore) per livello
- **Almeno 1** cella con `OBJECT=3` (Uscita) per livello
- La cella del Giocatore deve avere `GROUND` di tipo Terra (1, 6, 9–16)

### Fortemente consigliato

- **Almeno 1** cella con `OBJECT=2` (Chiave) — senza chiave l'uscita non si apre
- **Almeno qualche** cella con `OBJECT=4` (Candele) — il gioco è ingiocabile senza luce
- Lo spawn del Giocatore deve essere **separato dai nemici** da almeno 3–4 celle di distanza

### Coerenza dei waypoint

- Se un nemico ha `OBJ_DATA=Z-3-2`, deve esistere almeno un altro nemico con `OBJ_DATA=Z-3-1` (il percorso inizia da 1)
- I waypoint devono essere su celle di pavimento raggiungibili (non separate da muri impenetrabili)

### Confini mappa

- Le celle ai bordi esterni (ROW=0, ROW=24, COL=0, COL=44) dovrebbero avere muri `Occludente` o essere `Riempimento` — non lasciare bordi aperti

> Usa `scripts/validate-level.ts` per verificare automaticamente queste regole: `npx tsx scripts/validate-level.ts <livello.mark>`

---

## 12. Esempi di righe commentate

```
# Spawn del giocatore con muro ingresso sopra
17|34|10|1||5|0|0|0|
# ↑ ROW=17, COL=34, GROUND=Terra4(10), OBJECT=Giocatore(1), OBJ_DATA="", TOP=Ingresso(5)

# Uscita con muro uscita sopra
18|19|6|3||6|0|0|0|
# ↑ ROW=18, COL=19, GROUND=Terra2(6), OBJECT=Uscita(3), TOP=WallUscita(6)

# Chiave su pavimento con muro solido sopra
17|6|1|2||1|0|0|0|
# ↑ ROW=17, COL=6, GROUND=Terra1(1), OBJECT=Chiave(2), TOP=Occludente(1)

# Pacco di 3 candele
5|10|1|4|3|0|0|0|0|
# ↑ OBJECT=Candele(4), OBJ_DATA="3"

# Zombie con waypoint, angolo in alto a sinistra di un quadrato di pattuglia
7|27|1|5|Z-1-2|0|0|0|0|

# Scheletro con waypoint
14|8|1|6|S-1-1|0|0|0|0|

# Braciere (torcia ambientale statica, illumina la stanza)
10|20|1|12||0|0|0|0|
# ↑ OBJECT=Lucina(12) → BrazierLit

# Ostacolo solido (non attraversabile)
8|15|1|14||0|0|0|0|
# ↑ OBJECT=OggettoNonPassabile2(14)

# Riempimento (sfondo muro spesso, non calpestabile)
3|1|8|||0|0|0|0|
# ↑ GROUND=Riempimento1(8), nessun oggetto, nessun muro

# Trappola letale
12|22|3|||0|0|0|0|
# ↑ GROUND=TrappolaMorteIstantanea(3)

# Angolo stanza (muro in alto e a sinistra)
5|3|1|||1|0|0|1|
# ↑ TOP=Occludente(1), LEFT=Occludente(1)
```

---

## 13. Template livello minimo

Uno scheletro minimo funzionante (25×45, texture pack 1, nessun sporco). Include solo gli elementi obbligatori: una stanza 5×5 con giocatore, chiave, uscita e 2 candele. Tutti gli altri campi della griglia sono omessi (trattati come null/vuoto).

```
GameGridSerializerV4
25|45
1|0
10|20|1|1||0|0|0|0|
10|21|1|4|2|0|0|0|0|
10|22|1|4|2|0|0|0|0|
10|24|1|2||0|0|0|0|
10|26|1|3||6|0|0|0|
9|20|1|||1|0|0|1|
9|21|1|||1|0|0|0|
9|22|1|||1|0|0|0|
9|23|1|||1|0|0|0|
9|24|1|||1|0|0|0|
9|25|1|||1|0|0|0|
9|26|1|||1|0|1|0|
10|20|1|||0|0|0|1|
10|26|1|||0|1|0|0|
11|20|1|||0|0|1|1|
11|21|1|||0|0|1|0|
11|22|1|||0|0|1|0|
11|23|1|||0|0|1|0|
11|24|1|||0|0|1|0|
11|25|1|||0|0|1|0|
11|26|1|||0|1|1|0|
```

> Questo template è un punto di partenza minimo. Un livello reale dovrebbe avere circa 300–600 righe (celle di pavimento + riempimento + muri).
