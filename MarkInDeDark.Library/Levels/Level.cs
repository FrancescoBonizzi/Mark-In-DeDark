using FbonizziMonoGame.PlatformAbstractions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.GameGridCommon;

using System;
using System.Collections.Generic;

namespace MarkInDeDark.Library.Levels
{
    public class Level
    {
        public string LevelTitleToShow { get; private set; }

        public bool IsCompleted { get; private set; }
        public int? RecordUsedCandles { get; private set; }
        public TimeSpan? RecordCompletionTime { get; private set; }

        public Sprite GroundSpriteType1 { get; private set; }
        public Sprite GroundSpriteType2 { get; private set; }
        public Sprite DarkSprite { get; private set; }
        public List<Sprite> DirtySprites { get; private set; }
        public List<Sprite> RandomHorizontalWallSprites { get; private set; }
        public List<Sprite> RandomVerticalWallSprites { get; private set; }
        public Sprite HorizontalDestroyableWallSprite { get; private set; }
        public Sprite VerticalDestroyableWallSprite { get; private set; }
        public Sprite AngleSprite { get; private set; }
        public Sprite WallDoorDownSprite { get; private set; }
        public Sprite WallDoorArcSprite { get; private set; }
        public Sprite SpecialWallSprite { get; private set; }
        public double DirtyProbability { get; private set; }

        public GameGridBundle LevelEditorDeserializedGrid { get; }
        private readonly ISettingsRepository _settingsRepository;

        private readonly string _isCompletedKey;
        private readonly string _recordUsedCandlesKey;
        private readonly string _recordCompletionTimeKey;

        public Level(
            AssetsLoader assetsLoader,
            ISettingsRepository settingsRepository,
            string levelContentRaw,
            string levelTitleToShow)
        {
            LevelTitleToShow = levelTitleToShow;

            var serializer = new GameGridDeserializerManager(levelContentRaw);
            LevelEditorDeserializedGrid = serializer.DeserializeLevel();

            DarkSprite = assetsLoader.Sprites["Grounds/celldark"];

            string groundTextureLevelSelector = $"Grounds/Type{LevelEditorDeserializedGrid.EnvironmentTexturePackId}";
            GroundSpriteType1 = assetsLoader.Sprites[$"{groundTextureLevelSelector}/cell"];
            GroundSpriteType2 = assetsLoader.Sprites[$"{groundTextureLevelSelector}/cell2"];

            DirtySprites = new List<Sprite>();
            for (int i = 1; i < 20; ++i)
            {
                if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/Dirty/dirty_{i}"))
                    DirtySprites.Add(assetsLoader.Sprites[$"{groundTextureLevelSelector}/Dirty/dirty_{i}"]);
            }

            RandomHorizontalWallSprites = new List<Sprite>();
            for (int i = 1; i < 20; ++i)
            {
                if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/RandomWalls/wall_horizontal_{i}"))
                    RandomHorizontalWallSprites.Add(assetsLoader.Sprites[$"{groundTextureLevelSelector}/RandomWalls/wall_horizontal_{i}"]);
            }

            RandomVerticalWallSprites = new List<Sprite>();
            for (int i = 1; i < 20; ++i)
            {
                if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/RandomWalls/wall_vertical_{i}"))
                    RandomVerticalWallSprites.Add(assetsLoader.Sprites[$"{groundTextureLevelSelector}/RandomWalls/wall_vertical_{i}"]);
            }

            HorizontalDestroyableWallSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_horizontal_breakable"];
            VerticalDestroyableWallSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_vertical_breakable"];
            AngleSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/pillar"];
            WallDoorDownSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_door"];
            WallDoorArcSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_door_up"];
            SpecialWallSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_oversize"];
            DirtyProbability = LevelEditorDeserializedGrid.DirtyProbabilityPercentage / 100.0;

            _settingsRepository = settingsRepository;
            _isCompletedKey = $"{levelTitleToShow}-IsCompleted";
            _recordUsedCandlesKey = $"{levelTitleToShow}-RecordUsedCandles";
            _recordCompletionTimeKey = $"{levelTitleToShow}-RecordCompletionTime";

            IsCompleted = settingsRepository.GetOrSetBool(_isCompletedKey, false);

            RecordUsedCandles = settingsRepository.GetOrSetInt(_recordUsedCandlesKey, -1);
            if (RecordUsedCandles == -1)
                RecordUsedCandles = null;

            RecordCompletionTime = settingsRepository.GetOrSetTimeSpan(_recordCompletionTimeKey, TimeSpan.Zero);
            if (RecordCompletionTime == TimeSpan.Zero)
                RecordCompletionTime = null;
        }

        public void SetCompletionParameters(
            bool isCompleted,
            int usedCandles,
            TimeSpan completionTime)
        {
            if (isCompleted)
                SetCompleted();

            SetRecordUsedCandles(usedCandles);
            SetRecordCompletionTime(completionTime);
        }

        private void SetCompleted()
        {
            _settingsRepository.SetBool(_isCompletedKey, true);
            IsCompleted = true;
        }

        private void SetRecordUsedCandles(int usedCandles)
        {
            if (usedCandles < RecordUsedCandles || RecordUsedCandles == null)
            {
                _settingsRepository.SetInt(_recordUsedCandlesKey, usedCandles);
                RecordUsedCandles = usedCandles;
            }
        }

        private void SetRecordCompletionTime(TimeSpan completionTime)
        {
            if (completionTime < RecordCompletionTime || RecordCompletionTime == null)
            {
                _settingsRepository.SetTimeSpan(_recordCompletionTimeKey, completionTime);
                RecordCompletionTime = completionTime;
            }
        }

        public override string ToString()
            => LevelTitleToShow;
    }
}
