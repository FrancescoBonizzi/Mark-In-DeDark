using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Levels
{
    public enum DoorStates
    {
        Closed,
        Closing,
        Open,
        Opening
    }

    /// <summary>
    /// La porta si apre sempre nella direzione opposta la player 
    /// e cambia il suo punto di perno di conseguenza per fare in modo che si apra sempre alla sua destra
    /// </summary>
    public class WallInteractiveDoor : IWall
    {
        public bool IsMoving => _currentState == DoorStates.Closing || _currentState == DoorStates.Opening;
        public bool IsOpen => _currentState == DoorStates.Open;

        public LinkDirection Direction { get; }

        public Rectangle CollisionRectangle { get; }

        private readonly Hull _hullLeft;
        private readonly Hull _hullRight;
        public List<Hull> Hulls { get; }

        public Vector2 Position => _wallDoorDownDrawingInfos.Position;
        public Vector2 CenterPosition => _wallDoorDownDrawingInfos.Position + _wallDoorDownSprite.SpriteCenter;

        private readonly DrawingInfos _wallDoorDownDrawingInfos;
        private readonly DrawingInfos _wallDoorArcDrawingInfos;
        private readonly DrawingInfos _doorAnimationDrawingInfos;

        private readonly Sprite _wallDoorUpSprite;
        private readonly Sprite _wallDoorDownSprite;

        private readonly Sprite _wallDoorUpNormalSprite;
        private readonly Sprite _wallDoorDownNormalSprite;

        private DoorStates _currentState;
        private readonly Action _onClose;

        private readonly SpriteAnimation _doorOpenAnimation;
        private readonly SpriteAnimation _doorCloseAnimation;
        private SpriteAnimation _currentAnimation;

        private readonly double _doorAnimationDurationMilliseconds;

        private TimeSpan _currentAnimationDuration;
        private readonly float _openDoorHullWidth;
        private readonly float _closedDoorHullWidth;

        public float ObscurationOpacity { get; set; }
 
        public WallInteractiveDoor(
            Vector2 doorPosition,
            LinkDirection linkDirection,
            SpriteAnimation doorOpenAnimation,
            SpriteAnimation doorCloseAnimation,
            Sprite wallDoorArc,
            Sprite wallDoorDown,
            Sprite wallDoorArcNormal,
            Sprite wallDoorDownNormal,
            Action onClose)
        {
            _wallDoorUpSprite = wallDoorArc;
            _wallDoorDownSprite = wallDoorDown;
            _wallDoorUpNormalSprite = wallDoorArcNormal;
            _wallDoorDownNormalSprite = wallDoorDownNormal;

            Direction = linkDirection;

            _wallDoorDownDrawingInfos = new DrawingInfos()
            {
                Position = doorPosition,
                LayerDepth = ZIndexCalculator.ForWall(doorPosition + _wallDoorDownSprite.SpriteCenter)
            };

            _wallDoorArcDrawingInfos = new DrawingInfos()
            {
                Position = doorPosition,
                LayerDepth = ZIndexCalculator.OverAllLayerDepth
            };

            var doorAnimationPosition = doorPosition + new Vector2(0f, -11f);
            _doorAnimationDrawingInfos = new DrawingInfos()
            {
                Position = doorAnimationPosition,
                LayerDepth = _wallDoorDownDrawingInfos.LayerDepth + 0.0001f
            };

            // Copio le animazioni altrimenti apro tante porte contemporeanamente
            _doorOpenAnimation = new SpriteAnimation(doorOpenAnimation);
            _doorCloseAnimation = new SpriteAnimation(doorCloseAnimation);

            _onClose = onClose;

            // Quelli orizzontali hanno un boundingbox più stretto (alla base del muro)
            CollisionRectangle = new Rectangle(
                (int)doorPosition.X,
                (int)doorPosition.Y + 100,
                _wallDoorDownSprite.Width,
                40);

            _openDoorHullWidth = 60f;
            _closedDoorHullWidth = _wallDoorDownSprite.Width / 2;

            _currentAnimation = _doorOpenAnimation;
            _currentState = DoorStates.Open;

            // Left
            _hullLeft = new Hull()
            {
                Origin = Vector2.Zero,
                Position = new Vector2(doorPosition.X - 5, doorPosition.Y + 135),
                Scale = new Vector2(_openDoorHullWidth, 5)
            };

            // Right
            _hullRight = new Hull()
            {
                Origin = new Vector2(1, 0), // Considera i Points. Mi gioco l'origine per allargare o stringere l'Hull
                Position = new Vector2(doorPosition.X + _wallDoorDownSprite.Width + 8, doorPosition.Y + 135),
                Scale = new Vector2(_openDoorHullWidth, 5)
            };

            _hullLeft.Points.Add(new Vector2(0, 0));
            _hullLeft.Points.Add(new Vector2(0, 1));
            _hullLeft.Points.Add(new Vector2(1, 1));
            _hullLeft.Points.Add(new Vector2(1, 0));

            _hullRight.Points.Add(new Vector2(0, 0));
            _hullRight.Points.Add(new Vector2(0, 1));
            _hullRight.Points.Add(new Vector2(1, 1));
            _hullRight.Points.Add(new Vector2(1, 0));

            Hulls = new List<Hull>()
            {
                _hullLeft,
                _hullRight
            };

            _doorAnimationDurationMilliseconds = _doorOpenAnimation.FrameDuration.TotalMilliseconds * _doorOpenAnimation.Frames.Length;
        }

        public void Open()
        {
            _currentState = DoorStates.Opening;
            _currentAnimation = _doorOpenAnimation;
            _currentAnimation.Stop();
            _currentAnimation.Play();
            _currentAnimationDuration = TimeSpan.Zero;
        }

        public void Close()
        {
            _currentState = DoorStates.Closing;
            _currentAnimation = _doorCloseAnimation;
            _currentAnimation.Stop();
            _currentAnimation.Play();
            _currentAnimationDuration = TimeSpan.Zero;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_wallDoorDownSprite, _wallDoorDownDrawingInfos);
            spriteBatch.Draw(_wallDoorUpSprite, _wallDoorArcDrawingInfos);
            spriteBatch.Draw(_currentAnimation.CurrentFrame, _doorAnimationDrawingInfos);

            // Per debug
            //spriteBatch.DrawRectangle(
            //    new Rectangle(
            //        (int)_hullLeft.Position.X,
            //        (int)_hullLeft.Position.Y,
            //        (int)_hullLeft.Scale.X,
            //        (int)_hullLeft.Scale.Y),
            //    Color.Red,
            //    Vector2.Zero,
            //    1);

            //spriteBatch.DrawRectangle(
            //    new Rectangle(
            //        (int)_hullRight.Position.X,
            //        (int)_hullRight.Position.Y,
            //        (int)_hullRight.Scale.X,
            //        (int)_hullRight.Scale.Y),
            //    Color.Red,
            //    _hullRight.Origin,
            //    1);
        }

        public void Update(TimeSpan elapsed)
        {
            _currentAnimation.Update(elapsed);

            switch (_currentState)
            {
                case DoorStates.Closing:

                    _currentAnimationDuration += elapsed;

                    if (_currentAnimation.HasFinishedPlaying)
                    {
                        _currentState = DoorStates.Closed;
                        _onClose?.Invoke();
                    }

                    _hullLeft.Scale = new Vector2(MathHelper.Lerp(
                        _openDoorHullWidth,
                        _closedDoorHullWidth,
                        (float)(_currentAnimationDuration.TotalMilliseconds / _doorAnimationDurationMilliseconds)),
                        _hullLeft.Scale.Y);

                    _hullRight.Scale = new Vector2(MathHelper.Lerp(
                        _openDoorHullWidth,
                        _closedDoorHullWidth,
                        (float)(_currentAnimationDuration.TotalMilliseconds / _doorAnimationDurationMilliseconds)),
                        _hullRight.Scale.Y);

                    break;

                case DoorStates.Opening:

                    _currentAnimationDuration += elapsed;

                    if (_currentAnimation.HasFinishedPlaying)
                    {
                        _currentState = DoorStates.Open;
                    }

                    _hullLeft.Scale = new Vector2(MathHelper.Lerp(
                        _closedDoorHullWidth,
                        _openDoorHullWidth,
                        (float)(_currentAnimationDuration.TotalMilliseconds / _doorAnimationDurationMilliseconds)),
                        _hullLeft.Scale.Y);

                    _hullRight.Scale = new Vector2(MathHelper.Lerp(
                        _closedDoorHullWidth,
                        _openDoorHullWidth,
                        (float)(_currentAnimationDuration.TotalMilliseconds / _doorAnimationDurationMilliseconds)),
                        _hullRight.Scale.Y);

                    break;
            }
        }

        public void DrawNormalMap(SpriteBatch spriteBatch)
        {
            if (_wallDoorDownNormalSprite != null)
                spriteBatch.Draw(_wallDoorDownNormalSprite, _wallDoorDownDrawingInfos);
            if (_wallDoorUpNormalSprite != null)
                spriteBatch.Draw(_wallDoorUpNormalSprite, _wallDoorArcDrawingInfos);
        }
    }
}