# Task

- Alcuni sono da eseguire, flaggali come fatti
- Altri sono di approfondimento, sono quelli che iniziano con "Task di approfondimento" in quel caso crea un nuovo task in un heading h2 a parte in cui descrivi come lo faresti, ma non implementarlo. Tutti gli altri task implementali subito invece.

---

## Da fare

## Task di approfondimento — Mostri che possono aprire porte da soli

- Obiettivo: mappare, senza implementare modifiche, quali AI possono attraversare/azionare porte chiuse in autonomia e in quali condizioni.
- Come lo farei:
  1. Inventario comportamenti: leggo le classi AI dei nemici e la logica porte nel grid per trovare check su `isOpen`, collisioni e gestione muri/porte.
  2. Matrice per nemico: per Zombie, Scheletro, Goblin, Taurus, Ghost, Bubu distinguo "apre porta", "passa solo se aperta", "ignora porta".
  3. Verifica comparativa C#: confronto lo stesso comportamento nelle classi originali C# per evitare regressioni di porting.
  4. Validazione in runtime: preparo un livello test con stanza chiusa e una porta, poi osservo ogni nemico con Playwright debug per confermare il comportamento reale.
  5. Deliverable: aggiungo un report sintetico in docs con tabella finale e differenze eventuali C# vs Web.
- Output atteso: risposta univoca per ogni mostro (Sì/No/Condizionale) più note operative di level design.

## Ipotesi (non fare)

- [ ] Bubu: fare in modo che abbia paura dei braceri, che se un bracere è al centro di 9 celle, in quelle 9 celle non ci vada assolutamente. Sui braceri non deve MAI poterci andare (nel suo grafo evitali completamente), sulle celle appena adiacenti può andarci solo se ci piazzi un candela dall'altra parte e allora ci va. Devo poter implementare una meccanica che il player lo intrappa in una stanza con bracieri agli angoli. -> progettare bene prima di farlo fare
- [ ] La nostra candela, non serve a nulla se non a vedere, è un po' controintuitivo che i mostri seguano le altre candele ma non te. Se aggiungiamo un pulsante per spegnere la tua stessa candela, introduciamo un elemento di gameplay interessante? Tipo che lo scheletro e taurus non ti vedano - diventa stealth.

  Il tradeoff centrale è buono

  Luce accesa → vedi dove vai, ma Scheletro e Taurus ti vedono
  Luce spenta → invisibile ai nemici visivi, ma navighi al buio

  Questo è un rischio bilaterale, che è esattamente il tipo di meccanica interessante. Non è un  
  vantaggio gratuito.

  ***

  Interazione con ogni nemico — è più pulita di quanto sembra

  ┌───────────┬─────────────────────────────────────────────────────────────────────────────────┐
  │ Nemico │ Reagisce alla tua luce spenta? │  
  ├───────────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ Zombie │ Nessun cambiamento (patrol puro, non ti vede già) │  
  ├───────────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ Scheletro │ Sì — non ti vede più in linea di vista │  
  ├───────────┼─────────────────────────────────────────────────────────────────────────────────┤  
  │ Taurus │ Sì — non carica più │  
  ├───────────┼─────────────────────────────────────────────────────────────────────────────────┤  
  │ Ghost │ Nessun cambiamento — reagisce al movimento, non alla luce │
  ├───────────┼─────────────────────────────────────────────────────────────────────────────────┤  
  │ Bubu │ Potenzialmente sì — se il suo obiettivo primario è "spegni le luci", senza luce │
  │ │ ti ignora? │  
  ├───────────┼─────────────────────────────────────────────────────────────────────────────────┤
  │ Goblin │ Nessun cambiamento — conosce sempre la tua posizione │  
  └───────────┴─────────────────────────────────────────────────────────────────────────────────┘

  È pulito: funziona solo contro i nemici visivi, che è un sottoinsieme coerente.

  ***

  Il rischio di bilanciamento

  Scheletro e Taurus potrebbero diventare triviali da soli. Il costo del buio deve essere reale.
  Alcune opzioni:
  - Riaccendere richiede un consumabile — usi una candela dall'inventario per riaccenderti. Ora è  
    una vera decisione di risorse: la candela la uso come esca o per la mia luce?
  - Riaccensione lenta — ci vuole qualche secondo, durante i quali sei vulnerabile e cieco
  - Il buio ti rallenta — movimenti più lenti nel buio, crei finestre di rischio

  La terza opzione mi sembra la più elegante perché non aggiunge complessità di UI.

  ***

  Combinazione più interessante che sblocca

  ▎ Piazzi una candela come esca → spegni la tua → muovi nel buio mentre il nemico è distratto
  dalla candela

  Questo è un puzzle a due step che non esiste ancora nel design. La candela a terra diventa  
  davvero un decoy, non solo un ostacolo per il nemico.

  ***

  Una domanda da risolvere prima di implementare

  Il Ghost diventa banale in combinazione: spento + fermo = invincibile a tutto. Conviene decidere
  se questa combo è intenzionale o se il Ghost deve reagire anche alla tua presenza anche al buio  
  (magari percepisce il calore? il respiro?).
