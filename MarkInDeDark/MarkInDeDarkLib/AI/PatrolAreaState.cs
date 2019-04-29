#region Using

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine.AI
{
    // Questa AI si comporta in modo da seguire un suo path definito in Excel
    // Viene attratto dalla candela
    // I waypoint sono randomizzati ogni tot

    public class PatrolAreaState : EnemyAI
    {
        #region Dichiarazioni

        protected List<Cell> waypoints_;
        protected List<InputManager.InputAction> path_to_follow_;
        protected int current_waypoint_ = 0;
        protected int n_waypoints_;
        protected GameGrid grid_reference_;

        protected int current_path_index_ = 0;
        protected int current_path_max_index_;

        // Mi salvo i percorsi già calcolati, in modo da non ricalcolarli ogni volta
        // La cache l'ho messa qui e non nella griglia perchè non ha senso cachare i percorsi
        // di quelli che ti inseguono
        private Dictionary<KeyValuePair<Vector2, Vector2>, List<InputManager.InputAction>> paths_cache_;

        #endregion

        #region Costruttore

        public PatrolAreaState(List<Cell> Waypoints)
        {
            this.waypoints_ = Waypoints;
            this.grid_reference_ = GameReferencesLocator.GridReference;
            this.path_to_follow_ = new List<InputManager.InputAction>();
            this.n_waypoints_ = Waypoints.Count;

            this.paths_cache_ = new Dictionary<KeyValuePair<Vector2, Vector2>, List<InputManager.InputAction>>();
        }

        #endregion

        public virtual InputManager.InputAction Update(Enemy Myself, float elapsed)
        {
            #region Cerco candele nei dintorni
            // Le candele hanno sempre la priorità
            // Se ci sono delle candele vicine entro un certo range, scegli la più vicina
            // e vai nello stato: insegui candela
            // Non serve dirgli di guardare solo se non ci sono nuove candele, perchè in quel caso il for è fermo e non occupa calcolo
            if (!Myself.IsMoving)
            {
                KeyValuePair<Candle, List<InputManager.InputAction>> c = GameReferencesLocator.CandlesReference.GetNearCandle(Myself.WorldLocation);
                if (c.Key != null)
                {
                    // Cambio stato e lo metto in moto
                    current_path_index_ = 0;
                    current_path_max_index_ = 0;
                    Myself.current_state_ = new FollowingCandleState(Myself.WorldLocation, c.Key, c.Value, this);
                    return Myself.current_state_.Update(Myself, elapsed);
                }
            }

            #endregion

            #region Altrimenti seguo il mio path

            InputManager.InputAction action_to_do = InputManager.InputAction.Fermo;

            if (!Myself.IsMoving)
            {
                if (current_path_index_ == current_path_max_index_)
                {
                    NextWaypoint();
                    Vector2 Start = Myself.WorldLocation;
                    Vector2 End = waypoints_[current_waypoint_].WorldLocation;

                    if (Start == End)
                    {
                        NextWaypoint();
                        Start = Myself.WorldLocation;
                        End = waypoints_[current_waypoint_].WorldLocation;
                    }

                    if (paths_cache_.ContainsKey(new KeyValuePair<Vector2, Vector2>(Start, End)))
                    {
                        path_to_follow_ = paths_cache_[new KeyValuePair<Vector2, Vector2>(Start, End)];
                    }
                    else
                    {
                        var path = grid_reference_.FindPath(Start, End);
                        paths_cache_.Add(new KeyValuePair<Vector2, Vector2>(Start, End), new List<InputManager.InputAction>(path));
                        path_to_follow_ = new List<InputManager.InputAction>(path);
                    }

                    current_path_index_ = 0;
                    current_path_max_index_ = path_to_follow_.Count;
                }
                else
                {
                    if (!Myself.IsMoving)
                    {
                        action_to_do = path_to_follow_[current_path_index_];
                        ++current_path_index_;
                    }
                }
            }

            return action_to_do;

            #endregion
        }

        #region Metodi

        public void NextWaypoint()
        {
            int tmp = current_waypoint_;
            current_waypoint_ = Helper.random_.Next(0, n_waypoints_);
            if (current_waypoint_ == tmp)
                current_waypoint_ = (current_waypoint_ + 1) % n_waypoints_;
        }

        #endregion

    }
}
