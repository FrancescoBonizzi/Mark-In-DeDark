using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Characters
{
    public class CharacterTorchConfiguration
    {
        public Color FirstTorchColor { get; }
        public Color SecondTorchColor { get; }
        public float TorchScale { get; }
        public Vector2 TorchPositionTraslationUp { get; }
        public Vector2 TorchPositionTraslationLeft { get; }
        public Vector2 TorchPositionTraslationRight { get; }
        public Vector2 TorchPositionTraslationDown { get; }
        public float TorchScaleMultiplierForPenumbra { get; }

        public CharacterTorchConfiguration(
            Color firstTorchColor,
            Color secondTorchColor,
            float torchScale,
            float torchScaleMultiplierForPenumbra,
            Vector2 torchPositionTraslationUp,
            Vector2 torchPositionTraslationLeft,
            Vector2 torchPositionTraslationRight,
            Vector2 torchPositionTraslationDown)
        {
            FirstTorchColor = firstTorchColor;
            SecondTorchColor = secondTorchColor;
            TorchScale = torchScale;
            TorchScaleMultiplierForPenumbra = torchScaleMultiplierForPenumbra;
            TorchPositionTraslationUp = torchPositionTraslationUp;
            TorchPositionTraslationLeft = torchPositionTraslationLeft;
            TorchPositionTraslationRight = torchPositionTraslationRight;
            TorchPositionTraslationDown = torchPositionTraslationDown;
        }
    }
}
