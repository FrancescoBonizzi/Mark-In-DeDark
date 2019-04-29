#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine.AI
{
    public interface EnemyAI
    {
        InputManager.InputAction Update(Enemy Myself, float elapsed);
    }
}
