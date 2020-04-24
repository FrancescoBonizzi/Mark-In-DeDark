using FbonizziMonoGame.Sprites;

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Characters
{
    public class CharacterConfiguration
    {
        public Vector2 StartingPosition { get; }
        public float TorchScale { get; set; }
        public float MovementSpeed { get; }

        public CharacterTorchConfiguration CharacterTorchConfiguration { get; }
        public CharacterShadowConfiguration CharacterShadowConfiguration { get; }

        public SpriteAnimation WalkUpAnimation { get; }
        public SpriteAnimation WalkDownAnimation { get; }
        public SpriteAnimation WalkSideAnimation { get; }
        public SpriteAnimation IdleUpAnimation { get; }
        public SpriteAnimation IdleDownAnimation { get; }
        public SpriteAnimation IdleSideAnimation { get; }

        public List<Sprite> BodyParts { get; }
        
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
                var folderName = $"{char.ToUpper(characterName[0]) + characterName.Substring(1)}/Death";
                BodyParts = new List<Sprite>
                {
                    assetsLoader.Sprites[$"{folderName}/{characterName}_armL"],
                    assetsLoader.Sprites[$"{folderName}/{characterName}_armR"],
                    assetsLoader.Sprites[$"{folderName}/{characterName}_guts"],
                    assetsLoader.Sprites[$"{folderName}/{characterName}_head"],
                    assetsLoader.Sprites[$"{folderName}/{characterName}_legL"],
                    assetsLoader.Sprites[$"{folderName}/{characterName}_legR"],
                    assetsLoader.Sprites[$"{folderName}/{characterName}_torso"]
                };
            }

            IdleDownAnimation = assetsLoader.GetAnimation($"{characterName}IdleDown");
            IdleSideAnimation = assetsLoader.GetAnimation($"{characterName}IdleSide");
            IdleUpAnimation = assetsLoader.GetAnimation($"{characterName}IdleUp");
            WalkDownAnimation = assetsLoader.GetAnimation($"{characterName}WalkDown");
            WalkSideAnimation = assetsLoader.GetAnimation($"{characterName}WalkSide");
            WalkUpAnimation = assetsLoader.GetAnimation($"{characterName}WalkUp");

            var shadowType = "GenericCharacter";
            if (characterName == "goblin" || characterName == "skeleton" || characterName == "player")
                shadowType = "TorchCharacter";

            CharacterShadowConfiguration = new CharacterShadowConfiguration(
                shadowType,
                assetsLoader.GetAnimation($"{shadowType}WhileWalkingUpShadow"),
                assetsLoader.GetAnimation($"{shadowType}WhileWalkingDownShadow"),
                assetsLoader.GetAnimation($"{shadowType}WhileWalkingSideShadow"),
                assetsLoader.GetAnimation($"{shadowType}WhileIdleUpShadow"),
                assetsLoader.GetAnimation($"{shadowType}WhileIdleDownShadow"),
                assetsLoader.GetAnimation($"{shadowType}WhileIdleSideShadow"));

            StartingPosition = startingPosition;
            MovementSpeed = movementSpeed;
            CharacterTorchConfiguration = characterTorchConfiguration;
        }
    }
}
