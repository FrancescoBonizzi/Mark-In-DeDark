using MarkInDeDark.Library.Characters;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MarkInDeDark.Library
{
    public class MarkKeyboard
    {
        private KeyboardState _lastState;
        private readonly HashSet<CharacterActions> _actions;

        public MarkKeyboard()
        {
            _actions = new HashSet<CharacterActions>();
        }

        public HashSet<CharacterActions> Update()
        {
            var keyState = Keyboard.GetState();
            
            _actions.Clear();

            if (keyState.IsKeyDown(Keys.Up))
            {
                _actions.Add(CharacterActions.Up);
            }
            
            if (keyState.IsKeyDown(Keys.Down))
            {
                _actions.Add(CharacterActions.Down);
            }
            
            if (keyState.IsKeyDown(Keys.Right))
            {
                _actions.Add(CharacterActions.Right);
            }
            
            if (keyState.IsKeyDown(Keys.Left))
            {
                _actions.Add(CharacterActions.Left);
            }

            if (keyState.IsKeyDown(Keys.Space) && _lastState.IsKeyUp(Keys.Space))
            {
                _actions.Add(CharacterActions.Candle);
            }

            if ((keyState.IsKeyDown(Keys.LeftControl) && _lastState.IsKeyUp(Keys.LeftControl))
                || (keyState.IsKeyDown(Keys.RightControl) && _lastState.IsKeyUp(Keys.RightControl)))
            {
                _actions.Add(CharacterActions.DoorSwitch);
            }

            if (_actions.Count == 0)
                _actions.Add(CharacterActions.Idle);

            _lastState = keyState;
            return _actions;
        }
    }
}
