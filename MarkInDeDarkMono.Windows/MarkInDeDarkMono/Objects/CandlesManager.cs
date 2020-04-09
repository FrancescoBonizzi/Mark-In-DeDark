using MarkInDeDarkMono;

using MarkInDeDarkMono.Characters;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono
{
    public class CandlesManager
    {
        public List<Candle> Candles { get; }

        private int _avaibleCandlesNumber;
        private readonly SoundManager _soundManager;
        private readonly GameGrid _gameGrid;
        private readonly AssetsLoader _assetsLoader;
        private readonly GameConfig _gameConfig;
        private readonly int _cellWidth;
        private readonly bool _doMonstersAttackTheSameCandle;

        public int PlayerUsedCandles { get; private set; }
        public event EventHandler PlayerUsedACandle;

        public CandlesManager(
            int startingCandles,
            AssetsLoader assetsLoader,
            SoundManager soundManager,
            GameGrid gameGrid,
            GameConfig gameConfig)
        {
            Candles = new List<Candle>();
            _avaibleCandlesNumber = startingCandles;
            _soundManager = soundManager;
            _gameGrid = gameGrid;
            _cellWidth = gameGrid.CellWidth;
            _doMonstersAttackTheSameCandle = gameConfig.DoMonstersAttackTheSameCandle;

            _assetsLoader = assetsLoader;
            _gameConfig = gameConfig;

            if (_gameConfig.GodModeInfiniteCandles)
                _avaibleCandlesNumber = short.MaxValue;
        }

        public void AddCandlesToInventory(int nCandles)
            => _avaibleCandlesNumber += nCandles;

        public bool DoIHaveACandleLeft
            => _avaibleCandlesNumber > 0;

        public int LeftCandles
            => _avaibleCandlesNumber;

        public Candle IsThereACandle(GameCell where)
        {
            return Candles.FirstOrDefault(c => _gameGrid.GetCellAtPixel(c.Position) == where);
        }

        public KeyValuePair<Candle, IEnumerable<CharacterActions>> GetNearestCandleActionsPath(
            Vector2 searchingStartPosition,
            int maxActionsToFindCandle)
        {
            // Distanza quadratica di n celle (width perché quadrate) 
            var squared_distance_finding_range_ = (int)Math.Pow(_cellWidth * (maxActionsToFindCandle + 1), 2);

            // Cerco, se c'è, una candela entro un certo range
            // Non posso cercarla in tutta la mappa
            // se no se uno spawna in casino e un altro è vicino a te, quello vicino non insegue la candela subito
            // quindi non puoi evitarlo. Inoltre quelli lontani si farebbero il giro di tutta la mappa solo
            // per la candela

            var pathToFollow = new KeyValuePair<Candle, IEnumerable<CharacterActions>>();

            foreach (var candle in Candles)
            {
                if (candle.AreSomeMonsterIsFollowingMe && !_doMonstersAttackTheSameCandle)
                    continue;

                var thisCandleWorldCenter = candle.WorldCenter;

                float squaredDistanceBetweenStartingPositionAndCurrentCandle = Vector2.DistanceSquared(searchingStartPosition, thisCandleWorldCenter);
                if (squaredDistanceBetweenStartingPositionAndCurrentCandle > squared_distance_finding_range_)
                    continue;

                // Tra quelle vicine per l'euristica, provo a vedere quale di queste ha un path corto
                var actionsToCandle = _gameGrid.FindActionsToPath(searchingStartPosition, thisCandleWorldCenter);
                var numberOfActionsToCandle = actionsToCandle.Count();
                if (numberOfActionsToCandle > maxActionsToFindCandle)
                    continue;

                if (pathToFollow.Key == null)
                {
                    pathToFollow = new KeyValuePair<Candle, IEnumerable<CharacterActions>>(
                        candle,
                        actionsToCandle);
                }
                else
                {
                    // Prendo la candela più vicina
                    if (numberOfActionsToCandle < pathToFollow.Value.Count())
                    {
                        pathToFollow = new KeyValuePair<Candle, IEnumerable<CharacterActions>>(
                           candle,
                           actionsToCandle);
                    }
                }

                if (numberOfActionsToCandle == 1)
                    return pathToFollow;
            }

            if (pathToFollow.Key == null)
                return new KeyValuePair<Candle, IEnumerable<CharacterActions>>(null, null);

            return pathToFollow;
        }

        private bool IsThereACandle(Vector2 where)
            => Candles.Exists(c => c.Position == where);

        public void PlaceCandle(Vector2 where)
        {
            if (!DoIHaveACandleLeft)
                return;

            if (!IsThereACandle(where))
            {
                Candles.Add(new Candle(_assetsLoader, _gameConfig, where));
                ++PlayerUsedCandles;
                _avaibleCandlesNumber -= 1;
                _soundManager.PlayPutCandle();
                PlayerUsedACandle?.Invoke(null, EventArgs.Empty);
            }
        }

        public void Update(TimeSpan elapsed)
        {
            for (int i = Candles.Count - 1; i >= 0; --i)
            {
                Candles[i].Update(elapsed);
                if (!Candles[i].IsActive)
                    Candles.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Candle c in Candles)
            {
                c.Draw(spriteBatch);
            }
        }

        public void DrawEffects(SpriteBatch spriteBatch)
        {
            foreach (Candle c in Candles)
            {
                c.DrawSmoke(spriteBatch);
            }
        }

        public void DrawLightmap(SpriteBatch spriteBatch)
        {
            foreach (Candle c in Candles)
                c.LightMap.Draw(spriteBatch);
        }
    }
}
