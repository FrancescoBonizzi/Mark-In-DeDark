using MarkInDeDark.Common;
using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Linq;

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
        public int ZombieMaxCellsForNearCandle { get; set; }

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

        public GameConfig(string gameConfigRaw)
        {
            var gameConfigDictionary = gameConfigRaw
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Split(':'))
                .ToDictionary(
                    r => r[0],
                    r => r[1]);

            PlayerLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(PlayerLightColorRGB1)]);
            PlayerLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(PlayerLightColorRGB2)]);
            PlayerLightSize = LoadFloat(gameConfigDictionary[nameof(PlayerLightSize)]);
            PlayerSpeed = LoadFloat(gameConfigDictionary[nameof(PlayerSpeed)]);

            ZombieLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(ZombieLightColorRGB1)]);
            ZombieLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(ZombieLightColorRGB2)]);
            ZombieLightSize = LoadFloat(gameConfigDictionary[nameof(ZombieLightSize)]);
            ZombieSpeed = LoadFloat(gameConfigDictionary[nameof(ZombieSpeed)]);
            ZombieMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(ZombieMaxCellsForNearCandle)]);

            SkeletronLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(SkeletronLightColorRGB1)]);
            SkeletronLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(SkeletronLightColorRGB2)]);
            SkeletronLightSize = LoadFloat(gameConfigDictionary[nameof(SkeletronLightSize)]);
            SkeletronSpeed = LoadFloat(gameConfigDictionary[nameof(SkeletronSpeed)]);
            SkeletronMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(SkeletronMaxCellsForNearCandle)]);
            SkeletronRageColorRGB = LoadColor(gameConfigDictionary[nameof(SkeletronRageColorRGB)]);
            SkeletronHuntingSpeed = LoadFloat(gameConfigDictionary[nameof(SkeletronHuntingSpeed)]);

            GhostLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(GhostLightColorRGB1)]);
            GhostLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(GhostLightColorRGB2)]);
            GhostLightSize = LoadFloat(gameConfigDictionary[nameof(GhostLightSize)]);
            GhostSpeed = LoadFloat(gameConfigDictionary[nameof(GhostSpeed)]);

            GoblinLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(GoblinLightColorRGB1)]);
            GoblinLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(GoblinLightColorRGB2)]);
            GoblinLightSize = LoadFloat(gameConfigDictionary[nameof(GoblinLightSize)]);
            GoblinSpeed = LoadFloat(gameConfigDictionary[nameof(GoblinSpeed)]);
            GoblinMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(GoblinMaxCellsForNearCandle)]);
            SpeedIncrementForEachCandleTaken = LoadFloat(gameConfigDictionary[nameof(SpeedIncrementForEachCandleTaken)]);

            WerewolfLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(WerewolfLightColorRGB1)]);
            WerewolfLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(WerewolfLightColorRGB2)]);
            WerewolfLightSize = LoadFloat(gameConfigDictionary[nameof(WerewolfLightSize)]);
            WerewolfSpeed = LoadFloat(gameConfigDictionary[nameof(WerewolfSpeed)]);
            WerewolfMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(WerewolfMaxCellsForNearCandle)]);

            TaurusLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(TaurusLightColorRGB1)]);
            TaurusLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(TaurusLightColorRGB2)]);
            TaurusLightSize = LoadFloat(gameConfigDictionary[nameof(TaurusLightSize)]);
            TaurusSpeed = LoadFloat(gameConfigDictionary[nameof(TaurusSpeed)]);
            TaurusMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(TaurusMaxCellsForNearCandle)]);
            TaurusTimeToStartSeconds = LoadFloat(gameConfigDictionary[nameof(TaurusTimeToStartSeconds)]);
            TaurusTimeStunnedSeconds = LoadFloat(gameConfigDictionary[nameof(TaurusTimeStunnedSeconds)]);
            TaurusChargeSpeed = LoadFloat(gameConfigDictionary[nameof(TaurusChargeSpeed)]);
            TaurusChargeColorRGB = LoadColor(gameConfigDictionary[nameof(TaurusChargeColorRGB)]);

            CandleColorRGB1 = LoadColor(gameConfigDictionary[nameof(CandleColorRGB1)]);
            CandleColorRGB2 = LoadColor(gameConfigDictionary[nameof(CandleColorRGB2)]);
            CandleLightSize = LoadFloat(gameConfigDictionary[nameof(CandleLightSize)]);
            ExitLightSize = LoadFloat(gameConfigDictionary[nameof(ExitLightSize)]);
            CameraOffsetFromPlayer = LoadVector2(gameConfigDictionary[nameof(CameraOffsetFromPlayer)]);
            DoMonstersAttackTheSameCandle = LoadBoolean(gameConfigDictionary[nameof(DoMonstersAttackTheSameCandle)]);
            CandleTimeToLiveWhenAttackedSeconds = LoadFloat(gameConfigDictionary[nameof(CandleTimeToLiveWhenAttackedSeconds)]);
            IsFullScreen = LoadBoolean(gameConfigDictionary[nameof(IsFullScreen)]);
            BreakableTrapBreakingTimeSeconds = LoadFloat(gameConfigDictionary[nameof(BreakableTrapBreakingTimeSeconds)]);

            NotEnoughCandlesStringColorRGB = LoadColor(gameConfigDictionary[nameof(NotEnoughCandlesStringColorRGB)]);
            KeyFoundStringColorRGB = LoadColor(gameConfigDictionary[nameof(KeyFoundStringColorRGB)]);
            CandlePackFoundStringColorRGB = LoadColor(gameConfigDictionary[nameof(CandlePackFoundStringColorRGB)]);
            GameOverStringColorRGB = LoadColor(gameConfigDictionary[nameof(GameOverStringColorRGB)]);
            LevelCompletedStringColorRGB = LoadColor(gameConfigDictionary[nameof(LevelCompletedStringColorRGB)]);
            ClosedDoorStringColorRGB = LoadColor(gameConfigDictionary[nameof(ClosedDoorStringColorRGB)]);

            GodModeInfiniteCandles = LoadBoolean(gameConfigDictionary[nameof(GodModeInfiniteCandles)]);
            GodModePlayerDoesntDie = LoadBoolean(gameConfigDictionary[nameof(GodModePlayerDoesntDie)]);
            GodModePlayerSeesEverything = LoadBoolean(gameConfigDictionary[nameof(GodModePlayerSeesEverything)]);

            CharactersAnimationsMillisecondsBetweenFrames = LoadInt(gameConfigDictionary[nameof(CharactersAnimationsMillisecondsBetweenFrames)]);
        }

        private float LoadFloat(string value)
            => Convert.ToSingle(value, CultureInfo.InvariantCulture);

        private int LoadInt(string value)
            => Convert.ToInt32(value);

        private Color LoadColor(string value)
        {
            var splittedString = value.Split('-');
            return new Color(
                Convert.ToInt32(splittedString[0]),
                Convert.ToInt32(splittedString[1]),
                Convert.ToInt32(splittedString[2]));
        }

        private ClassVector2 LoadVector2(string value)
        {
            var splittedString = value.Split('-');
            return new ClassVector2(
                Convert.ToSingle(splittedString[0], CultureInfo.InvariantCulture),
                Convert.ToSingle(splittedString[1], CultureInfo.InvariantCulture));
        }

        private bool LoadBoolean(string value)
            => Convert.ToInt32(value) == 1 ? true : false;
    }
}
