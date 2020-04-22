using Microsoft.Xna.Framework.Graphics;

namespace MarkInDeDark.Library.Objects
{
    public interface IDrawableObject
    {
        void Draw(SpriteBatch spriteBatch);
        void DrawForDebug(SpriteBatch spriteBatch);
    }
}
