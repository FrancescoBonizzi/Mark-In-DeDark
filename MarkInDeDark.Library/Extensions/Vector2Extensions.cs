using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Extensions
{
    public static class Vector2Extensions
    {
        public static bool IsEqualsConvertedToInt(this Vector2 vector2, Vector2 other)
            => (int)vector2.X == (int)other.X && (int)vector2.Y == (int)other.Y;
    }
}
