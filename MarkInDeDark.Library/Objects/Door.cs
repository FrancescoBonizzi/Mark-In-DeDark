using FbonizziMonoGame.Drawing;
using FbonizziMonoGame.Extensions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Extensions;
using MarkInDeDark.Library.Levels;
using MarkInDeDark.Library.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarkInDeDark.Library.Objects
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
    public class Door
    {
        public bool IsMoving => _currentState == DoorStates.Closing || _currentState == DoorStates.Opening;
        public bool IsOpen => _currentState == DoorStates.Open;

        public LinkDirection DoorDirectionInCell { get; }
        public DrawingInfos _wallDoorDownDrawingInfos;
        public DrawingInfos _wallDoorUpDrawingInfos;
        public DrawingInfos DoorAnimationDrawingInfos;

        private readonly Sprite _wallDoorUpSprite;
        private readonly Sprite _wallDoorDownSprite;

        private DoorStates _currentState;
        private readonly Action _onClose;

        private readonly SpriteAnimation _doorOpenAnimation;
        private readonly SpriteAnimation _doorCloseAnimation;
        private SpriteAnimation _currentAnimation;

        public Door(
            Vector2 doorPosition,
            LinkDirection linkDirection,
            AssetsLoader assetsLoader,
            Sprite wallDoorUp,
            Sprite wallDoorDown,
            Action onClose)
        {
            _wallDoorUpSprite = wallDoorUp;
            _wallDoorDownSprite = wallDoorDown;

            DoorDirectionInCell = linkDirection;

            _wallDoorDownDrawingInfos = new DrawingInfos()
            {
                Position = doorPosition,
                LayerDepth = ZIndexCalculator.ForWall(doorPosition)
            };

            _wallDoorUpDrawingInfos = new DrawingInfos()
            {
                Position = doorPosition,
                LayerDepth = ZIndexCalculator.OverAllLayerDepth
            };

            DoorAnimationDrawingInfos = new DrawingInfos()
            {
                Position = doorPosition,
                LayerDepth = ZIndexCalculator.ForWall(doorPosition)
            };

            _doorOpenAnimation = assetsLoader.GetAnimation("DoorOpen");
            _doorCloseAnimation = assetsLoader.GetAnimation("DoorClose");
            
            _currentAnimation = _doorOpenAnimation;
            _currentState = DoorStates.Open;

            _onClose = onClose;
        }

        public void Open()
        {
            _currentState = DoorStates.Opening;
            _currentAnimation = _doorOpenAnimation;
            _currentAnimation.Stop();
            _currentAnimation.Play();
        }

        public void Close()
        {
            _currentState = DoorStates.Closing;
            _currentAnimation = _doorCloseAnimation;
            _currentAnimation.Stop();
            _currentAnimation.Play();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_wallDoorDownSprite, _wallDoorDownDrawingInfos);
            spriteBatch.Draw(_wallDoorUpSprite, _wallDoorUpDrawingInfos);
            spriteBatch.Draw(_currentAnimation.CurrentFrame, DoorAnimationDrawingInfos);
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