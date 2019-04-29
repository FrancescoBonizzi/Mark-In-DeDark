#region Using

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine.AI
{
    public class FollowingCandleState : EnemyAI
    {
        #region Dichiarazioni

        private List<InputManager.InputAction> path_to_follow_;
        private EnemyAI next_state_;
        private Candle candle_;
        private bool has_touched_candle_;

        #endregion

        #region Costruttore

        public FollowingCandleState(Vector2 StartingLocation, Candle Candle, List<InputManager.InputAction> Path, EnemyAI NextState)
        {
            this.candle_ = Candle;
            this.path_to_follow_ = Path;
            this.next_state_ = NextState;
            this.has_touched_candle_ = false;
            Candle.SomeMonsterIsFollowingMe = true;
        }

        #endregion

        public InputManager.InputAction Update(Enemy Myself, float elapsed)
        {
            InputManager.InputAction action_to_do = InputManager.InputAction.Fermo;

            // Se non è già stata beccata controllo la collisione
            if (!candle_.Dying || !has_touched_candle_)
            {
                if (candle_.IsCollidingWith(Myself.CollisionRectangle))
                {
                    has_touched_candle_ = true;
                    candle_.GoingToDie();
                    Myself.Attack();
                }

                // Se non l'ha ancora raggiunta
                if (path_to_follow_.Count > 0)
                {
                    // Se sono fermo, invio il prossimo comando
                    if (!Myself.IsMoving)
                    {
                        action_to_do = path_to_follow_[0];
                        path_to_follow_.RemoveAt(0);
                    }
                }
                else // Se arriva qui ( NON DOVREBBE ARRIVARCI, MA SUCCEDE! ), significata che l'omino è fermo e la candela è viva, ricalcolo il percorso
                {
                    this.path_to_follow_ = GameReferencesLocator.GridReference.FindPath(Myself.WorldLocation, this.candle_.WorldLocation);
                }
            }
            else
            {
                // Se stava scomparendo ed è proprio scomparsa
                if (!candle_.Active)
                {
                    // Torno allo stato precedente in pratica
                    Myself.StopAttacking();
                    Myself.current_state_ = next_state_;
                    return Myself.current_state_.Update(Myself, elapsed);
                }
                else
                {
                    // Faccio stare fermo il mostro sulla candela
                    return action_to_do;
                }
            }

            return action_to_do;
        }
    }
}

