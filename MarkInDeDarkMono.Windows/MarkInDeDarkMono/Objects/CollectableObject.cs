using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using FbonizziMonoGame.TransformationObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDarkMono.Objects
{
    public class CollectableObject
    {
        private readonly Sprite _sprite;
        private readonly DrawingInfos _drawingInfos;
        private readonly FadeObject _fadeObject;
        private readonly ScalingObject _scalingObject;
        private readonly SoundManager _soundManager;

        private bool _fadeOutCompleted;

        public bool IsCollected { get; private set; }

        public CollectableObject(
            Sprite sprite,
            Vector2 position,
            SoundManager soundManager)
        {
            _drawingInfos = new DrawingInfos()
            {
                Position = position,
                Origin = sprite.SpriteCenter,
                HitBoxTolerance = new Rectangle(50, 50, 50, 50)
            };

            _sprite = sprite;
            _fadeObject = new FadeObject(TimeSpan.FromMilliseconds(500), Color.White);
            _fadeObject.FadeOutCompleted += (sender, e) => _fadeOutCompleted = true;
            _scalingObject = new ScalingObject(maxScale: 1.0f, scalingSpeed: 2.0f);
            _soundManager = soundManager;
            _fadeOutCompleted = false;
        }

        public Rectangle CollisionRectangle
           => _drawingInfos.HitBox(
               _sprite.Width,
               _sprite.Height);

        public bool IsActive
            => !_fadeOutCompleted;

        public void Collect()
        {
            if (!IsCollected)
            {
                IsCollected = true;
                _soundManager.PlayCollectObject();
                _fadeObject.FadeOut();
            }
        }

        public bool IsFading
            => _fadeObject.IsFading;

        public void Update(TimeSpan elapsed)
        {
            if (!IsActive)
                return;

            if (IsFading)
            {
                _fadeObject.Update(elapsed);
                _drawingInfos.OverlayColor = _fadeObject.OverlayColor;
            }

            _scalingObject.Update(elapsed);
            _drawingInfos.Scale = _scalingObject.Scale;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive)
                return;

            spriteBatch.Draw(_sprite, _drawingInfos);
        }
    }
}
