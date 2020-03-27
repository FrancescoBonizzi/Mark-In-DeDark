using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;

namespace MarkInDeDarkMono.Characters
{
    public class CharacterConfiguration
    {
        public Vector2 StartingPosition { get; }
        public float TorchScale { get; set; }
        public float MovementSpeed { get; }
        public CharacterTorchConfiguration CharacterTorchConfiguration { get; }
        public SpriteAnimation WalkUpAnimation { get; }
        public SpriteAnimation WalkDownAnimation { get; }
        public SpriteAnimation WalkSideAnimation { get; }
        public SpriteAnimation IdleUpAnimation { get; }
        public SpriteAnimation IdleDownAnimation { get; }
        public SpriteAnimation IdleSideAnimation { get; }
        public SpriteAnimation DieAnimation { get; }

        public CharacterConfiguration(
            AssetsLoader assetsLoader,
            string characterName,
            Vector2 startingPosition,
            CharacterTorchConfiguration characterTorchConfiguration,
            float movementSpeed,
          
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

            StartingPosition = startingPosition;
            MovementSpeed = movementSpeed;
            CharacterTorchConfiguration = characterTorchConfiguration;
        }
    }
}
