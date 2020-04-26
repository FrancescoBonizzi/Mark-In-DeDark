using FbonizziMonoGame;
using Microsoft.Xna.Framework;
using Penumbra;

namespace MarkInDeDark.Library.Maths
{
    public static class Utilities
    {
        public static T RandomBetween<T>(params T[] choices)
            => choices[Numbers.RandomBetween(0, choices.Length)];

        public static Hull HullFromRectangle(Rectangle bounds)
        {
            return new Hull()
            {
                Position = new Vector2(bounds.X, bounds.Y),
                Scale = new Vector2(bounds.Width, bounds.Height)
            };
        }
    }
}
