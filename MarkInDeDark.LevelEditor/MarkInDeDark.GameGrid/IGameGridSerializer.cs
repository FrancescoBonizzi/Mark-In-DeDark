using System.Collections.Generic;

namespace MarkInDeDark.GameGrid
{
    public interface IGameGridSerializer
    {
        GameGridBundle DeserializeLevel(List<string> levelContentRawLine);

        string SerializeLevel(GameGridBundle gameGrid);

        bool WasTheFileSerializedWithThisClass(string levelContentRawFirstRow);
    }
}
