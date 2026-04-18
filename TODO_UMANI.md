## Migliorare il gioco

### UI

Prendi il file LEVEL_DESIGN_GUIDELINES.md e fai generare all'AI un asset grafico per ogni meccanica, così posso fare un pulsante "tutorial" e mostrarle lì a mo' di gallery

### Livelli

- [ ] Farli tutti con la mia storia
- [ ] Rivedi tutti i testi in levels.ts alla fine (anche a Chiara magari)

### Qualità grafica

- Dato che tutti gli asset sono disegnati in realtà in 32x32, conviene rifare tutti gli spritesheet in 32x32 e caricarli davvero così, in questo modo costano molto meno lato hardware! E poi upscaliamo dentro al gioco.

Quindi devo:

- Riprendere in mano Spriter
- Batchare con photoshop o script AI con qualche libreria tutte le immagini per rigenerarle davvero 32x32, ma con la stessa identica struttura cartelle
- Rigenereare gli spritesheet, se ho culo ne viene uno solo
- Ricaricare tutto
- Impostare il gioco per zoommare/upscalare perché sarebbe tutto minuscolo
- FORSE potrebbero esserci problemi di offset/pivot in giro, non lo so

### Colonna sonora

- [ ] Rach in sottofondo molto leggero e loppato
- [ ] Suoni ambientali generati con AI -> repo Michele
  - [ ] Crepitio del fuoco bracieri
  - [ ] Crepitio della candela
  - [ ] Versi dei mostri (uno diverso per ogni mostro)
  - [ ] Passi del player
  - [ ] Passi dei mostri con volume legato alla distanza e direzionalità
  - [ ] Suono da gioco quando prendi il pacco di candele, quando prendi la chiave
  - [ ] Suono quando apri e chiudi le porte
- [ ] Chiedi all'AI consigli dopo aver comunque già impostato alcune cose

## Erogarlo sul mio sito

- [ ] Mark: articolo sul sito, parla di shader. Ho unito la mia passione per i videogiochi con la mia passione per la matematica e spiega gli shader-chiedi aiuto all’ai per scrivere articoli. E ancje quello di infatt e starfall arricchisco meglio, non farlo solo markettaro
- [ ] Fare in modo che funzioni online
- [ ] Post LinkedIn e tutto il resto
- [ ] Open source
