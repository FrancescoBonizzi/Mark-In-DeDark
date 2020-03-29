using FbonizziMonoGame.PlatformAbstractions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.GameGrid;
using MarkInDeDarkMono.Assets;
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
        public Sprite MacerieCellSprite { get; private set; }
        public Sprite DirtySprite { get; private set; }
        public Sprite WallSprite { get; private set; }
        public Sprite DestroyableWallSprite { get; private set; }
        public Sprite AngleSprite { get; private set; }
        public Sprite DoorSprite { get; private set; }
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

            GroundSpriteType1 = assetsLoader.Sprites[$"cell{LevelEditorDeserializedGrid.EnvironmentTexturePackId}"];

            if (assetsLoader.Sprites.ContainsKey($"cell{LevelEditorDeserializedGrid.EnvironmentTexturePackId}_2"))
                GroundSpriteType2 = assetsLoader.Sprites[$"cell{LevelEditorDeserializedGrid.EnvironmentTexturePackId}_2"];

            DirtySprite = assetsLoader.Sprites[$"dirty{LevelEditorDeserializedGrid.EnvironmentTexturePackId}"];
            WallSprite = assetsLoader.Sprites[$"wall{LevelEditorDeserializedGrid.EnvironmentTexturePackId}"];
            DestroyableWallSprite = assetsLoader.Sprites[$"wall_breakable"];
            AngleSprite = assetsLoader.Sprites[$"angle{LevelEditorDeserializedGrid.EnvironmentTexturePackId}"];
            MacerieCellSprite = assetsLoader.Sprites["macerie0"];
            DoorSprite = assetsLoader.Sprites["door"];
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
