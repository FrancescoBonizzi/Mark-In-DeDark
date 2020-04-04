using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkInDeDark.Common
{
    public class TextFileStringsManager
    {
        private TraslatedStrings _traslatedStrings;
        private readonly string _filePath;

        public TextFileStringsManager(string filePath)
        {
            if (!File.Exists(filePath))
                throw new ArgumentException($"Il file delle traduzioni al path {filePath} non esiste", "Attenzione");

            _filePath = Path.GetFullPath(filePath);
            var rawTranslations = File.ReadAllText(filePath);
            _traslatedStrings = JsonConvert.DeserializeObject<TraslatedStrings>(rawTranslations);
        }

        // Mi serve per l'editor
        public void SetTranslations(Dictionary<StringsTraslationLanguages, Dictionary<string, string>> traslatedStrings)
        {
            _traslatedStrings.Traslations = traslatedStrings;
        }

        public void Save()
        {
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(_traslatedStrings));
        }

        public void SetString(StringsTraslationLanguages language, string key, string value)
        {
            if (!_traslatedStrings.Traslations.ContainsKey(language))
                throw new ArgumentException($"No traslations found for {language}");

            _traslatedStrings.Traslations[language][key] = value;
        }

        public void RemoveString(StringsTraslationLanguages language, string key, string value)
        {
            if (!_traslatedStrings.Traslations.ContainsKey(language))
                throw new ArgumentException($"No traslations found for {language}");

            if (_traslatedStrings.Traslations[language].ContainsKey(key))
                _traslatedStrings.Traslations[language].Remove(value);
        }

        public TraslatedStrings GetAllTranslations()
        {
            return _traslatedStrings;
        }

        public Dictionary<string, string> GetTranslations(StringsTraslationLanguages language)
        {
            if (!_traslatedStrings.Traslations.ContainsKey(language))
                throw new ArgumentException($"No traslations found for {language}");

            return _traslatedStrings.Traslations[language];
        }
    }
}
