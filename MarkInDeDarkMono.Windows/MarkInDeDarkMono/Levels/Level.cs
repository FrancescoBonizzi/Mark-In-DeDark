using FlowsoftGamesMonogame;
using System;

namespace MarkInDeDarkMono.Levels
{
    public class Level
    {
        public string LevelTitleToShow { get; private set; }
        public string LevelContentRaw { get; }

        public bool IsCompleted { get; private set; }
        public int? RecordUsedCandles { get; private set; }
        public TimeSpan? RecordCompletionTime { get; private set; }

        public Sprite Cell { get; private set; }
        public Sprite WhiteCell { get; private set; }
        public Sprite Dirty { get; private set; }
        public Sprite Wall { get; private set; }
        public Sprite DestroyableWall { get; private set; }
        public Sprite Angle { get; private set; }
        public double DirtyProbability { get; private set; }

        private readonly ISettingsRepository _settingsRepository;

        private readonly string _isCompletedKey;
        private readonly string _recordUsedCandlesKey;
        private readonly string _recordCompletionTimeKey;

        public Level(
            ISettingsRepository settingsRepository,
            string levelContentRaw,
            string levelTitleToShow,
            Sprite cell,
            Sprite dirty,
            Sprite wall,
            Sprite destroyableWall,
            Sprite angle,
            Sprite whiteCell,
            double dirtyProbability)
        {
            LevelTitleToShow = levelTitleToShow;
            LevelContentRaw = levelContentRaw;

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

            Cell = cell;
            Dirty = dirty;
            DirtyProbability = dirtyProbability;
            Wall = wall;
            DestroyableWall = destroyableWall;
            Angle = angle;
            WhiteCell = whiteCell;
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
