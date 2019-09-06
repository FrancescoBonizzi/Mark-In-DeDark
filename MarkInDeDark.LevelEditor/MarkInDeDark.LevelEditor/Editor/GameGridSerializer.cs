using MarkInDeDark.LevelEditor.Domain;
using MarkInDeDark.LevelEditor.Exceptions;
using System;
using System.Linq;
using System.Text;

namespace MarkInDeDark.LevelEditor.Editor
{
    public class GameGridSerializer
    {
        private const char _separator = '|';

        public void DeserializeLevel(
            string levelContentRaw,
            GameGrid gameGrid)
        {
            if (string.IsNullOrEmpty(levelContentRaw))
                throw new InvalidFileException("File vuoto");

            var lines = levelContentRaw
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(s => !string.IsNullOrWhiteSpace(s));

            if (lines.Count() != gameGrid.Rows * gameGrid.Columns)
                throw new InvalidFileException("Dimensione della mappa non compatibile");

            int i = 0;
            int j = 0;

            foreach (var riga in lines)
            {
                var rigaParts = riga.Split(':');

                gameGrid.SetGroundType(j, i, GroundTypes.Terra);
                if (rigaParts[0] == "NULL")
                {
                    gameGrid.SetGroundType(j, i, GroundTypes.Buio);
                }
                else if (rigaParts[1] == "MACERIE")
                {
                    gameGrid.SetGroundType(j, i, GroundTypes.Macerie);
                }
                // Considerando che faccio il Contains, vanno SEMPRE prima i caratteri multipli
                else if (rigaParts[1].Contains("GT"))
                {
                    gameGrid.SetGroundType(j, i, GroundTypes.TriggerGoblin);
                }
                else if (rigaParts[1].Contains("DT"))
                {
                    gameGrid.SetGroundType(j, i, GroundTypes.TrappolaMorteIstantanea);
                }
                else if (rigaParts[1].Contains("BT"))
                {
                    gameGrid.SetGroundType(j, i, GroundTypes.TrappolaTemporizzata);
                }
                else if (rigaParts[1].Contains("P"))
                {
                    gameGrid.SetObjectType(j, i, ObjectTypes.Giocatore);
                }
                else if (rigaParts[1].Contains("E"))
                {
                    gameGrid.SetObjectType(j, i, ObjectTypes.Uscita);
                }
                else if (rigaParts[1].Contains("W"))
                {
                    gameGrid.SetObjectType(j, i, ObjectTypes.Werewolf);
                }
                else if (rigaParts[1].Contains("Z"))
                {
                    gameGrid.SetObjectType(j, i, ObjectTypes.Zombie, rigaParts[1]);
                }
                else if (rigaParts[1].Contains("S"))
                {
                    gameGrid.SetObjectType(j, i, ObjectTypes.Scheletro, rigaParts[1]);
                }
                else if (rigaParts[1].Contains("T"))
                {
                    gameGrid.SetObjectType(j, i, ObjectTypes.Taurus, rigaParts[1]);
                }
                else if (rigaParts[1].Contains("G"))
                {
                    gameGrid.SetObjectType(j, i, ObjectTypes.Goblin);
                }
                else if (rigaParts[1].Contains("C"))
                {
                    var numberOfCandles = rigaParts[1].Split('-')[1];
                    gameGrid.SetObjectType(j, i, ObjectTypes.Candele, numberOfCandles);
                }
                else if (rigaParts[1].Contains("K"))
                {
                    gameGrid.SetObjectType(j, i, ObjectTypes.Chiave);
                }

                if (rigaParts[0] != "NULL")
                {
                    for (int c = 0; c < 4; c++)
                    {
                        if (rigaParts[0][c] != '0')
                        {
                            switch (c)
                            {
                                case 0:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        gameGrid.SetWallType(j, i, WallTypes.Fragile, WallPositions.Top);
                                    else
                                        gameGrid.SetWallType(j, i, WallTypes.Occludente, WallPositions.Top);
                                    break;

                                case 1:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        gameGrid.SetWallType(j, i, WallTypes.Fragile, WallPositions.Right);
                                    else
                                        gameGrid.SetWallType(j, i, WallTypes.Occludente, WallPositions.Right);
                                    break;

                                case 2:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        gameGrid.SetWallType(j, i, WallTypes.Fragile, WallPositions.Bottom);
                                    else
                                        gameGrid.SetWallType(j, i, WallTypes.Occludente, WallPositions.Bottom);
                                    break;

                                case 3:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        gameGrid.SetWallType(j, i, WallTypes.Fragile, WallPositions.Left);
                                    else
                                        gameGrid.SetWallType(j, i, WallTypes.Occludente, WallPositions.Left);
                                    break;
                            }
                        }
                    }
                }

                // Gioco con gli indici della griglia per far scorrere verticalmente
                // la griglia al momento dell'inserimento
                // (Perchè ho fatto esportare da Excel (vecchio editor) in questo ordine)
                if (j == gameGrid.Rows - 1)
                {
                    j = 0;
                    ++i;
                }
                else
                {
                    ++j;
                }
            }

        }

        public string SerializeLevel(GameGrid gameGrid)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(gameGrid.Rows).Append(_separator).Append(gameGrid.Columns).AppendLine();

            // Scrivo solo le celle che hanno un valore, così risparmio spazio
            for (int i = 0; i < gameGrid.Rows; ++i)
            {
                for(int j = 0; j < gameGrid.Columns; ++j)
                {
                    var cell = gameGrid.Grid[j, i];

                    if (cell.GroundType == GroundTypes.Buio)
                        continue;

                    stringBuilder
                        .Append(i)
                        .Append(_separator)
                        .Append(j)
                        .Append(_separator)
                        .Append((int)cell.GroundType)
                        .Append(_separator)
                        .Append(cell.ObjectType == null ? string.Empty : ((int)cell.ObjectType).ToString())
                        .Append(_separator)
                        .Append(cell.ObjectTypeQuantityInfo ?? string.Empty)
                        .Append(_separator)
                        .Append((int)cell.Top)
                        .Append(_separator)
                        .Append((int)cell.Right)
                        .Append(_separator)
                        .Append((int)cell.Bottom)
                        .Append(_separator)
                        .Append((int)cell.Left)
                        .AppendLine();
                }
            }

            return stringBuilder.ToString();
        }
    }
}
