using FbonizziMonoGame.Sprites;

using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Characters
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

        public SpriteAnimation ShadowWhileWalkingUpAnimation { get; }
        public SpriteAnimation ShadowWhileWalkingDownAnimation { get; }
        public SpriteAnimation ShadowWhileWalkingSideAnimation { get; }
        public SpriteAnimation ShadowWhileIdleUpAnimation { get; }
        public SpriteAnimation ShadowWhileIdleDownAnimation { get; }
        public SpriteAnimation ShadowWhileIdleSideAnimation { get; }

        public CharacterConfiguration(
            AssetsLoader assetsLoader,
            string characterName,
            Vector2 startingPosition,
            CharacterTorchConfiguration characterTorchConfiguration,
            float movementSpeed,
            bool withDieAnimation = true,
            bool withShadowAnimation = false)
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

            if (withShadowAnimation)
            {
                var shadowName = "GenericCharacter";
                if (characterName == "goblin" || characterName == "skeleton" || characterName == "player")
                    shadowName = "TorchCharacter";

                ShadowWhileWalkingUpAnimation = assetsLoader.GetAnimation($"{shadowName}WhileWalkingUpShadow");
                ShadowWhileWalkingDownAnimation = assetsLoader.GetAnimation($"{shadowName}WhileWalkingDownShadow");
                ShadowWhileWalkingSideAnimation = assetsLoader.GetAnimation($"{shadowName}WhileWalkingSideShadow");
                ShadowWhileIdleUpAnimation = assetsLoader.GetAnimation($"{shadowName}WhileIdleUpShadow");
                ShadowWhileIdleDownAnimation = assetsLoader.GetAnimation($"{shadowName}WhileIdleDownShadow");
                ShadowWhileIdleSideAnimation = assetsLoader.GetAnimation($"{shadowName}WhileIdleSideShadow");
            }

            StartingPosition = startingPosition;
            MovementSpeed = movementSpeed;
            CharacterTorchConfiguration = characterTorchConfiguration;
        }
    }
}
