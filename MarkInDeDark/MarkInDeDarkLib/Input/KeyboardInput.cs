using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mark.Engine
{
    public class KeyboardInput : InputManager
    {
        public override void Update(float elapsed)
        {
            KeyboardState keyState = Keyboard.GetState();

            touched_something_ = false;

            if (keyState.IsKeyDown(Keys.Up))
            {
                current_inputaction_ = InputAction.Su;
                touched_something_ = true;
            }
            else if (keyState.IsKeyDown(Keys.Down))
            {
                current_inputaction_ = InputAction.Giu;
                touched_something_ = true;
            }
            else if (keyState.IsKeyDown(Keys.Right))
            {
                current_inputaction_ = InputAction.Destra;
                touched_something_ = true;
            }
            else if (keyState.IsKeyDown(Keys.Left))
            {
                current_inputaction_ = InputAction.Sinistra;
                touched_something_ = true;
            }

            if (keyState.IsKeyDown(Keys.Space))
            {
                current_inputaction_ = InputAction.Candela;
                touched_something_ = true;
            }

            last_action_ = current_inputaction_;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }
    }
}
