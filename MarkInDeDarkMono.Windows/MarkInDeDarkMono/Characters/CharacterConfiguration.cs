using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;

namespace MarkInDeDarkMono.Characters
{
    public class CharacterConfiguration
    {
        public CharacterConfiguration(
            AssetsLoader assetsLoader,
            string characterName,
            Vector2 startingPosition,
            Color firstTorchColor,
            Color secondTorchColor,
            float torchScale,
            float movementSpeed,
            Vector2 torchPositionTraslationUp,
            Vector2 torchPositionTraslationLeft,
            Vector2 torchPositionTraslationRight,
            Vector2 torchPositionTraslationDown,
            bool withDieAnimation = true)
        {
            if (withDieAnimation)
            {
                DieAnimation = assetsLoader.GetAnimation($"{characterName}Dying");
            }

            IdleDownAnimation = assetsLoader.GetAnimation($"{characterName}IdleDown");
            IdleSideAnimation = assetsLoader.GetAnimation($"{characterName}IdleSide");
            IdleUpAnimation = assetsLoader.GetAnimation($"{characterName}IdleUp");
            WalkDownAnimation = assetsLoader.GetAnimation($"{characterName}WalkDown");
            WalkSideAnimation = assetsLoader.GetAnimation($"{characterName}WalkSide");
            WalkUpAnimation = assetsLoader.GetAnimation($"{characterName}WalkUp");
            LightMapSprite = assetsLoader.Sprites["light_1"];
            TorchSprite = assetsLoader.Sprites["light_pixel"];

            StartingPosition = startingPosition;
            FirstTorchColor = firstTorchColor;
            SecondTorchColor = secondTorchColor;
            TorchScale = torchScale;
            MovementSpeed = movementSpeed;
            TorchPositionTraslationUp = torchPositionTraslationUp;
            TorchPositionTraslationLeft = torchPositionTraslationLeft;
            TorchPositionTraslationRight = torchPositionTraslationRight;
            TorchPositionTraslationDown = torchPositionTraslationDown;
        }

        public Vector2 StartingPosition { get; }

        public Sprite LightMapSprite { get; }
        public Sprite TorchSprite { get; }
        public Color FirstTorchColor { get; }
        public Color SecondTorchColor { get; }
        public float TorchScale { get; set; }
        public float MovementSpeed { get; }

        public Vector2 TorchPositionTraslationUp { get; }
        public Vector2 TorchPositionTraslationLeft { get; }
        public Vector2 TorchPositionTraslationRight { get; }
        public Vector2 TorchPositionTraslationDown { get; }

        public SpriteAnimation WalkUpAnimation { get; }
        public SpriteAnimation WalkDownAnimation { get; }
        public SpriteAnimation WalkSideAnimation { get; }
        public SpriteAnimation IdleUpAnimation { get; }
        public SpriteAnimation IdleDownAnimation { get; }
        public SpriteAnimation IdleSideAnimation { get; }
        public SpriteAnimation DieAnimation { get; }
    }
}
