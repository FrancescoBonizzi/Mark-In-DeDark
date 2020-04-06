using MarkInDeDarkMono;
using MarkInDeDarkMono.Characters;
using System.Collections.Generic;

namespace MarkInDeDarkMono.AI
{
    public class SearchForNearCandlePathResult
    {
        public SearchForNearCandlePathResult(Candle candle, IEnumerable<CharacterActions> actionsToGetTheCandle)
        {
            Candle = candle;
            ActionsToGetTheCandle = actionsToGetTheCandle;
        }

        public Candle Candle { get; }
        public IEnumerable<CharacterActions> ActionsToGetTheCandle { get; }
    }
}
