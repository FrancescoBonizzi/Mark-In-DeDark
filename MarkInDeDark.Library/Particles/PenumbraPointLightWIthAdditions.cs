using Microsoft.Xna.Framework;
using Penumbra;

namespace MarkInDeDark.Library.Particles
{
    public class PenumbraPointLightWithAdditions : PointLight
    {
        public float Z { get; set; } = 120f;
        public float Opacity { get; set; } = 1f;
        public float DistanceSquared { get; set; } = 40_000f;
        public Vector3 PositionForGround { get; set; }
        public Vector3 PositionForWall { get; set; }
        public Vector3 ColorNormal { get; set; } = Vector3.One;
    }
}
