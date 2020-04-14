using FbonizziMonoGame.Assets;
using FbonizziMonoGame.PlatformAbstractions;
using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.Common;
using MarkInDeDarkMono.Levels;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkInDeDarkMono
{
    public class AssetsLoader
    {
        private readonly IDictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();
        public IDictionary<string, Sprite> Sprites { get; } = new Dictionary<string, Sprite>();
        public IDictionary<string, SoundEffect> Sounds { get; } = new Dictionary<string, SoundEffect>();
        public SpriteFont Font { get; private set; }
        public List<Level> Levels { get; private set; } = new List<Level>();

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

            for (int i = 0; i < 2; ++i)
            {
                var gameSpriteSheet = _contentManager.Load<Texture2D>($"{gameSpriteSheetName}-{i}");
                var gameSpritesDescriptions = _textureImporter.Import($"Content/{gameSpriteSheetName}-{i}.txt");
                AddSpritesFromDictionary(gameSpritesDescriptions, gameSpriteSheet);
            }

            var menuTexture = _contentManager.Load<Texture2D>("Backgrounds/menu");
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

            var levelsTexture = _contentManager.Load<Texture2D>("Backgrounds/levels");
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
            Sounds.Add("attack1", _contentManager.Load<SoundEffect>("Soundtrack/attack1"));
            Sounds.Add("attack2", _contentManager.Load<SoundEffect>("Soundtrack/attack2"));
            Sounds.Add("attack3", _contentManager.Load<SoundEffect>("Soundtrack/attack3"));
            Sounds.Add("attack4", _contentManager.Load<SoundEffect>("Soundtrack/attack4"));
            Sounds.Add("accendi_fiamma", _contentManager.Load<SoundEffect>("Soundtrack/accendi_fiamma"));
            Sounds.Add("fuoco", _contentManager.Load<SoundEffect>("Soundtrack/fuoco"));
            Sounds.Add("player_walk", _contentManager.Load<SoundEffect>("Soundtrack/player_walk"));
            Sounds.Add("key", _contentManager.Load<SoundEffect>("Soundtrack/key"));
            Sounds.Add("blood", _contentManager.Load<SoundEffect>("Soundtrack/blood"));
            Sounds.Add("fall", _contentManager.Load<SoundEffect>("Soundtrack/fall"));
            Sounds.Add("ladder", _contentManager.Load<SoundEffect>("Soundtrack/ladder"));
            Sounds.Add("candela", _contentManager.Load<SoundEffect>("Soundtrack/candela"));
            Sounds.Add("zombie_voice", _contentManager.Load<SoundEffect>("Soundtrack/zombie_voice"));
            Sounds.Add("goblin_voice", _contentManager.Load<SoundEffect>("Soundtrack/goblin_voice"));
            Sounds.Add("skeletron_voice", _contentManager.Load<SoundEffect>("Soundtrack/skeletron_voice"));
            Sounds.Add("wolf_voice", _contentManager.Load<SoundEffect>("Soundtrack/wolf_voice"));
            Sounds.Add("taurus_starting_charge", _contentManager.Load<SoundEffect>("Soundtrack/taurus_starting_charge"));
            Sounds.Add("break_wall", _contentManager.Load<SoundEffect>("Soundtrack/break_wall"));
            Sounds.Add("patrol_seen_you", _contentManager.Load<SoundEffect>("Soundtrack/patrol_seen_you"));
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
            void LoadCharacterAnimations(string characterName)
            {
                const int _maxFramesNumber = 10;

                void LoadCharacterAnimation(
                    string animationName,
                    string frameName,
                    bool isAnimationLooped)
                {
                    var animation = new List<Sprite>();

                    for (int f = 1; f <= _maxFramesNumber; ++f)
                    {
                        string completeFrameName = $"{frameName}_{f}";
                        if (Sprites.ContainsKey(completeFrameName))
                            animation.Add(Sprites[completeFrameName]);
                        else break;
                    }

                    if (animation.Count > 0)
                    {
                        _animations[animationName] = new SpriteAnimation(
                            animation,
                            TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames),
                            isAnimationLooped);
                    }
                }

                var folderName = char.ToUpper(characterName[0]) + characterName.Substring(1);
                LoadCharacterAnimation($"{characterName}WalkUp", $"{folderName}/{characterName}_back_walking", true);
                LoadCharacterAnimation($"{characterName}WalkDown", $"{folderName}/{characterName}_front_walking", true);
                LoadCharacterAnimation($"{characterName}WalkSide", $"{folderName}/{characterName}_side_walking", true);
                LoadCharacterAnimation($"{characterName}IdleUp", $"{folderName}/{characterName}_back_iddle", true);
                LoadCharacterAnimation($"{characterName}IdleDown", $"{folderName}/{characterName}_front_iddle", true);
                LoadCharacterAnimation($"{characterName}IdleSide", $"{folderName}/{characterName}_side_iddle", true);
                LoadCharacterAnimation($"{characterName}Dying", $"{folderName}/{characterName}_front_dying", false);
                LoadCharacterAnimation($"{characterName}AttackUp", $"{folderName}/{characterName}_back_attack", true);
                LoadCharacterAnimation($"{characterName}AttackDown", $"{folderName}/{characterName}_front_attack", true);
                LoadCharacterAnimation($"{characterName}AttackLeft", $"{folderName}/{characterName}_side_attack", true);
                LoadCharacterAnimation($"{characterName}AttackRight", $"{folderName}/{characterName}_side_attack", true);

                LoadCharacterAnimation($"{characterName}WhileWalkingUpShadow", $"{folderName}/{characterName}_back_walking_shadow", true);
                LoadCharacterAnimation($"{characterName}WhileWalkingDownShadow", $"{folderName}/{characterName}_front_walking_shadow", true);
                LoadCharacterAnimation($"{characterName}WhileWalkingSideShadow", $"{folderName}/{characterName}_side_walking_shadow", true);
                LoadCharacterAnimation($"{characterName}WhileIdleUpShadow", $"{folderName}/{characterName}_back_iddle_shadow", true);
                LoadCharacterAnimation($"{characterName}WhileIdleDownShadow", $"{folderName}/{characterName}_front_iddle_shadow", true);
                LoadCharacterAnimation($"{characterName}WhileIdleSideShadow", $"{folderName}/{characterName}_side_iddle_shadow", true);
            }

            LoadCharacterAnimations(characterName: "player");
            LoadCharacterAnimations(characterName: "goblin");
            LoadCharacterAnimations(characterName: "werewolf");
            LoadCharacterAnimations(characterName: "skeleton");
            LoadCharacterAnimations(characterName: "zombie");
            LoadCharacterAnimations(characterName: "taurus");
            LoadCharacterAnimations(characterName: "ghost");
            LoadCharacterAnimations(characterName: "bubu");

            var candleAnimation = new List<Sprite>();
            for (int i = 1; i <= 9; ++i)
                candleAnimation.Add(Sprites[$"candle_{i}"]);
            _animations["Candle"] = new SpriteAnimation(candleAnimation, TimeSpan.FromMilliseconds(200));

            var littleLightAnimation = new List<Sprite>();
            for (int i = 1; i <= 60; ++i)
            {
                if (Sprites.ContainsKey($"LightObject/lightobject_{i}"))
                    littleLightAnimation.Add(Sprites[$"LightObject/lightobject_{i}"]);
                else break;
            }
            _animations["LittleLight"] = new SpriteAnimation(littleLightAnimation, TimeSpan.FromMilliseconds(100));

            var doorOpeningAnimation = new List<Sprite>();
            for (int i = 0; i <= 8; ++i)
                doorOpeningAnimation.Add(Sprites[$"Door/door_{i}"]);
            _animations["DoorOpen"] = new SpriteAnimation(doorOpeningAnimation, TimeSpan.FromMilliseconds(20), false);

            var doorClosingAnimation = new List<Sprite>(doorOpeningAnimation);
            doorClosingAnimation.Reverse();
            _animations["DoorClose"] = new SpriteAnimation(doorClosingAnimation, TimeSpan.FromMilliseconds(20), false);
        }

        private void LoadOtherResources()
        {
            Font = _contentManager.Load<SpriteFont>("font");
        }
    }
}