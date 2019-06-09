using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Assets;
using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.Levels
{
    public class GameCell
    {
        private readonly Sprite _cellSprite;
        private readonly Sprite _maceriaSprite;
        private readonly Sprite _wallSprite;
        private readonly Sprite _destroyableWallSprite;
        private readonly Sprite _angleSprite;
        private readonly Sprite _dirtySprite;
        private readonly Sprite _bloodDecal;

        private int _width;
        private int _height;
        private float _wallHeightOver2;
        private float _angleHeightOver2;

        private readonly SoundManager _soundManager;

        public List<DrawingInfos> AnglesDrawingInfos { get; }
        private readonly List<DrawingInfos> _dirtysDrawingInfos;
        public List<Wall> Walls { get; }

        private readonly DrawingInfos _cellDrawingInfos;
        private readonly DrawingInfos _bloodDecalDrawingInfos;
        public IDictionary<LinkDirection, GameCell> Links { get; }

        public bool IsUnwalkable { get; set; }
        public bool IsUnwalkableForEnemies { get; set; }
        public bool DontDraw { get; set; }
        private bool _drawBloodDecal;
        private readonly BrokenWallSmokeParticleSystem _smokeParticleSystem;

        public GameCell(
            Vector2 position,
            AssetsLoader assetsLoader,
            Sprite cell,
            Sprite wall,
            Sprite destroyableWall,
            Sprite angle,
            Sprite dirty,
            Sprite maceria,
            double dirtyProbability,
            SoundManager soundManager)
        {
            _bloodDecal = assetsLoader.Sprites["blood_decal"];
            _drawBloodDecal = false;
            DontDraw = false;

            _cellSprite = cell;
            _wallSprite = wall;
            _angleSprite = angle;
            _dirtySprite = dirty;
            _maceriaSprite = maceria;
            _destroyableWallSprite = destroyableWall;

            IsUnwalkable = false;
            IsUnwalkableForEnemies = false;

            _width = cell.Width;
            _height = cell.Height;
            _wallHeightOver2 = _wallSprite.Height / 2f;
            _angleHeightOver2 = _angleSprite.Height / 2f;

            _soundManager = soundManager;

            _cellDrawingInfos = new DrawingInfos()
            {
                Position = position
            };

            _bloodDecalDrawingInfos = new DrawingInfos()
            {
                Position = position,
                Rotation = Numbers.RandomBetween(-360f, 360f)
            };

            Links = new Dictionary<LinkDirection, GameCell>()
            {
                [LinkDirection.NorthLink] = null,
                [LinkDirection.EastLink] = null,
                [LinkDirection.SouthLink] = null,
                [LinkDirection.WestLink] = null
            };

            AnglesDrawingInfos = new List<DrawingInfos>();
            _dirtysDrawingInfos = new List<DrawingInfos>();

            for (int i = 0; i < 2; ++i)
            {
                if (Numbers.RandomBetween(0D, 1D) <= dirtyProbability)
                {
                    _dirtysDrawingInfos.Add(new DrawingInfos()
                    {
                        Position = new Vector2(
                            position.X + Numbers.RandomBetween(0f, _width - _dirtySprite.Width / 2f),
                            position.Y + Numbers.RandomBetween(0f, _width - _dirtySprite.Height / 2f)),
                        Scale = Numbers.RandomBetween(0.5f, 1.2f),
                        Rotation = Numbers.RandomBetween(0, 360f),
                        Origin = _dirtySprite.SpriteCenter
                    });
                }
            }

            _smokeParticleSystem = new BrokenWallSmokeParticleSystem(assetsLoader);
            Walls = new List<Wall>();

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
            => Links[LinkDirection.NorthLink];

        public GameCell SouthCell
            => Links[LinkDirection.SouthLink];

        public GameCell EastCell
            => Links[LinkDirection.EastLink];

        public GameCell WestCell
            => Links[LinkDirection.WestLink];

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

        public bool IsPassableWithAction(CharacterActions action)
        {
            switch (action)
            {
                case CharacterActions.Up:
                    return IsNorthPassable;

                case CharacterActions.Down:
                    return IsSouthPassable;

                case CharacterActions.Right:
                    return IsEastPassable;

                case CharacterActions.Left:
                    return IsWestPassable;
            }

            return false;
        }

        public void BreakWall(LinkDirection direction, Vector2 breakerPosition)
        {
            var wallToRemove = Walls.FirstOrDefault(w => w.Direction == direction);
            if (wallToRemove != null)
            {
                Walls.Remove(wallToRemove);
                _soundManager.PlayBreakWallSound();
                _smokeParticleSystem.AddParticles(breakerPosition);
                _smokeParticleSystem.AddParticles(breakerPosition);
            }
        }

        public void GenerateWalls()
        {
            if (!IsNorthPassable)
            {
                Walls.Add(new Wall(
                    LinkDirection.NorthLink,
                    new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y - _wallHeightOver2),
                    0.0f,
                    IsNorthDestroyable));
            }

            if (!IsEastPassable)
            {
                Walls.Add(new Wall(
                    LinkDirection.EastLink,
                    new Vector2(_cellDrawingInfos.Position.X + _width + _wallHeightOver2, _cellDrawingInfos.Position.Y),
                    MathHelper.PiOver2,
                    IsEastDestroyable));
            }

            if (!IsSouthPassable)
            {
                Walls.Add(new Wall(
                    LinkDirection.SouthLink,
                    new Vector2(_cellDrawingInfos.Position.X, _cellDrawingInfos.Position.Y + _width - _wallHeightOver2),
                    0.0f,
                    IsSouthDestroyable));
            }

            if (!IsWestPassable)
            {
                Walls.Add(new Wall(
                    LinkDirection.WestLink,
                    new Vector2(_cellDrawingInfos.Position.X + _wallHeightOver2, _cellDrawingInfos.Position.Y),
                    MathHelper.PiOver2,
                    IsWestDestroyable));
            }
        }

        public void AddLink(GameCell destionationCell, LinkDirection direction)
            => Links[direction] = destionationCell;

        public void ApplyBloodDecal()
            => _drawBloodDecal = true;

        public void AddNWAngle()
        {
            AnglesDrawingInfos.Add(new DrawingInfos()
            {
                Position = new Vector2(
                    _cellDrawingInfos.Position.X - _angleHeightOver2,
                    _cellDrawingInfos.Position.Y - _angleHeightOver2)
            });
        }

        public void AddNEAngle()
        {
            AnglesDrawingInfos.Add(new DrawingInfos()
            {
                Position = new Vector2(
                      _cellDrawingInfos.Position.X + _width - _angleHeightOver2,
                      _cellDrawingInfos.Position.Y - _angleHeightOver2)
            });
        }

        public void AddSWAngle()
        {
            AnglesDrawingInfos.Add(new DrawingInfos()
            {
                Position = new Vector2(
                    _cellDrawingInfos.Position.X - _angleHeightOver2,
                    _cellDrawingInfos.Position.Y + _width - _angleHeightOver2)
            });
        }

        public void AddSEAngle()
        {
            AnglesDrawingInfos.Add(new DrawingInfos()
            {
                Position = new Vector2(
                    _cellDrawingInfos.Position.X + _width - _angleHeightOver2,
                    _cellDrawingInfos.Position.Y + _width - _angleHeightOver2)
            });
        }

        public void SetSimpleAngles()
        {
            if (!IsNorthPassable && !IsEastPassable)
                AddNEAngle();
            if (!IsNorthPassable && !IsWestPassable)
                AddNWAngle();
            if (!IsSouthPassable && !IsEastPassable)
                AddSEAngle();
            if (!IsSouthPassable && !IsWestPassable)
                AddSWAngle();
        }

        public void Update(TimeSpan elapsed)
        {
            if (_smokeParticleSystem.HasActiveParticles)
                _smokeParticleSystem.Update(elapsed);
        }

        public void DrawCell(SpriteBatch _spritebatch)
        {
            if (!IsUnwalkable)
            {
                _spritebatch.Draw(_cellSprite, _cellDrawingInfos);
            }
            else
            {
                _spritebatch.Draw(_maceriaSprite, _cellDrawingInfos);
            }
        }

        public void DrawDirtys(SpriteBatch spriteBatch)
        {
            foreach (var dirtyDrawingInfos in _dirtysDrawingInfos)
                spriteBatch.Draw(_dirtySprite, dirtyDrawingInfos);

            if (_drawBloodDecal)
            {
                spriteBatch.Draw(_bloodDecal, _bloodDecalDrawingInfos);
            }

            // Lo metto qua perché è già sopra le celle
            if (_smokeParticleSystem != null)
            {
                _smokeParticleSystem.Draw(spriteBatch);
            }
        }

        public void DrawWalls(SpriteBatch spritebatch)
        {
            foreach (var wall in Walls)
            {
                if (!wall.IsDestroyable)
                {
                    spritebatch.Draw(_wallSprite, wall.DrawingInfos);
                }
                else
                {
                    spritebatch.Draw(_destroyableWallSprite, wall.DrawingInfos);
                }
            }
        }

        public void DrawAngles(SpriteBatch spritebatch)
        {
            foreach (var angleDrawingInfos in AnglesDrawingInfos)
                spritebatch.Draw(_angleSprite, angleDrawingInfos);
        }
    }
}