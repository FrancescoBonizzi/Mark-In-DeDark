using MarkInDeDark.Library.Common;
using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Linq;

namespace MarkInDeDark.Common
{
    public static class GameConfigSerializerV1
    {
        public static GameConfig Deserialize(string gameConfigRaw)
        {
            var gameConfigDictionary = gameConfigRaw
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Split(':'))
                .ToDictionary(
                    r => r[0],
                    r => r[1]);

            var gameConfig = new GameConfig();

            gameConfig.PlayerLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(gameConfig.PlayerLightColorRGB1)]);
            gameConfig.PlayerLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(gameConfig.PlayerLightColorRGB2)]);
            gameConfig.PlayerSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.PlayerSpeed)]);

            gameConfig.ZombieLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(gameConfig.ZombieLightColorRGB1)]);
            gameConfig.ZombieLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(gameConfig.ZombieLightColorRGB2)]);
            gameConfig.ZombieSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.ZombieSpeed)]);

            gameConfig.BubuSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.BubuSpeed)]);
            gameConfig.BubuMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(gameConfig.BubuMaxCellsForNearCandle)]);

            gameConfig.SkeletronLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(gameConfig.SkeletronLightColorRGB1)]);
            gameConfig.SkeletronLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(gameConfig.SkeletronLightColorRGB2)]);
            gameConfig.SkeletronSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.SkeletronSpeed)]);
            gameConfig.SkeletronMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(gameConfig.SkeletronMaxCellsForNearCandle)]);
            gameConfig.SkeletronRageColorRGB = LoadColor(gameConfigDictionary[nameof(gameConfig.SkeletronRageColorRGB)]);
            gameConfig.SkeletronHuntingSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.SkeletronHuntingSpeed)]);

            gameConfig.GhostLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(gameConfig.GhostLightColorRGB1)]);
            gameConfig.GhostLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(gameConfig.GhostLightColorRGB2)]);
            gameConfig.GhostSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.GhostSpeed)]);

            gameConfig.GoblinLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(gameConfig.GoblinLightColorRGB1)]);
            gameConfig.GoblinLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(gameConfig.GoblinLightColorRGB2)]);
            gameConfig.GoblinSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.GoblinSpeed)]);
            gameConfig.GoblinMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(gameConfig.GoblinMaxCellsForNearCandle)]);
            gameConfig.GoblinSpeedIncrementForEachCandleTaken = LoadFloat(gameConfigDictionary[nameof(gameConfig.GoblinSpeedIncrementForEachCandleTaken)]);

            gameConfig.WerewolfLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(gameConfig.WerewolfLightColorRGB1)]);
            gameConfig.WerewolfLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(gameConfig.WerewolfLightColorRGB2)]);
            gameConfig.WerewolfSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.WerewolfSpeed)]);
            gameConfig.WerewolfMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(gameConfig.WerewolfMaxCellsForNearCandle)]);

            gameConfig.TaurusLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(gameConfig.TaurusLightColorRGB1)]);
            gameConfig.TaurusLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(gameConfig.TaurusLightColorRGB2)]);
            gameConfig.TaurusSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.TaurusSpeed)]);
            gameConfig.TaurusMaxCellsForNearCandle = LoadInt(gameConfigDictionary[nameof(gameConfig.TaurusMaxCellsForNearCandle)]);
            gameConfig.TaurusTimeToStartSeconds = LoadFloat(gameConfigDictionary[nameof(gameConfig.TaurusTimeToStartSeconds)]);
            gameConfig.TaurusTimeStunnedSeconds = LoadFloat(gameConfigDictionary[nameof(gameConfig.TaurusTimeStunnedSeconds)]);
            gameConfig.TaurusChargeSpeed = LoadFloat(gameConfigDictionary[nameof(gameConfig.TaurusChargeSpeed)]);
            gameConfig.TaurusChargeColorRGB = LoadColor(gameConfigDictionary[nameof(gameConfig.TaurusChargeColorRGB)]);

            gameConfig.CandleColorRGB1 = LoadColor(gameConfigDictionary[nameof(gameConfig.CandleColorRGB1)]);
            gameConfig.CandleColorRGB2 = LoadColor(gameConfigDictionary[nameof(gameConfig.CandleColorRGB2)]);
            gameConfig.CameraOffsetFromPlayer = LoadVector2(gameConfigDictionary[nameof(gameConfig.CameraOffsetFromPlayer)]);
            gameConfig.DoMonstersAttackTheSameCandle = LoadBoolean(gameConfigDictionary[nameof(gameConfig.DoMonstersAttackTheSameCandle)]);
            gameConfig.CandleTimeToLiveWhenAttackedSeconds = LoadFloat(gameConfigDictionary[nameof(gameConfig.CandleTimeToLiveWhenAttackedSeconds)]);
            gameConfig.IsFullScreen = LoadBoolean(gameConfigDictionary[nameof(gameConfig.IsFullScreen)]);
            gameConfig.BreakableTrapBreakingTimeSeconds = LoadFloat(gameConfigDictionary[nameof(gameConfig.BreakableTrapBreakingTimeSeconds)]);

            gameConfig.LittleLightColorRGB1 = LoadColor(gameConfigDictionary[nameof(gameConfig.LittleLightColorRGB1)]);
            gameConfig.LittleLightColorRGB2 = LoadColor(gameConfigDictionary[nameof(gameConfig.LittleLightColorRGB2)]);

            gameConfig.NotEnoughCandlesStringColorRGB = LoadColor(gameConfigDictionary[nameof(gameConfig.NotEnoughCandlesStringColorRGB)]);
            gameConfig.KeyFoundStringColorRGB = LoadColor(gameConfigDictionary[nameof(gameConfig.KeyFoundStringColorRGB)]);
            gameConfig.CandlePackFoundStringColorRGB = LoadColor(gameConfigDictionary[nameof(gameConfig.CandlePackFoundStringColorRGB)]);
            gameConfig.GameOverStringColorRGB = LoadColor(gameConfigDictionary[nameof(gameConfig.GameOverStringColorRGB)]);
            gameConfig.LevelCompletedStringColorRGB = LoadColor(gameConfigDictionary[nameof(gameConfig.LevelCompletedStringColorRGB)]);
            gameConfig.ClosedDoorStringColorRGB = LoadColor(gameConfigDictionary[nameof(gameConfig.ClosedDoorStringColorRGB)]);

            gameConfig.GodModeInfiniteCandles = LoadBoolean(gameConfigDictionary[nameof(gameConfig.GodModeInfiniteCandles)]);
            gameConfig.GodModePlayerDoesntDie = LoadBoolean(gameConfigDictionary[nameof(gameConfig.GodModePlayerDoesntDie)]);
            gameConfig.GodModePlayerSeesEverything = LoadBoolean(gameConfigDictionary[nameof(gameConfig.GodModePlayerSeesEverything)]);

            gameConfig.CharactersAnimationsMillisecondsBetweenFrames = LoadInt(gameConfigDictionary[nameof(gameConfig.CharactersAnimationsMillisecondsBetweenFrames)]);

            return gameConfig;
        }

        private static float LoadFloat(string value)
            => Convert.ToSingle(value, CultureInfo.InvariantCulture);

        private static int LoadInt(string value)
            => Convert.ToInt32(value);

        private static Color LoadColor(string value)
        {
            var splittedString = value.Split('-');
            return new Color(
                Convert.ToInt32(splittedString[0]),
                Convert.ToInt32(splittedString[1]),
                Convert.ToInt32(splittedString[2]));
        }

        private static ClassVector2 LoadVector2(string value)
        {
            var splittedString = value.Split('-');
            return new ClassVector2(
                Convert.ToSingle(splittedString[0], CultureInfo.InvariantCulture),
                Convert.ToSingle(splittedString[1], CultureInfo.InvariantCulture));
        }

        private static bool LoadBoolean(string value)
            => Convert.ToInt32(value) == 1 ? true : false;
    }
}
