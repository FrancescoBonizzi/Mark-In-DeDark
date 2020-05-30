using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Lighting;
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
        private GameGridLuminator _gameGridLuminator;

        public int CellWidth { get; }
        public int CellHeight { get; }

        public Vector2 PlayerStartingPosition { get; set; }
        public Vector2 ExitPosition { get; set; }
        public Vector2 KeyPosition { get; set; }
        public List<CandlePack> CandlePacks { get; } = new List<CandlePack>();
        public List<LittleLight> LittleLights { get; } = new List<LittleLight>();
        public List<IDrawableObject> DrawableObjects { get; } = new List<IDrawableObject>();

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

        public WallEsteticDoor EntranceDoor { get; set; }
        public WallEsteticDoor ExitDoor { get; set; }

        public Rectangle? GoblinTriggerRectangle { get; set; }

        private int _currentViewportMinCellX = 0;
        private int _currentViewportMaxCellX = 0;
        private int _currentViewportMinCellY = 0;
        private int _currentViewportMaxCellY = 0;

        private readonly Dictionary<KeyValuePair<Vector2, Vector2>, List<CharacterActions>> _calculatedPathsCache;
        private readonly MarkInDeDarkGame _game;

        public const float GridMaxHeight = VerticalCellsNumber * 192;

        public List<IWall> AllWalls { get; }
        public List<Pillar> AllPillars { get; }

        public GameGrid(
            Level levelToLoad,
            AssetsLoader assetsLoader,
            Camera2D camera,
            SoundManager soundManager,
            GameConfig gameConfig,
            MarkInDeDarkGame game)
        {
            AllWalls = new List<IWall>();
            AllPillars = new List<Pillar>();
            _position = Vector2.Zero;
            _camera = camera;

            CellWidth = levelToLoad.GroundSpriteType1.Base.Width;
            CellHeight = levelToLoad.GroundSpriteType1.Base.Height;

            _cells = new GameCell[HorizontalCellsNumber, VerticalCellsNumber];

            var nextCellPosition = _position;
            int currentDirtyIndex = 0;

            for (int i = 0; i < HorizontalCellsNumber; ++i)
            {
                for (int j = 0; j < VerticalCellsNumber; ++j)
                {
                    Sprite thisCellDirtySprite = null;
                    if (levelToLoad.DirtySprites.Count > 0)
                    {
                        if (Numbers.RandomBetween(0D, 1D) <= levelToLoad.DirtyProbability)
                        {
                            thisCellDirtySprite = levelToLoad.DirtySprites[currentDirtyIndex];
                            currentDirtyIndex = (currentDirtyIndex + 1) % levelToLoad.DirtySprites.Count;
                        }
                    }

                    _cells[i, j] = new GameCell(
                        position: nextCellPosition,
                        assetsLoader: assetsLoader,
                        cell: levelToLoad.GroundSpriteType1.Base,
                        dirty: thisCellDirtySprite,
                        normalSprite: levelToLoad.GroundSpriteType1.Normal,
                        specularSprite: levelToLoad.GroundSpriteType1.Specular,
                        soundManager: soundManager,
                        iIndex: i, jIndex: j,
                        gameGrid: this,
                        game: game);

                    nextCellPosition.Y += CellHeight;
                }
                nextCellPosition.X += CellWidth;
                nextCellPosition.Y = _position.Y;
            }

            _calculatedPathsCache = new Dictionary<KeyValuePair<Vector2, Vector2>, List<CharacterActions>>();
            GameGridLoader.LoadGrid(assetsLoader, soundManager, levelToLoad.LevelEditorDeserializedGrid, this, levelToLoad, gameConfig);
            _game = game;
        }

        public void InitializeLuminator(List<MarkLight> lights)
            => _gameGridLuminator = new GameGridLuminator(this, lights);

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
                        _calculatedPathsCache.Clear();
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
                        _calculatedPathsCache.Clear();
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
                        _calculatedPathsCache.Clear();
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
                        _calculatedPathsCache.Clear();
                    }
                    break;
            }

            if (effectivelyBrokeTheWall)
                _game.StartCameraShake(15);
            else _game.StartCameraShake(5);
        }

        public void UpdateAllWalls()
        {
            AllWalls.Clear();

            for (int x = 0; x < HorizontalCellsNumber; x++)
            {
                for (int y = 0; y < VerticalCellsNumber; y++)
                {
                    if (_cells[x, y] != null)
                    {
                        foreach (var w in _cells[x, y].Walls)
                        {
                            AllWalls.Add(w.Value);
                        }
                    }
                }
            }
        }

        public void UpdateAllPillars()
        {
            AllPillars.Clear();

            for (int x = 0; x < HorizontalCellsNumber; x++)
            {
                for (int y = 0; y < VerticalCellsNumber; y++)
                {
                    if (_cells[x, y] != null)
                    {
                        foreach (var p in _cells[x, y].Pillars)
                        {
                            AllPillars.Add(p.Value);
                        }
                    }
                }
            }
        }

        //, LinkDirection watchDirection
        public void SwitchDoorIfAny(GameCell cell)
        {
            var doors = cell.Walls.Where(w => w.Value is WallInteractiveDoor);
            var doorsToExamine = new Dictionary<LinkDirection, WallInteractiveDoor>();
            foreach (var d in doors)
            {
                doorsToExamine.Add(d.Key, d.Value as WallInteractiveDoor);
            }

            var thisCellIndex = GetCellIndexByPixel(cell.CenterPosition);
            int i = (int)thisCellIndex.X;
            int j = (int)thisCellIndex.Y;

            // Devo usare questi metodi perché quelli della cella utilizzano i link,
            // ma se la porta è chiusa non ci sono link
            var northCell = GetCellByIndex(i, j - 1);
            var southCell = GetCellByIndex(i, j + 1);
            var eastCell = GetCellByIndex(i + 1, j);
            var westCell = GetCellByIndex(i - 1, j);

            // TODO ALTERNATIVA: Fare un bounding box largo per la porta e calcolare la collisione
            // (guardando solo alla cella in cui c'è il player e quelle nelle celle intorno)

            // Per gestire il fatto che io possa aprire una porta che è a cavallo tra due celle
            if (!doorsToExamine.ContainsKey(LinkDirection.NorthLink) && northCell != null && northCell.Walls.ContainsKey(LinkDirection.SouthLink) && northCell.Walls[LinkDirection.SouthLink] is WallInteractiveDoor)
                doorsToExamine.Add(LinkDirection.NorthLink, northCell.Walls[LinkDirection.SouthLink] as WallInteractiveDoor);

            if (!doorsToExamine.ContainsKey(LinkDirection.SouthLink) && southCell != null && southCell.Walls.ContainsKey(LinkDirection.NorthLink) && southCell.Walls[LinkDirection.NorthLink] is WallInteractiveDoor)
                doorsToExamine.Add(LinkDirection.SouthLink, southCell.Walls[LinkDirection.NorthLink] as WallInteractiveDoor);

            if (!doorsToExamine.ContainsKey(LinkDirection.EastLink) && eastCell != null && eastCell.Walls.ContainsKey(LinkDirection.WestLink) && eastCell.Walls[LinkDirection.WestLink] is WallInteractiveDoor)
                doorsToExamine.Add(LinkDirection.EastLink, eastCell.Walls[LinkDirection.WestLink] as WallInteractiveDoor);

            if (!doorsToExamine.ContainsKey(LinkDirection.WestLink) && westCell != null && westCell.Walls.ContainsKey(LinkDirection.EastLink) && westCell.Walls[LinkDirection.EastLink] is WallInteractiveDoor)
                doorsToExamine.Add(LinkDirection.WestLink, westCell.Walls[LinkDirection.EastLink] as WallInteractiveDoor);

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

        public void OpenDoor(GameCell cell, WallInteractiveDoor door, LinkDirection direction)
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
                        _calculatedPathsCache.Clear();
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
                        _calculatedPathsCache.Clear();
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
                        _calculatedPathsCache.Clear();
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
                        _calculatedPathsCache.Clear();
                    }
                    break;
            }
        }

        public void CloseDoor(GameCell cell, WallInteractiveDoor door, LinkDirection direction)
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
                        _calculatedPathsCache.Clear();
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
                        _calculatedPathsCache.Clear();
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
                        _calculatedPathsCache.Clear();
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
                        _calculatedPathsCache.Clear();
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

        // E' una BFS greedy
        private List<GameCell> FindPath(GameCell A, GameCell B)
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

        private List<GameCell> FindPathWithoutConsideringWalls(GameCell A, GameCell B)
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

        private IEnumerable<CharacterActions> PathToDirectionsWithoutWalls(List<GameCell> Path)
        {
            var directions = new List<CharacterActions>();

            for (int i = 0; i < Path.Count - 1; ++i)
            {
                if (Path[i].NorthCellWithoutLinks == Path[i + 1])
                {
                    directions.Add(CharacterActions.Up);
                }
                else if (Path[i].EastCellWithoutLinks == Path[i + 1])
                {
                    directions.Add(CharacterActions.Right);
                }
                else if (Path[i].SouthCellWithoutLinks == Path[i + 1])
                {
                    directions.Add(CharacterActions.Down);
                }
                else if (Path[i].WestCellWithoutLinks == Path[i + 1])
                {
                    directions.Add(CharacterActions.Left);
                }
            }

            return directions;
        }

        public IEnumerable<CharacterActions> FindActionsToPathWithoutConsideringWalls(Vector2 A, Vector2 B)
        {
            var startingCell = GetCellAtPixel((int)A.X, (int)A.Y);
            var destinationCell = GetCellAtPixel((int)B.X, (int)B.Y);

            return PathToDirectionsWithoutWalls(FindPathWithoutConsideringWalls(
                startingCell,
                destinationCell));
        }

        public List<CharacterActions> FindActionsToPath(Vector2 A, Vector2 B)
        {
            var pathKey = new KeyValuePair<Vector2, Vector2>(A, B);
            if (_calculatedPathsCache.ContainsKey(pathKey))
                return _calculatedPathsCache[pathKey];

            var cellA = GetCellAtPixel((int)A.X, (int)A.Y);
            var cellB = GetCellAtPixel((int)B.X, (int)B.Y);

            if (cellA == null || cellB == null)
                return new List<CharacterActions>();

            var actionsToPath = PathToDirections(FindPath(cellA, cellB));

            _calculatedPathsCache.Add(pathKey, actionsToPath);
            return actionsToPath;
        }

        public bool CollideWithAWall(Rectangle collisionRectangle)
        {
            var cell = GetCellAtPixel(collisionRectangle.X, collisionRectangle.Y);
            foreach (var wall in cell.Walls.Values)
                if (wall.CollisionRectangle.Intersects(collisionRectangle))
                    return true;

            return false;
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
            _currentViewportMinCellX = GetCellByPixelX((int)(cameraBoundingRectangle.X));
            _currentViewportMaxCellX = GetCellByPixelX((int)(cameraBoundingRectangle.Right));
            _currentViewportMinCellY = GetCellByPixelY((int)(cameraBoundingRectangle.Top));
            _currentViewportMaxCellY = GetCellByPixelY((int)(cameraBoundingRectangle.Bottom)) + 1;

            OnAllVisibleCells(c => c.Update(elapsed));
            _gameGridLuminator.Luminate();
        }

        public void DrawGroundWithNormalMap(
            SpriteBatch spriteBatch,
            Matrix viewMatrix,
            Effect normalMapEffect,
            Effect specularMapEffect,
            GraphicsDevice graphicsDevice,
            RenderTarget2D baseRenderTarget,
            RenderTarget2D normalMapRenderTarget,
            RenderTarget2D specularMapRenderTarget,
            RenderTarget2D specularMapLightmaskRenderTarget,
            RenderTarget2D specularMapWithAppliedLightmaskRenderTarget,
            RenderTarget2D appliedNormalMapRenderTarget,
            RenderTarget2D resultRenderTarget,
            Action<SpriteBatch> drawGlows,
            List<MarkLight> lights)
        {
            // Pavimento di base
            graphicsDevice.SetRenderTarget(baseRenderTarget);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);
            OnAllVisibleCells(c =>
            {
                c.DrawGround(spriteBatch);
            });
            spriteBatch.End();

            // Normal map
            graphicsDevice.SetRenderTarget(normalMapRenderTarget);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);
            OnAllVisibleCells(c =>
            {
                c.DrawGroundNormalMap(spriteBatch);
            });
            spriteBatch.End();

            // Specular map
            graphicsDevice.SetRenderTarget(specularMapRenderTarget);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);
            OnAllVisibleCells(c =>
            {
                c.DrawGroundSpecularMap(spriteBatch);
            });
            spriteBatch.End();

            // Lightmask for the specular map
            graphicsDevice.SetRenderTarget(specularMapLightmaskRenderTarget);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);
            drawGlows(spriteBatch);
            spriteBatch.End();

            // Normal map applicata
            graphicsDevice.SetRenderTarget(appliedNormalMapRenderTarget);
            graphicsDevice.Clear(Color.Black);
            normalMapEffect.Parameters["NormalTexture"].SetValue(normalMapRenderTarget);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, null, null, normalMapEffect);
            foreach (var light in lights)
            {
                if (!light.ShouldDraw)
                    continue;
                normalMapEffect.Parameters["LightPosition"].SetValue(light.PositionForGround);
                normalMapEffect.Parameters["LightColor"].SetValue(light.ColorNormal);
                normalMapEffect.Parameters["LightDistanceSquared"].SetValue(light.DistanceSquared);
                normalMapEffect.Parameters["LightOpacity"].SetValue(light.Opacity);
                spriteBatch.Draw(baseRenderTarget, Vector2.Zero, Color.White);

//#warning Cappato alla sola luce del player la normal del pavimento
//                break;
            }
            spriteBatch.End();

            // Lightmask sulla specular map
            graphicsDevice.SetRenderTarget(specularMapWithAppliedLightmaskRenderTarget);
            graphicsDevice.Clear(Color.Black);
            specularMapEffect.Parameters["SpecularTexture"].SetValue(specularMapLightmaskRenderTarget);
            specularMapEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Begin(effect: specularMapEffect);
            spriteBatch.Draw(specularMapRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Composizione finale
            graphicsDevice.SetRenderTarget(resultRenderTarget);
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(blendState: BlendState.Additive);
            spriteBatch.Draw(appliedNormalMapRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.Draw(specularMapWithAppliedLightmaskRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        public void DrawAllNormalMapsButGround(SpriteBatch spriteBatch)
        {
            OnAllVisibleCells(c => c.DrawNormalMapButGrounds(spriteBatch));
        }

        public void DrawWalls(SpriteBatch spriteBatch)
        {
            OnAllVisibleCells(c => c.DrawWalls(spriteBatch));
        }

        public void DrawForDebug(SpriteBatch spriteBatch)
        {
            OnAllVisibleCells(c => c.DrawForDebug(spriteBatch));
        }

        private void OnAllVisibleCells(Action<GameCell> action)
        {
            for (int x = _currentViewportMinCellX; x <= _currentViewportMaxCellX; x++)
            {
                for (int y = _currentViewportMinCellY; y <= _currentViewportMaxCellY; y++)
                {
                    if ((x >= 0) && (y >= 0) && (x < HorizontalCellsNumber) && (y < VerticalCellsNumber))
                    {
                        if (_cells[x, y] != null)
                        {
                            action(_cells[x, y]);
                        }
                    }
                }
            }
        }

    }
}
