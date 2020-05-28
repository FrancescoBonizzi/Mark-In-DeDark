using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Characters
{
    public class CharacterTorchConfiguration
    {
        public Color FirstTorchColor { get; }
        public Color SecondTorchColor { get; }
        public Vector2 TorchPositionTraslationUp { get; }
        public Vector2 TorchPositionTraslationLeft { get; }
        public Vector2 TorchPositionTraslationRight { get; }
        public Vector2 TorchPositionTraslationDown { get; }

        public float Z { get; }
        public float Opacity { get; }
        public float DistanceSquared { get; }

        public CharacterTorchConfiguration(
            Color firstTorchColor,
            Color secondTorchColor,
            Vector2 torchPositionTraslationUp,
            Vector2 torchPositionTraslationLeft,
            Vector2 torchPositionTraslationRight,
            Vector2 torchPositionTraslationDown,
            float z,
            float opacity,
            float distanceSquared)
        {
            FirstTorchColor = firstTorchColor;
            SecondTorchColor = secondTorchColor;
            TorchPositionTraslationUp = torchPositionTraslationUp;
            TorchPositionTraslationLeft = torchPositionTraslationLeft;
            TorchPositionTraslationRight = torchPositionTraslationRight;
            TorchPositionTraslationDown = torchPositionTraslationDown;
            Z = z;
            Opacity = opacity;
            DistanceSquared = distanceSquared;
        }
    }
}
