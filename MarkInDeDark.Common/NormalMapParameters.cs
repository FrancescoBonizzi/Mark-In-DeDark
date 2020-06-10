using Microsoft.Xna.Framework;

namespace MarkInDeDark.Common
{
    public class NormalMapParameters
    {
        public const string FileName = "tempNormalMapConfiguration.txt";

        public Vector3 LightColor;
        public Vector3 AmbienceColor;
        public float LightDirectionZ;
        public float LightDistance;
        public float LightOpacity;

        public bool AmbienceColorOnly { get; set; }
    }
}
