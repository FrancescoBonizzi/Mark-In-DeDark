using FbonizziMonoGame;
using FbonizziMonoGame.Drawing;
using MarkInDeDarkMono.Levels;
using MarkInDeDarkMono.Maths;
using Microsoft.Xna.Framework;
using System;

namespace MarkInDeDarkMono.Objects
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

        public Vector2 Size { get; }
        public LinkDirection DoorDirectionInCell { get; }
        public DrawingInfos DrawingInfos { get; private set; }

        private readonly Vector2 _startingPosition;
        private DoorStates _currentState;
        private readonly float _rotationSpeed = 4f;
        private readonly Action _onClose;
        private TimeSpan _totalElapsed;

        private float _openPositionRotation;
        private float _closedPositionRotation;

        public Door(
            Vector2 position,
            LinkDirection linkDirection,
            Vector2 size,
            Action onClose = null)
        {
            Size = size;
            DoorDirectionInCell = linkDirection;

            DrawingInfos = new DrawingInfos()
            {
                Position = position,
                Rotation = _closedPositionRotation
            };

            _startingPosition = position;

            _currentState = DoorStates.Closed;
            SetNewPinPointToOpen(linkDirection);

            _onClose = onClose;
        }

        private void SetNewPinPointToOpen(LinkDirection openingDirection)
        {
            switch (openingDirection)
            {
                case LinkDirection.NorthLink:

                    if (DoorDirectionInCell == LinkDirection.NorthLink || DoorDirectionInCell == LinkDirection.SouthLink)
                    {
                        DrawingInfos.Origin = new Vector2(0f, Size.Y / 2 - 11f);
                        DrawingInfos.Position = _startingPosition + new Vector2(0f, -11f);
                        _openPositionRotation = -(float)Math.PI / 2.0f;
                        _closedPositionRotation = 0f;
                    }

                    break;

                case LinkDirection.SouthLink:

                    if (DoorDirectionInCell == LinkDirection.NorthLink || DoorDirectionInCell == LinkDirection.SouthLink)
                    {
                        DrawingInfos.Origin = new Vector2(0f, Size.Y / 2 + 11f);
                        DrawingInfos.Position = _startingPosition + new Vector2(0f, +11f);
                        _openPositionRotation = (float)Math.PI / 2.0f;
                        _closedPositionRotation = 0f;
                    }

                    break;

                case LinkDirection.EastLink:

                    if (DoorDirectionInCell == LinkDirection.WestLink || DoorDirectionInCell == LinkDirection.EastLink)
                    {
                        DrawingInfos.Origin = new Vector2(0f, Size.Y / 2 - 11f);
                        DrawingInfos.Position = _startingPosition + new Vector2(+11f, 0);
                        _openPositionRotation = 0f;
                        _closedPositionRotation = (float)Math.PI / 2;
                    }

                    break;

                case LinkDirection.WestLink:

                    if (DoorDirectionInCell == LinkDirection.WestLink || DoorDirectionInCell == LinkDirection.EastLink)
                    {
                        DrawingInfos.Origin = new Vector2(0f, Size.Y / 2 + 11f);
                        DrawingInfos.Position = _startingPosition + new Vector2(-11f, 0);
                        _openPositionRotation = (float)Math.PI;
                        _closedPositionRotation = (float)Math.PI / 2;
                    }

                    break;
            }

            // Per levare le approssimazioni del Lerp, che fanno scattare subito l'equals in range
            // Devo aprire, quindi impongo la posizione da chiuso
            DrawingInfos.Rotation = _closedPositionRotation;
        }

        public void Open(LinkDirection openingDirection)
        {
            SetNewPinPointToOpen(openingDirection);
            _currentState = DoorStates.Opening;
            _totalElapsed = TimeSpan.Zero;
        }

        public void Close()
        {
            _currentState = DoorStates.Closing;
            _totalElapsed = TimeSpan.Zero;
        }

        public void Update(TimeSpan elapsed)
        {
            switch (_currentState)
            {
                case DoorStates.Closing:
                    _totalElapsed += elapsed;

                    if (DrawingInfos.Rotation.IsEqualToInRange(_closedPositionRotation))
                    {
                        _currentState = DoorStates.Closed;
                        if (_onClose != null)
                            _onClose();
                    }
                    else
                    {
                        DrawingInfos.Rotation = MathHelper.Lerp(
                            _openPositionRotation,
                            _closedPositionRotation,
                            (float)(_totalElapsed.TotalSeconds * _rotationSpeed));
                    }

                    break;

                case DoorStates.Opening:
                    _totalElapsed += elapsed;

                    if (DrawingInfos.Rotation.IsEqualToInRange(_openPositionRotation))
                    {
                        _currentState = DoorStates.Open;
                    }
                    else
                    {
                        DrawingInfos.Rotation = MathHelper.Lerp(
                            _closedPositionRotation,
                            _openPositionRotation,
                            (float)(_totalElapsed.TotalSeconds * _rotationSpeed));
                    }

                    break;
            }
        }

    }
}