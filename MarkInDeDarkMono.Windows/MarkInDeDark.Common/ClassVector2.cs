using Microsoft.Xna.Framework;

namespace MarkInDeDark.Common
{
    // Mi serve per il binding nel level editor
    public class ClassVector2
    {
        public ClassVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }

        public Vector2 ToXnaVector2()
            => new Vector2(X, Y);
    }
}
