using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
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
            LinkDirection direction,
            Vector2 position,
            Sprite spriteCenterOpen,
            Sprite spriteCenterClosed,
            Sprite spriteArc)
        {
            Direction = direction;

            _spriteCenterDrawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForWall(position + spriteCenterOpen.SpriteCenter)
            };

            _spriteCenterOpen = spriteCenterOpen;
            _spriteCenterClosed = spriteCenterClosed;

            _spriteArcDrawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.OverAllLayerDepth
            };

            _spriteArc = spriteArc;
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