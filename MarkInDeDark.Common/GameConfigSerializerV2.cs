using MarkInDeDark.Library.Common;
using Newtonsoft.Json;

namespace MarkInDeDark.Common
{
    public static class GameConfigSerializerV2
    {
        public static string Serialize(GameConfig gameConfig)
            => JsonConvert.SerializeObject(gameConfig);

        public static GameConfig Deserialize(string fileContent)
        {
            try
            {
                return JsonConvert.DeserializeObject<GameConfig>(fileContent);
            }
            catch (JsonException)
            {
                // Se c'è un'eccezione JSON, provo a deserializzare col vecchio metodo
                return GameConfigSerializerV1.Deserialize(fileContent);
            }
        }
    }
}
