using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MarkInDeDark.Library.AI
{
    public class SearchForNearCandlePathResult
    {
        public SearchForNearCandlePathResult(Candle candle, List<Vector2> pathToTheCandle)
        {
            Candle = candle;
            PathToTheCandle = pathToTheCandle;
        }

        public Candle Candle { get; }
        public List<Vector2> PathToTheCandle { get; }
    }
}
