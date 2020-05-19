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
                IsSnowing = false
            };

            if (levelId == 1 || levelId == 2)
            {
                l.AmbienceColor = new Vector3(0f, 0.03f, 0.05f);
                l.PenumbraAmbientColor = new Color(1, 1, 4);
                l.IsSnowing = true;
            }

            return l;
        }
    }
}
