using FbonizziMonoGame.PlatformAbstractions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.GameGrid;

using System;

namespace MarkInDeDarkMono.Levels
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
        public Sprite DirtySprite { get; private set; }
        public Sprite HorizontalWallSprite { get; private set; }
        public Sprite VerticalWallSprite { get; private set; }
        public Sprite HorizontalDestroyableWallSprite { get; private set; }
        public Sprite VerticalDestroyableWallSprite { get; private set; }
        public Sprite AngleSprite { get; private set; }
        public Sprite WallDoorDownSprite { get; private set; }
        public Sprite WallDoorUpSprite { get; private set; }
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

            DarkSprite = assetsLoader.Sprites["celldark"];

            string groundTextureLevelSelector = $"Grounds/Type{LevelEditorDeserializedGrid.EnvironmentTexturePackId}";
            GroundSpriteType1 = assetsLoader.Sprites[$"{groundTextureLevelSelector}/cell"];
            GroundSpriteType2 = assetsLoader.Sprites[$"{groundTextureLevelSelector}/cell_2"];
            DirtySprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/dirty"];
            HorizontalWallSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_horizontal"];
            VerticalWallSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_vertical"];
            HorizontalDestroyableWallSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_horizontal_breakable"];
            VerticalDestroyableWallSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_vertical_breakable"];
            AngleSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/pillar"];
            WallDoorDownSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_door"];
            WallDoorUpSprite = assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_door_up"];
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
