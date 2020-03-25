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

namespace MarkInDeDarkMono.Assets
{
    public class AssetsLoader
    {
        private IDictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();
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

            for(int i= 0; i < 2; ++i)
            {
                var gameSpriteSheet = _contentManager.Load<Texture2D>($"{gameSpriteSheetName}-{i}");
                var gameSpritesDescriptions = _textureImporter.Import($"Content/{gameSpriteSheetName}-{i}.txt");
                AddSpritesFromDictionary(gameSpritesDescriptions, gameSpriteSheet);
            }

            var menuTexture = _contentManager.Load<Texture2D>("Backgrounds/menu");
            var menuSprite = new Sprite(
                new SpriteDescription()
                {
                    X = 0, Y = 0,
                    Width = menuTexture.Width, Height = menuTexture.Height,
                    Name = menuTexture.Name
                },
                menuTexture);
            Sprites.Add("menu", menuSprite);

            var levelsTexture = _contentManager.Load<Texture2D>("Backgrounds/levels");
            var levelsBackgroundSprite = new Sprite(
                new SpriteDescription()
                {
                    X = 0, Y = 0,
                    Width = menuTexture.Width, Height = menuTexture.Height,
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
            void LoadCharacterAnimations(
                string characterName, 
                bool hasAttackAnimations)
            {
                var characterWalkUp = new List<Sprite>() { Sprites[$"{characterName}_back_walking_1"], Sprites[$"{characterName}_back_walking_2"] };
                var characterWalkDown = new List<Sprite>() { Sprites[$"{characterName}_front_walking_1"], Sprites[$"{characterName}_front_walking_2"] };
                var characterWalkSide = new List<Sprite>() { Sprites[$"{characterName}_side_walking_1"], Sprites[$"{characterName}_side_walking_2"]};
                
                var characterIdleUp = new List<Sprite>() { Sprites[$"{characterName}_back_iddle"] };
                var characterIdleDown = new List<Sprite>() { Sprites[$"{characterName}_front_iddle"] };
                var characterIdleSide = new List<Sprite>() { Sprites[$"{characterName}_side_iddle"] };

                // Per gli Idle dei mostri vedremo
                if (characterName == "player")
                {
                    characterIdleUp.Add(Sprites[$"{characterName}_back_iddle_2"]);
                    characterIdleUp.Add(Sprites[$"{characterName}_back_iddle_3"]);

                    characterIdleDown.Add(Sprites[$"{characterName}_front_iddle_2"]);
                    characterIdleDown.Add(Sprites[$"{characterName}_front_iddle_3"]);

                    characterIdleSide.Add(Sprites[$"{characterName}_side_iddle_2"]);
                    characterIdleSide.Add(Sprites[$"{characterName}_side_iddle_3"]);

                    characterWalkSide.Add(Sprites[$"{characterName}_side_walking_3"]);
                    characterWalkSide.Add(Sprites[$"{characterName}_side_walking_4"]);
                }

                var characterDying = new List<Sprite>() { Sprites[$"{characterName}_front_dying_1"], Sprites[$"{characterName}_front_dying_2"], Sprites[$"{characterName}_front_dying_3"] };
                
                _animations[$"{characterName}WalkUp"] = new SpriteAnimation(characterWalkUp, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                _animations[$"{characterName}WalkDown"] = new SpriteAnimation(characterWalkDown, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                _animations[$"{characterName}WalkSide"] = new SpriteAnimation(characterWalkSide, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                _animations[$"{characterName}IdleUp"] = new SpriteAnimation(characterIdleUp, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                _animations[$"{characterName}IdleDown"] = new SpriteAnimation(characterIdleDown, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                _animations[$"{characterName}IdleSide"] = new SpriteAnimation(characterIdleSide, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                _animations[$"{characterName}Dying"] = new SpriteAnimation(characterDying, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames), false);

                if (hasAttackAnimations)
                {
                    var characterAttackUp = new List<Sprite>() { Sprites[$"{characterName}_back_attack_1"], Sprites[$"{characterName}_back_attack_2"] };
                    var characterAttackDown = new List<Sprite>() { Sprites[$"{characterName}_front_attack_1"], Sprites[$"{characterName}_front_attack_2"] };
                    var characterAttackLeft = new List<Sprite>() { Sprites[$"{characterName}_side_attack_1"], Sprites[$"{characterName}_side_attack_2"] };
                    var characterAttackRight = new List<Sprite>() { Sprites[$"{characterName}_side_attack_1"], Sprites[$"{characterName}_side_attack_2"] };

                    _animations[$"{characterName}AttackUp"] = new SpriteAnimation(characterAttackUp, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                    _animations[$"{characterName}AttackDown"] = new SpriteAnimation(characterAttackDown, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                    _animations[$"{characterName}AttackLeft"] = new SpriteAnimation(characterAttackLeft, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                    _animations[$"{characterName}AttackRight"] = new SpriteAnimation(characterAttackRight, TimeSpan.FromMilliseconds(_gameConfig.CharactersAnimationsMillisecondsBetweenFrames));
                }
            }

            LoadCharacterAnimations("player", false);
            LoadCharacterAnimations("goblin", true);
            LoadCharacterAnimations("werewolf", true);
            LoadCharacterAnimations("skeleton", true);
            LoadCharacterAnimations("zombie", true);
            LoadCharacterAnimations("taurus", true);

            var candleAnimation = new List<Sprite>();
            for (int i = 1; i <= 6; ++i)
                candleAnimation.Add(Sprites[$"candle{i}"]);
            _animations["Candle"] = new SpriteAnimation(candleAnimation, TimeSpan.FromMilliseconds(200));
        }

        private void LoadOtherResources()
        {
            Font = _contentManager.Load<SpriteFont>("font");
        }
    }
}