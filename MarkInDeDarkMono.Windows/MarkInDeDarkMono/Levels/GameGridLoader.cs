using MarkInDeDark.GameGrid;
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
            var serializer = new GameGridDeserializerManager(levelContentRaw);
            var levelEditorGrid = serializer.DeserializeLevel();

            var skeletronsPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            var zombiesPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            var taurusPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            grid.InstantlyDieTrapCollisionRectangles = new List<Rectangle>();
            grid.BrekableTrapCollisionRectangles = new List<Rectangle>();

            for (int j = 0; j < levelEditorGrid.Rows; ++j)
            {
                for (int i = 0; i < levelEditorGrid.Columns; ++i)
                {
                    var gameCell = grid.GetCellByIndex(i, j);
                    var levelEditorCell = levelEditorGrid.Grid[i, j];

                    switch (levelEditorCell.GroundType)
                    {
                        case GroundTypes.Buio:
                            grid.SetNullCell(i, j);
                            continue;

                        case GroundTypes.Macerie:
                            gameCell.IsUnwalkable = true;
                            break;

                        case GroundTypes.Terra:
                            break;

                        case GroundTypes.TrappolaMorteIstantanea:
                            grid.InstantlyDieTrapCollisionRectangles.Add(gameCell.CollisionRectangle);
                            break;

                        case GroundTypes.TrappolaTemporizzata:
                            grid.BrekableTrapCollisionRectangles.Add(gameCell.CollisionRectangle);
                            break;

                        case GroundTypes.TriggerGoblin:
                            grid.GoblinTriggerRectangle = gameCell.CollisionRectangle;
                            break;
                    }

                    if (levelEditorCell.ObjectType != null)
                    {
                        switch (levelEditorCell.ObjectType.Value)
                        {
                            case ObjectTypes.Giocatore:
                                grid.PlayerStartingPosition = grid.GetPixelBy_ij(i, j);
                                break;

                            case ObjectTypes.Chiave:
                                grid.KeyPosition = grid.GetPixelBy_ij(i, j);
                                break;

                            case ObjectTypes.Uscita:
                                grid.ExitPosition = grid.GetPixelBy_ij(i, j);
                                break;

                            case ObjectTypes.Candele:
                                var numberOfCandles = Convert.ToInt32(levelEditorCell.ObjectTypeAdditionalData);
                                grid.CandlePacks.Add(new CandlePack(numberOfCandles, assetsLoader, grid.GetPixelBy_ij(i, j), soundManager));
                                break;

                            case ObjectTypes.Zombie:

                                {
                                    // Zombie
                                    // Essendo un patrol, avrò il suo percorso nel formato Z-1-1 ecc
                                    var patrolPart = levelEditorCell.ObjectTypeAdditionalData.Split('-');
                                    var patrolNumber = Convert.ToInt32(patrolPart[1]);
                                    var patrolWaypointNumber = Convert.ToInt32(patrolPart[2]);
                                    var cellWithOrder = new KeyValuePair<int, GameCell>(patrolWaypointNumber, gameCell);

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
                                break;

                            case ObjectTypes.Scheletro:

                                {
                                    // Scheletro
                                    // Essendo un patrol, avrò il suo percorso nel formato K-1-1 ecc
                                    var patrolPart = levelEditorCell.ObjectTypeAdditionalData.Split('-');
                                    var patrolNumber = Convert.ToInt32(patrolPart[1]);
                                    var patrolWaypointNumber = Convert.ToInt32(patrolPart[2]);
                                    var cellWithOrder = new KeyValuePair<int, GameCell>(patrolWaypointNumber, gameCell);

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
                                break;

                            case ObjectTypes.Goblin:
                                grid.GoblinStartPosition = grid.GetPixelBy_ij(i, j);
                                break;

                            case ObjectTypes.Taurus:

                                // Toro
                                // Essendo un patrol, avrò il suo percorso nel formato T-1-1 ecc
                                {
                                    var patrolPart = levelEditorCell.ObjectTypeAdditionalData.Split('-');
                                    var patrolNumber = Convert.ToInt32(patrolPart[1]);
                                    var patrolWaypointNumber = Convert.ToInt32(patrolPart[2]);
                                    var cellWithOrder = new KeyValuePair<int, GameCell>(patrolWaypointNumber, gameCell);

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
                                break;

                            case ObjectTypes.Werewolf:
                                grid.WerewolfStartingPosition = grid.GetPixelBy_ij(i, j);
                                break;
                        }
                    }

                    gameCell.IsNorthDestroyable = levelEditorCell.Top == WallTypes.Fragile;
                    gameCell.IsNorthPassable = levelEditorCell.Top == WallTypes.Niente;
                    
                    gameCell.IsEastDestroyable = levelEditorCell.Right == WallTypes.Fragile;
                    gameCell.IsEastPassable = levelEditorCell.Right == WallTypes.Niente;

                    gameCell.IsSouthDestroyable = levelEditorCell.Bottom == WallTypes.Fragile;
                    gameCell.IsSouthPassable = levelEditorCell.Bottom == WallTypes.Niente;

                    gameCell.IsWestDestroyable = levelEditorCell.Left == WallTypes.Fragile;
                    gameCell.IsWestPassable = levelEditorCell.Left == WallTypes.Niente;

                    gameCell.GenerateWalls();
                    gameCell.SetSimpleAngles();
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
                        // É un altro check rispetto al *passable per evitare di dover disegnare il doppio muro nell'editor
                        var northCell = grid.GetCellByIndex(i, j - 1);
                        if (northCell.IsUnwalkable || !northCell.IsSouthPassable)
                        {
                            cell.AddLink(null, LinkDirection.NorthLink);
                            cell.IsNorthPassable = false;
                        }
                        else
                        {
                            cell.AddLink(northCell, LinkDirection.NorthLink);
                        }
                    }
                    else
                        cell.AddLink(null, LinkDirection.NorthLink);

                    if (cell.IsEastPassable)
                    {
                        // É un altro check rispetto al *passable per evitare di dover disegnare il doppio muro nell'editor
                        var eastCell = grid.GetCellByIndex(i + 1, j);
                        if (eastCell.IsUnwalkable || !eastCell.IsWestPassable)
                        {
                            cell.AddLink(null, LinkDirection.EastLink);
                            cell.IsEastPassable = false;
                        }
                        else
                        {
                            cell.AddLink(eastCell, LinkDirection.EastLink);
                        }
                    }
                    else
                        cell.AddLink(null, LinkDirection.EastLink);

                    if (cell.IsSouthPassable)
                    {
                        var southCell = grid.GetCellByIndex(i, j + 1);
                        if (southCell.IsUnwalkable || !southCell.IsNorthPassable)
                        {
                            cell.AddLink(null, LinkDirection.SouthLink);
                            cell.IsSouthPassable = false;
                        }
                        else
                        {
                            cell.AddLink(southCell, LinkDirection.SouthLink);
                        }
                    }
                    else
                        cell.AddLink(null, LinkDirection.SouthLink);

                    if (cell.IsWestPassable)
                    {
                        var westCell = grid.GetCellByIndex(i - 1, j);
                        if (westCell.IsUnwalkable || !westCell.IsEastPassable)
                        {
                            cell.AddLink(null, LinkDirection.WestLink);
                            cell.IsWestPassable = false;
                        }
                        else
                        {
                            cell.AddLink(westCell, LinkDirection.WestLink);
                        }
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