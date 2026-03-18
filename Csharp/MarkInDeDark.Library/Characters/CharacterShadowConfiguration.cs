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

            if (shadowType == "GenericCharacter")
            {
                ShadowPositionTraslationUp = new Vector2(0, 20f);
                ShadowPositionTraslationLeft = new Vector2(0, 20f);
                ShadowPositionTraslationRight = new Vector2(0, 20f);
                ShadowPositionTraslationDown = new Vector2(0, 20f);

                ShadowPositionTraslationDownIdle = Vector2.Zero;
            }
            else
            {
                ShadowPositionTraslationUp = new Vector2(32, 32);
                ShadowPositionTraslationLeft = new Vector2(32, 0);
                ShadowPositionTraslationRight = new Vector2(-32, 0);
                ShadowPositionTraslationDown = new Vector2(-8, 0);
                ShadowPositionTraslationDownIdle = new Vector2(-32, 0);
            }
        }
    }
}
