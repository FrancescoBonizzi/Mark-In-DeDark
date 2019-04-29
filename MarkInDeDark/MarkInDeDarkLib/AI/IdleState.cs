#region Using

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine.AI
{
    public class IdleState : EnemyAI
    {
        public InputManager.InputAction Update(Enemy Myself, float elapsed)
        {
            return InputManager.InputAction.Fermo;
        }
    }
}
