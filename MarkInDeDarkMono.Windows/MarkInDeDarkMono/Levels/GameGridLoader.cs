using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Objects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.Levels
{
    public static class GameGridLoader
    {
        public static void LoadGrid(
            AssetsLoader assetsLoader,
            SoundManager soundManager,
            string levelContentRaw,
            GameGrid grid)
        {
            // La sintassi è: NESW (TOP-RIGHT-SUD-WEST) -> 1 se c'è, 0 se non c'è
            // Per gli altri comandi vedi il level editor in Excel

            // Gli indici della matrice delle celle
            int i = 0;
            int j = 0;
            var lines = levelContentRaw
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(s => !string.IsNullOrWhiteSpace(s));

            var skeletronsPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            var zombiesPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            var taurusPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            grid.InstantlyDieTrapCollisionRectangles = new List<Rectangle>();
            grid.BrekableTrapCollisionRectangles = new List<Rectangle>();

            foreach (var riga in lines)
            {
                var rigaParts = riga.Split(':');
                var cell = grid.GetCellByIndex(j, i);

                if (rigaParts[0] == "NULL")
                {
                    grid.SetNullCell(j, i);
                }
                else if (rigaParts[1] == "MACERIE")
                {
                    cell.IsUnwalkable = true;
                }
                // Considerando che faccio il Contains, vanno SEMPRE prima i caratteri multipli
                else if (rigaParts[1].Contains("GT"))
                {
                    // Trigger per generare il goblin
                    grid.GoblinTriggerRectangle = grid.GetCellByIndex(j, i).CollisionRectangle;
                }
                else if (rigaParts[1].Contains("DT")) // Die Trap
                {
                    grid.InstantlyDieTrapCollisionRectangles.Add(grid.GetCellByIndex(j, i).CollisionRectangle);
                }
                else if (rigaParts[1].Contains("BT")) // Breakable Trap
                {
                    grid.BrekableTrapCollisionRectangles.Add(grid.GetCellByIndex(j, i).CollisionRectangle);
                }
                else if (rigaParts[1].Contains("P"))
                {
                    // Imposto la posizione iniziale del player
                    grid.PlayerStartingPosition = grid.GetPixelBy_ij(j, i);
                }
                else if (rigaParts[1].Contains("E"))
                {
                    // Imposto il punto di uscita
                    grid.ExitPosition = grid.GetPixelBy_ij(j, i);
                    cell.IsUnwalkableForEnemies = true; // Non voglio che i mostri ci vadano sopra
                }
                else if (rigaParts[1].Contains("W"))
                {
                    // Lupo
                    grid.WerewolfStartingPosition = grid.GetPixelBy_ij(j, i);
                }
                else if (rigaParts[1].Contains("Z"))
                {
                    // Zombie
                    // Essendo un patrol, avrò il suo percorso nel formato Z-1-1 ecc

                    var patrolPart = rigaParts[1].Split('-');
                    var patrolNumber = Convert.ToInt32(patrolPart[1]);
                    var patrolWaypointNumber = Convert.ToInt32(patrolPart[2]);
                    var cellWithOrder = new KeyValuePair<int, GameCell>(patrolWaypointNumber, cell);

                    // Ogni riga di _zombies_path è uno zombie
                    if (zombiesPaths.ContainsKey(patrolNumber))
                    {
                        zombiesPaths[patrolNumber].Add(cellWithOrder);
                    }
                    else
                    {
                        zombiesPaths.Add(
                            patrolNumber,
                            new List<KeyValuePair<int, GameCell>>() { cellWithOrder });
                    }
                }
                else if (rigaParts[1].Contains("S"))
                {
                    // Scheletro
                    // Essendo un patrol, avrò il suo percorso nel formato K-1-1 ecc

                    var patrolPart = rigaParts[1].Split('-');
                    var patrolNumber = Convert.ToInt32(patrolPart[1]);
                    var patrolWaypointNumber = Convert.ToInt32(patrolPart[2]);
                    var cellWithOrder = new KeyValuePair<int, GameCell>(patrolWaypointNumber, cell);

                    // Ogni riga di _skeletrons_path è uno zombie
                    if (skeletronsPaths.ContainsKey(patrolNumber))
                    {
                        skeletronsPaths[patrolNumber].Add(cellWithOrder);
                    }
                    else
                    {
                        skeletronsPaths.Add(
                            patrolNumber,
                            new List<KeyValuePair<int, GameCell>>() { cellWithOrder });
                    }
                }
                else if (rigaParts[1].Contains("T"))
                {
                    // Toro
                    // Essendo un patrol, avrò il suo percorso nel formato T-1-1 ecc

                    var patrolPart = rigaParts[1].Split('-');
                    var patrolNumber = Convert.ToInt32(patrolPart[1]);
                    var patrolWaypointNumber = Convert.ToInt32(patrolPart[2]);
                    var cellWithOrder = new KeyValuePair<int, GameCell>(patrolWaypointNumber, cell);

                    // Ogni riga di _toro_path è un toro
                    if (taurusPaths.ContainsKey(patrolNumber))
                    {
                        taurusPaths[patrolNumber].Add(cellWithOrder);
                    }
                    else
                    {
                        taurusPaths.Add(
                            patrolNumber,
                            new List<KeyValuePair<int, GameCell>>() { cellWithOrder });
                    }
                }
                else if (rigaParts[1].Contains("G"))
                {
                    // Goblin
                    grid.GoblinStartPosition = grid.GetPixelBy_ij(j, i);
                }
                else if (rigaParts[1].Contains("C"))
                {
                    // Pacchi di candele
                    var numberOfCandles = Convert.ToInt32(rigaParts[1].Split('-')[1]);
                    grid.CandlePacks.Add(new CandlePack(numberOfCandles, assetsLoader, grid.GetPixelBy_ij(j, i), soundManager));
                }
                else if (rigaParts[1].Contains("K"))
                {
                    grid.KeyPosition = grid.GetPixelBy_ij(j, i);
                }

                // Ho messo direttamente 4 per escludere l'ultimo carattere se c'è,
                // che ho già controllato sopra
                if (cell != null)
                {
                    for (int c = 0; c < 4; c++)
                    {
                        if (rigaParts[0][c] != '0')
                        {
                            switch (c)
                            {
                                case 0:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        cell.IsNorthDestroyable = true;
                                    cell.IsNorthPassable = false;
                                    break;

                                case 1:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        cell.IsEastDestroyable = true;
                                    cell.IsEastPassable = false;
                                    break;

                                case 2:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        cell.IsSouthDestroyable = true;
                                    cell.IsSouthPassable = false;
                                    break;

                                case 3:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        cell.IsWestDestroyable = true;
                                    cell.IsWestPassable = false;
                                    break;
                            }
                        }
                    }

                    cell.GenerateWalls();
                    cell.SetSimpleAngles();
                }

                // Gioco con gli indici della griglia per far scorrere verticalmente
                // la griglia al momento dell'inserimento
                // (Perchè ho fatto esportare da Excel in questo ordine)
                if (i == GameGrid.VerticalCellsNumber - 1)
                {
                    i = 0;
                    ++j;
                }
                else
                {
                    ++i;
                }
            }

            grid.SkeletronsPath = new List<List<GameCell>>();
            foreach (var path in skeletronsPaths)
            {
                grid.SkeletronsPath.Add(
                    path.Value
                    .OrderBy(p => p.Key)
                    .Select(p => p.Value)
                    .ToList());
            }

            grid.ZombiesPath = new List<List<GameCell>>();
            foreach (var path in zombiesPaths)
            {
                grid.ZombiesPath.Add(
                    path.Value
                    .OrderBy(p => p.Key)
                    .Select(p => p.Value)
                    .ToList());
            }

            grid.TaurusPath = new List<List<GameCell>>();
            foreach (var path in taurusPaths)
            {
                grid.TaurusPath.Add(
                    path.Value
                    .OrderBy(p => p.Key)
                    .Select(p => p.Value)
                    .ToList());
            }

            GridAsGraph(grid);
            CleanGrid(grid);
        }

        private static void GridAsGraph(GameGrid grid)
        {
            for (int i = 0; i < GameGrid.HorizontalCellsNumber; ++i)
            {
                for (int j = 0; j < GameGrid.VerticalCellsNumber; ++j)
                {
                    var cell = grid.GetCellByIndex(i, j);

                    if (cell == null || cell.IsUnwalkable)
                        continue;

                    // Non può andare out of range l'indice perchè se è passable l'altra è nella griglia
                    if (cell.IsNorthPassable)
                    {
                        cell.AddLink(grid.GetCellByIndex(i, j - 1), LinkDirection.NorthLink);
                    }
                    else
                        cell.AddLink(null, LinkDirection.NorthLink);

                    if (cell.IsEastPassable)
                    {
                        cell.AddLink(grid.GetCellByIndex(i + 1, j), LinkDirection.EastLink);
                    }
                    else
                        cell.AddLink(null, LinkDirection.EastLink);

                    if (cell.IsSouthPassable)
                    {
                        cell.AddLink(grid.GetCellByIndex(i, j + 1), LinkDirection.SouthLink);
                    }
                    else
                        cell.AddLink(null, LinkDirection.SouthLink);

                    if (cell.IsWestPassable)
                    {
                        cell.AddLink(grid.GetCellByIndex(i - 1, j), LinkDirection.WestLink);
                    }
                    else
                        cell.AddLink(null, LinkDirection.WestLink);

                    // Aggiungo altri muretti. Posso farlo solo qua perchè prima non ho i link alle celle intorno!

                    // Diagonale alto a destra
                    if (!cell.IsEastPassable)
                        if (cell.Links[LinkDirection.NorthLink] != null)
                            if (cell.Links[LinkDirection.NorthLink].Links[LinkDirection.EastLink] != null)
                                if (!cell.Links[LinkDirection.NorthLink].Links[LinkDirection.EastLink].IsSouthPassable)
                                    cell.AddNEAngle();

                    // Diagonale alto a sinistra
                    if (!cell.IsWestPassable)
                        if (cell.Links[LinkDirection.NorthLink] != null)
                            if (cell.Links[LinkDirection.NorthLink].Links[LinkDirection.WestLink] != null)
                                if (!cell.Links[LinkDirection.NorthLink].Links[LinkDirection.WestLink].IsSouthPassable)
                                    cell.AddNWAngle();

                    // Sotto e destra
                    if (cell.Links[LinkDirection.NorthLink] != null)
                        if (!cell.Links[LinkDirection.NorthLink].IsEastPassable)
                            if (cell.Links[LinkDirection.EastLink] != null)
                                if (!cell.Links[LinkDirection.EastLink].IsNorthPassable)
                                    cell.AddNEAngle();

                    // Sotto e a sinistra
                    if (cell.Links[LinkDirection.NorthLink] != null)
                        if (!cell.Links[LinkDirection.NorthLink].IsWestPassable)
                            if (cell.Links[LinkDirection.WestLink] != null)
                                if (!cell.Links[LinkDirection.WestLink].IsNorthPassable)
                                    cell.AddNWAngle();

                    // Per disegnare i pilastri in cui i muri non sono da soli (solo delle righe senza angoli)
                    if (!cell.IsNorthPassable)
                        if (cell.Links[LinkDirection.EastLink] != null)
                            if (cell.Links[LinkDirection.EastLink].IsNorthPassable)
                                cell.AddNEAngle();

                    if (!cell.IsNorthPassable)
                        if (cell.Links[LinkDirection.WestLink] != null)
                            if (cell.Links[LinkDirection.WestLink].IsNorthPassable)
                                cell.AddNWAngle();

                    if (!cell.IsEastPassable)
                        if (cell.Links[LinkDirection.SouthLink] != null)
                            if (cell.Links[LinkDirection.SouthLink].IsEastPassable)
                                cell.AddSEAngle();

                    if (!cell.IsEastPassable)
                        if (cell.Links[LinkDirection.NorthLink] != null)
                            if (cell.Links[LinkDirection.NorthLink].IsEastPassable)
                                cell.AddNEAngle();

                }
            }
        }

#warning MAH! Se i muri e gli angoli NON fossero proprietà delle celle, ma della griglia, sarebbe molto più semplice pulirla, o addirittura non mettere il muro

        private static void CleanGrid(GameGrid grid)
        {
            // Elimino le tutti i muri doppi gli angoli doppi
            // Per ogni angolo devo vedere se quell'angolo o quel muro esiste già uguale vicino
            for (int i = 0; i < GameGrid.HorizontalCellsNumber; ++i)
            {
                for (int j = 0; j < GameGrid.VerticalCellsNumber; ++j)
                {
                    var cell = grid.GetCellByIndex(i, j);

                    if (cell == null)
                        continue;

                    foreach (var wall in cell.Walls)
                    {
                        foreach (var nearCell in cell.Links.Select(a => a.Value))
                        {
                            if (nearCell != null)
                            {
                                if (nearCell.Walls.Count != 0)
                                {
                                    for (int w = nearCell.Walls.Count - 1; w >= 0; --w)
                                    {
                                        if (wall.DrawingInfos.Position == nearCell.Walls[w].DrawingInfos.Position)
                                        {
                                            // Fix balordo per evitare di perdere l'info sulla rompibilità
                                            if (!nearCell.Walls[w].IsDestroyable)
                                                nearCell.Walls.RemoveAt(w);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (var cellAngle in cell.AnglesDrawingInfos)
                        {
                            foreach (var nearCell in cell.Links.Select(a => a.Value))
                            {
                                if (nearCell != null)
                                {
                                    if (nearCell.AnglesDrawingInfos.Count != 0)
                                    {
                                        for (int w = nearCell.AnglesDrawingInfos.Count - 1; w >= 0; --w)
                                        {
                                            if (cellAngle == nearCell.AnglesDrawingInfos[w])
                                            {
                                                nearCell.AnglesDrawingInfos.RemoveAt(w);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

    }
}