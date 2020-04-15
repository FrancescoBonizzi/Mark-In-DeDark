using FbonizziMonoGame.StringsLocalization.Abstractions;
using MarkInDeDark.Common;
using System.Globalization;

namespace MarkInDeDark.Library
{
    public class GameStringsLoader
    {
        public GameStringsLoader(
            ILocalizedStringsRepository localizedStringsRepository, 
            CultureInfo cultureInfo,
            TextFileStringsManager textFileStringsManager)
        {
            if (cultureInfo.TwoLetterISOLanguageName == "it")
            {
                foreach(var t in textFileStringsManager.GetAllTranslations().Traslations[StringsTraslationLanguages.Italian])
                    localizedStringsRepository.AddString(t.Key, t.Value.Text);
            }
            else
            {
                foreach (var t in textFileStringsManager.GetAllTranslations().Traslations[StringsTraslationLanguages.English])
                    localizedStringsRepository.AddString(t.Key, t.Value.Text);
            }
        }
    }
}
