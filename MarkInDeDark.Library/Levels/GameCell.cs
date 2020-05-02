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

        private readonly Sprite _dirty;
        private readonly Sprite _bloodDecal;

        public int Width { get; }
        public int Height { get; }

        private readonly SoundManager _soundManager;

        private readonly DrawingInfos _dirtyDrawingInfos;
        public Dictionary<PillarPositions, Pillar> Pillars { get; private set; }
        public Dictionary<LinkDirection, IWall> Walls { get; private set; }

        private readonly DrawingInfos _cellDrawingInfos;
        private DrawingInfos _bloodDecalDrawingInfos;
        public Dictionary<LinkDirection, GameCell> Links { get; }

        public bool IsUnLinked { get; set; }
        public bool IsUnwalkableForEnemies { get; set; }
        public bool IsUnwalkable { get; set; }
        public bool IsRiempitiveWallCell { get; set; }
        public bool DontDraw { get; set; }
        public int IIndex { get; }
        public int JIndex { get; }
        public bool NoDecals { get; set; }

        public GroundTypes GroundType { get; set; }

        private readonly BrokenWallSmokeParticleSystem _smokeParticleSystem;
        private readonly GameGrid _gameGrid;
        private readonly MarkInDeDarkGame _game;

        public GameCell(
            Vector2 position,
            AssetsLoader assetsLoader,
            Sprite cell,
            Sprite dirty,
            SoundManager soundManager,
            int iIndex,
            int jIndex,
            GameGrid gameGrid,
            MarkInDeDarkGame game)
        {
            _bloodDecal = assetsLoader.Sprites["Effects/blood_decal"];

            IIndex = iIndex;
            JIndex = jIndex;

            Sprite = cell;

            IsUnLinked = false;
            IsUnwalkableForEnemies = false;
            IsUnwalkable = false;

            Width = cell.Width;
            Height = cell.Height;

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

            Pillars = new Dictionary<PillarPositions, Pillar>();

            _gameGrid = gameGrid;

            if (dirty != null)
            {
                _dirty = dirty;
                _dirtyDrawingInfos = new DrawingInfos()
                {
                    Position = new Vector2(
                        position.X + Numbers.RandomBetween(0f, Width - _dirty.Width / 2f),
                        position.Y + Numbers.RandomBetween(0f, Width - _dirty.Height / 2f)),
                    Scale = 1f,
                    Rotation = Utilities.RandomBetween(0f, (float)(Math.PI / 2f), (float)Math.PI, 3f / 2f * (float)Math.PI),
                    Origin = _dirty.SpriteCenter,
                    LayerDepth = ZIndexCalculator.GroundDirtyLayerDepth
                };
            }

            _smokeParticleSystem = new BrokenWallSmokeParticleSystem(assetsLoader)
            {
                LayerDepth = ZIndexCalculator.OverAllLayerDepth
            };

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
            IsRiempitiveWallCell = true;
            _cellDrawingInfos.LayerDepth = ZIndexCalculator.ForSpecialExternalWall(
                _cellDrawingInfos.Position);
            _cellDrawingInfos.Origin += new Vector2(0f, +28);
            IsUnLinked = true;
        }

        public void InitializeWalls(
            Cell levelEditorCell,
            Level level,
            AssetsLoader assets)
        {
            // TODO vedi se riesci a mettere privato il SET di questi campi
            GroundType = levelEditorCell.GroundType;

            IsNorthDestroyable = levelEditorCell.Top == WallTypes.Fragile;
            IsNorthPassable = levelEditorCell.Top == WallTypes.Niente || levelEditorCell.Top == WallTypes.Porta;
            IsNorthDoor = levelEditorCell.Top == WallTypes.Porta;

            IsEastDestroyable = levelEditorCell.Right == WallTypes.Fragile;
            IsEastPassable = levelEditorCell.Right == WallTypes.Niente || levelEditorCell.Right == WallTypes.Porta;

            IsSouthDestroyable = levelEditorCell.Bottom == WallTypes.Fragile;
            IsSouthPassable = levelEditorCell.Bottom == WallTypes.Niente || levelEditorCell.Bottom == WallTypes.Porta;
            IsSouthDoor = levelEditorCell.Bottom == WallTypes.Porta;

            IsWestDestroyable = levelEditorCell.Left == WallTypes.Fragile;
            IsWestPassable = levelEditorCell.Left == WallTypes.Niente || levelEditorCell.Left == WallTypes.Porta;

            {
                var walls = WallFactory.GenerateWalls(
                    _cellDrawingInfos.Position,
                    levelEditorCell,
                    level,
                    new Vector2(Width, Height),
                    () => _game.StartCameraShake(3));

                Walls = new Dictionary<LinkDirection, IWall>();
                foreach (var w in walls)
                {
                    Walls.Add(w.Direction, w);
                }
            }

        }

        public void InitializePillars(Level level)
        {
            var pillars = PillarFactory.GeneratePillars(this, level);
            Pillars = new Dictionary<PillarPositions, Pillar>();
            foreach (var p in pillars)
            {
                Pillars.Add(p.PillarPosition, p);
            }
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
            => _cellDrawingInfos.Center(Width, Height);

        public Rectangle CollisionRectangle
            => _cellDrawingInfos.HitBox(Width, Height);

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

        public bool IsNorthDoorPassable => !IsNorthDoor || (Walls[LinkDirection.NorthLink] as WallInteractiveDoor).IsOpen;
        public bool IsEastDoorPassable => true; // Non esistono
        public bool IsSouthDoorPassable => !IsSouthDoor || (Walls[LinkDirection.SouthLink] as WallInteractiveDoor).IsOpen;
        public bool IsWestDoorPassable => true; // Non esistono

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
                var wall = Walls[direction] as WallBreakable;
                wall.Break();

                //Walls.Remove(direction);
                //_soundManager.PlayBreakWallSound();
                _smokeParticleSystem.AddParticles(breakerPosition);
                _smokeParticleSystem.AddParticles(breakerPosition);
                return true;
            }

            return false;
        }

        public void AddLink(GameCell destionationCell, LinkDirection direction)
            => Links[direction] = destionationCell;

        public void ApplyBloodDecal(CellRelativePosition cellRelativePosition)
        {
            if (IsRiempitiveWallCell)
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

        public void Update(TimeSpan elapsed)
        {
            if (_smokeParticleSystem.HasActiveParticles)
                _smokeParticleSystem.Update(elapsed);
            foreach (var w in Walls.Values)
                w.Update(elapsed);
        }

        public void DrawGround(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, _cellDrawingInfos);

            if (!NoDecals)
            {
                if (_dirty != null)
                    spriteBatch.Draw(_dirty, _dirtyDrawingInfos);

                if (_bloodDecalDrawingInfos != null)
                {
                    spriteBatch.Draw(_bloodDecal, _bloodDecalDrawingInfos);
                }
            }
        }

        public void DrawForDebug(SpriteBatch spriteBatch)
            => spriteBatch.DrawRectangle(CollisionRectangle, Color.Red.WithAlpha(0.5f));

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
            foreach (var a in Pillars)
                a.Value.Draw(spritebatch);
        }
    }
}