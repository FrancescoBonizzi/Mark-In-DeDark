using FbonizziMonoGame;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MarkInDeDark.Library.Particles
{
    public class CharacterExplosionParticleSystem
    {
        private readonly List<BodyPartParticle> _characterBodyPartsParticles;
        private readonly List<Sprite> _bodyParts;
        private readonly Sprite _bloodParticleSprite;
        private bool _withFading;

        public bool HasFinished { get; private set; }
        public bool HasStarted { get; private set; }

        public CharacterExplosionParticleSystem(
            CharacterConfiguration characterConfiguration,
            AssetsLoader assetsLoader)
        {
            _characterBodyPartsParticles = new List<BodyPartParticle>();
            _bodyParts = characterConfiguration.BodyParts;
            _bloodParticleSprite = assetsLoader.Sprites["Effects/particle"];
        }

        public void Explode(
            Vector2 where,
            float layerDepth,
            TimeSpan particleLife,
            bool withFading)
        {
            _withFading = withFading;

            foreach (var bodyPartSprite in _bodyParts)
            {
                var angle = Numbers.RandomBetween(0f, 360f);
                angle = MathHelper.ToRadians(-angle);
                var velocity = new Vector2(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle))
                    * Numbers.RandomBetween(80, 140);

                _characterBodyPartsParticles.Add(new BodyPartParticle(
                    sprite: bodyPartSprite,
                    bloodParticleSprite: _bloodParticleSprite,
                    position: where,
                    velocity: velocity,
                    acceleration: Vector2.Zero,
                    initialRotation: angle,
                    rotationSpeed: (float)(Numbers.RandomBetween(0D, 1D) * 5),
                    color: Color.White,
                    scale: 1f,
                    lifetime: particleLife,
                    layerDepth: layerDepth));
            }

            HasStarted = true;
        }

        public void Update(TimeSpan elapsed)
        {
            if (HasFinished)
                return;

            if (_characterBodyPartsParticles.Count == 0)
                HasFinished = true;

            for (int i = _characterBodyPartsParticles.Count - 1; i >= 0; --i)
            {
                _characterBodyPartsParticles[i].Update(elapsed);

                if (!_characterBodyPartsParticles[i].IsActive)
                    _characterBodyPartsParticles.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var particle in _characterBodyPartsParticles)
            {
                if (_withFading)
                {
                    double normalizedLifetime = particle.TimeSinceStart.TotalSeconds / particle.LifeTime.TotalSeconds;
                    double alpha = 1 - normalizedLifetime;
                    particle.OverlayColor = particle.StartingColor.WithAlpha((float)alpha);
                }

                spriteBatch.Draw(particle.Sprite, particle);
                particle.BodyPartBloodParticleSystem.Draw(spriteBatch);
            }
        }

    }
}
