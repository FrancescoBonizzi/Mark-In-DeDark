using System.Collections.Generic;

namespace MarkInDeDark.GameGridCommon
{
    public interface IGameGridSerializer
    {
        GameGridBundle DeserializeLevel(List<string> levelContentRawLine);

        string SerializeLevel(GameGridBundle gameGrid);

        bool WasTheFileSerializedWithThisClass(string levelContentRawFirstRow);
    }
}
