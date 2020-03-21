using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.GameGrid
{
    public class GameGridDeserializerManager
    {
        private readonly List<string> _lines;
        private readonly IGameGridSerializer _thisFileSerializer;

        public GameGridDeserializerManager(string levelContentRaw)
        {
            if (string.IsNullOrEmpty(levelContentRaw))
                throw new InvalidFileException("File vuoto");

            _lines = levelContentRaw
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var serializerV2 = new GameGridSerializerV2();
            var serializerV3 = new GameGridSerializerV3();
            if (serializerV2.WasTheFileSerializedWithThisClass(_lines[0]))
                _thisFileSerializer = serializerV2;
            else if (serializerV3.WasTheFileSerializedWithThisClass(_lines[0]))
                _thisFileSerializer = serializerV3;
            else
                _thisFileSerializer = new GameGridSerializerVExcel();
        }

        public GameGridBundle DeserializeLevel()
            => _thisFileSerializer.DeserializeLevel(_lines);
    }
}
