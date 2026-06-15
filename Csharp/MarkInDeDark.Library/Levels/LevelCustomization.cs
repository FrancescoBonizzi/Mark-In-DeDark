using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Levels
{
    public class LevelCustomization
    {
        public int LevelId { get; set; }
        public Vector3 AmbienceColor { get; set; }
        public Color PenumbraAmbientColor { get; set; }
        public bool IsSnowing { get; set; }
        public float GlowScale { get; set; }

        public Rectangle WallFixedHorizontalOffset { get; set; }
        public Rectangle WallFixedVerticalOffset { get; set; }
        public Rectangle PillarOffset { get; set; }
    }
}
