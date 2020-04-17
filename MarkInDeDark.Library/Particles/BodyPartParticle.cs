using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Sprites;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDark.Library.Particles
{
    public class BodyPartParticle : DrawingInfos
    {
        public Vector2 Velocity { get; private set; }
        public Vector2 Acceleration { get; private set; }
        public TimeSpan TimeSinceStart { get; private set; }
        public float InitalScale { get; private set; }
        public float RotationSpeed { get; private set; }
        public TimeSpan LifeTime { get; private set; }
        public Color StartingColor { get; private set; }
        
        public Sprite Sprite { get; }

        public bool IsActive
            => TimeSinceStart < LifeTime || OverlayColor.A < 0;

        public BodyPartParticle(
            Sprite sprite,
            Vector2 position,
            Vector2 velocity,
            Vector2 acceleration,
            float initialRotation,
            float rotationSpeed,
            Color color,
            float scale,
            TimeSpan lifetime,
            float layerDepth)
        {
            Sprite = sprite;
            Position = position;
            Velocity = velocity;
            Acceleration = acceleration;
            Rotation = initialRotation;
            RotationSpeed = rotationSpeed;
            OverlayColor = color;
            InitalScale = scale;
            Scale = InitalScale;
            LifeTime = lifetime;
            LayerDepth = layerDepth;
            TimeSinceStart = TimeSpan.Zero;
            StartingColor = color;

            Origin = sprite.SpriteCenter;
        }

        public void Update(TimeSpan elapsed)
        {
            float elapsedSeconds = (float)elapsed.TotalSeconds;
            Velocity += Acceleration * elapsedSeconds;
            Position += Velocity * elapsedSeconds;
            Rotation += RotationSpeed * elapsedSeconds;

            TimeSinceStart += elapsed;
        }
    }
}
