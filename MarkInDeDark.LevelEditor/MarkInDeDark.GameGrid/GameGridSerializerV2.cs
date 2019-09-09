using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkInDeDark.GameGrid
{
    public class GameGridSerializerV2 : IGameGridSerializer
    {
        private const string _firstRowVersionPlaceholder = "GameGridSerializerV2";
        private const char _separator = '|';

        public GameGridBundle DeserializeLevel(List<string> levelContentRawLines)
        {
            if (levelContentRawLines[0] != _firstRowVersionPlaceholder)
                throw new InvalidFileException($"First row is {levelContentRawLines[0]} which is not compatible with this serializer");

            var firstLineSplitted = levelContentRawLines[1].Split(_separator);
            var rows = Convert.ToInt32(firstLineSplitted[0]);
            var columns = Convert.ToInt32(firstLineSplitted[1]);
            var gameGrid = new GameGridBundle(rows, columns);

            foreach (var line in levelContentRawLines.Skip(2))
            {
                var splitted = line.Split(_separator);
                int i = Convert.ToInt32(splitted[0]);
                int j = Convert.ToInt32(splitted[1]);
                GroundTypes groundType = (GroundTypes)Convert.ToInt32(splitted[2]);
                ObjectTypes? objectType = string.IsNullOrEmpty(splitted[3]) ? (ObjectTypes?)null : (ObjectTypes)Convert.ToInt32(splitted[3]);
                string objectTypeAdditionalData = splitted[4];
                WallTypes top = (WallTypes)Convert.ToInt32(splitted[5]);
                WallTypes right = (WallTypes)Convert.ToInt32(splitted[6]);
                WallTypes bottom = (WallTypes)Convert.ToInt32(splitted[7]);
                WallTypes left = (WallTypes)Convert.ToInt32(splitted[8]);

                gameGrid.SetGroundType(i, j, groundType);
                if (objectType != null)
                    gameGrid.SetObjectType(i, j, objectType.Value, objectTypeAdditionalData);
                gameGrid.SetWallType(i, j, top, WallPositions.Top);
                gameGrid.SetWallType(i, j, right, WallPositions.Right);
                gameGrid.SetWallType(i, j, bottom, WallPositions.Bottom);
                gameGrid.SetWallType(i, j, left, WallPositions.Left);
            }

            return gameGrid;
        }

        public string SerializeLevel(GameGridBundle gameGrid)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(_firstRowVersionPlaceholder).AppendLine();
            stringBuilder.Append(gameGrid.Rows).Append(_separator).Append(gameGrid.Columns).AppendLine();

            // Scrivo solo le celle che hanno un valore, così risparmio spazio
            for (int i = 0; i < gameGrid.Rows; ++i)
            {
                for (int j = 0; j < gameGrid.Columns; ++j)
                {
                    var cell = gameGrid.Grid[j, i];

                    if (cell.GroundType == GroundTypes.Buio)
                        continue;

                    stringBuilder
                        .Append(i) // 0
                        .Append(_separator)
                        .Append(j) // 1
                        .Append(_separator)
                        .Append((int)cell.GroundType) // 2
                        .Append(_separator)
                        .Append(cell.ObjectType == null ? string.Empty : ((int)cell.ObjectType).ToString()) // 3
                        .Append(_separator)
                        .Append(cell.ObjectTypeAdditionalData ?? string.Empty) // 4
                        .Append(_separator)
                        .Append((int)cell.Top) // 5
                        .Append(_separator)
                        .Append((int)cell.Right) // 6
                        .Append(_separator)
                        .Append((int)cell.Bottom) // 7
                        .Append(_separator)
                        .Append((int)cell.Left) // 8
                        .AppendLine();
                }
            }

            return stringBuilder.ToString();
        }

        public bool WasTheFileSerializedWithThisClass(string levelContentRawFirstRow)
            => levelContentRawFirstRow == _firstRowVersionPlaceholder;
    }
}
