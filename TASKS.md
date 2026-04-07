# Task

- Alcuni sono da eseguire, flaggali come fatti
- Altri sono di approfondimento, sono quelli che iniziano con "Task di approfondimento" in quel caso crea un nuovo task in un heading h2 a parte in cui descrivi come lo faresti, ma non implementarlo. Tutti gli altri task implementali subito invece.

---

## Da fare

- [ ] Elimina completamente dal goblin la meccanica dell'accelerazione quando piazzi le candele, elimina tutto anche gli effetti luminosi relativi
- [ ] Prendi il file LEVEL_DESIGN_GUIDELINES e trasformalo in una pagina da linkare dentro al level editor così uno può leggersi le linee guida. Facciamo in modo di tenerla aggiornata in qualche modo, tipo mettilo in build. Se il file un giorno qualcuno lo rinomina o non esiste più, tira errore in build. Impaginalo bene considerando già che è un .md
- [ ] Task di approfondimento: quali mostri possono aprire le porte da soli se li chiudo dentro?

## Ipotesi (non fare)

- [ ] Bubu: fare in modo che abbia paura dei braceri, che se un bracere è al centro di 9 celle, in quelle 9 celle non ci vada assolutamente. Sui braceri non deve MAI poterci andare (nel suo grafo evitali completamente), sulle celle appena adiacenti può andarci solo se ci piazzi un candela dall'altra parte e allora ci va. Devo poter implementare una meccanica che il player lo intrappa in una stanza con bracieri agli angoli. -> progettare bene prima di farlo fare
- [ ] La nostra candela, non serve a nulla se non a vedere, è un po' controintuitivo che i mostri seguano le altre candele ma non te. Se aggiungiamo un pulsante per spegnere la tua stessa candela, introduciamo un elemento di gameplay interessante? Tipo che lo scheletro e taurus non ti vedano - diventa stealth
