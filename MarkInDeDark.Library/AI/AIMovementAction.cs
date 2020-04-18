using MarkInDeDark.Library.Characters;
using MarkInDeDark.Library.Levels;

namespace MarkInDeDark.Library.AI
{
    public class AIMovementAction
    {
        public CharacterActions Action { get; set; }
        public GameCell DestinationCell { get; set; }

        public AIMovementAction(CharacterActions action, GameCell destinationCell)
        {
            Action = action;
            DestinationCell = destinationCell;
        }
    }
}
