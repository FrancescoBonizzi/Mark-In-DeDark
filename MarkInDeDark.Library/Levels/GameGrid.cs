using FbonizziMonoGame.Drawing;
using MarkInDeDark.Library.AI;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.Levels
{
    public class GameGrid
    {
        private readonly Camera2D _camera;

        private Vector2 _position;
        public const int HorizontalCellsNumber = 45;
        public const int VerticalCellsNumber = 25;

        private readonly GameCell[,] _cells;

        public int CellWidth { get; }
        public int CellHeight { get; }

        public Vector2 PlayerStartingPosition { get; set; }
        public Vector2 ExitPosition { get; set; }
        public Vector2 KeyPosition { get; set; }
        public List<CandlePack> CandlePacks { get; } = new List<CandlePack>();
        public List<LittleLight> LittleLights { get; } = new List<LittleLight>();

        public Vector2? GoblinStartPosition { get; set; }
        public Vector2? WerewolfStartingPosition { get; set; }
        public List<List<GameCell>> ZombiesPath { get; set; }
        public List<List<GameCell>> SkeletronsPath { get; set; }
        public List<List<GameCell>> TaurusPath { get; set; }
        public List<List<GameCell>> GhostsPath { get; set; }
        public List<List<GameCell>> BubusPath { get; set; }
        public List<Rectangle> InstantlyDieTrapCollisionRectangles { get; set; }
        public List<Rectangle> BrekableTrapCollisionRectangles { get; set; }
        public List<KeyValuePair<Rectangle, string>> CellTriggerText { get; set; }

        public Rectangle? GoblinTriggerRectangle { get; set; }

        private int _currentViewportMinCellX = 0;
        private int _currentViewportMaxCellX = 0;
        private int _currentViewportMinCellY = 0;
        private int _currentViewportMaxCellY = 0;

        private readonly Dictionary<KeyValuePair<Vector2, Vector2>, List<Vector2>> _calculatedPathsOfVectorsCache;
        private readonly Dictionary<KeyValuePair<Vector2, Vector2>, List<CharacterActions>> _calculatedPathsOfActionsCache;
        private readonly MarkInDeDarkGame _game;

        public const float GridMaxHeight = VerticalCellsNumber * 192;

        public GameGrid(
            Level levelToLoad,
            AssetsLoader assetsLoader,
            Camera2D camera,
            SoundManager soundManager,
            GameConfig gameConfig,
            MarkInDeDarkGame game)
        {
            _position = Vector2.Zero;
            _camera = camera;

            CellWidth = levelToLoad.GroundSpriteType1.Width;
            CellHeight = levelToLoad.GroundSpriteType1.Height;

            _cells = new GameCell[HorizontalCellsNumber, VerticalCellsNumber];

            var nextCellPosition = _position;

            for (int i = 0; i < HorizontalCellsNumber; ++i)
            {
                for (int j = 0; j < VerticalCellsNumber; ++j)
                {
                    _cells[i, j] = new GameCell(
                        position: nextCellPosition,
                        assetsLoader: assetsLoader,
                        width: levelToLoad.GroundSpriteType1.Width,
                        height: levelToLoad.GroundSpriteType1NormalMap.Width,
                        dirtySprite: levelToLoad.DirtySprite,
                        dirtyProbability: levelToLoad.DirtyProbability,
                        soundManager: soundManager,
                        iIndex: i, jIndex: j,
                        gameGrid: this,
                        game: game);

                    nextCellPosition.Y += CellHeight;
                }
                nextCellPosition.X += CellWidth;
                nextCellPosition.Y = _position.Y;
            }

            _calculatedPathsOfVectorsCache = new Dictionary<KeyValuePair<Vector2, Vector2>, List<Vector2>>();
            _calculatedPathsOfActionsCache = new Dictionary<KeyValuePair<Vector2, Vector2>, List<CharacterActions>>();

            GameGridLoader.LoadGrid(
                assetsLoader,
                soundManager,
                levelToLoad.LevelEditorDeserializedGrid,
                this,
                levelToLoad,
                gameConfig);

            _game = game;
        }

        public void SetNullCell(int cellX, int cellY)
            => _cells[cellX, cellY] = null;

        public void BreakWall(
            GameCell cell,
            CharacterActions direction,
            Vector2 breakerPosition)
        {
            var thisCellIndexes = GetCellIndexByPixel(cell.TopLeftPosition);
            Vector2 afterTheWallCellIndexes;
            GameCell cellAfterWall;

            bool effectivelyBrokeTheWall = false;

            switch (direction)
            {
                case CharacterActions.Up:

                    if (!cell.IsNorthDestroyable)
                        break;

                    afterTheWallCellIndexes = new Vector2(thisCellIndexes.X, thisCellIndexes.Y - 1);
                    cellAfterWall = GetCellByIndex((int)afterTheWallCellIndexes.X, (int)afterTheWallCellIndexes.Y);

                    if (cellAfterWall != null && !cellAfterWall.IsUnLinked && !cellAfterWall.IsUnwalkable)
                    {
                        effectivelyBrokeTheWall = effectivelyBrokeTheWall || cell.BreakWall(LinkDirection.NorthLink, breakerPosition);
                        effectivelyBrokeTheWall = effectivelyBrokeTheWall || cellAfterWall.BreakWall(LinkDirection.SouthLink, breakerPosition);
                        cell.AddLink(cellAfterWall, LinkDirection.NorthLink);
                        cellAfterWall.AddLink(cell, LinkDirection.SouthLink);
                        cell.IsNorthPassable = true;
                        cellAfterWall.IsSouthPassable = true;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }

                    break;

                case CharacterActions.Down:

                    if (!cell.IsSouthDestroyable)
                        break;

                    afterTheWallCellIndexes = new Vector2(thisCellIndexes.X, thisCellIndexes.Y + 1);
                    cellAfterWall = GetCellByIndex((int)afterTheWallCellIndexes.X, (int)afterTheWallCellIndexes.Y);

                    if (cellAfterWall != null && !cellAfterWall.IsUnLinked && !cellAfterWall.IsUnwalkable)
                    {
                        effectivelyBrokeTheWall = effectivelyBrokeTheWall || cell.BreakWall(LinkDirection.SouthLink, breakerPosition);
                        effectivelyBrokeTheWall = effectivelyBrokeTheWall || cellAfterWall.BreakWall(LinkDirection.NorthLink, breakerPosition);
                        cell.AddLink(cellAfterWall, LinkDirection.SouthLink);
                        cellAfterWall.AddLink(cell, LinkDirection.NorthLink);
                        cell.IsSouthPassable = true;
                        cellAfterWall.IsNorthPassable = true;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }
                    break;

                case CharacterActions.Right:

                    if (!cell.IsEastDestroyable)
                        break;

                    afterTheWallCellIndexes = new Vector2(thisCellIndexes.X + 1, thisCellIndexes.Y);
                    cellAfterWall = GetCellByIndex((int)afterTheWallCellIndexes.X, (int)afterTheWallCellIndexes.Y);

                    if (cellAfterWall != null && !cellAfterWall.IsUnLinked && !cellAfterWall.IsUnwalkable)
                    {
                        effectivelyBrokeTheWall = effectivelyBrokeTheWall || cell.BreakWall(LinkDirection.EastLink, breakerPosition);
                        effectivelyBrokeTheWall = effectivelyBrokeTheWall || cellAfterWall.BreakWall(LinkDirection.WestLink, breakerPosition);
                        cell.AddLink(cellAfterWall, LinkDirection.EastLink);
                        cellAfterWall.AddLink(cell, LinkDirection.WestLink);
                        cell.IsEastPassable = true;
                        cellAfterWall.IsWestPassable = true;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }
                    break;

                case CharacterActions.Left:

                    if (!cell.IsWestDestroyable)
                        break;

                    afterTheWallCellIndexes = new Vector2(thisCellIndexes.X - 1, thisCellIndexes.Y);
                    cellAfterWall = GetCellByIndex((int)afterTheWallCellIndexes.X, (int)afterTheWallCellIndexes.Y);

                    if (cellAfterWall != null && !cellAfterWall.IsUnLinked && !cellAfterWall.IsUnwalkable)
                    {
                        effectivelyBrokeTheWall = effectivelyBrokeTheWall || cell.BreakWall(LinkDirection.WestLink, breakerPosition);
                        effectivelyBrokeTheWall = effectivelyBrokeTheWall || cellAfterWall.BreakWall(LinkDirection.EastLink, breakerPosition); // Potrebbe essere il muro dell'altra cella
                        cell.AddLink(cellAfterWall, LinkDirection.WestLink);
                        cellAfterWall.AddLink(cell, LinkDirection.EastLink);
                        cell.IsWestPassable = true;
                        cellAfterWall.IsEastPassable = true;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }
                    break;
            }

            if (effectivelyBrokeTheWall)
                _game.StartCameraShake(15);
            else _game.StartCameraShake(5);
        }

        public void SwitchDoorIfAny(GameCell cell)
        {
            var doorsToExamine = new Dictionary<LinkDirection, Door>(cell.Doors);

            var thisCellIndex = GetCellIndexByPixel(cell.CenterPosition);
            int i = (int)thisCellIndex.X;
            int j = (int)thisCellIndex.Y;

            // Devo usare questi metodi perché quelli della cella utilizzano i link,
            // ma se la porta è chiusa non ci sono link
            var northCell = GetCellByIndex(i, j - 1);
            var southCell = GetCellByIndex(i, j + 1);
            var eastCell = GetCellByIndex(i + 1, j);
            var westCell = GetCellByIndex(i - 1, j);

            // Per gestire il fatto che io possa aprire una porta che è a cavallo tra due celle
            if (northCell != null && northCell.Doors.ContainsKey(LinkDirection.SouthLink))
                doorsToExamine.Add(LinkDirection.NorthLink, northCell.Doors[LinkDirection.SouthLink]);
            if (southCell != null && southCell.Doors.ContainsKey(LinkDirection.NorthLink))
                doorsToExamine.Add(LinkDirection.SouthLink, southCell.Doors[LinkDirection.NorthLink]);
            if (eastCell != null && eastCell.Doors.ContainsKey(LinkDirection.WestLink))
                doorsToExamine.Add(LinkDirection.EastLink, eastCell.Doors[LinkDirection.WestLink]);
            if (westCell != null && westCell.Doors.ContainsKey(LinkDirection.EastLink))
                doorsToExamine.Add(LinkDirection.WestLink, westCell.Doors[LinkDirection.EastLink]);

            foreach (var door in doorsToExamine)
            {
                if (door.Value.IsMoving)
                    continue;

                if (door.Value.IsOpen)
                {
                    CloseDoor(cell, door.Value, door.Key);
                }
                else
                {
                    OpenDoor(cell, door.Value, door.Key);
                }
            }
        }

        public void OpenDoor(GameCell cell, Door door, LinkDirection direction)
        {
            var thisCellIndexes = GetCellIndexByPixel(cell.TopLeftPosition);
            Vector2 afterTheDoorCellIndexes;
            GameCell cellAfterDoor;

            switch (direction)
            {
                case LinkDirection.NorthLink:

                    afterTheDoorCellIndexes = new Vector2(thisCellIndexes.X, thisCellIndexes.Y - 1);
                    cellAfterDoor = GetCellByIndex((int)afterTheDoorCellIndexes.X, (int)afterTheDoorCellIndexes.Y);

                    if (cellAfterDoor != null && !cellAfterDoor.IsUnLinked && !cellAfterDoor.IsUnwalkable)
                    {
                        door.Open();
                        cell.AddLink(cellAfterDoor, LinkDirection.NorthLink);
                        cellAfterDoor.AddLink(cell, LinkDirection.SouthLink);
                        cell.IsNorthPassable = true;
                        cellAfterDoor.IsSouthPassable = true;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }

                    break;

                case LinkDirection.SouthLink:

                    afterTheDoorCellIndexes = new Vector2(thisCellIndexes.X, thisCellIndexes.Y + 1);
                    cellAfterDoor = GetCellByIndex((int)afterTheDoorCellIndexes.X, (int)afterTheDoorCellIndexes.Y);

                    if (cellAfterDoor != null && !cellAfterDoor.IsUnLinked && !cellAfterDoor.IsUnwalkable)
                    {
                        door.Open();
                        cell.AddLink(cellAfterDoor, LinkDirection.SouthLink);
                        cellAfterDoor.AddLink(cell, LinkDirection.NorthLink);
                        cell.IsSouthPassable = true;
                        cellAfterDoor.IsNorthPassable = true;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }
                    break;

                case LinkDirection.EastLink:

                    afterTheDoorCellIndexes = new Vector2(thisCellIndexes.X + 1, thisCellIndexes.Y);
                    cellAfterDoor = GetCellByIndex((int)afterTheDoorCellIndexes.X, (int)afterTheDoorCellIndexes.Y);

                    if (cellAfterDoor != null && !cellAfterDoor.IsUnLinked && !cellAfterDoor.IsUnwalkable)
                    {
                        door.Open();
                        cell.AddLink(cellAfterDoor, LinkDirection.EastLink);
                        cellAfterDoor.AddLink(cell, LinkDirection.WestLink);
                        cell.IsEastPassable = true;
                        cellAfterDoor.IsWestPassable = true;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }
                    break;

                case LinkDirection.WestLink:

                    afterTheDoorCellIndexes = new Vector2(thisCellIndexes.X - 1, thisCellIndexes.Y);
                    cellAfterDoor = GetCellByIndex((int)afterTheDoorCellIndexes.X, (int)afterTheDoorCellIndexes.Y);

                    if (cellAfterDoor != null && !cellAfterDoor.IsUnLinked && !cellAfterDoor.IsUnwalkable)
                    {
                        door.Open();
                        cell.AddLink(cellAfterDoor, LinkDirection.WestLink);
                        cellAfterDoor.AddLink(cell, LinkDirection.EastLink);
                        cell.IsWestPassable = true;
                        cellAfterDoor.IsEastPassable = true;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }
                    break;
            }
        }

        public void CloseDoor(GameCell cell, Door door, LinkDirection direction)
        {
            var thisCellIndexes = GetCellIndexByPixel(cell.TopLeftPosition);
            Vector2 afterTheDoorCellIndexes;
            GameCell cellAfterDoor;

            switch (direction)
            {
                case LinkDirection.NorthLink:

                    afterTheDoorCellIndexes = new Vector2(thisCellIndexes.X, thisCellIndexes.Y - 1);
                    cellAfterDoor = GetCellByIndex((int)afterTheDoorCellIndexes.X, (int)afterTheDoorCellIndexes.Y);

                    if (cellAfterDoor != null && !cellAfterDoor.IsUnLinked && !cellAfterDoor.IsUnwalkable)
                    {
                        door.Close();
                        cell.Links.Remove(LinkDirection.NorthLink);
                        cellAfterDoor.Links.Remove(LinkDirection.SouthLink);
                        cell.IsNorthPassable = false;
                        cellAfterDoor.IsSouthPassable = false;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }

                    break;

                case LinkDirection.SouthLink:

                    afterTheDoorCellIndexes = new Vector2(thisCellIndexes.X, thisCellIndexes.Y + 1);
                    cellAfterDoor = GetCellByIndex((int)afterTheDoorCellIndexes.X, (int)afterTheDoorCellIndexes.Y);

                    if (cellAfterDoor != null && !cellAfterDoor.IsUnLinked && !cellAfterDoor.IsUnwalkable)
                    {
                        door.Close();
                        cell.Links.Remove(LinkDirection.SouthLink);
                        cellAfterDoor.Links.Remove(LinkDirection.NorthLink);
                        cell.IsSouthPassable = false;
                        cellAfterDoor.IsNorthPassable = false;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }
                    break;

                case LinkDirection.EastLink:

                    afterTheDoorCellIndexes = new Vector2(thisCellIndexes.X + 1, thisCellIndexes.Y);
                    cellAfterDoor = GetCellByIndex((int)afterTheDoorCellIndexes.X, (int)afterTheDoorCellIndexes.Y);

                    if (cellAfterDoor != null && !cellAfterDoor.IsUnLinked && !cellAfterDoor.IsUnwalkable)
                    {
                        door.Close();
                        cell.Links.Remove(LinkDirection.EastLink);
                        cellAfterDoor.Links.Remove(LinkDirection.WestLink);
                        cell.IsEastPassable = false;
                        cellAfterDoor.IsWestPassable = false;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }
                    break;

                case LinkDirection.WestLink:

                    afterTheDoorCellIndexes = new Vector2(thisCellIndexes.X - 1, thisCellIndexes.Y);
                    cellAfterDoor = GetCellByIndex((int)afterTheDoorCellIndexes.X, (int)afterTheDoorCellIndexes.Y);

                    if (cellAfterDoor != null && !cellAfterDoor.IsUnLinked && !cellAfterDoor.IsUnwalkable)
                    {
                        door.Close();
                        cell.Links.Remove(LinkDirection.WestLink);
                        cellAfterDoor.Links.Remove(LinkDirection.EastLink);
                        cell.IsWestPassable = false;
                        cellAfterDoor.IsEastPassable = false;
                        _calculatedPathsOfVectorsCache.Clear();
                        _calculatedPathsOfActionsCache.Clear();
                    }
                    break;
            }
        }

        public Vector2 GetPixelBy_ij(int i, int j)
        {
            return new Vector2(
                i * CellWidth + _position.X + CellWidth / 2,
                j * CellWidth + _position.Y + CellHeight / 2);
        }

        public int GetCellByPixelX(int X)
        {
            X -= (int)_position.X; // Se traslo la griglia
            return X / CellWidth;
        }

        public int GetCellByPixelY(int Y)
        {
            Y -= (int)_position.Y; // Se traslo la griglia
            return Y / CellHeight;
        }

        public Vector2 GetCellIndexByPixel(Vector2 pixelLocation)
        {
            return new Vector2(
                GetCellByPixelX((int)pixelLocation.X),
                GetCellByPixelY((int)pixelLocation.Y));
        }

        public Rectangle CellWorldRectangle(int cellX, int cellY)
        {
            return new Rectangle(
                cellX * CellWidth,
                cellY * CellHeight,
                CellWidth,
                CellHeight);
        }

        public Rectangle CellWorldRectangle(Vector2 cell)
        {
            return CellWorldRectangle(
                (int)cell.X,
                (int)cell.Y);
        }

        public GameCell GetCellByIndex(int cellX, int cellY)
        {
            if ((cellX >= 0) && (cellX < HorizontalCellsNumber) &&
                (cellY >= 0) && (cellY < VerticalCellsNumber))
            {
                return _cells[cellX, cellY];
            }
            else
            {
                return null;
            }
        }

        public GameCell GetCellAtPixel(int pixelX, int pixelY)
        {
            return GetCellByIndex(
                GetCellByPixelX(pixelX),
                GetCellByPixelY(pixelY));
        }

        public GameCell GetCellAtPixel(Vector2 pixelLocation)
        {
            return GetCellAtPixel(
                (int)pixelLocation.X,
                (int)pixelLocation.Y);
        }

        public bool CollideWithAWall(Rectangle collisionRectangle)
        {
            var cell = GetCellAtPixel(collisionRectangle.X, collisionRectangle.Y);
            foreach (var wall in cell.Walls.Values)
                if (wall.CollisionRectangle.Intersects(collisionRectangle))
                    return true;

            return false;
        }

        public bool WillCollideWithSomethingNotPassable(
            Rectangle futureCollisionRectangle,
            Vector2 direction)
        {
            var cell = GetCellAtPixel(futureCollisionRectangle.X, futureCollisionRectangle.Y);
            var nearCells = new List<GameCell>()
            {
                cell,
                cell.NorthCellWithoutLinks,
                cell.SouthCellWithoutLinks,
                cell.EastCellWithoutLinks,
                cell.WestCellWithoutLinks,
                cell.NorthCellWithoutLinks?.EastCellWithoutLinks,
                cell.NorthCellWithoutLinks?.WestCellWithoutLinks,
                cell.SouthCellWithoutLinks?.EastCellWithoutLinks,
                cell.SouthCellWithoutLinks?.WestCellWithoutLinks,
            };

            foreach (var toAnalize in nearCells)
            {
                if (toAnalize == null)
                    continue;

                // Check muri
                foreach (var wall in toAnalize.Walls.Values)
                    if (wall.CollisionRectangle.Intersects(futureCollisionRectangle))
                        return true;

                // Check oggetti non passabili
                foreach (var unpassableObject in toAnalize.UnpassableObjects)
                    if (unpassableObject.CollisionRectangle.Intersects(futureCollisionRectangle))
                        return true;

                // Check pilastri
                foreach (var pillar in toAnalize.Pillars.Values)
                    if (pillar.CollisionRectangle.Intersects(futureCollisionRectangle))
                        return true;

                // Check porte
                foreach (var door in toAnalize.Doors.Values)
                {
                    if (!door.IsOpen)
                    {
                        if (door.CollisionRectangle.Intersects(futureCollisionRectangle))
                        {
                            // Vado a sbattere contro una porta chiusa...

                            if (direction.Y < 0)
                            {
                                // ...se sto andando in su e la porta è sopra di me, mi blocco
                                if (door.CollisionRectangle.Y <= futureCollisionRectangle.Y)
                                    return true;
                            }
                            else if (direction.Y > 0)
                            {
                                // ...se sto andando in giù e la porta è sotto di me, mi blocco
                                if (door.CollisionRectangle.Y >= futureCollisionRectangle.Y)
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        // E' un BFS greedy
        private List<GameCell> FindPathOfCells(GameCell A, GameCell B)
        {
            var parent = new Dictionary<GameCell, GameCell>();

            Queue<GameCell> queue = new Queue<GameCell>();
            List<GameCell> visited = new List<GameCell>();

            queue.Enqueue(A);
            parent.Add(A, null);

            while (queue.Count != 0)
            {
                GameCell c = queue.Dequeue();

                visited.Add(c);

                if (c == B)
                    break;

                foreach (var near in c.Links.Select(a => a.Value))
                {
                    if (near != null
                        && !near.IsUnwalkableForEnemies
                        && !near.IsUnwalkable)
                    {
                        if (!visited.Contains(near))
                        {
                            parent.Add(near, c);
                            visited.Add(near);
                            queue.Enqueue(near);
                        }
                    }
                }
            }

            var path = new List<GameCell>();

            if (parent.ContainsKey(B))
            {
                GameCell backTrack = B;
                do
                {
                    path.Add(backTrack);
                    backTrack = parent[backTrack];
                }
                while (backTrack != null);
                path.Reverse();
            }

            return path;
        }

        private List<Vector2> FindPathOfVectors(GameCell A, GameCell B)
        {
            var parent = new Dictionary<GameCell, GameCell>();

            Queue<GameCell> queue = new Queue<GameCell>();
            List<GameCell> visited = new List<GameCell>();

            queue.Enqueue(A);
            parent.Add(A, null);

            while (queue.Count != 0)
            {
                GameCell c = queue.Dequeue();

                visited.Add(c);

                if (c == B)
                    break;

                foreach (var near in c.Links.Select(a => a.Value))
                {
                    if (near != null
                        && !near.IsUnwalkableForEnemies
                        && !near.IsUnwalkable)
                    {
                        if (!visited.Contains(near))
                        {
                            parent.Add(near, c);
                            visited.Add(near);
                            queue.Enqueue(near);
                        }
                    }
                }
            }

            var path = new List<Vector2>();

            if (parent.ContainsKey(B))
            {
                GameCell backTrack = B;
                do
                {
                    path.Add(backTrack.CenterPosition);
                    backTrack = parent[backTrack];
                }
                while (backTrack != null);
                path.Reverse();
            }

            return path;
        }

        public List<Vector2> FindPathOfVectorsWithoutConsideringWalls(Vector2 A, Vector2 B)
        {
            return FindPathOfVectorsWithoutConsideringWalls(
                GetCellAtPixel((int)A.X, (int)A.Y),
                GetCellAtPixel((int)B.X, (int)B.Y));
        }

        public List<Vector2> FindPathOfVectorsWithoutConsideringWalls(GameCell A, GameCell B)
        {
            var parent = new Dictionary<GameCell, GameCell>();

            Queue<GameCell> queue = new Queue<GameCell>();
            List<GameCell> visited = new List<GameCell>();

            queue.Enqueue(A);
            parent.Add(A, null);

            while (queue.Count != 0)
            {
                GameCell c = queue.Dequeue();

                visited.Add(c);

                if (c == B)
                    break;

                // Non considero i muri, quindi prendo direttamente le celle per index
                var northCell = c.NorthCellWithoutLinks;
                var southCell = c.SouthCellWithoutLinks;
                var eastCell = c.EastCellWithoutLinks;
                var westCell = c.WestCellWithoutLinks;

                var nearCells = new List<GameCell>();
                if (northCell != null)
                    nearCells.Add(northCell);
                if (southCell != null)
                    nearCells.Add(southCell);
                if (eastCell != null)
                    nearCells.Add(eastCell);
                if (westCell != null)
                    nearCells.Add(westCell);

                foreach (var near in nearCells)
                {
                    if (near != null)
                    {
                        if (!visited.Contains(near))
                        {
                            parent.Add(near, c);
                            visited.Add(near);
                            queue.Enqueue(near);
                        }
                    }
                }
            }

            var path = new List<Vector2>();

            if (parent.ContainsKey(B))
            {
                GameCell backTrack = B;
                do
                {
                    path.Add(backTrack.CenterPosition);
                    backTrack = parent[backTrack];
                }
                while (backTrack != null);
                path.Reverse();
            }

            return path;
        }

        private List<CharacterActions> PathToDirections(List<GameCell> Path)
        {
            var directions = new List<CharacterActions>();
            for (int i = 0; i < Path.Count - 1; ++i)
            {
                if (Path[i].NorthCell == Path[i + 1])
                {
                    directions.Add(CharacterActions.Up);
                }
                else if (Path[i].EastCell == Path[i + 1])
                {
                    directions.Add(CharacterActions.Right);
                }
                else if (Path[i].SouthCell == Path[i + 1])
                {
                    directions.Add(CharacterActions.Down);
                }
                else if (Path[i].WestCell == Path[i + 1])
                {
                    directions.Add(CharacterActions.Left);
                }
            }

            return directions;
        }

        public List<Vector2> FindPathOfVectors(Vector2 A, Vector2 B)
        {
            var pathKey = new KeyValuePair<Vector2, Vector2>(A, B);
            if (_calculatedPathsOfVectorsCache.ContainsKey(pathKey))
                return _calculatedPathsOfVectorsCache[pathKey];

            var foundPath = FindPathOfVectors(
                GetCellAtPixel((int)A.X, (int)A.Y),
                GetCellAtPixel((int)B.X, (int)B.Y));

            _calculatedPathsOfVectorsCache.Add(pathKey, foundPath);
            return foundPath;
        }

        public List<CharacterActions> FindPathOfActions(Vector2 A, Vector2 B)
        {
            var pathKey = new KeyValuePair<Vector2, Vector2>(A, B);

            if (_calculatedPathsOfActionsCache.ContainsKey(pathKey))
                return _calculatedPathsOfActionsCache[pathKey];

            var pathToActions = PathToDirections(FindPathOfCells(
                GetCellAtPixel((int)A.X, (int)A.Y),
                GetCellAtPixel((int)B.X, (int)B.Y)));

            _calculatedPathsOfActionsCache.Add(pathKey, pathToActions);
            return pathToActions;
        }

        public void ApplyBloodDecal(Vector2 where)
        {
            var cell = GetCellAtPixel(where);

            cell.ApplyBloodDecal(CellRelativePosition.Center);

            // Spargo il sangue anche sulle celle adiacenti

            // Su
            if (cell.NorthCell != null)
                cell.NorthCell.ApplyBloodDecal(CellRelativePosition.Up);

            // Su-Destra
            if (cell.NorthCell != null)
                if (cell.NorthCell.EastCell != null)
                    cell.NorthCell.EastCell.ApplyBloodDecal(CellRelativePosition.UpRight);

            // Destra
            if (cell.EastCell != null)
                cell.EastCell.ApplyBloodDecal(CellRelativePosition.Right);

            // Destra-Giu
            if (cell.EastCell != null)
                if (cell.EastCell.SouthCell != null)
                    cell.EastCell.SouthCell.ApplyBloodDecal(CellRelativePosition.DownRight);

            // Sotto
            if (cell.SouthCell != null)
                cell.SouthCell.ApplyBloodDecal(CellRelativePosition.Down);

            // Sotto-Sinistra
            if (cell.SouthCell != null)
                if (cell.SouthCell.WestCell != null)
                    cell.SouthCell.WestCell.ApplyBloodDecal(CellRelativePosition.DownLeft);

            // Sinistra
            if (cell.WestCell != null)
                cell.WestCell.ApplyBloodDecal(CellRelativePosition.Left);

            // Sinistra-Su
            if (cell.WestCell != null)
                if (cell.WestCell.NorthCell != null)
                    cell.WestCell.NorthCell.ApplyBloodDecal(CellRelativePosition.UpLeft);
        }

        public void Update(TimeSpan elapsed)
        {
            // Mi segno le celle che l'utente vede, per Update e Draw
            var cameraBoundingRectangle = _camera.BoundingRectangle;
            _currentViewportMinCellX = GetCellByPixelX((int)(_camera.Position.X - _camera.Position.X * _camera.Zoom));
            _currentViewportMaxCellX = GetCellByPixelX((int)(cameraBoundingRectangle.Right + cameraBoundingRectangle.Right * _camera.Zoom));
            _currentViewportMinCellY = GetCellByPixelY((int)(_camera.Position.Y - _camera.Position.Y * _camera.Zoom));
            _currentViewportMaxCellY = GetCellByPixelY((int)(cameraBoundingRectangle.Bottom + cameraBoundingRectangle.Bottom * _camera.Zoom));

            DoOverAllCells(cell => cell.Update(elapsed));
        }

        public void Draw(SpriteBatch spriteBatch)
            => DoOverAllCells(cell => cell.Draw(spriteBatch));

        public void DrawNormalMap(SpriteBatch spriteBatch)
            => DoOverAllCells(cell => cell.DrawNormalMap(spriteBatch));

        public void DrawForDebug(SpriteBatch spriteBatch)
            => DoOverAllCells(cell => cell.DrawForDebug(spriteBatch));

        private void DoOverAllCells(Action<GameCell> action)
        {
            for (int x = _currentViewportMinCellX; x <= _currentViewportMaxCellX; x++)
            {
                for (int y = _currentViewportMinCellY; y <= _currentViewportMaxCellY; y++)
                {
                    if ((x >= 0) && (y >= 0) && (x < HorizontalCellsNumber) && (y < VerticalCellsNumber))
                    {
                        if (_cells[x, y] != null)
                            action(_cells[x, y]);
                    }
                }
            }
        }
    }
}
