using MarkInDeDark.Common;
using Microsoft.Xna.Framework;

namespace MarkInDeDarkMono.Common
{
    public class GameConfig
    {
        public Color PlayerLightColorRGB1 { get; set; }
        public Color PlayerLightColorRGB2 { get; set; }
        public float PlayerLightSize { get; set; }
        public float PlayerSpeed { get; set; }

        public Color ZombieLightColorRGB1 { get; set; }
        public Color ZombieLightColorRGB2 { get; set; }
        public float ZombieLightSize { get; set; }
        public float ZombieSpeed { get; set; }

        public float BubuSpeed { get; set; }
        public int BubuMaxCellsForNearCandle { get; set; }

        public Color GhostLightColorRGB1 { get; set; }
        public Color GhostLightColorRGB2 { get; set; }
        public float GhostLightSize { get; set; }
        public float GhostSpeed { get; set; }

        public Color SkeletronLightColorRGB1 { get; set; }
        public Color SkeletronLightColorRGB2 { get; set; }
        public float SkeletronLightSize { get; set; }
        public float SkeletronSpeed { get; set; }
        public int SkeletronMaxCellsForNearCandle { get; set; }
        public Color SkeletronRageColorRGB { get; set; }
        public float SkeletronHuntingSpeed { get; set; }

        public Color GoblinLightColorRGB1 { get; set; }
        public Color GoblinLightColorRGB2 { get; set; }
        public float GoblinLightSize { get; set; }
        public float GoblinSpeed { get; set; }
        public int GoblinMaxCellsForNearCandle { get; set; }
        public float SpeedIncrementForEachCandleTaken { get; set; }

        public Color WerewolfLightColorRGB1 { get; set; }
        public Color WerewolfLightColorRGB2 { get; set; }
        public float WerewolfLightSize { get; set; }
        public float WerewolfSpeed { get; set; }
        public int WerewolfMaxCellsForNearCandle { get; set; }

        public Color TaurusLightColorRGB1 { get; set; }
        public Color TaurusLightColorRGB2 { get; set; }
        public float TaurusLightSize { get; set; }
        public float TaurusSpeed { get; set; }
        public int TaurusMaxCellsForNearCandle { get; set; }
        public float TaurusTimeToStartSeconds { get; set; }
        public float TaurusTimeStunnedSeconds { get; set; }
        public float TaurusChargeSpeed { get; set; }
        public Color TaurusChargeColorRGB { get; set; }

        public Color CandleColorRGB1 { get; set; }
        public Color CandleColorRGB2 { get; set; }
        public float CandleLightSize { get; set; }
        public float ExitLightSize { get; set; }
        public ClassVector2 CameraOffsetFromPlayer { get; set; }
        public bool DoMonstersAttackTheSameCandle { get; set; }
        public float CandleTimeToLiveWhenAttackedSeconds { get; set; }
        public bool IsFullScreen { get; set; }
        public float BreakableTrapBreakingTimeSeconds { get; set; }

        public Color LittleLightColorRGB1 { get; set; }
        public Color LittleLightColorRGB2 { get; set; }
        public float LittleLightLightSize { get; set; }

        public Color NotEnoughCandlesStringColorRGB { get; set; }
        public Color KeyFoundStringColorRGB { get; set; }
        public Color CandlePackFoundStringColorRGB { get; set; }
        public Color GameOverStringColorRGB { get; set; }
        public Color LevelCompletedStringColorRGB { get; set; }
        public Color ClosedDoorStringColorRGB { get; set; }

        public bool GodModeInfiniteCandles { get; set; }
        public bool GodModePlayerDoesntDie { get; set; }
        public bool GodModePlayerSeesEverything { get; set; }

        public int CharactersAnimationsMillisecondsBetweenFrames { get; set; }
    }
}
