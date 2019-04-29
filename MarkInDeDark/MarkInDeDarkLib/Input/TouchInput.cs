#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
#if WINDOWS_PHONE
    public class TouchInput : InputManager
    {
        #region Dichiarazioni

        #region Per il disegno

        private Texture2D atlas_;
        private Color draw_color_ = Color.White * 0.3f;

        private Vector2 pad_position_;
        private Rectangle empty_pad_rectangle_;
        private Rectangle right_pad_rectangle_;
        private Rectangle left_pad_rectangle_;
        private Rectangle up_pad_rectangle_;
        private Rectangle down_pad_rectangle_;

        private Vector2 candle_button_position_;
        private Rectangle candle_button_released_rect_;
        private Rectangle candle_button_pressed_rect_;

        #endregion

        #region Per l'input

        private TouchCollection touchCollection_;

        private Rectangle top_input_rect_;
        private Rectangle right_input_rect_;
        private Rectangle bottom_input_rect_;
        private Rectangle left_input_rect_;

        private Rectangle candle_input_rect_;

        #endregion

        private const float default_candle_timer_ = 0.2f;
        private float candle_timer_ = 0.2f;

        #endregion

        #region DFS

        enum CandleButtonStates { Pressed, Released };
        enum PadButtonStates { Up, Down, Left, Right, Released };

        private CandleButtonStates candle_button_state_;
        private PadButtonStates pad_button_state_;

        #endregion

        #region Costruttore

        public TouchInput()
        {
            this.current_inputaction_ = InputAction.Fermo;
            this.atlas_ = Loader.atlas_;

            this.candle_button_state_ = CandleButtonStates.Released;
            this.pad_button_state_ = PadButtonStates.Released;

            #region Per il disegno

            this.pad_position_ = new Vector2(40.0f, 320.0f);
            this.empty_pad_rectangle_ = Loader.dpad_empty_;
            this.right_pad_rectangle_ = Loader.dpad_right_;
            this.left_pad_rectangle_ = Loader.dpad_left_;
            this.down_pad_rectangle_ = Loader.dpad_down_;
            this.up_pad_rectangle_ = Loader.dpad_up_;

            this.candle_button_position_ = new Vector2(640.0f, 320.0f);
            this.candle_button_released_rect_ = Loader.dpad_buttonNormal_;
            this.candle_button_pressed_rect_ = Loader.dpad_buttonPressed_;

            #endregion

            #region Per l'input

            int modifier = 40;

            this.top_input_rect_ = new Rectangle(
                (int)pad_position_.X + 45, (int)pad_position_.Y + -modifier,
                60, 60 + modifier);

            this.bottom_input_rect_ = new Rectangle(
                 (int)pad_position_.X + 45, (int)pad_position_.Y + empty_pad_rectangle_.Height - 60,
                 60, 60 + modifier);

            this.left_input_rect_ = new Rectangle(
                (int)pad_position_.X + -modifier - 20, (int)pad_position_.Y + 45,
                60 + modifier, 60);

            this.right_input_rect_ = new Rectangle(
                (int)pad_position_.X + empty_pad_rectangle_.Width - modifier, (int)pad_position_.Y + 45,
                 60 + modifier, 60);

            this.candle_input_rect_ = new Rectangle(
                (int)candle_button_position_.X, (int)candle_button_position_.Y,
                candle_button_pressed_rect_.Width, candle_button_pressed_rect_.Height);

            #endregion
        }

        #endregion

        #region Metodi

        private void TapHandle(Vector2 TouchPosition)
        {
            int x = (int)TouchPosition.X;
            int y = (int)TouchPosition.Y;

            if (right_input_rect_.Contains(x, y))
            {
                current_inputaction_ = InputAction.Destra;
            }
            else if (left_input_rect_.Contains(x, y))
            {
                current_inputaction_ = InputAction.Sinistra;
            }
            else if (top_input_rect_.Contains(x, y))
            {
                current_inputaction_ = InputAction.Su;
            }
            else if (bottom_input_rect_.Contains(x, y))
            {
                current_inputaction_ = InputAction.Giu;
            }
            else if (candle_input_rect_.Contains(x, y))
            {
                if (candle_timer_ <= 0.0f)
                {
                    current_inputaction_ = InputAction.Candela;
                    candle_timer_ = default_candle_timer_;
                }
            }
            else
            {
                current_inputaction_ = InputAction.Fermo;
            }

            last_action_ = current_inputaction_;
        }

        #endregion

        #region Update/Draw

        public override void Update(float elapsed)
        {
            touchCollection_ = TouchPanel.GetState();
            candle_timer_ -= elapsed;

            touched_something_ = false;
            foreach (TouchLocation tl in touchCollection_)
            {
                touched_something_ = true;
                if ((tl.State == TouchLocationState.Pressed)
                    || (tl.State == TouchLocationState.Moved))
                {
                    TapHandle(tl.Position);
                }
                else
                {
                    last_action_ = InputAction.Fermo;
                    current_inputaction_ = InputAction.Fermo;
                }
            }

            #region Candle DFS

            switch (candle_button_state_)
            {
                case CandleButtonStates.Pressed:

                    // Ridondante, ma chiaro
                    if (last_action_ == InputAction.Candela)
                    {
                        candle_button_state_ = CandleButtonStates.Pressed;
                    }
                    else
                    {
                        candle_button_state_ = CandleButtonStates.Released;
                    }

                    break;

                case CandleButtonStates.Released:

                    // Ridondante, ma chiaro
                    if (last_action_ == InputAction.Candela)
                    {
                        candle_button_state_ = CandleButtonStates.Pressed;
                    }
                    else
                    {
                        candle_button_state_ = CandleButtonStates.Released;
                    }

                    break;
            }

            #endregion

            #region Pad DFS

            switch (pad_button_state_)
            {
                case PadButtonStates.Released:

                    if (last_action_ == InputAction.Destra)
                        pad_button_state_ = PadButtonStates.Right;
                    else if (last_action_ == InputAction.Sinistra)
                        pad_button_state_ = PadButtonStates.Left;
                    else if (last_action_ == InputAction.Giu)
                        pad_button_state_ = PadButtonStates.Down;
                    else if (last_action_ == InputAction.Su)
                        pad_button_state_ = PadButtonStates.Up;

                    break;

                case PadButtonStates.Left:

                    if (last_action_ == InputAction.Destra)
                        pad_button_state_ = PadButtonStates.Right;
                    else if (last_action_ == InputAction.Fermo)
                        pad_button_state_ = PadButtonStates.Released;
                    else if (last_action_ == InputAction.Giu)
                        pad_button_state_ = PadButtonStates.Down;
                    else if (last_action_ == InputAction.Su)
                        pad_button_state_ = PadButtonStates.Up;

                    break;

                case PadButtonStates.Right:

                    if (last_action_ == InputAction.Fermo)
                        pad_button_state_ = PadButtonStates.Released;
                    else if (last_action_ == InputAction.Sinistra)
                        pad_button_state_ = PadButtonStates.Left;
                    else if (last_action_ == InputAction.Giu)
                        pad_button_state_ = PadButtonStates.Down;
                    else if (last_action_ == InputAction.Su)
                        pad_button_state_ = PadButtonStates.Up;

                    break;

                case PadButtonStates.Down:

                    if (last_action_ == InputAction.Destra)
                        pad_button_state_ = PadButtonStates.Right;
                    else if (last_action_ == InputAction.Sinistra)
                        pad_button_state_ = PadButtonStates.Left;
                    else if (last_action_ == InputAction.Fermo)
                        pad_button_state_ = PadButtonStates.Released;
                    else if (last_action_ == InputAction.Su)
                        pad_button_state_ = PadButtonStates.Up;

                    break;

                case PadButtonStates.Up:

                    if (last_action_ == InputAction.Destra)
                        pad_button_state_ = PadButtonStates.Right;
                    else if (last_action_ == InputAction.Sinistra)
                        pad_button_state_ = PadButtonStates.Left;
                    else if (last_action_ == InputAction.Giu)
                        pad_button_state_ = PadButtonStates.Down;
                    else if (last_action_ == InputAction.Fermo)
                        pad_button_state_ = PadButtonStates.Released;
                    break;
            }

            #endregion
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            #region Candle DFS

            switch (candle_button_state_)
            {
                case CandleButtonStates.Pressed:
                    spriteBatch.Draw(atlas_, candle_button_position_, candle_button_pressed_rect_, draw_color_);
                    break;

                case CandleButtonStates.Released:
                    spriteBatch.Draw(atlas_, candle_button_position_, candle_button_released_rect_, draw_color_);
                    break;
            }

            #endregion

            #region Pad DFS

            switch (pad_button_state_)
            {
                case PadButtonStates.Down:
                    spriteBatch.Draw(atlas_, pad_position_, down_pad_rectangle_, draw_color_);
                    break;

                case PadButtonStates.Up:
                    spriteBatch.Draw(atlas_, pad_position_, up_pad_rectangle_, draw_color_);
                    break;

                case PadButtonStates.Left:
                    spriteBatch.Draw(atlas_, pad_position_, left_pad_rectangle_, draw_color_);
                    break;

                case PadButtonStates.Right:
                    spriteBatch.Draw(atlas_, pad_position_, right_pad_rectangle_, draw_color_);
                    break;

                case PadButtonStates.Released:
                    spriteBatch.Draw(atlas_, pad_position_, empty_pad_rectangle_, draw_color_);
                    break;
            }

            #endregion
        }

        #endregion
    }
#endif
}
