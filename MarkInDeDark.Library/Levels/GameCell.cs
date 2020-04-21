using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.GameGridCommon;

using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Maths;
using MarkInDeDark.Library.Objects;
using MarkInDeDark.Library.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Levels
{
    public class GameCell
    {
        public Sprite Sprite { get; set; }
        public Sprite SpriteNormalMap { get; set; }

        private readonly Sprite _dirtySprite;
        private readonly Sprite _bloodDecal;

        private readonly AssetsLoader _assetsLoader;

        private readonly int _width;
        private readonly int _height;

        private readonly SoundManager _soundManager;

        private readonly List<DrawingInfos> _dirtysDrawingInfos;
        public Dictionary<PillarPositions, Pillar> Pillars { get; }
        public Dictionary<LinkDirection, Wall> Walls { get; }

        private readonly DrawingInfos _cellDrawingInfos;
        private DrawingInfos _bloodDecalDrawingInfos;
        public Dictionary<LinkDirection, GameCell> Links { get; }
        public Dictionary<LinkDirection, Door> Doors { get; }

        public bool IsUnLinked { get; set; }
        public bool IsUnwalkableForEnemies { get; set; }
        public bool IsUnwalkable { get; set; }
        public bool IsSpecialWallCell { get; set; }
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
            int width,
            int height,
            Sprite dirtySprite,
            double dirtyProbability,
            SoundManager soundManager,
            int iIndex,
            int jIndex,
            GameGrid gameGrid,
            MarkInDeDarkGame game)
        {
            _assetsLoader = assetsLoader;
            _bloodDecal = assetsLoader.Sprites["Effects/blood_decal"];
            DontDraw = false;

            IIndex = iIndex;
            JIndex = jIndex;

            _dirtySprite = dirtySprite;

            IsUnLinked = false;
            IsUnwalkableForEnemies = false;
            IsUnwalkable = false;

            _width = width;
            _height = height;
            
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
                    LayerDepth = ZIndexCalculator.GroundDirtyLayerDepth
                });
            }

            _smokeParticleSystem = new BrokenWallSmokeParticleSystem(assetsLoader)
            {
                LayerDepth = ZIndexCalculator.OverAllLayerDepth
            };

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

        public void SetSpecialWallCell()
        {
            IsSpecialWallCell = true;
            _cellDrawingInfos.LayerDepth = ZIndexCalculator.ForSpecialExternalWall(
                _cellDrawingInfos.Position);
            _cellDrawingInfos.Origin += new Vector2(0f, +28);
            IsUnLinked = true;
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

        public Vector2 TopRightPosition
            => _cellDrawingInfos.Position + new Vector2(_width, 0f);

        public Vector2 BottomLeftPosition
            => _cellDrawingInfos.Position + new Vector2(0f, _height);

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
        public bool IsEastDoor => false; // Non esistono
        public bool IsSouthDoor { get; set; }
        public bool IsWestDoor => false; // Non esistono

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
                //_soundManager.PlayBreakWallSound();
                _smokeParticleSystem.AddParticles(breakerPosition);
                _smokeParticleSystem.AddParticles(breakerPosition);
                return true;
            }

            return false;
        }

        public void GenerateDoors(Level level)
        {
            int shakeIntensity = 3;

            if (IsNorthDoor)
            {
                Doors.Add(LinkDirection.NorthLink, new Door(
                    doorPosition: new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y - level.HorizontalWallSprite.Height + 30 + 60),
                    linkDirection: LinkDirection.NorthLink,
                    assetsLoader: _assetsLoader,
                    wallDoorArc: level.WallDoorArcSprite,
                    wallDoorDown: level.WallDoorDownSprite,
                    onClose: () => _game.StartCameraShake(shakeIntensity)));
            }

            if (IsSouthDoor)
            {
                var southDoorPosition = new Vector2(_cellDrawingInfos.Position.X + 96, _cellDrawingInfos.Position.Y + _height);
                Doors.Add(LinkDirection.SouthLink, new Door(
                    doorPosition: new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y + 30 + 60),
                    linkDirection: LinkDirection.SouthLink,
                    assetsLoader: _assetsLoader,
                    wallDoorArc: level.WallDoorArcSprite,
                    wallDoorDown: level.WallDoorDownSprite,
                    onClose: () => _game.StartCameraShake(shakeIntensity)));
            }
        }

        public void GenerateWalls(Level level)
        {
            if (!IsNorthPassable)
            {
                Walls.Add(LinkDirection.NorthLink, new Wall(
                    LinkDirection.NorthLink,
                    new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y - level.HorizontalWallSprite.Height + 30 + 60),
                    IsNorthDestroyable,
                    IsNorthDestroyable ? level.HorizontalDestroyableWallSprite : level.HorizontalWallSprite,
                    IsNorthDestroyable ? level.HorizontalDestroyableWallSpriteNormalMap : level.HorizontalWallSpriteNormalMap));
            }

            if (!IsSouthPassable)
            {
                Walls.Add(LinkDirection.SouthLink, new Wall(
                    LinkDirection.SouthLink,
                    new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y + 30 + 60),
                    IsSouthDestroyable,
                    IsSouthDestroyable ? level.HorizontalDestroyableWallSprite : level.HorizontalWallSprite,
                    IsSouthDestroyable ? level.HorizontalDestroyableWallSpriteNormalMap : level.HorizontalWallSpriteNormalMap));
            }

            if (!IsEastPassable)
            {
                Walls.Add(LinkDirection.EastLink, new Wall(
                    LinkDirection.EastLink,
                    new Vector2(_cellDrawingInfos.Position.X + _width - level.VerticalWallSprite.Width / 2f, _cellDrawingInfos.Position.Y - 30),
                    IsEastDestroyable,
                    IsEastDestroyable ? level.VerticalDestroyableWallSprite : level.VerticalWallSprite,
                    null)); // I muri verticali non hanno normal map
            }

            if (!IsWestPassable)
            {
                Walls.Add(LinkDirection.WestLink, new Wall(
                    LinkDirection.WestLink,
                    new Vector2(_cellDrawingInfos.Position.X - level.VerticalWallSprite.Width / 2f, _cellDrawingInfos.Position.Y - 30),
                    IsWestDestroyable,
                    IsWestDestroyable ? level.VerticalDestroyableWallSprite : level.VerticalWallSprite,
                    null)); // I muri verticali non hanno normal map
            }
        }

        public void AddLink(GameCell destionationCell, LinkDirection direction)
            => Links[direction] = destionationCell;

        public void ApplyBloodDecal(CellRelativePosition cellRelativePosition)
        {
            if (IsSpecialWallCell)
                return;

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
                LayerDepth = ZIndexCalculator.GroundBloodLayerDepth
            };
        }

        public void AddNWPillar(Level level)
        {
            if (Pillars.ContainsKey(PillarPositions.NW))
                return;

            var angleWidthOver2 = level.PillarSprite.Width / 2f;
            var angleHeightOver2 = level.PillarSprite.Height / 2f;

            Pillars.Add(PillarPositions.NW, new Pillar(
                new Vector2(
                    _cellDrawingInfos.Position.X - angleWidthOver2,
                    _cellDrawingInfos.Position.Y - angleHeightOver2 - 70 + 60),
                level.PillarSprite));
        }

        public void AddNEPillar(Level level)
        {
            if (Pillars.ContainsKey(PillarPositions.NE))
                return;

            var angleWidthOver2 = level.PillarSprite.Width / 2f;
            var angleHeightOver2 = level.PillarSprite.Height / 2f;

            Pillars.Add(PillarPositions.NE, new Pillar(
                new Vector2(
                      _cellDrawingInfos.Position.X + _width - angleWidthOver2,
                      _cellDrawingInfos.Position.Y - angleHeightOver2 - 70 + 60),
                level.PillarSprite));
        }

        public void AddSWPillar(Level level)
        {
            if (Pillars.ContainsKey(PillarPositions.SW))
                return;

            var angleWidthOver2 = level.PillarSprite.Width / 2f;
            var angleHeightOver2 = level.PillarSprite.Height / 2f;

            Pillars.Add(PillarPositions.SW, new Pillar(
                new Vector2(
                    _cellDrawingInfos.Position.X - angleWidthOver2,
                    _cellDrawingInfos.Position.Y + _width - angleHeightOver2 - 70 + 60),
                level.PillarSprite));
        }

        public void AddSEPillar(Level level)
        {
            if (Pillars.ContainsKey(PillarPositions.SE))
                return;

            var angleWidthOver2 = level.PillarSprite.Width / 2f;
            var angleHeightOver2 = level.PillarSprite.Height / 2f;

            Pillars.Add(PillarPositions.SE, new Pillar(
                new Vector2(
                    _cellDrawingInfos.Position.X + _width - angleWidthOver2,
                    _cellDrawingInfos.Position.Y + _width - angleHeightOver2 - 70 + 60),
                level.PillarSprite));
        }

        public void SetSimplePillars(Level level)
        {
            if (!IsNorthPassable && !IsEastPassable)
                AddNEPillar(level);
            if (!IsNorthPassable && !IsWestPassable)
                AddNWPillar(level);
            if (!IsSouthPassable && !IsEastPassable)
                AddSEPillar(level);
            if (!IsSouthPassable && !IsWestPassable)
                AddSWPillar(level);

            if (IsNorthDoor)
            {
                AddNEPillar(level);
                AddNWPillar(level);
            }

            if (IsSouthDoor)
            {
                AddSEPillar(level);
                AddSWPillar(level);
            }

            if (IsEastDoor)
            {
                AddNEPillar(level);
                AddSEPillar(level);
            }

            if (IsWestDoor)
            {
                AddNWPillar(level);
                AddSWPillar(level);
            }
        }

        public void Update(TimeSpan elapsed)
        {
            if (_smokeParticleSystem.HasActiveParticles)
                _smokeParticleSystem.Update(elapsed);
            foreach (var door in Doors.Values)
                door.Update(elapsed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, _cellDrawingInfos);

            if (!NoDecals)
            {
                foreach (var dirtyDrawingInfos in _dirtysDrawingInfos)
                    spriteBatch.Draw(_dirtySprite, dirtyDrawingInfos);

                if (_bloodDecalDrawingInfos != null)
                {
                    spriteBatch.Draw(_bloodDecal, _bloodDecalDrawingInfos);
                }
            }

            if (_smokeParticleSystem != null)
            {
                _smokeParticleSystem.Draw(spriteBatch);
            }

            foreach (var w in Walls)
                w.Value.Draw(spriteBatch);
            foreach (var d in Doors)
                d.Value.Draw(spriteBatch);
            foreach (var a in Pillars)
                a.Value.Draw(spriteBatch);
        }

        public void DrawNormalMap(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteNormalMap, _cellDrawingInfos);

            foreach (var w in Walls)
                w.Value.DrawNormalMap(spriteBatch);
            //foreach (var d in Doors)
            //    d.Value.DrawNormalMap(spriteBatch);
            //foreach (var a in Pillars)
            //    a.Value.DrawNormalMap(spriteBatch);
        }

        public void DrawForDebug(SpriteBatch spriteBatch)
        {
            foreach (var w in Walls)
                w.Value.DrawForDebug(spriteBatch);
            foreach (var d in Doors)
                d.Value.DrawForDebug(spriteBatch);
        }
    }
}