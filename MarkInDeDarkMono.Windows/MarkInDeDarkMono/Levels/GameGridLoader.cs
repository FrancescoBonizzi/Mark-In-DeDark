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
            GameGridBundle levelEditorGrid,
            GameGrid grid,
            Level level)
        {
            var skeletronsPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            var zombiesPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            var taurusPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            var ghostPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
            var bubuPaths = new Dictionary<int, List<KeyValuePair<int, GameCell>>>();
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
                        case GroundTypes.NonCella:
                            grid.SetNullCell(i, j);
                            gameCell.NoDecals = true;
                            continue;

                        case GroundTypes.Buio:
                            gameCell.Sprite = level.DarkSprite;
                            gameCell.IsUnwalkable = true;
                            gameCell.NoDecals = true;
                            break;

                        case GroundTypes.Macerie:
                            gameCell.Sprite = level.MacerieCellSprite;
                            gameCell.IsUnwalkable = true;
                            gameCell.NoDecals = true;
                            break;

                        case GroundTypes.Terra:
                            gameCell.Sprite = level.GroundSpriteType1;
                            break;

                        case GroundTypes.Terra2:
                            if (level.GroundSpriteType2 != null)
                                gameCell.Sprite = level.GroundSpriteType2;
                            else gameCell.Sprite = level.GroundSpriteType1;
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

                            case ObjectTypes.Ghost:

                                {
                                    // Ghost
                                    // Essendo un patrol, avrò il suo percorso nel formato G-1-1 ecc
                                    var patrolPart = levelEditorCell.ObjectTypeAdditionalData.Split('-');
                                    var patrolNumber = Convert.ToInt32(patrolPart[1]);
                                    var patrolWaypointNumber = Convert.ToInt32(patrolPart[2]);
                                    var cellWithOrder = new KeyValuePair<int, GameCell>(patrolWaypointNumber, gameCell);

                                    // Ogni riga di _zombies_path è uno zombie
                                    if (ghostPaths.ContainsKey(patrolNumber))
                                    {
                                        ghostPaths[patrolNumber].Add(cellWithOrder);
                                    }
                                    else
                                    {
                                        ghostPaths.Add(
                                            patrolNumber,
                                            new List<KeyValuePair<int, GameCell>>() { cellWithOrder });
                                    }
                                }
                                break;

                            case ObjectTypes.Bubu:

                                {
                                    // Bubu
                                    // Essendo un patrol, avrò il suo percorso nel formato B-1-1 ecc
                                    var patrolPart = levelEditorCell.ObjectTypeAdditionalData.Split('-');
                                    var patrolNumber = Convert.ToInt32(patrolPart[1]);
                                    var patrolWaypointNumber = Convert.ToInt32(patrolPart[2]);
                                    var cellWithOrder = new KeyValuePair<int, GameCell>(patrolWaypointNumber, gameCell);

                                    // Ogni riga di _zombies_path è uno zombie
                                    if (bubuPaths.ContainsKey(patrolNumber))
                                    {
                                        bubuPaths[patrolNumber].Add(cellWithOrder);
                                    }
                                    else
                                    {
                                        bubuPaths.Add(
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
                    gameCell.IsNorthPassable = levelEditorCell.Top == WallTypes.Niente || levelEditorCell.Top == WallTypes.Porta;
                    gameCell.IsNorthDoor = levelEditorCell.Top == WallTypes.Porta;

                    gameCell.IsEastDestroyable = levelEditorCell.Right == WallTypes.Fragile;
                    gameCell.IsEastPassable = levelEditorCell.Right == WallTypes.Niente || levelEditorCell.Right == WallTypes.Porta;
                    gameCell.IsEastDoor = levelEditorCell.Right == WallTypes.Porta;

                    gameCell.IsSouthDestroyable = levelEditorCell.Bottom == WallTypes.Fragile;
                    gameCell.IsSouthPassable = levelEditorCell.Bottom == WallTypes.Niente || levelEditorCell.Bottom == WallTypes.Porta;
                    gameCell.IsSouthDoor = levelEditorCell.Bottom == WallTypes.Porta;

                    gameCell.IsWestDestroyable = levelEditorCell.Left == WallTypes.Fragile;
                    gameCell.IsWestPassable = levelEditorCell.Left == WallTypes.Niente || levelEditorCell.Left == WallTypes.Porta;
                    gameCell.IsWestDoor = levelEditorCell.Left == WallTypes.Porta;

                    gameCell.GenerateWalls();
                    gameCell.GenerateDoors();
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

            grid.GhostsPath = new List<List<GameCell>>();
            foreach (var path in ghostPaths)
            {
                grid.GhostsPath.Add(
                    path.Value
                    .OrderBy(p => p.Key)
                    .Select(p => p.Value)
                    .ToList());
            }

            grid.BubusPath = new List<List<GameCell>>();
            foreach (var path in bubuPaths)
            {
                grid.BubusPath.Add(
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
            GenerateAngles(grid);
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
                        if (northCell.IsUnwalkable || !northCell.IsSouthPassable || !northCell.IsSouthDoorPassable)
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

                    // Per ovviare al problema che ogni cella può avere un muro rompibile e si possono sovrapporre
                    if (cell.IsNorthDestroyable)
                    {
                        var northCell = grid.GetCellByIndex(i, j - 1);
                        northCell.IsSouthDestroyable = true;
                    }

                    if (cell.IsEastPassable)
                    {
                        // É un altro check rispetto al *passable per evitare di dover disegnare il doppio muro nell'editor
                        var eastCell = grid.GetCellByIndex(i + 1, j);
                        if (eastCell.IsUnwalkable || !eastCell.IsWestPassable || !eastCell.IsWestDoorPassable)
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

                    // Per ovviare al problema che ogni cella può avere un muro rompibile e si possono sovrapporre
                    if (cell.IsEastDestroyable)
                    {
                        var eastCell = grid.GetCellByIndex(i + 1, j);
                        eastCell.IsWestDestroyable = true;
                    }

                    if (cell.IsSouthPassable)
                    {
                        var southCell = grid.GetCellByIndex(i, j + 1);
                        if (southCell.IsUnwalkable || !southCell.IsNorthPassable || !southCell.IsNorthDoorPassable)
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

                    // Per ovviare al problema che ogni cella può avere un muro rompibile e si possono sovrapporre
                    if (cell.IsSouthDestroyable)
                    {
                        var southCell = grid.GetCellByIndex(i, j + 1);
                        southCell.IsNorthDestroyable = true;
                    }

                    if (cell.IsWestPassable)
                    {
                        var westCell = grid.GetCellByIndex(i - 1, j);
                        if (westCell.IsUnwalkable || !westCell.IsEastPassable || !westCell.IsEastDoorPassable)
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

                    // Per ovviare al problema che ogni cella può avere un muro rompibile e si possono sovrapporre
                    if (cell.IsWestDestroyable)
                    {
                        var westCell = grid.GetCellByIndex(i - 1, j);
                        westCell.IsEastDestroyable = true;
                    }
                }
            }
        }

        private static void GenerateAngles(GameGrid grid)
        {
            for (int i = 0; i < GameGrid.HorizontalCellsNumber; ++i)
            {
                for (int j = 0; j < GameGrid.VerticalCellsNumber; ++j)
                {
                    var cell = grid.GetCellByIndex(i, j);

                    if (cell == null || cell.IsUnwalkable)
                        continue;

                    cell.SetSimpleAngles();

                    // Aggiungo altri muretti. Posso farlo solo qua perchè prima non ho i link alle celle intorno!

                    if (!cell.IsEastPassable)
                        if (cell.Links[LinkDirection.NorthLink] != null)
                            if (cell.Links[LinkDirection.NorthLink].Links[LinkDirection.EastLink] != null)
                                if (!cell.Links[LinkDirection.NorthLink].Links[LinkDirection.EastLink].IsSouthPassable)
                                    cell.AddNEAngle();
               
                    if (!cell.IsWestPassable)
                        if (cell.Links[LinkDirection.NorthLink] != null)
                            if (cell.Links[LinkDirection.NorthLink].Links[LinkDirection.WestLink] != null)
                                if (!cell.Links[LinkDirection.NorthLink].Links[LinkDirection.WestLink].IsSouthPassable)
                                    cell.AddNWAngle();

                    if (cell.Links[LinkDirection.NorthLink] != null)
                        if (!cell.Links[LinkDirection.NorthLink].IsEastPassable)
                            if (cell.Links[LinkDirection.EastLink] != null)
                                if (!cell.Links[LinkDirection.EastLink].IsNorthPassable)
                                    cell.AddNEAngle();

                    if (cell.Links[LinkDirection.NorthLink] != null)
                        if (!cell.Links[LinkDirection.NorthLink].IsWestPassable)
                            if (cell.Links[LinkDirection.WestLink] != null)
                                if (!cell.Links[LinkDirection.WestLink].IsNorthPassable)
                                    cell.AddNWAngle();

                    if (!cell.IsNorthPassable)
                        if (cell.Links[LinkDirection.EastLink] != null)
                            if (cell.Links[LinkDirection.EastLink].IsNorthPassable || cell.Links[LinkDirection.EastLink].IsNorthDestroyable)
                                if (!cell.IsNorthDestroyable || !cell.Links[LinkDirection.EastLink].IsNorthDestroyable)
                                    cell.AddNEAngle();

                    if (!cell.IsNorthPassable)
                        if (cell.Links[LinkDirection.WestLink] != null)
                            if (cell.Links[LinkDirection.WestLink].IsNorthPassable || cell.Links[LinkDirection.WestLink].IsNorthDestroyable)
                                if (!cell.IsNorthDestroyable || !cell.Links[LinkDirection.WestLink].IsNorthDestroyable)
                                    cell.AddNWAngle();

                    if (!cell.IsEastPassable)
                        if (cell.Links[LinkDirection.SouthLink] != null)
                            if (cell.Links[LinkDirection.SouthLink].IsEastPassable || cell.Links[LinkDirection.SouthLink].IsEastDestroyable)
                                if (!cell.IsEastDestroyable || !cell.Links[LinkDirection.SouthLink].IsEastDestroyable)
                                    cell.AddSEAngle();

                    if (!cell.IsEastPassable)
                        if (cell.Links[LinkDirection.NorthLink] != null)
                            if (cell.Links[LinkDirection.NorthLink].IsEastPassable || cell.Links[LinkDirection.NorthLink].IsEastDestroyable)
                                if (!cell.IsEastDestroyable || !cell.Links[LinkDirection.NorthLink].IsEastDestroyable)
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
                                            if (cellAngle.Position == nearCell.AnglesDrawingInfos[w].Position)
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