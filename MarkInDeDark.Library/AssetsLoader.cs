using FbonizziMonoGame.Assets;
using FbonizziMonoGame.PlatformAbstractions;
using FbonizziMonoGame.Sprites;
using MarkInDeDark.Library.Common;
using MarkInDeDark.Library.Levels;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkInDeDark.Library
{
    public class AssetsLoader
    {
        private readonly IDictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();
        public IDictionary<string, Sprite> Sprites { get; } = new Dictionary<string, Sprite>();
        public IDictionary<string, SoundEffect> Sounds { get; } = new Dictionary<string, SoundEffect>();
        public SpriteFont Font { get; private set; }
        public List<Level> Levels { get; private set; } = new List<Level>();

        public Effect LightMaskShader { get; private set; }
        public Effect CircularNormalMapShader { get; private set; }

        private readonly CustomSpriteImporter _textureImporter;
        private readonly GameConfig _gameConfig;
        private readonly ContentManager _contentManager;
        private readonly ISettingsRepository _settingsRepository;
        private readonly ITextFileLoader _fileLoader;

        private IEnumerable<string> _levelPaths;

        public AssetsLoader(
            ContentManager contentManager,
            ITextFileLoader fileLoader,
            IEnumerable<string> levelPaths,
            ISettingsRepository settingsRepository,
            GameConfig gameConfig)
        {
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
            _fileLoader = fileLoader ?? throw new ArgumentNullException(nameof(fileLoader));
            _levelPaths = levelPaths ?? throw new ArgumentNullException(nameof(levelPaths));
            _textureImporter = new CustomSpriteImporter(fileLoader);
            _gameConfig = gameConfig ?? throw new ArgumentNullException(nameof(gameConfig));
            LoadResources();
        }

        public SpriteAnimation GetAnimation(string key)
        {
            if (!_animations.ContainsKey(key))
            {
                throw new KeyNotFoundException($"{key} not found");
            }

            // La devo clonare perché ognuno deve avere la propria, anche per le animazioni condivise
            return new SpriteAnimation(_animations[key]);
        }

        private void LoadResources()
        {
            Font = _contentManager.Load<SpriteFont>("font");
            LightMaskShader = _contentManager.Load<Effect>("Effects/lightmask");
            CircularNormalMapShader = _contentManager.Load<Effect>("Effects/circularnormalmap");

            LoadSprites();
            LoadAnimations();
            LoadSounds();
            LoadOtherResources();
            LoadLevels();
        }

        public void ForceLoadLevel(string levelFilePath)
        {
            _levelPaths = new List<string>() { levelFilePath };
            Levels.Clear();
            LoadLevels();
        }

        private void LoadLevels()
        {
            foreach (var levelPath in _levelPaths)
            {
                Levels.Add(
                   new Level(
                       this,
                       _settingsRepository,
                       _fileLoader.LoadFile(levelPath),
                       Path.GetFileNameWithoutExtension(levelPath)));
            }

            Levels = Levels.OrderBy(l => l.LevelTitleToShow).ToList();
        }

        private void LoadSprites()
        {
            const string gameSpriteSheetName = "Spritesheet";

            for (int i = 0; i < 3; ++i)
            {
                var gameSpriteSheet = _contentManager.Load<Texture2D>($"{gameSpriteSheetName}-{i}");
                var gameSpritesDescriptions = _textureImporter.Import($"Assets/{gameSpriteSheetName}-{i}.txt");
                AddSpritesFromDictionary(gameSpritesDescriptions, gameSpriteSheet);
            }

            var menuTexture = _contentManager.Load<Texture2D>("menu");
            var menuSprite = new Sprite(
                new SpriteDescription()
                {
                    X = 0,
                    Y = 0,
                    Width = menuTexture.Width,
                    Height = menuTexture.Height,
                    Name = menuTexture.Name
                },
                menuTexture);
            Sprites.Add("menu", menuSprite);

            var levelsTexture = _contentManager.Load<Texture2D>("levels");
            var levelsBackgroundSprite = new Sprite(
                new SpriteDescription()
                {
                    X = 0,
                    Y = 0,
                    Width = menuTexture.Width,
                    Height = menuTexture.Height,
                    Name = menuTexture.Name
                },
                levelsTexture);
            Sprites.Add("levels", levelsBackgroundSprite);
        }

        private void LoadSounds()
        {
            Sounds.Add("TaurusStartingCharge", _contentManager.Load<SoundEffect>("Soundtrack/TaurusStartingCharge"));
            Sounds.Add("PatrolSeenYou", _contentManager.Load<SoundEffect>("Soundtrack/PatrolSeenYou"));
        }

        private void AddSpritesFromDictionary(
            IDictionary<string, SpriteDescription> textureDictionary,
            Texture2D spriteSheet)
        {
            foreach (var texture in textureDictionary)
            {
                Sprites.Add(texture.Key, new Sprite(
                    texture.Value,
                    spriteSheet));
            }
        }

        private void LoadAnimations()
        {
            void LoadCharacterAnimation(
                string animationName,
                string frameName,
                bool isAnimationLooped)
            {
                const int _maxFramesNumber = 30;

                var animation = new List<Sprite>();

                for (int f = 1; f <= _maxFramesNumber; ++f)
                {
                    string completeFrameName = $"{frameName}_{f}";
                    if (Sprites.ContainsKey(completeFrameName))
                        animation.Add(Sprites[completeFrameName]);
                }

                if (animation.Count > 0)
                {
                    _animations[animationName] = new SpriteAnimation(
                        animation,
                        TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames),
                        isAnimationLooped);
                }
            }

            void LoadCharacterAnimations(string characterName)
            {
                var folderName = char.ToUpper(characterName[0]) + characterName.Substring(1);
                LoadCharacterAnimation($"{characterName}WalkUp", $"{folderName}/{characterName}_back_walking", true);
                LoadCharacterAnimation($"{characterName}WalkDown", $"{folderName}/{characterName}_front_walking", true);
                LoadCharacterAnimation($"{characterName}WalkSide", $"{folderName}/{characterName}_side_walking", true);
                LoadCharacterAnimation($"{characterName}IdleUp", $"{folderName}/{characterName}_back_iddle", true);
                LoadCharacterAnimation($"{characterName}IdleDown", $"{folderName}/{characterName}_front_iddle", true);
                LoadCharacterAnimation($"{characterName}IdleSide", $"{folderName}/{characterName}_side_iddle", true);
                LoadCharacterAnimation($"{characterName}AttackUp", $"{folderName}/{characterName}_back_attack", true);
                LoadCharacterAnimation($"{characterName}AttackDown", $"{folderName}/{characterName}_front_attack", true);
                LoadCharacterAnimation($"{characterName}AttackLeft", $"{folderName}/{characterName}_side_attack", true);
                LoadCharacterAnimation($"{characterName}AttackRight", $"{folderName}/{characterName}_side_attack", true);

                if (characterName == "player")
                    LoadCharacterAnimation($"{characterName}Dying", $"{folderName}/Death/{characterName}_front_dying", false);
            }

            LoadCharacterAnimations(characterName: "player");
            LoadCharacterAnimations(characterName: "goblin");
            LoadCharacterAnimations(characterName: "skeleton");
            LoadCharacterAnimations(characterName: "zombie");
            LoadCharacterAnimations(characterName: "taurus");
            LoadCharacterAnimations(characterName: "ghost");
            LoadCharacterAnimations(characterName: "bubu");

            LoadCharacterAnimation("TaurusRunSide", "Taurus/taurus_side_run", true);
            LoadCharacterAnimation("TaurusRunFront", "Taurus/taurus_front_run", true);
            LoadCharacterAnimation("TaurusRunBack", "Taurus/taurus_back_run", true);
            LoadCharacterAnimation("TaurusChargeSide", "Taurus/taurus_side_charge", true);
            LoadCharacterAnimation("TaurusChargeFront", "Taurus/taurus_front_charge", true);
            LoadCharacterAnimation("TaurusChargeBack", "Taurus/taurus_back_charge", true);

            // TODO: tutte queste diventano LoadCharacterAnimation, basta passare poi anche i MS

            // Candle
            {
                var candleAnimation = new List<Sprite>();
                for (int i = 1; i <= 9; ++i)
                    candleAnimation.Add(Sprites[$"Objects/Candle/candle_{i}"]);
                _animations["Candle"] = new SpriteAnimation(candleAnimation, TimeSpan.FromMilliseconds(200));
            }

            // Instantly die trap
            {
                var istantlyDieTrapAnimation = new List<Sprite>();
                for (int i = 1; i <= 10; ++i)
                    istantlyDieTrapAnimation.Add(Sprites[$"TrapIstant/{i}"]);
                _animations["TrapIstant"] = new SpriteAnimation(istantlyDieTrapAnimation, TimeSpan.FromMilliseconds(100), false);
            }

            // Little Light
            {
                var littleLightAnimation = new List<Sprite>();
                for (int i = 1; i <= 60; ++i)
                {
                    if (Sprites.ContainsKey($"LightObject/lightobject_{i}"))
                        littleLightAnimation.Add(Sprites[$"LightObject/lightobject_{i}"]);
                    else break;
                }
                _animations["LittleLight"] = new SpriteAnimation(littleLightAnimation, TimeSpan.FromMilliseconds(100));
            }

            // Shadows
            {
                LoadCharacterAnimation($"TorchCharacterWhileWalkingUpShadow", $"Shadows/TorchCharacter/shadow_back_walking", true);
                LoadCharacterAnimation($"TorchCharacterWhileWalkingDownShadow", $"Shadows/TorchCharacter/shadow_front_walking", true);
                LoadCharacterAnimation($"TorchCharacterWhileWalkingSideShadow", $"Shadows/TorchCharacter/shadow_side_walking", true);
                LoadCharacterAnimation($"TorchCharacterWhileIdleUpShadow", $"Shadows/TorchCharacter/shadow_back_iddle", true);
                LoadCharacterAnimation($"TorchCharacterWhileIdleDownShadow", $"Shadows/TorchCharacter/shadow_front_iddle", true);
                LoadCharacterAnimation($"TorchCharacterWhileIdleSideShadow", $"Shadows/TorchCharacter/shadow_side_iddle", true);

                LoadCharacterAnimation($"GenericCharacterWhileWalkingUpShadow", $"Shadows/Generic/shadow", true);
                LoadCharacterAnimation($"GenericCharacterWhileWalkingDownShadow", $"Shadows/Generic/shadow", true);
                LoadCharacterAnimation($"GenericCharacterWhileWalkingSideShadow", $"Shadows/Generic/shadow", true);
                LoadCharacterAnimation($"GenericCharacterWhileIdleUpShadow", $"Shadows/Generic/shadow", true);
                LoadCharacterAnimation($"GenericCharacterWhileIdleDownShadow", $"Shadows/Generic/shadow", true);
                LoadCharacterAnimation($"GenericCharacterWhileIdleSideShadow", $"Shadows/Generic/shadow", true);
            }
        }

        private void LoadOtherResources()
        {
            Font = _contentManager.Load<SpriteFont>("font");
        }
    }
}