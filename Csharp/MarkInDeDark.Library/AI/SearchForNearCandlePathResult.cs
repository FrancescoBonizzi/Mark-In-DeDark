using MarkInDeDark.Library;
using MarkInDeDark.Library.Characters;
using System.Collections.Generic;

namespace MarkInDeDark.Library.AI
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
