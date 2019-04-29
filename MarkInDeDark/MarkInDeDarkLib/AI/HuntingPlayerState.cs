#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine.AI
{
    public class HuntingPlayerState : EnemyAI
    {
        #region Dichiarazioni

        Cell old_destination_;
        List<InputManager.InputAction> path_to_follow_;
        protected Player player_reference_;
        GameGrid grid_reference_;

        #endregion

        #region Costruttore

        public HuntingPlayerState()
        {
            this.old_destination_ = null;
            this.path_to_follow_ = new List<InputManager.InputAction>();
            this.player_reference_ = GameReferencesLocator.PlayerReference;
            this.grid_reference_ = GameReferencesLocator.GridReference;
        }

        #endregion

        protected virtual Cell GetDestinationCell()
        {
            return grid_reference_.GetCellAtPixel(player_reference_.WorldLocation);
        }

        protected virtual KeyValuePair<Candle, List<InputManager.InputAction>> SearchForNearCandle(Enemy Myself)
        {
            return GameReferencesLocator.CandlesReference.GetNearCandle(Myself.WorldLocation);
        }

        public virtual InputManager.InputAction Update(Enemy Myself, float elapsed)
        {
            #region Cerco candele nei dintorni
            // Le candele hanno sempre la priorità
            // Se ci sono delle candele vicine entro un certo range, scegli la più vicina
            // e vai nello stato: insegui candela
            if (!Myself.IsMoving)
            {
                KeyValuePair<Candle, List<InputManager.InputAction>> c = SearchForNearCandle(Myself);
                if (c.Key != null)
                {
                    // Cambio stato e lo metto in moto
                    path_to_follow_.Clear();
                    Myself.current_state_ = new FollowingCandleState(Myself.WorldLocation, c.Key, c.Value, this);
                    return Myself.current_state_.Update(Myself, elapsed);
                }
            }

            #endregion

            #region Altrimenti seguo il player

            Cell Destination = GetDestinationCell();
            InputManager.InputAction action_to_do = InputManager.InputAction.Fermo;

            if (old_destination_ != Destination || path_to_follow_.Count == 0)
            {
                old_destination_ = Destination;
                path_to_follow_ = grid_reference_.FindPath(Myself.WorldLocation, Destination.WorldLocation);
            }

            if (path_to_follow_.Count > 0)
            {
                if (!Myself.IsMoving)
                {
                    action_to_do = path_to_follow_[0];
                    path_to_follow_.RemoveAt(0);
                }
            }

            return action_to_do;

            #endregion
        }


    }
}