using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Levels
{
    public static class LevelCustomizations
    {
        public static LevelCustomization CustomizeForLevel(int levelId)
        {
            var l = new LevelCustomization()
            {
                LevelId = levelId,
                AmbienceColor = Vector3.Zero,
                PenumbraAmbientColor = Color.Black,
                IsSnowing = false,
                GlowScale = 1,
                WallFixedVerticalOffset = new Rectangle(12, 96, 40, 192),
                WallFixedHorizontalOffset = new Rectangle(0, 136, 192, 32),
                PillarOffset = new Rectangle(16, 144, 48, 32)
            };

            if (levelId == 1 || levelId == 2)
            {
                l.AmbienceColor = new Vector3(0f, 0.01f, 0.03f);
                l.PenumbraAmbientColor = new Color(0, 0, 3);
                l.IsSnowing = true;
                l.GlowScale = 2f;
                l.PillarOffset = new Rectangle(22, 144, 40, 24);
            }

            return l;
        }
    }
}
