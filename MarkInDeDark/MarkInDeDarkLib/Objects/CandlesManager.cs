#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Mark.Engine
{
    public class CandlesManager
    {
        #region Dichiarazioni

        private List<Candle> candles_;
        private int used_candels_ = 0;

        private const int n_cells_finding_range_ = 4;
        private const int squared_distance_finding_range_ = 120000;

        private int starting_candles_;

        #endregion

        #region Costruttore

        public CandlesManager(int StartingCandles)
        {
            candles_ = new List<Candle>();
            starting_candles_ = StartingCandles;
        }

        #endregion

        #region Metodi

        public KeyValuePair<Candle,List<InputManager.InputAction>> GetNearCandle(Vector2 Location)
        {
            // Cerco, se c'è, una candela entro un certo range
            // Non posso cercarla in tutta la mappa
            // se no se uno spawna in casino e un altro è vicino a te, quello vicino non insegue la candela subito
            // quindi non puoi evitarlo. Inoltre quelli lontani si farebbero il giro di tutta la mappa solo
            // per la candela

            for (int k = 0; k < candles_.Count; ++k)
            {
                if (!candles_[k].SomeMonsterIsFollowingMe)
                {
                    float dist = Vector2.DistanceSquared(Location, candles_[k].WorldCenter);
                    if (dist <= squared_distance_finding_range_)
                    {
                        // Tra quelle vicine per l'euristica, provo a vedere quale di queste ha un path corto
                        var path = GameReferencesLocator.GridReference.FindPath(Location, candles_[k].WorldCenter);
                        if (path.Count() <= n_cells_finding_range_)
                        {
                            return new KeyValuePair<Candle, List<InputManager.InputAction>>(
                                candles_[k],
                                path);
                        }
                    }
                }
            }

            return new KeyValuePair<Candle, List<InputManager.InputAction>>(null, null);
        }

        private bool CandleAlreadyHere(Vector2 Where)
        {
            for (int k = 0; k < candles_.Count; ++k)
            {
                if (candles_[k].WorldLocation == Where)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddNewCandle(Vector2 Where)
        {
            if (CandlesLeftToUse > 0)
            {
                if (!CandleAlreadyHere(Where))
                {
                    candles_.Add(new Candle(Where));
                    ++used_candels_;
                    GameReferencesLocator.SoundManagerReference.PlayPutCandle();
                }
            }
        }

        #endregion

        #region Get/Set

        public int PlayerUsedCandles
        {
            get { return used_candels_; }
        }

        public int CandlesLeftToUse
        {
            get { return starting_candles_ - used_candels_; }
        }

        #endregion

        #region Update/Draw

        public void Update(float elapsed)
        {
            for (int i = candles_.Count - 1; i >= 0; --i)
            {
                candles_[i].Update(elapsed);
                if (!candles_[i].Active)
                    candles_.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Candle c in candles_)
                c.Draw(spriteBatch);
        }

        public void DrawSmoke(SpriteBatch spriteBatch)
        {
            foreach (Candle c in candles_)
                c.DrawSmoke(spriteBatch);
        }

        public void DrawLightmap(SpriteBatch spriteBatch)
        {
            foreach (Candle c in candles_)
                c.MyLight.Draw(spriteBatch);
        }

        #endregion
    }
}
