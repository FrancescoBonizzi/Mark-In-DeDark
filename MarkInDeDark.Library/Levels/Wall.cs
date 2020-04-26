using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;

namespace MarkInDeDark.Library.Levels
{
    public class Wall
    {
        public LinkDirection Direction { get; }
        public bool IsDestroyable { get; }

        public DrawingInfos DrawingInfos { get; }
        private readonly Sprite _sprite;

        public Rectangle CollisionRectangle { get; }
        public Hull Hull { get; }

        public Wall(
            LinkDirection direction,
            Vector2 position,
            bool isDestroyable,
            Sprite sprite)
        {
            Direction = direction;
            DrawingInfos = new DrawingInfos()
            {
                Position = position,
                LayerDepth = ZIndexCalculator.ForWall(position + sprite.SpriteCenter)
            };

            if (direction == LinkDirection.EastLink || direction == LinkDirection.WestLink)
            {
                CollisionRectangle = new Rectangle(
                    (int)position.X + 20,
                    (int)position.Y,
                    sprite.Width - 40,
                    sprite.Height + 30);
            }
            else
            {
                // Quelli orizzontali hanno un boundingbox più stretto (alla base del muro)
                CollisionRectangle = new Rectangle(
                    (int)position.X,
                    (int)position.Y + 90,
                    sprite.Width,
                    65);
            }

            IsDestroyable = isDestroyable;
            _sprite = sprite;

            Hull = HullFromRectangle(CollisionRectangle);
           
            Hull.Points.Add(new Vector2(0, 0));
            Hull.Points.Add(new Vector2(0, 1));
            Hull.Points.Add(new Vector2(1, 1));
            Hull.Points.Add(new Vector2(1, 0));
        }

        public void Draw(SpriteBatch spriteBatch)
            => spriteBatch.Draw(_sprite, DrawingInfos);


        private static Hull HullFromRectangle(Rectangle bounds)
        {
            return new Hull()
            {
                Position = new Vector2(bounds.X, bounds.Y),
                Scale = new Vector2(bounds.Width, bounds.Height)
            };
        }

    }

}
