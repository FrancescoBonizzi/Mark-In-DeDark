using FbonizziMonoGame.Drawing;
using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Extensions
{
    // TODO Da riportare nella libreria originale
    public static class FbonizziMonoGameExtensions
    {
        public static Rectangle HitBoxFixed(this DrawingInfos drawingInfos, int width, int height)
        {
            var hitbox = drawingInfos.HitBox(width, height);
            hitbox.X += (int)drawingInfos.Origin.X / 2;
            hitbox.Y += (int)drawingInfos.Origin.Y / 2;

            return hitbox;
        }
    }
}
