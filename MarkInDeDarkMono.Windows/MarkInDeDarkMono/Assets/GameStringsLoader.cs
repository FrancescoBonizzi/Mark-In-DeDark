using FbonizziMonoGame.StringsLocalization.Abstractions;
using System.Globalization;

namespace MarkInDeDarkMono.Assets
{
    public class GameStringsLoader
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

        public GameStringsLoader(
            ILocalizedStringsRepository localizedStringsRepository, 
            CultureInfo cultureInfo)
        {
            if (cultureInfo.TwoLetterISOLanguageName == "it")
            {
                localizedStringsRepository.AddString(PlayButtonString, "Gioca");
                localizedStringsRepository.AddString(CandlePackFoundString, "Hai trovato {0} candele");
                localizedStringsRepository.AddString(ClosedDoorString, "Tutto chiuso...");
                localizedStringsRepository.AddString(ShopString, "Negozio");
                localizedStringsRepository.AddString(LevelsPageTitleString, "Livelli");
                localizedStringsRepository.AddString(NotEnoughCandlesString, "Non hai candele");
                localizedStringsRepository.AddString(GameOverString, "Game Over!");
                localizedStringsRepository.AddString(KeyFoundString, "Hai trovato una chiave...");
                localizedStringsRepository.AddString(LevelCompletedString, "Livello completato!");
            }
            else
            {
                localizedStringsRepository.AddString(PlayButtonString, "Play");
                localizedStringsRepository.AddString(CandlePackFoundString, "{0} candles found");
                localizedStringsRepository.AddString(ClosedDoorString, "It's locked...");
                localizedStringsRepository.AddString(ShopString, "Shop");
                localizedStringsRepository.AddString(LevelsPageTitleString, "Levels");
                localizedStringsRepository.AddString(NotEnoughCandlesString, "You don't have candles");
                localizedStringsRepository.AddString(GameOverString, "Game Over!");
                localizedStringsRepository.AddString(KeyFoundString, "You found a key...");
                localizedStringsRepository.AddString(LevelCompletedString, "Level completed!");
            }
        }
    }
}
