using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.GameGrid;

using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Extensions;
using MarkInDeDarkMono.Maths;
using MarkInDeDarkMono.Objects;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MarkInDeDarkMono.Levels
{
    public class GameCell
    {
        public Sprite Sprite { get; set; }
        private readonly Sprite _horizontalWallSprite;
        private readonly Sprite _verticalWallSprite;
        private readonly Sprite _horizontalDestroyableWallSprite;
        private readonly Sprite _verticalDestroyableWallSprite;
        private readonly Sprite _pillarSprite;
        private readonly Sprite _dirtySprite;
        private readonly Sprite _bloodDecal;
        private readonly Sprite _doorSprite;

        private readonly int _width;
        private readonly int _height;

        private readonly float _widthCellDoorDifferenceOver2;

        private readonly float _angleWidthOver2;
        private readonly float _angleHeightOver2;

        private readonly SoundManager _soundManager;

        private readonly List<DrawingInfos> _dirtysDrawingInfos;
        public Dictionary<PillarPositions, Pillar> Pillars { get; }
        public Dictionary<LinkDirection, Wall> Walls { get; }

        private readonly DrawingInfos _cellDrawingInfos;
        private DrawingInfos _bloodDecalDrawingInfos;
        public Dictionary<LinkDirection, GameCell> Links { get; }
        public Dictionary<LinkDirection, Door> Doors { get; }

        public bool IsNonLinked { get; set; }
        public bool IsUnwalkableForEnemies { get; set; }
        public bool IsUnwalkable { get; set; }
        public bool DontDraw { get; set; }
        public int IIndex { get; }
        public int JIndex { get; }
        public bool NoDecals { get; set; }

        public GroundTypes GroundType { get; set; }

        private readonly BrokenWallSmokeParticleSystem _smokeParticleSystem;
        private readonly GameGrid _gameGrid;
        private readonly MarkInDeDarkGame _game;

        public enum PillarPositions
        {
            NE,
            NW,
            SE,
            SW
        }

        public GameCell(
            Vector2 position,
            AssetsLoader assetsLoader,
            Sprite cell,
            Sprite horizontalWall,
            Sprite verticalWall,
            Sprite horizontalDestroyableWall,
            Sprite verticalDestroyableWall,
            Sprite pillar,
            Sprite dirty,
            Sprite door,
            double dirtyProbability,
            SoundManager soundManager,
            int iIndex,
            int jIndex,
            GameGrid gameGrid,
            MarkInDeDarkGame game)
        {
            _bloodDecal = assetsLoader.Sprites["blood_decal"];
            DontDraw = false;

            IIndex = iIndex;
            JIndex = jIndex;

            Sprite = cell;
            _horizontalWallSprite = horizontalWall;
            _verticalWallSprite = verticalWall;
            _pillarSprite = pillar;
            _dirtySprite = dirty;
            _horizontalDestroyableWallSprite = horizontalDestroyableWall;
            _verticalDestroyableWallSprite = verticalDestroyableWall;
            _doorSprite = door;

            IsNonLinked = false;
            IsUnwalkableForEnemies = false;
            IsUnwalkable = false;

            _width = cell.Width;
            _height = cell.Height;

            // Considerando che la texture del muro parte orizzontale
            _widthCellDoorDifferenceOver2 = (_width - _doorSprite.Width) / 2f;

            _angleWidthOver2 = _pillarSprite.Width / 2f;
            _angleHeightOver2 = _pillarSprite.Height / 2f;

            _soundManager = soundManager;

            _cellDrawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.GroundLayerDepth
            };

            _game = game;

            Links = new Dictionary<LinkDirection, GameCell>()
            {
                [LinkDirection.NorthLink] = null,
                [LinkDirection.EastLink] = null,
                [LinkDirection.SouthLink] = null,
                [LinkDirection.WestLink] = null
            };

            Doors = new Dictionary<LinkDirection, Door>();

            Pillars = new Dictionary<PillarPositions, Pillar>();
            _dirtysDrawingInfos = new List<DrawingInfos>();

            _gameGrid = gameGrid;

            for (int i = 0; i < 2; ++i)
            {
                if (Numbers.RandomBetween(0D, 1D) <= dirtyProbability)
                {
                    _dirtysDrawingInfos.Add(new DrawingInfos()
                    {
                        Position = new Vector2(
                            position.X + Numbers.RandomBetween(0f, _width - _dirtySprite.Width / 2f),
                            position.Y + Numbers.RandomBetween(0f, _width - _dirtySprite.Height / 2f)),
                        Scale = 1f,
                        Rotation = Utilities.RandomBetween(0f, (float)(Math.PI / 2f), (float)Math.PI, 3f / 2f * (float)Math.PI),
                        Origin = _dirtySprite.SpriteCenter,
                        LayerDepth = ZIndexCalculator.ForGroundDirty
                    });
                }
            }

            _smokeParticleSystem = new BrokenWallSmokeParticleSystem(assetsLoader);
            Walls = new Dictionary<LinkDirection, Wall>();

            IsNorthPassable = true;
            IsEastPassable = true;
            IsSouthPassable = true;
            IsWestPassable = true;

            IsNorthDestroyable = false;
            IsEastDestroyable = false;
            IsSouthDestroyable = false;
            IsWestDestroyable = false;
        }

        public GameCell NorthCell
            => !Links.ContainsKey(LinkDirection.NorthLink) ? null : Links[LinkDirection.NorthLink];

        public GameCell SouthCell
            => !Links.ContainsKey(LinkDirection.SouthLink) ? null : Links[LinkDirection.SouthLink];

        public GameCell EastCell
            => !Links.ContainsKey(LinkDirection.EastLink) ? null : Links[LinkDirection.EastLink];

        public GameCell WestCell
            => !Links.ContainsKey(LinkDirection.WestLink) ? null : Links[LinkDirection.WestLink];

        public GameCell NorthCellWithoutLinks
            => _gameGrid.GetCellByIndex(IIndex, JIndex - 1);

        public GameCell SouthCellWithoutLinks
            => _gameGrid.GetCellByIndex(IIndex, JIndex + 1);

        public GameCell EastCellWithoutLinks
            => _gameGrid.GetCellByIndex(IIndex + 1, JIndex);

        public GameCell WestCellWithoutLinks
            => _gameGrid.GetCellByIndex(IIndex - 1, JIndex);

        public Vector2 TopLeftPosition
            => _cellDrawingInfos.Position;

        public Vector2 CenterPosition
            => _cellDrawingInfos.Center(_width, _height);

        public Rectangle CollisionRectangle
            => _cellDrawingInfos.HitBox(_width, _height);

        public bool IsNorthPassable { get; set; }
        public bool IsEastPassable { get; set; }
        public bool IsSouthPassable { get; set; }
        public bool IsWestPassable { get; set; }

        public bool IsNorthDestroyable { get; set; }
        public bool IsEastDestroyable { get; set; }
        public bool IsSouthDestroyable { get; set; }
        public bool IsWestDestroyable { get; set; }

        public bool IsNorthDoor { get; set; }
        public bool IsEastDoor { get; set; }
        public bool IsSouthDoor { get; set; }
        public bool IsWestDoor { get; set; }

        public bool IsNorthDoorPassable => !IsNorthDoor || Doors[LinkDirection.NorthLink].IsOpen;
        public bool IsEastDoorPassable => !IsEastDoor || Doors[LinkDirection.EastLink].IsOpen;
        public bool IsSouthDoorPassable => !IsSouthDoor || Doors[LinkDirection.SouthLink].IsOpen;
        public bool IsWestDoorPassable => !IsWestDoor || Doors[LinkDirection.WestLink].IsOpen;

        public bool IsPassableWithAction(CharacterActions action)
        {
            switch (action)
            {
                case CharacterActions.Up:
                    return IsNorthPassable && IsNorthDoorPassable;

                case CharacterActions.Down:
                    return IsSouthPassable && IsSouthDoorPassable;

                case CharacterActions.Right:
                    return IsEastPassable && IsEastDoorPassable;

                case CharacterActions.Left:
                    return IsWestPassable && IsWestDoorPassable;
            }

            return false;
        }

        /// <summary>
        /// Ritorna true se effettivamente rompe il muro
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="breakerPosition"></param>
        /// <returns></returns>
        public bool BreakWall(LinkDirection direction, Vector2 breakerPosition)
        {
            if (Walls.ContainsKey(direction))
            { 
                Walls.Remove(direction);
                _soundManager.PlayBreakWallSound();
                _smokeParticleSystem.AddParticles(breakerPosition);
                _smokeParticleSystem.AddParticles(breakerPosition);
                return true;
            }

            return false;
        }

        public void GenerateDoors()
        {
            var doorSize = new Vector2(_doorSprite.Width, _doorSprite.Height);
            int shakeIntensity = 3;

            if (IsNorthDoor)
            {
                var northDoorPosition = new Vector2(_cellDrawingInfos.Position.X + _widthCellDoorDifferenceOver2, _cellDrawingInfos.Position.Y);
                Doors.Add(LinkDirection.NorthLink, new Door(
                    northDoorPosition,
                    LinkDirection.NorthLink,
                    doorSize,
                    _doorSprite,
                    () => _game.StartCameraShake(shakeIntensity)));
            }

            if (IsEastDoor)
            {
                var eastDoorPosition = new Vector2(_cellDrawingInfos.Position.X + _width, _cellDrawingInfos.Position.Y + _widthCellDoorDifferenceOver2);
                Doors.Add(LinkDirection.EastLink, new Door(
                    eastDoorPosition,
                    LinkDirection.EastLink,
                    doorSize,
                    _doorSprite,
                    () => _game.StartCameraShake(shakeIntensity)));
            }

            if (IsSouthDoor)
            {
                var southDoorPosition = new Vector2(_cellDrawingInfos.Position.X + _widthCellDoorDifferenceOver2, _cellDrawingInfos.Position.Y + _height);
                Doors.Add(LinkDirection.SouthLink, new Door(
                    southDoorPosition,
                    LinkDirection.SouthLink,
                    doorSize,
                    _doorSprite,
                    () => _game.StartCameraShake(shakeIntensity)));
            }

            if (IsWestDoor)
            {
                var westDoorPosition = new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y + _widthCellDoorDifferenceOver2);
                Doors.Add(LinkDirection.WestLink, new Door(
                    westDoorPosition,
                    LinkDirection.WestLink,
                    doorSize,
                    _doorSprite,
                    () => _game.StartCameraShake(shakeIntensity)));
            }
        }

        public void GenerateWalls()
        {
            if (!IsNorthPassable)
            {
                Walls.Add(LinkDirection.NorthLink, new Wall(
                    LinkDirection.NorthLink,
                    new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y - _horizontalWallSprite.Height + 30 + 60),
                    IsNorthDestroyable,
                    IsNorthDestroyable ? _horizontalDestroyableWallSprite : _horizontalWallSprite));
            }

            if (!IsSouthPassable)
            {
                Walls.Add(LinkDirection.SouthLink, new Wall(
                    LinkDirection.SouthLink,
                    new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y + 30 + 60),
                    IsSouthDestroyable,
                    IsSouthDestroyable ? _horizontalDestroyableWallSprite : _horizontalWallSprite));
            }

            if (!IsEastPassable)
            {
                Walls.Add(LinkDirection.EastLink, new Wall(
                    LinkDirection.EastLink,
                    new Vector2(_cellDrawingInfos.Position.X + _width - _verticalWallSprite.Width / 2f, _cellDrawingInfos.Position.Y - 30),
                    IsEastDestroyable,
                    IsEastDestroyable ? _verticalDestroyableWallSprite : _verticalWallSprite));
            }

            if (!IsWestPassable)
            {
                Walls.Add(LinkDirection.WestLink, new Wall(
                    LinkDirection.WestLink,
                    new Vector2(_cellDrawingInfos.Position.X - _verticalWallSprite.Width / 2f, _cellDrawingInfos.Position.Y - 30),
                    IsWestDestroyable,
                    IsWestDestroyable ? _verticalDestroyableWallSprite : _verticalWallSprite));
            }
        }

        public void AddLink(GameCell destionationCell, LinkDirection direction)
            => Links[direction] = destionationCell;

        public void ApplyBloodDecal(CellRelativePosition cellRelativePosition)
        {
            if (NoDecals)
                return;

            var decalPosition = _cellDrawingInfos.Center(Sprite.Width, Sprite.Height);
            var randomDeltaFromPosition = Numbers.RandomBetween(2f, 3f);

            switch (cellRelativePosition)
            {
                case CellRelativePosition.Up:
                    decalPosition += new Vector2(0, (float)Sprite.Height / randomDeltaFromPosition);
                    break;

                case CellRelativePosition.Down:
                    decalPosition += new Vector2(0, -(float)Sprite.Height / randomDeltaFromPosition);
                    break;

                case CellRelativePosition.Left:
                    decalPosition += new Vector2((float)Sprite.Width / randomDeltaFromPosition, 0);
                    break;

                case CellRelativePosition.Right:
                    decalPosition += new Vector2(-(float)Sprite.Width / randomDeltaFromPosition, 0);
                    break;

                case CellRelativePosition.UpLeft:
                    decalPosition += new Vector2((float)Sprite.Width / randomDeltaFromPosition, (float)Sprite.Height / randomDeltaFromPosition);
                    break;

                case CellRelativePosition.UpRight:
                    decalPosition += new Vector2(-(float)Sprite.Width / randomDeltaFromPosition, (float)Sprite.Height / randomDeltaFromPosition);
                    break;

                case CellRelativePosition.DownLeft:
                    decalPosition += new Vector2((float)Sprite.Width / randomDeltaFromPosition, -(float)Sprite.Height / randomDeltaFromPosition);
                    break;

                case CellRelativePosition.DownRight:
                    decalPosition += new Vector2(-(float)Sprite.Width / randomDeltaFromPosition, -(float)Sprite.Height / randomDeltaFromPosition);
                    break;
            }

            _bloodDecalDrawingInfos = new DrawingInfos()
            {
                Position = decalPosition,
                Rotation = Numbers.RandomBetween(0f, 20f),
                Scale = Numbers.RandomBetween(0.3f, 1f),
                Origin = _bloodDecal.SpriteCenter,
                LayerDepth = ZIndexCalculator.ForGroundBlood
            };

        }

        public void AddNWPillar()
        {
            if (Pillars.ContainsKey(PillarPositions.NW))
                return;

            Pillars.Add(PillarPositions.NW, new Pillar(
                new Vector2(
                    _cellDrawingInfos.Position.X - _angleWidthOver2,
                    _cellDrawingInfos.Position.Y - _angleHeightOver2 - 70 + 60),
                _pillarSprite));
        }

        public void AddNEPillar()
        {
            if (Pillars.ContainsKey(PillarPositions.NE))
                return;

            Pillars.Add(PillarPositions.NE, new Pillar(
                new Vector2(
                      _cellDrawingInfos.Position.X + _width - _angleWidthOver2,
                      _cellDrawingInfos.Position.Y - _angleHeightOver2 - 70 + 60),
                _pillarSprite));
        }

        public void AddSWPillar()
        {
            if (Pillars.ContainsKey(PillarPositions.SW))
                return;

            Pillars.Add(PillarPositions.SW, new Pillar(
                new Vector2(
                    _cellDrawingInfos.Position.X - _angleWidthOver2,
                    _cellDrawingInfos.Position.Y + _width - _angleHeightOver2 - 70 + 60),
                _pillarSprite));
        }

        public void AddSEPillar()
        {
            if (Pillars.ContainsKey(PillarPositions.SE))
                return;

            Pillars.Add(PillarPositions.SE, new Pillar(
                new Vector2(
                    _cellDrawingInfos.Position.X + _width - _angleWidthOver2,
                    _cellDrawingInfos.Position.Y + _width - _angleHeightOver2 - 70 + 60),
                _pillarSprite));
        }

        public void SetSimplePillars()
        {
            if (!IsNorthPassable && !IsEastPassable)
                AddNEPillar();
            if (!IsNorthPassable && !IsWestPassable)
                AddNWPillar();
            if (!IsSouthPassable && !IsEastPassable)
                AddSEPillar();
            if (!IsSouthPassable && !IsWestPassable)
                AddSWPillar();

            if (IsNorthDoor)
            {
                AddNEPillar();
                AddNWPillar();
            }

            if (IsSouthDoor)
            {
                AddSEPillar();
                AddSWPillar();
            }

            if (IsEastDoor)
            {
                AddNEPillar();
                AddSEPillar();
            }

            if (IsWestDoor)
            {
                AddNWPillar();
                AddSWPillar();
            }
        }

        public void Update(TimeSpan elapsed)
        {
            if (_smokeParticleSystem.HasActiveParticles)
                _smokeParticleSystem.Update(elapsed);
            foreach (var door in Doors.Values)
                door.Update(elapsed);
        }

        public void DrawGround(SpriteBatch _spritebatch)
            => _spritebatch.Draw(Sprite, _cellDrawingInfos);

        public void DrawDirty(SpriteBatch spriteBatch)
        {
            if (!NoDecals)
            {
                foreach (var dirtyDrawingInfos in _dirtysDrawingInfos)
                    spriteBatch.Draw(_dirtySprite, dirtyDrawingInfos);

                if (_bloodDecalDrawingInfos != null)
                {
                    spriteBatch.Draw(_bloodDecal, _bloodDecalDrawingInfos);
                }
            }
        }

        public void DrawEffects(SpriteBatch spriteBatch)
        {
            if (_smokeParticleSystem != null)
            {
                _smokeParticleSystem.Draw(spriteBatch);
            }
        }

        public void DrawWalls(SpriteBatch spritebatch)
        {
            foreach (var w in Walls)
                w.Value.Draw(spritebatch);
            foreach (var d in Doors)
                d.Value.Draw(spritebatch);
            foreach (var a in Pillars)
                a.Value.Draw(spritebatch);
        }
    }
}