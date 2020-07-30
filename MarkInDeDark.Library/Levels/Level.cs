using FbonizziMonoGame.PlatformAbstractions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.GameGridCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MarkInDeDark.Library.Levels
{
    public class Level
    {
        public string LevelTitleToShow { get; private set; }

        public bool IsCompleted { get; private set; }
        public int? RecordUsedCandles { get; private set; }
        public TimeSpan? RecordCompletionTime { get; private set; }

        public Dictionary<GroundTypes, (Sprite Base, Sprite Normal, Sprite Specular)> GroundSprites;
        public Sprite DarkSprite { get; private set; }

        public Dictionary<WallTypes, (Sprite Base, Sprite Normal)> Decals;
        public Dictionary<GroundTypes, (Sprite Base, Sprite Normal)> Riempitives;

        public List<Sprite> DirtySprites { get; private set; }
        public List<(Sprite Base, Sprite Normal)> RandomHorizontalWallSprites { get; private set; }
        public List<(Sprite Base, Sprite Normal)> RandomVerticalWallSprites { get; private set; }
        public List<(Sprite Base, Sprite Normal)> RandomPillarSprites { get; private set; }

        public SpriteAnimation DoorOpenAnimation { get; private set; }
        public SpriteAnimation DoorCloseAnimation { get; private set; }

        public (Sprite Base, Sprite Normal) WallHorizontalBreakableSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallHorizontalBrokenSprite { get; private set; }

        public (Sprite Base, Sprite Normal) WallVerticalBreakableSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallVerticalBrokenASprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallVerticalBrokenBSprite { get; private set; }

        public (Sprite Base, Sprite Normal) WallSpecial1HorizontalSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallSpecial2HorizontalSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallSpecial3HorizontalSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallSpecial4HorizontalSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallSpecial5HorizontalSprite { get; private set; }

        public (Sprite Base, Sprite Normal) WallEntranceClosedHorizontalSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallEntranceOpenHorizontalSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallEntranceArcHorizontalSprite { get; private set; }

        public (Sprite Base, Sprite Normal) WallExitClosedHorizontalSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallExitOpenHorizontalSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallExitArcHorizontalSprite { get; private set; }

        public (Sprite Base, Sprite Normal) WallDoorBaseSprite { get; private set; }
        public (Sprite Base, Sprite Normal) WallDoorArcSprite { get; private set; }

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

            string groundTextureLevelSelector = $"Grounds/pacco{LevelEditorDeserializedGrid.EnvironmentTexturePackId}";

            // Carico le texture pavimento basandomi sul fatto che si chiamano cellx
            {
                int terraNumber = 1;
                GroundSprites = new Dictionary<GroundTypes, (Sprite Base, Sprite Normal, Sprite Specular)>();
                foreach (var groundType in ((GroundTypes[])Enum.GetValues(typeof(GroundTypes))).Where(g => g.ToString().StartsWith("Terra")))
                {
                    if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/cell{terraNumber}"))
                    {
                        GroundSprites.Add(
                            groundType,
                            (assetsLoader.Sprites[$"{groundTextureLevelSelector}/cell{terraNumber}"],
                            GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/cell{terraNumber}"),
                            GetSpecularOrNull(assetsLoader, $"{groundTextureLevelSelector}/cell{terraNumber}")));
                    }
                    ++terraNumber;
                }
            }

            // Carico le texture decal basandomi sul fatto che si chiamano decal_x
            {
                int decalNumber = 1;
                Decals = new Dictionary<WallTypes, (Sprite Base, Sprite Normal)>();
                foreach (var decalType in ((WallTypes[])Enum.GetValues(typeof(WallTypes))).Where(w => w.ToString().StartsWith("Decal")))
                {
                    if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/Decals/decal_{decalNumber}"))
                    {
                        Decals.Add(
                            decalType,
                            (assetsLoader.Sprites[$"{groundTextureLevelSelector}/Decals/decal_{decalNumber}"],
                            GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/Decals/decal_{decalNumber}")));
                    }

                    ++decalNumber;
                }
            }

            // Carico le texture riempitive basandomi sul fatto che si chiamano wall_riempimento_x
            {
                int riempitiveNumber = 1;
                Riempitives = new Dictionary<GroundTypes, (Sprite Base, Sprite Normal)>();
                foreach (var riempitiveType in ((GroundTypes[])Enum.GetValues(typeof(GroundTypes))).Where(r => r.ToString().StartsWith("Riempimento")))
                {
                    if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/wall_riempimento_{riempitiveNumber}"))
                    {
                        Riempitives.Add(
                            riempitiveType,
                            (assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_riempimento_{riempitiveNumber}"],
                            GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_riempimento_{riempitiveNumber}")));
                    }

                    ++riempitiveNumber;
                }
            }

            DirtySprites = new List<Sprite>();
            for (int i = 1; i < 20; ++i)
            {
                if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/Dirty/dirty_{i}"))
                    DirtySprites.Add(assetsLoader.Sprites[$"{groundTextureLevelSelector}/Dirty/dirty_{i}"]);
            }

            RandomHorizontalWallSprites = new List<(Sprite Base, Sprite Normal)>();
            for (int i = 1; i < 20; ++i)
            {
                (Sprite Base, Sprite Normal) thisWallSprites = (null, null);
                if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/RandomWalls/wall_horizontal_{i}"))
                    thisWallSprites.Base = assetsLoader.Sprites[$"{groundTextureLevelSelector}/RandomWalls/wall_horizontal_{i}"];
                if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/RandomWalls/wall_horizontal_{i}_normal"))
                    thisWallSprites.Normal = assetsLoader.Sprites[$"{groundTextureLevelSelector}/RandomWalls/wall_horizontal_{i}_normal"];

                if (thisWallSprites.Base != null)
                    RandomHorizontalWallSprites.Add(thisWallSprites);
            }

            RandomVerticalWallSprites = new List<(Sprite Base, Sprite Normal)>();
            for (int i = 1; i < 20; ++i)
            {
                (Sprite Base, Sprite Normal) thisWallSprites = (null, null);
                if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/RandomWalls/wall_vertical_{i}"))
                    thisWallSprites.Base = assetsLoader.Sprites[$"{groundTextureLevelSelector}/RandomWalls/wall_vertical_{i}"];
                thisWallSprites.Normal = GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/RandomWalls/wall_vertical_{i}");

                if (thisWallSprites.Base != null)
                    RandomVerticalWallSprites.Add(thisWallSprites);
            }

            RandomPillarSprites = new List<(Sprite Base, Sprite Normal)>();
            for (int i = 1; i < 20; ++i)
            {
                (Sprite Base, Sprite Normal) thisPillarSprites = (null, null);
                if (assetsLoader.Sprites.ContainsKey($"{groundTextureLevelSelector}/RandomPillars/pillar_{i}"))
                    thisPillarSprites.Base = assetsLoader.Sprites[$"{groundTextureLevelSelector}/RandomPillars/pillar_{i}"];
                thisPillarSprites.Normal = GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/RandomPillars/pillar_{i}");

                if (thisPillarSprites.Base != null)
                    RandomPillarSprites.Add(thisPillarSprites);
            }

            // Door
            var doorOpeningAnimation = new List<Sprite>();
            for (int i = 1; i <= 8; ++i)
                doorOpeningAnimation.Add(assetsLoader.Sprites[$"{groundTextureLevelSelector}/Door/door_{i}"]);
            DoorOpenAnimation = new SpriteAnimation(doorOpeningAnimation, TimeSpan.FromMilliseconds(20), false);

            var doorClosingAnimation = new List<Sprite>(doorOpeningAnimation);
            doorClosingAnimation.Reverse();
            DoorCloseAnimation = new SpriteAnimation(doorClosingAnimation, TimeSpan.FromMilliseconds(20), false);

            WallHorizontalBreakableSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_horizontal_breakable"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_horizontal_breakable"));
            WallHorizontalBrokenSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_horizontal_broken"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_horizontal_broken"));
            WallVerticalBreakableSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_vertical_breakable"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_vertical_breakable"));
            WallVerticalBrokenASprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_vertical_broken_A"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_vertical_broken_A"));
            WallVerticalBrokenBSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_vertical_broken_B"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_vertical_broken_B"));

            WallSpecial1HorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/SpecialWalls/wall_special_1"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/SpecialWalls/wall_special_1"));
            WallSpecial2HorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/SpecialWalls/wall_special_2"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/SpecialWalls/wall_special_2"));
            WallSpecial3HorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/SpecialWalls/wall_special_3"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/SpecialWalls/wall_special_3"));
            WallSpecial4HorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/SpecialWalls/wall_special_4"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/SpecialWalls/wall_special_4"));
            WallSpecial5HorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/SpecialWalls/wall_special_5"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/SpecialWalls/wall_special_5"));

            WallEntranceClosedHorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_ingresso_closed"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_ingresso_closed"));
            WallEntranceOpenHorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_ingresso_open"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_ingresso_open"));
            WallEntranceArcHorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_ingresso_up"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_ingresso_up"));

            WallExitClosedHorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_uscita_closed"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_uscita_closed"));
            WallExitOpenHorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_uscita_open"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_uscita_open"));
            WallExitArcHorizontalSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_uscita_up"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_uscita_up"));

            WallDoorBaseSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_door"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_door"));
            WallDoorArcSprite = (
                assetsLoader.Sprites[$"{groundTextureLevelSelector}/wall_door_up"],
                GetNormalOrNull(assetsLoader, $"{groundTextureLevelSelector}/wall_door_up"));

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

        private Sprite GetNormalOrNull(AssetsLoader assetsLoader, string baseTextureName)
        {
            var normalName = $"{baseTextureName}_normal";
            if (assetsLoader.Sprites.ContainsKey(normalName))
                return assetsLoader.Sprites[normalName];

            return null;
        }

        private Sprite GetSpecularOrNull(AssetsLoader assetsLoader, string baseTextureName)
        {
            var specularName = $"{baseTextureName}_spec";
            if (assetsLoader.Sprites.ContainsKey(specularName))
                return assetsLoader.Sprites[specularName];

            return null;
        }

        public void SetCompletionParameters(bool isCompleted)
        {
            if (isCompleted)
                SetCompleted();
        }

        private void SetCompleted()
        {
            _settingsRepository.SetBool(_isCompletedKey, true);
            IsCompleted = true;
        }

        public override string ToString()
            => LevelTitleToShow;
    }
}
