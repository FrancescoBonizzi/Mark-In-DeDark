using MarkInDeDarkMono.Characters;
using Microsoft.Xna.Framework.Input;

namespace MarkInDeDarkMono
{
    public class MarkKeyboard
    {
        private CharacterActions _currentAction;
        private CharacterActions _lastAction;

        private KeyboardState _lastState;

        public CharacterActions CurrentInputAction
        {
            get
            {
                var toReturn = _currentAction;
                _currentAction = CharacterActions.Idle;

                return toReturn;
            }
        }

        public void Update()
        {
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Up))
            {
                _currentAction = CharacterActions.Up;
            }
            else if (keyState.IsKeyDown(Keys.Down))
            {
                _currentAction = CharacterActions.Down;
            }
            else if (keyState.IsKeyDown(Keys.Right))
            {
                _currentAction = CharacterActions.Right;
            }
            else if (keyState.IsKeyDown(Keys.Left))
            {
                _currentAction = CharacterActions.Left;
            }

            if (keyState.IsKeyDown(Keys.Space) && _lastState.IsKeyUp(Keys.Space))
            {
                _currentAction = CharacterActions.Candle;
            }

            if ((keyState.IsKeyDown(Keys.LeftControl) && _lastState.IsKeyUp(Keys.LeftControl))
                || (keyState.IsKeyDown(Keys.LeftControl) && _lastState.IsKeyUp(Keys.LeftControl)))
            {
                _currentAction = CharacterActions.DoorSwitch;
            }

            _lastAction = _currentAction;
            _lastState = keyState;
        }
    }
}
