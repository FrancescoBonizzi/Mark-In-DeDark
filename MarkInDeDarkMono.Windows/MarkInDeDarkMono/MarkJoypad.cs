using MarkInDeDarkMono.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MarkInDeDarkMono
{
    public class MarkJoypad
    {
        private CharacterActions _currentAction;

        private GamePadState _lastState;
        private PlayerIndex[] _possibleGamePads;

        private GamePadState? _currentState;

        public MarkJoypad()
        {
            _possibleGamePads = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));
        }

        public CharacterActions CurrentInputAction
        {
            get
            {
                var toReturn = _currentAction;
                _currentAction = CharacterActions.Idle;

                return toReturn;
            }
        }


        //public void Shake(int intensity)
        //{
        //    GamePad.SetVibration(
        //        PlayerIndex.One, 
        //        Intensity);
        //}

        public bool IsConnected
            => GetFirstConnectedGamePadState() != null;

        private GamePadState? GetFirstConnectedGamePadState()
        {
            foreach (var gamePad in _possibleGamePads)
            {
                var capabilities = GamePad.GetCapabilities(gamePad);
                if (capabilities.IsConnected && capabilities.GamePadType == GamePadType.GamePad)
                    return GamePad.GetState(gamePad);
            }

            return null;
        }

        public void Update()
        {
            _currentState = GetFirstConnectedGamePadState();
            if (_currentState == null)
                return;

            var gamePadState = _currentState.Value;

            if (gamePadState.ThumbSticks.Left.Y < -0.5f || gamePadState.DPad.Down == ButtonState.Pressed)
            {
                _currentAction = CharacterActions.Down;
            }
            else if (gamePadState.ThumbSticks.Left.Y > 0.5f || gamePadState.DPad.Up == ButtonState.Pressed)
            {
                _currentAction = CharacterActions.Up;
            }
            else if (gamePadState.ThumbSticks.Left.X < -0.5f || gamePadState.DPad.Left == ButtonState.Pressed)
            {
                _currentAction = CharacterActions.Left;
            }
            else if (gamePadState.ThumbSticks.Left.X > 0.5f || gamePadState.DPad.Right == ButtonState.Pressed)
            {
                _currentAction = CharacterActions.Right;
            }

            if (gamePadState.Buttons.A == ButtonState.Pressed && _lastState.Buttons.A == ButtonState.Released)
            {
                _currentAction = CharacterActions.Candle;
            }

            if (gamePadState.Buttons.B == ButtonState.Pressed && _lastState.Buttons.B == ButtonState.Released)
            {
                _currentAction = CharacterActions.DoorSwitch;
            }

            _lastState = gamePadState;
        }
    }
}
