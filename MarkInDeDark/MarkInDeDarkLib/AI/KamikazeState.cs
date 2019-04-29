#region Using

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine.AI
{
    // Questa AI si comporta in modo da lanciarsi a kamikaze contro il player

    public class KamikazeState : PatrolAreaState
    {
        #region Dichiarazioni

        private bool kamikazing_ = false;
        private const float speed_increment_ = 2.6f;

        #endregion

        #region Costruttore

        public KamikazeState(List<Cell> Waypoints)
            : base(Waypoints)
        { }

        #endregion

        public override InputManager.InputAction Update(Enemy Myself, float elapsed)
        {
            // Devo renderlo immune alle candele... come fare? Creando uno stato personalizzato, però non so come 
            // farlo andare lì senza incasinare la classe
            if (!Myself.IsMoving)
            {

                if (!kamikazing_)
                {
                    // Se incontra il player si lancia a kamikaze
                    int dir = -1;
                    bool found = false;

                    Vector2 p_pos = GameReferences.PlayerReference.WorldLocation;
                    Vector2 m_pos = Myself.WorldLocation;

                    // Se sono sulla stessa Y
                    if (p_pos.Y == m_pos.Y)
                    {
                        if (p_pos.X - m_pos.X > 0)
                            dir = 1;
                        else dir = 3;
                    }
                    // Se sono sulla stessa X
                    else if (p_pos.X == m_pos.X)
                    {
                        if (p_pos.Y - m_pos.Y > 0)
                            dir = 2;
                        else dir = 0;
                    }

                    if (dir != -1)
                    {
                        Cell cell_p = grid_reference_.GetCellAtPixel(p_pos);
                        Cell cell_m = grid_reference_.GetCellAtPixel(m_pos);

                        List<Cell> possible_path = new List<Cell>();

                        bool player_found = false;

                        possible_path.Add(cell_m);
                        while (!found)
                        {
                            if (cell_m.Links[dir] != null)
                            {
                                cell_m = cell_m.Links[dir];
                                possible_path.Add(cell_m);

                                if (cell_m == cell_p)
                                    player_found = true;
                            }
                            else
                            {
                                // Se trovo un muro senza aver incrociato il player, blocco tutto
                                if (!player_found)
                                    break;
                                else found = true; // Altrimenti lo prendo come percorso, così va a sbattere contro il muro 
                            }
                        }

                        // Assegno il path da seguire
                        if (found)
                        {
                            Myself.Speed += speed_increment_;
                            current_path_index_ = 0;
                            path_to_follow_ = grid_reference_.PathToDirections(possible_path);
                            current_path_max_index_ = path_to_follow_.Count;
                            kamikazing_ = true;
                        }
                    }
                }
                else
                {
                    // Se sta già kamikazzando, lo fermo quando va contro il muro o contro il player
                    if (current_path_index_ == current_path_max_index_
                        || GameReferences.PlayerReference.IsCollidingWith(Myself.CollisionRectangle))
                    {
                        kamikazing_ = false;
                        Myself.Speed -= speed_increment_;
                        Myself.Stun();
                    }

                    // Se becca il player, devo stunnarlo
                    if (GameReferences.PlayerReference.IsCollidingWith(Myself.CollisionRectangle))
                    {
                        GameReferences.PlayerReference.Stun();
                    }
                }
            }

            // Se no segue il suo path
            return base.Update(Myself, elapsed);
        }
    }
}
