using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Particles.Snowing
{
    public class SnowingLayer
    {
        private readonly Camera2D _camera;
        private readonly Sprite _snowFlakeParticle;

        private TimeSpan _currentParticleGenerationElapsed;
        private TimeSpan _currentWindElapsed;
        
        private readonly List<SnowFlake> _snowFlakes;
        private Rectangle _cameraBoundingRectangle;
        private readonly int _heightOffset;

        public TimeSpan ParticleGenerationInterval { get; } = TimeSpan.FromSeconds(1f);

        /// <summary>
        /// How often we apply the wind to our particles
        /// </summary>
        public TimeSpan WindInterval { get; } = TimeSpan.FromSeconds(4);

        public int MaxParticles { get; } = 5;

        public SnowingLayer(
            Sprite snowFlakeParticle, 
            Camera2D camera,
            int heightOffset)
        {
            _camera = camera;
            _snowFlakeParticle = snowFlakeParticle;
            _snowFlakes = new List<SnowFlake>();
            
            _heightOffset = heightOffset;
            _cameraBoundingRectangle = _camera.BoundingRectangle;
        }

        private void ApplyWindVelocityToAllParticles()
        {
            var xSway = Numbers.RandomBetween(-20, 20);
            foreach (var particle in _snowFlakes)
                particle.Velocity += new Vector2((xSway * particle.Scale) / 50, 0);
        }

        private SnowFlake GenerateParticle()
        {
            var sprite = new SnowFlake(_snowFlakeParticle);

            var xPosition = Numbers.RandomBetween(_camera.BoundingRectangle.X, _camera.BoundingRectangle.Width);
            var ySpeed = Numbers.RandomBetween(70, 75) / 100f;

            sprite.Position = new Vector2(xPosition, -sprite.Bounds.Height);
            sprite.Opacity = Numbers.RandomBetween(0f, 1f);
            sprite.Velocity = new Vector2(0, ySpeed);

            return sprite;
        }

        public void Update(TimeSpan elapsed)
        {
            _currentParticleGenerationElapsed += elapsed;
            _currentWindElapsed += elapsed;
            _cameraBoundingRectangle = _camera.BoundingRectangle;

            AddParticles();

            if (_currentWindElapsed > WindInterval)
            {
                _currentWindElapsed = TimeSpan.Zero;
                ApplyWindVelocityToAllParticles();
            }

            foreach (var particle in _snowFlakes)
            {
                if (!IsOnScreen(particle))
                    particle.IsRemoved = true;

                particle.Update(elapsed);
            }

            RemovedFinishedParticles();
        }

        private void RemovedFinishedParticles()
        {
            for (int i = 0; i < _snowFlakes.Count; i++)
            {
                if (_snowFlakes[i].IsRemoved)
                {
                    _snowFlakes.RemoveAt(i);
                    --i;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var particle in _snowFlakes)
                particle.Draw(spriteBatch);
        }

        private bool IsOnScreen(SnowFlake snowFlake)
        {
            return snowFlake.Position.Y >= _cameraBoundingRectangle.Y
                && snowFlake.Position.Y <= (_cameraBoundingRectangle.Y + _cameraBoundingRectangle.Height + _heightOffset)
                && snowFlake.Position.X >= 0
                && snowFlake.Position.X >= (_cameraBoundingRectangle.X + _cameraBoundingRectangle.Width);
        }

        private void AddParticles()
        {
            if (_currentParticleGenerationElapsed > ParticleGenerationInterval)
            {
                _currentParticleGenerationElapsed = TimeSpan.Zero;

                if (_snowFlakes.Count < MaxParticles)
                {
                    _snowFlakes.Add(GenerateParticle());
                }
            }
        }
    }

}
