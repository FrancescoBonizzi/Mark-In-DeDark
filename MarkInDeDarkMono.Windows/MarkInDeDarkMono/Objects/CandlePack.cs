using FlowsoftGamesMonogame;
using MarkInDeDarkMono.Assets;
using Microsoft.Xna.Framework;

namespace MarkInDeDarkMono.Objects
{
    public class CandlePack : CollectableObject
    {
        public int NCandlesInThisPack { get; protected set; }

        public CandlePack(
            int nCandles,
            AssetsLoader assetsLoader,
            Vector2 position, 
            SoundManager soundManager) 
            : base(
                  nCandles == 1 ? assetsLoader.Sprites["candle_pack_1"] 
                  : nCandles == 2 ? assetsLoader.Sprites["candle_pack_2"] 
                  : assetsLoader.Sprites["candle_pack_3"], position, soundManager)
        {
            NCandlesInThisPack = nCandles;
        }
    }
}
