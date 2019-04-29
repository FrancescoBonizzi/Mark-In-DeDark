using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MarkInDeDarkMono.Assets
{
    public class GameConfig
    {
        private IDictionary<string, string> _gameConfigDictionary;

        public Color PlayerLightColorRGB1 { get; private set; }
        public Color PlayerLightColorRGB2 { get; private set; }
        public float PlayerLightSize { get; private set; }
        public float PlayerSpeed { get; private set; }

        public Color ZombieLightColorRGB1 { get; private set; }
        public Color ZombieLightColorRGB2 { get; private set; }
        public float ZombieLightSize { get; private set; }
        public float ZombieSpeed { get; private set; }
        public int ZombieMaxCellsForNearCandle { get; private set; }

        public Color SkeletronLightColorRGB1 { get; private set; }
        public Color SkeletronLightColorRGB2 { get; private set; }
        public float SkeletronLightSize { get; private set; }
        public float SkeletronSpeed { get; private set; }
        public int SkeletronMaxCellsForNearCandle { get; private set; }
        public Color SkeletronRageColorRGB { get; private set; }
        public float SkeletronHuntingSpeed { get; private set; }

        public Color GoblinLightColorRGB1 { get; private set; }
        public Color GoblinLightColorRGB2 { get; private set; }
        public float GoblinLightSize { get; private set; }
        public float GoblinSpeed { get; private set; }
        public int GoblinMaxCellsForNearCandle { get; private set; }
        public float SpeedIncrementForEachCandleTaken { get; private set; }

        public Color WerewolfLightColorRGB1 { get; private set; }
        public Color WerewolfLightColorRGB2 { get; private set; }
        public float WerewolfLightSize { get; private set; }
        public float WerewolfSpeed { get; private set; }
        public int WerewolfMaxCellsForNearCandle { get; private set; }

        public Color TaurusLightColorRGB1 { get; private set; }
        public Color TaurusLightColorRGB2 { get; private set; }
        public float TaurusLightSize { get; private set; }
        public float TaurusSpeed { get; private set; }
        public int TaurusMaxCellsForNearCandle { get; private set; }
        public float TaurusTimeToStartSeconds { get; private set; }
        public float TaurusTimeStunnedSeconds { get; private set; }
        public float TaurusChargeSpeed { get; private set; }
        public Color TaurusChargeColorRGB { get; private set; }

        public Color CandleColorRGB1 { get; private set; }
        public Color CandleColorRGB2 { get; private set; }
        public float CandleLightSize { get; private set; }
        public float ExitLightSize { get; private set; }
        public Vector2 CameraOffsetFromPlayer { get; private set; }
        public bool DoMonstersAttackTheSameCandle { get; private set; }
        public float CandleTimeToLiveWhenAttackedSeconds { get; private set; }
        public bool IsFullScreen { get; private set; }
        public float BreakableTrapBreakingTimeSeconds { get; private set; }

        public Color NotEnoughCandlesStringColorRGB { get; private set; }
        public Color KeyFoundStringColorRGB { get; private set; }
        public Color CandlePackFoundStringColorRGB { get; private set; }
        public Color GameOverStringColorRGB { get; private set; }
        public Color LevelCompletedStringColorRGB { get; private set; }
        public Color ClosedDoorStringColorRGB { get; private set; }

        public GameConfig(string gameConfigRaw)
        {
            _gameConfigDictionary = gameConfigRaw
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Split(':'))
                .ToDictionary(
                    r => r[0],
                    r => r[1]);

            PlayerLightColorRGB1 = LoadColor("PlayerLightColorRGB1");
            PlayerLightColorRGB2 = LoadColor("PlayerLightColorRGB2");
            PlayerLightSize = LoadFloat("PlayerLightSize");
            PlayerSpeed = LoadFloat("PlayerSpeed");

            ZombieLightColorRGB1 = LoadColor("ZombieLightColorRGB1");
            ZombieLightColorRGB2 = LoadColor("ZombieLightColorRGB2");
            ZombieLightSize = LoadFloat("ZombieLightSize");
            ZombieSpeed = LoadFloat("ZombieSpeed");
            ZombieMaxCellsForNearCandle = LoadInt("ZombieMaxCellsForNearCandle");

            SkeletronLightColorRGB1 = LoadColor("SkeletronLightColorRGB1");
            SkeletronLightColorRGB2 = LoadColor("SkeletronLightColorRGB2");
            SkeletronLightSize = LoadFloat("SkeletronLightSize");
            SkeletronSpeed = LoadFloat("SkeletronSpeed");
            SkeletronMaxCellsForNearCandle = LoadInt("SkeletronMaxCellsForNearCandle");
            SkeletronRageColorRGB = LoadColor("SkeletronRageColorRGB");
            SkeletronHuntingSpeed = LoadFloat("SkeletronHuntingSpeed");

            GoblinLightColorRGB1 = LoadColor("GoblinLightColorRGB1");
            GoblinLightColorRGB2 = LoadColor("GoblinLightColorRGB2");
            GoblinLightSize = LoadFloat("GoblinLightSize");
            GoblinSpeed = LoadFloat("GoblinSpeed");
            GoblinMaxCellsForNearCandle = LoadInt("GoblinMaxCellsForNearCandle");
            SpeedIncrementForEachCandleTaken = LoadFloat("SpeedIncrementForEachCandleTaken");

            WerewolfLightColorRGB1 = LoadColor("WerewolfLightColorRGB1");
            WerewolfLightColorRGB2 = LoadColor("WerewolfLightColorRGB2");
            WerewolfLightSize = LoadFloat("WerewolfLightSize");
            WerewolfSpeed = LoadFloat("WerewolfSpeed");
            WerewolfMaxCellsForNearCandle = LoadInt("WerewolfMaxCellsForNearCandle");

            TaurusLightColorRGB1 = LoadColor("TaurusLightColorRGB1");
            TaurusLightColorRGB2 = LoadColor("TaurusLightColorRGB2");
            TaurusLightSize = LoadFloat("TaurusLightSize");
            TaurusSpeed = LoadFloat("TaurusSpeed");
            TaurusMaxCellsForNearCandle = LoadInt("TaurusMaxCellsForNearCandle");
            TaurusTimeToStartSeconds = LoadFloat("TaurusTimeToStartSeconds");
            TaurusTimeStunnedSeconds = LoadFloat("TaurusTimeStunnedSeconds");
            TaurusChargeSpeed = LoadFloat("TaurusChargeSpeed");
            TaurusChargeColorRGB = LoadColor("TaurusChargeColorRGB");

            CandleColorRGB1 = LoadColor("CandleColorRGB1");
            CandleColorRGB2 = LoadColor("CandleColorRGB2");
            CandleLightSize = LoadFloat("CandleLightSize");
            ExitLightSize = LoadFloat("ExitLightSize");
            CameraOffsetFromPlayer = LoadVector2("CameraOffsetFromPlayer");
            DoMonstersAttackTheSameCandle = LoadBoolean("DoMonstersAttackTheSameCandle");
            CandleTimeToLiveWhenAttackedSeconds = LoadFloat("CandleTimeToLiveWhenAttackedSeconds");
            IsFullScreen = LoadBoolean("IsFullScreen");
            BreakableTrapBreakingTimeSeconds = LoadFloat("BreakableTrapBreakingTimeSeconds");

            NotEnoughCandlesStringColorRGB = LoadColor("NotEnoughCandlesStringColorRGB");
            KeyFoundStringColorRGB = LoadColor("KeyFoundStringColorRGB");
            CandlePackFoundStringColorRGB = LoadColor("CandlePackFoundStringColorRGB");
            GameOverStringColorRGB = LoadColor("GameOverStringColorRGB");
            LevelCompletedStringColorRGB = LoadColor("LevelCompletedStringColorRGB");
            ClosedDoorStringColorRGB = LoadColor("ClosedDoorStringColorRGB");
        }

        private float LoadFloat(string key)
            => Convert.ToSingle(_gameConfigDictionary[key], CultureInfo.InvariantCulture);

        private int LoadInt(string key)
            => Convert.ToInt32(_gameConfigDictionary[key]);

        private Color LoadColor(string key)
        {
            var splittedString = _gameConfigDictionary[key].Split('-');
            return new Color(
                Convert.ToInt32(splittedString[0]),
                Convert.ToInt32(splittedString[1]),
                Convert.ToInt32(splittedString[2]));
        }

        private Vector2 LoadVector2(string key)
        {
            var splittedString = _gameConfigDictionary[key].Split('-');
            return new Vector2(
                Convert.ToSingle(splittedString[0], CultureInfo.InvariantCulture),
                Convert.ToSingle(splittedString[1], CultureInfo.InvariantCulture));
        }

        private bool LoadBoolean(string key)
            => Convert.ToInt32(_gameConfigDictionary[key]) == 1 ? true : false;

    }
}
