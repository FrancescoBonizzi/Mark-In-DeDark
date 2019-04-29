#region Using

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine.AI
{
    // Questa AI si comporta in modo da seguire un suo path definito in Excel, se il player arriva ad una certa
    // distanza da lui, questo inizia ad inseguirlo. Quando si allontana, ritorna dov'era prima e ricomincia il suo giro
    // Alla fine è un HuntingPlayer a tempo e con apparizione

    public class GhostState : HuntingPlayerState
    {
        #region Dichiarazioni

        private float ghost_timer_ = 0.0f;
        private float activation_interval_ = 10.0f;

        private bool appearing_ = false;

        #endregion

        #region Costruttore

        public GhostState(Enemy Myself)
            : base()
        {
        }

        #endregion

        #region Metodi

        private void FadeIn(Enemy Myself)
        {
            // Mi posizione dalle parti del player
            Cell c = base.GetDestinationCell(); // Da la cella del player
            bool found = false;

            int su = 0, destra = 1, giu = 2, sinistra = 3;
            int not_search_in;

            if (player_reference_.CurrentAction == InputManager.InputAction.Su)
                not_search_in = su;
            else if (player_reference_.CurrentAction == InputManager.InputAction.Destra)
                not_search_in = destra;
            else if (player_reference_.CurrentAction == InputManager.InputAction.Giu)
                not_search_in = giu;
            else if (player_reference_.CurrentAction == InputManager.InputAction.Sinistra)
                not_search_in = sinistra;
            else not_search_in = -1;

            // Ne scelgo una 2 passi dietro a lui
            for (int i = 0; i <= 3 && !found; ++i)
            {
                if (i != not_search_in)
                {
                    for (int k = 0; k <= 4; ++k)
                    {
                        if (c.Links[i] != null)
                        {
                            c = c.Links[i];
                        }
                        else
                        {
                            break;
                        }
                        if (k == 4)
                            found = true;
                    }
                }
            }

            // Se non trova lo spazio per apparire, niente, riprova dopo
            if (found)
            {
                appearing_ = true;
                Myself.WorldLocation = c.WorldLocation;
                Myself.MyLight.WorldLocation = c.WorldLocation;
                if (Myself is Werewolf)
                    (Myself as Werewolf).AppearEffect();
            }
        }

        private void FadeOut(Enemy Myself)
        {
            appearing_ = false;
            if (Myself is Werewolf)
                (Myself as Werewolf).AppearEffect();
            // Si mette fuori dalla mappa (per essere lontano da collisioni ed invisibile)
            Myself.WorldLocation = new Vector2(-100, -100);
        }

        protected override KeyValuePair<Candle, List<InputManager.InputAction>> SearchForNearCandle(Enemy Myself)
        {
            // Non voglio che lui segua le candele
            // perchè poi se le marca, poi non so come fare a farle smarcare
            return new KeyValuePair<Candle, List<InputManager.InputAction>>(null, null);
        }

        #endregion

        public override InputManager.InputAction Update(Enemy Myself, float elapsed)
        {
            ghost_timer_ += elapsed;

            if (ghost_timer_ >= activation_interval_)
            {
                if (!appearing_)
                {
                    FadeIn(Myself);
                    ghost_timer_ = 0.0f;
                }
                else
                {
                    FadeOut(Myself);
                    ghost_timer_ = 0.0f;
                }
            }

            if (!appearing_)
                return InputManager.InputAction.Fermo;

            return base.Update(Myself, elapsed);
        }

    }
}
