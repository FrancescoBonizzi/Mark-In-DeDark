using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;

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
        public Hull Hull { get; }
        public Vector2 Position => _wallDoorDownDrawingInfos.Position;

        private readonly DrawingInfos _wallDoorDownDrawingInfos;
        private readonly DrawingInfos _wallDoorArcDrawingInfos;
        private readonly DrawingInfos _doorAnimationDrawingInfos;

        private readonly Sprite _wallDoorUpSprite;
        private readonly Sprite _wallDoorDownSprite;

        private DoorStates _currentState;
        private readonly Action _onClose;

        private readonly SpriteAnimation _doorOpenAnimation;
        private readonly SpriteAnimation _doorCloseAnimation;
        private SpriteAnimation _currentAnimation;

        public WallInteractiveDoor(
            Vector2 doorPosition,
            LinkDirection linkDirection,
            SpriteAnimation doorOpenAnimation,
            SpriteAnimation doorCloseAnimation,
            Sprite wallDoorArc,
            Sprite wallDoorDown,
            Action onClose)
        {
            _wallDoorUpSprite = wallDoorArc;
            _wallDoorDownSprite = wallDoorDown;

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

            var doorAnimationPosition = doorPosition + new Vector2(0f, -15f);
            _doorAnimationDrawingInfos = new DrawingInfos()
            {
                Position = doorAnimationPosition,
                LayerDepth = ZIndexCalculator.ForWall(doorAnimationPosition + doorOpenAnimation.FirstFrameSprite.SpriteCenter)
            };

            _doorOpenAnimation = doorOpenAnimation;
            _doorCloseAnimation = doorCloseAnimation;
            
            _currentAnimation = _doorOpenAnimation;
            _currentState = DoorStates.Open;

            _onClose = onClose;

            // Quelli orizzontali hanno un boundingbox più stretto (alla base del muro)
            CollisionRectangle = new Rectangle(
                (int)doorPosition.X,
                (int)doorPosition.Y + 100,
                _wallDoorDownSprite.Width,
                40);

            Hull = Utilities.HullFromRectangle(CollisionRectangle);

            Hull.Points.Add(new Vector2(0, 0));
            Hull.Points.Add(new Vector2(0, 1));
            Hull.Points.Add(new Vector2(1, 1));
            Hull.Points.Add(new Vector2(1, 0));
        }

        public void Open()
        {
            _currentState = DoorStates.Opening;
            _currentAnimation = _doorOpenAnimation;
            _currentAnimation.Stop();
            _currentAnimation.Play();
            Hull.Enabled = false;
        }

        public void Close()
        {
            _currentState = DoorStates.Closing;
            _currentAnimation = _doorCloseAnimation;
            _currentAnimation.Stop();
            _currentAnimation.Play();
            Hull.Enabled = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_wallDoorDownSprite, _wallDoorDownDrawingInfos);
            spriteBatch.Draw(_wallDoorUpSprite, _wallDoorArcDrawingInfos);
            spriteBatch.Draw(_currentAnimation.CurrentFrame, _doorAnimationDrawingInfos);
        }

        public void Update(TimeSpan elapsed)
        {
            _currentAnimation.Update(elapsed);
            
            switch (_currentState)
            {
                case DoorStates.Closing:

                    if (_currentAnimation.HasFinishedPlaying)
                    {
                        _currentState = DoorStates.Closed;
                        _onClose?.Invoke();
                    }

                    break;

                case DoorStates.Opening:

                    if (_currentAnimation.HasFinishedPlaying)
                    {
                        _currentState = DoorStates.Open;
                    }

                    break;
            }
        }

    }
}