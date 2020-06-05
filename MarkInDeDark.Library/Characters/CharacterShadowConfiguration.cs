using FbonizziMonoGame.Sprites;
using Microsoft.Xna.Framework;

namespace MarkInDeDark.Library.Characters
{
    public class CharacterShadowConfiguration
    {
        public SpriteAnimation ShadowWhileWalkingUpAnimation { get; }
        public SpriteAnimation ShadowWhileWalkingDownAnimation { get; }
        public SpriteAnimation ShadowWhileWalkingSideAnimation { get; }
        public SpriteAnimation ShadowWhileIdleUpAnimation { get; }
        public SpriteAnimation ShadowWhileIdleDownAnimation { get; }
        public SpriteAnimation ShadowWhileIdleSideAnimation { get; }

        public Vector2 ShadowPositionTraslationDownIdle { get; }
        public Vector2 ShadowPositionTraslationUp { get; }
        public Vector2 ShadowPositionTraslationLeft { get; }
        public Vector2 ShadowPositionTraslationRight { get; }
        public Vector2 ShadowPositionTraslationDown { get; }

        public CharacterShadowConfiguration(
            string shadowType,
            SpriteAnimation shadowWhileWalkingUpAnimation,
            SpriteAnimation shadowWhileWalkingDownAnimation,
            SpriteAnimation shadowWhileWalkingSideAnimation,
            SpriteAnimation shadowWhileIdleUpAnimation,
            SpriteAnimation shadowWhileIdleDownAnimation,
            SpriteAnimation shadowWhileIdleSideAnimation)
        {
            ShadowWhileWalkingUpAnimation = shadowWhileWalkingUpAnimation;
            ShadowWhileWalkingDownAnimation = shadowWhileWalkingDownAnimation;
            ShadowWhileWalkingSideAnimation = shadowWhileWalkingSideAnimation;
            ShadowWhileIdleUpAnimation = shadowWhileIdleUpAnimation;
            ShadowWhileIdleDownAnimation = shadowWhileIdleDownAnimation;
            ShadowWhileIdleSideAnimation = shadowWhileIdleSideAnimation;

            // Up quando il personaggio guarda in su
            ShadowPositionTraslationUp = new Vector2(0, 35);

            // Left quando guarda a sinistra
            ShadowPositionTraslationLeft = new Vector2(0, 40);
            ShadowPositionTraslationRight = new Vector2(0, 40);
            ShadowPositionTraslationDown = new Vector2(0, 0);
            ShadowPositionTraslationDownIdle = new Vector2(0, 0);
        }
    }
}
