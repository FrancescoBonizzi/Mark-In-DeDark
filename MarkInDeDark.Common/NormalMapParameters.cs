using Microsoft.Xna.Framework;

namespace MarkInDeDark.Common
{
    public class NormalMapParameters
    {
        public const string FileName = "tempNormalMapConfiguration.txt";

        public Vector3 PlayerLightColor;
        public float PlayerLightDirectionZ;
        public float PlayerLightDistance;
        public float PlayerLightOpacity;

        public bool EditAmbienceLights { get; set; }
        public bool EditPlayerLights { get; set; }

        public Vector3 AmbienceColor;
        public Color PenumbraAmbienceColor { get; set; }
    }
}
