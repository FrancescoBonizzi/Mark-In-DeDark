using FbonizziMonoGame;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;

namespace MarkInDeDarkMono.Extensions
{
    public static class ZIndexCalculator
    {
        /// <summary>
        /// Le cose più in alto sono disegnate prima
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static float Calculate(Vector2 position)
            => Numbers.MapValueFromIntervalToInterval(position.Y, 0, GameGrid.GridMaxHeight, 0f, 1f);
    }
}
