using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Levels
{
    public interface IWall
    {
        LinkDirection Direction { get; }
        Rectangle CollisionRectangle { get; }
        List<Hull> Hulls { get; }
        Vector2 Position { get; }

        void Update(TimeSpan elapsed);
        void Draw(SpriteBatch spriteBatch);
        void DrawNormalMap(SpriteBatch spriteBatch);
    }
}
