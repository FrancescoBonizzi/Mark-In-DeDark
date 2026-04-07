# Linee guida per il design dei livelli — Mark In DeDark

## Meccaniche del gioco

Ogni meccanica deve essere **introdotta una alla volta**, non introdurre mai due meccaniche nuove nello stesso livello.

1. Prendo la chiave per aprire la porta di fine livello
2. Cerco di non farmi toccare dai mostri altrimenti muoio, ho bisogno di un giusto timing dei movimenti per passare in alcune aree
3. Piazzo una candela per attirare/distrarre/bloccare alcuni mostri e superarli
4. Piazzo una candela sulla trappola temporizzata così i mostri che si fermano per distruggerla fanno scattare la trappola e muoiono
5. Piazzo una candela/attiro mostri facendomi seguire dietro alle porte che si aprono e si chiudono manualmente così li rinchiudo (quali sono i mostri che possono aprire le porte in autonomia?)
6. Quando incontro lo skeleton, cerco di passare quando è di spalle e non sono in linea di vista, altrimenti mi insegue. Se mi insegue, devo uscire dalla sua linea di vista così non vedendomi più se ne va
7. Attiro il Taurus in alcune direzioni per fargli rompere un muro
8. Attiro il Taurus sulla trappola a morte istantaea per liberarmi di lui (e altri mostri forse? Se non erro gli altri le evitano)
9. Attiro il Taurus contro altri nemici così se li carica, li ammazza
10. Sto fermo immobile quando il fantasma mi sta per attraversa, così mi passa attraverso. Se camminiamo insieme, muoio. Quindi capisco perché devo fare dei momenti in cui cammino e devi momenti in cui aspetto che mi attraversi e riprendo. (meccanica contro intruitiva ma per farla capire basta un testo)
11. Attiro il Bubu in stanze con tanti braceri così si blocca e non mi segue più. Infatti il Bubu ha paura dei braceri grossi. La meccanica è "Come faccio a far sì che il Bubu rimanga dall'altra parte del braciere mentre io passo?". Lo attiri con una candela, lo fai avvicinare al braciere, lui si ferma, tu passi dall'altro lato. Oppure lo intrappi in una stanza con bracieri agli angoli.

## Trigger di testo / Come veicolare la storia

I trigger di testo popolano il diario (che si apre con il tasto T), e servono per veicolare una storia. È fondamentale mettere i trigger legati alla storia in punti in cui il personaggio deve passare per forza, quindi corridoi da 1 cella, così non se li perde.

## Principi

### Non fare mai un livello dove la soluzione è "cerca nel buio"

Il buio è uno strumento di tensione, narrativo ed estetico, il senso del gioco è capire le meccaniche e superare i mostri, non cercare le cose in un labirinto cieco. Usa luci ambientali (bracieri) per illuminare i punti di riferimento, strade, bivi importanti.

Il buio dev'essere sempre attraversabile, non tutta la mappa.

Eviterei vicoli ciechi se non sono stanze in cui c'è da prendere qualcosa.

## La candela in mano è inesauribile

Il giocatore ha **sempre** una candela accesa in mano che illumina il suo intorno. Questa luce è **inesauribile** e non si spegne mai da sola. Le candele raccoglibili nella mappa servono esclusivamente a essere **piazzate a terra** per distrarre i nemici.

## Pacchetti candela vs candela singola

Non dare mai troppe candele al player, altrimenti ne piazza 10 e passa dove vuole. Devono essere centellinate in modo tale che se sbagli, ricominci.

Non mettere mai pacchi di candele in zone prive di nemici. Le candele si usano esclusivamente per distrarre i nemici: piazzarle in un corridoio vuoto non ha senso dal punto di vista del gameplay.

## Ogni livello è autonomo — nessun carry-over

**L'inventario del giocatore si azzera a ogni livello.** Le candele raccolte in un livello non vengono portate in quello successivo: ogni livello inizia sempre con inventario vuoto. Di conseguenza, ogni livello deve essere progettato come un sistema chiuso: le candele presenti nella mappa sono esattamente quelle disponibili per quel livello, né di più né di meno. Non ha senso lasciare candele "di riserva per dopo" — o servono in quel livello, o non devono esserci.

---

## I nemici

### Zombie

- **Personalità:** patrugliante lento e meccanico — va avanti e indietro senza deviare
- **Spegne candele:** sì, al contatto
- **Vede il giocatore:** no (patrol puro)
- **Luce propria:** sì

### Scheletro

- **Personalità:** patrugliante con visuale lineare — ti insegue se ti vede in linea retta
- **Spegne candele:** sì, se sono nel suo raggio di ricerca
- **Vede il giocatore:** sì, su linea orizzontale o verticale senza ostacoli
- **Luce propria:** sì

### Taurus

- **Personalità:** guardiano lento che carica in linea retta quando ti vede. Abbatte i muri rompibili nella sua traiettoria.
- **Spegne candele:** no
- **Vede il giocatore:** sì, solo su linea orizzontale o verticale
- **Luce propria:** sì

### Ghost

- **Personalità:** attraversa i muri, attacca solo se il giocatore si muove (se sta fermo non gli fa niente)
- **Spegne candele:** sì, molto velocemente
- **Attraversa i muri:** sì
- **Luce propria:** sì

### Bubu

- **Personalità:** patrugliante che ha come obiettivo primario spegnere le candele del giocatore
- **Spegne candele:** sì, è il suo comportamento principale
- **Luce propria:** sì

### Goblin

- **Personalità:** conosce sempre la posizione del giocatore e lo insegue. Diventa più veloce ogni volta che il giocatore usa una candela.
- **Spegne candele:** sì, le cerca attivamente
- **Luce propria:** sì
