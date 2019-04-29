using FlowsoftGamesMonogame.UI;
using System.Globalization;

namespace MarkInDeDarkMono.Assets
{
    public class GameStrings
    {
        public const string PlayButtonString = "PlayButtonString";
        public const string CandlePackFoundString = "CandlePackFoundString";
        public const string ClosedDoorString = "ClosedDoorString";
        public const string ShopString = "ShopString";
        public const string LevelsPageTitleString = "LevelsPageTitleString";
        public const string NotEnoughCandlesString = "NotEnoughCandlesString";
        public const string GameOverString = "GameOverString";
        public const string KeyFoundString = "KeyFoundString";
        public const string LevelCompletedString = "LevelCompletedString";

        public GameStrings(CultureInfo cultureInfo)
        {
            if (MultilanguageStrings.IsItalian(cultureInfo))
            {
                MultilanguageStrings.Add(PlayButtonString, "Gioca");
                MultilanguageStrings.Add(CandlePackFoundString, "Hai trovato {0} candele");
                MultilanguageStrings.Add(ClosedDoorString, "Tutto chiuso...");
                MultilanguageStrings.Add(ShopString, "Negozio");
                MultilanguageStrings.Add(LevelsPageTitleString, "Livelli");
                MultilanguageStrings.Add(NotEnoughCandlesString, "Non hai candele");
                MultilanguageStrings.Add(GameOverString, "Game Over!");
                MultilanguageStrings.Add(KeyFoundString, "Hai trovato una chiave...");
                MultilanguageStrings.Add(LevelCompletedString, "Livello completato!");
            }
            else
            {
                MultilanguageStrings.Add(PlayButtonString, "Play");
                MultilanguageStrings.Add(CandlePackFoundString, "{0} candles found");
                MultilanguageStrings.Add(ClosedDoorString, "It's locked...");
                MultilanguageStrings.Add(ShopString, "Shop");
                MultilanguageStrings.Add(LevelsPageTitleString, "Levels");
                MultilanguageStrings.Add(NotEnoughCandlesString, "You don't have candles");
                MultilanguageStrings.Add(GameOverString, "Game Over!");
                MultilanguageStrings.Add(KeyFoundString, "You found a key...");
                MultilanguageStrings.Add(LevelCompletedString, "Level completed!");
            }
        }
    }
}
