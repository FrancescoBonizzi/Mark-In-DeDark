# Bug

Valutare le differenze rispetto alla versione CSharp originale e implementare le correzioni.

- [ ] La trappola continua a "muoversi", animarsi anche quando non c'è più nessuno sopra. Inoltre la texture della parte sotto alla copertura della trappola, non c'è, c'è solo la tile "SLOT". Per entrambe le trappole vanno usata la texture trap_breaking e trapHole. Guarda com'era la versione CSharp originale, cerca: Csharp/MarkInDeDark.Library/Objects/TimedBreakableTrap.cs e Csharp/MarkInDeDark.Library/Objects/InstantlyDieTrap.cs. Adegua il comportamento sia logico che grafico
- [ ] Verifica che anche i mostri muoiano sulle trappole
- [ ] Verifica che il dirty sia applicato correttamente e nel claso applicalo
- [ ] Gli UnpassableObject non sembrano implementati. Implementali guardando qui: /Users/fbonizzi/Source/Mark-In-DeDark/Csharp/MarkInDeDark.Library/Objects/UnpassableObject.cs
- [ ] La glow decal viene utilizzata?
- [ ] La texture snoflake non viene utilizzata per far nevicare, man -> /Users/fbonizzi/Source/Mark-In-DeDark/Csharp/MarkInDeDark.Library/Particles/Snowing/SnowFLake.cs
- [ ] LevelEditor: quando utilizzi il comando "Cancella" fai cancellare anche il testo aggiunto come testo visualizzabile in game
