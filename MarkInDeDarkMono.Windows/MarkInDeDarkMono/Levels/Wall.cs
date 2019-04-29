using FlowsoftGamesMonogame.Drawing;
using Microsoft.Xna.Framework;

namespace MarkInDeDarkMono.Levels
{
    public class Wall
    {
        public DrawingInfos DrawingInfos { get; }
        public LinkDirection Direction { get; }
        public bool IsDestroyable { get; }

        public Wall(
            LinkDirection direction,
            Vector2 position,
            float rotation,
            bool isDestroyable)
        {
            Direction = direction;
            DrawingInfos = new DrawingInfos()
            {
                Position = position,
                Rotation = rotation
            };
            IsDestroyable = isDestroyable;
        }
    }
}
