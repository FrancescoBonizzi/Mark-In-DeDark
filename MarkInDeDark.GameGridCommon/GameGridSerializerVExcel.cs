using System;
using System.Collections.Generic;

namespace MarkInDeDark.GameGridCommon
{
    public class GameGridSerializerVExcel : IGameGridSerializer
    {
        private const int _gridRows = 25;
        private const int _gridCols = 45;

        public GameGridBundle DeserializeLevel(List<string> levelContentRawLine)
        {
            int i = 0;
            int j = 0;

            var gameGrid = new GameGridBundle(_gridRows, _gridCols);

            foreach (var riga in levelContentRawLine)
            {
                var rigaParts = riga.Split(':');

                gameGrid.SetGroundType(j, i, GroundTypes.Terra);
                if (rigaParts[0] == "NULL")
                {
                    gameGrid.SetGroundType(j, i, GroundTypes.NonCella);
                }
                else if (rigaParts[1] == "MACERIE")
                {
                    gameGrid.SetGroundType(j, i, GroundTypes.Terra2);
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

            return gameGrid;
        }

        public string SerializeLevel(GameGridBundle gameGrid)
        {
            throw new NotImplementedException();
        }

        public bool WasTheFileSerializedWithThisClass(string levelContentRawFirstRow)
        {
            throw new NotImplementedException();
        }
    }
}
