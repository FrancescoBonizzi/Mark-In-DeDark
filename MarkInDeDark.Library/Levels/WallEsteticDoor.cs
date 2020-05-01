using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.GameGridCommon;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;

namespace MarkInDeDark.Library.Levels
{
    /// <summary>
    /// La porta d'ingresso o di uscita
    /// </summary>
    public class WallEsteticDoor : IWall
    {
        public WallEsteticDoorType WallEsteticDoorType { get; }
        public LinkDirection Direction { get; }
        public Rectangle CollisionRectangle { get; }
        public Hull Hull { get; }

        private readonly DrawingInfos _spriteCenterDrawingInfos;
        private readonly DrawingInfos _spriteArcDrawingInfos;

        private readonly Sprite _spriteCenterOpen;
        private readonly Sprite _spriteCenterClosed;
        private readonly Sprite _spriteArc;

        public Vector2 Position => _spriteCenterDrawingInfos.Position;

        public bool IsOpen { get; set; }

        public WallEsteticDoor(
            WallEsteticDoorType wallEsteticDoorType,
            LinkDirection direction,
            Vector2 position,
            Sprite spriteCenterOpen,
            Sprite spriteCenterClosed,
            Sprite spriteArc)
        {
            WallEsteticDoorType = wallEsteticDoorType;
            Direction = direction;

            _spriteCenterDrawingInfos = new DrawingInfos()
            {
                Position = position + new Vector2(0f, -2f),
                LayerDepth = ZIndexCalculator.ForWall(position + spriteCenterOpen.SpriteCenter)
            };

            _spriteCenterOpen = spriteCenterOpen;
            _spriteCenterClosed = spriteCenterClosed;

            _spriteArcDrawingInfos = new DrawingInfos()
            {
                Position = position + new Vector2(20f, -40f),
                LayerDepth = ZIndexCalculator.OverAllLayerDepth
            };

            _spriteArc = spriteArc;

            CollisionRectangle = new Rectangle(
                 (int)position.X,
                 (int)position.Y + 100,
                 _spriteCenterOpen.Width,
                 40);

            Hull = Utilities.HullFromRectangle(CollisionRectangle);

            Hull.Points.Add(new Vector2(0, 0));
            Hull.Points.Add(new Vector2(0, 1));
            Hull.Points.Add(new Vector2(1, 1));
            Hull.Points.Add(new Vector2(1, 0));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_spriteArc, _spriteArcDrawingInfos);
            if (IsOpen)
                spriteBatch.Draw(_spriteCenterOpen, _spriteCenterDrawingInfos);
            else spriteBatch.Draw(_spriteCenterClosed, _spriteCenterDrawingInfos);
        }

        public void Update(TimeSpan elapsed)
        {

        }
    }
}