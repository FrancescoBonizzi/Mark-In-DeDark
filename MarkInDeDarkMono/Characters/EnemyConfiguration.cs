using FbonizziMonoGame.Sprites;
using MarkInDeDarkMono.AI;


namespace MarkInDeDarkMono.Characters
{
    public class EnemyConfiguration
    {
        public EnemyConfiguration(
            AssetsLoader assetsLoader, 
            string enemyName,
            int attackSoundIndex,
            int screamSoundIndex,
            int? nCellsFindingRange,
            IEnemyAI startingAI)
        {
            AttackDownAnimation = assetsLoader.GetAnimation($"{enemyName}AttackDown");
            AttackUpAnimation = assetsLoader.GetAnimation($"{enemyName}AttackUp");
            AttackLeftAnimation = assetsLoader.GetAnimation($"{enemyName}AttackLeft");
            AttackRightAnimation = assetsLoader.GetAnimation($"{enemyName}AttackRight");

            AttackSoundIndex = attackSoundIndex;
            ScreamSoundIndex = screamSoundIndex;
            NCellsFindingRange = nCellsFindingRange;
            StartingAI = startingAI;
        }

        public SpriteAnimation AttackUpAnimation { get; }
        public SpriteAnimation AttackDownAnimation { get; }
        public SpriteAnimation AttackLeftAnimation { get; }
        public SpriteAnimation AttackRightAnimation { get; }

        public int AttackSoundIndex { get; }
        public int ScreamSoundIndex { get; }
        public int? NCellsFindingRange { get; }

        public IEnemyAI StartingAI { get; }
    }
}
