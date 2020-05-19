using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Levels
{
    public class LevelCustomization
    {
        public int LevelId { get; set; }
        public Vector3 AmbienceColor { get; set; }
        public Color PenumbraAmbientColor { get; set; }
        public bool IsSnowing { get; set; }
    }
}
