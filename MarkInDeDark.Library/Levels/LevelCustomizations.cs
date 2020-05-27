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
                AmbienceColor = new Vector3(0f),
                PenumbraAmbientColor = Color.Black,
                IsSnowing = false
            };

            if (levelId == 1 || levelId == 2)
            {
                l.AmbienceColor = new Vector3(0f, 0.01f, 0.03f);
                l.PenumbraAmbientColor = new Color(0, 0, 3);
                l.IsSnowing = true;
            }

            return l;
        }
    }
}
