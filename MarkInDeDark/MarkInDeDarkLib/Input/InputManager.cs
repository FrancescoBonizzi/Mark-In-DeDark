#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public abstract class InputManager
    {
        public enum InputAction
        {
            Su,
            Giu,
            Destra,
            Sinistra,
            Fermo,
            Candela
        }

        #region Dichiarazioni

        protected InputAction current_inputaction_;
        protected InputAction last_action_;
        protected bool touched_something_;

        #endregion

        #region Costruttore

        public InputManager()
        {
            current_inputaction_ = InputAction.Fermo;
        }

        #endregion

        #region Get/Set

        public InputAction CurrentInputAction
        {
            get
            {
                // Quanto viene letta la direzione dall'esterno, resetto la direzione, così aspetta il prossimo tap
                InputAction ret = current_inputaction_;
                current_inputaction_ = InputAction.Fermo;

                return ret;
            }
        }

        public bool TouchedSomething
        {
            get { return touched_something_; }
        }

        #endregion

        #region Update/Draw

        public abstract void Update(float elapsed);
        public abstract void Draw(SpriteBatch spriteBatch);

        #endregion
    }
}