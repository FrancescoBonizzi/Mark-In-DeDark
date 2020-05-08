using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;

namespace MarkInDeDark.Library.Levels
{
    public interface IWall
    {
        LinkDirection Direction { get; }
        Rectangle CollisionRectangle { get; }
        Hull Hull { get; }
        Vector2 Position { get; }

        void Update(TimeSpan elapsed);
        void Draw(SpriteBatch spriteBatch);
        void DrawNormalMap(SpriteBatch spriteBatch);
    }
}
