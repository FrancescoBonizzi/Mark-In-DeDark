using MarkInDeDark.LevelEditor.Domain;
using MarkInDeDark.LevelEditor.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.LevelEditor.Editor
{
    public class GameGridLoader
    {
        private readonly IEnumerable<string> _lines;
        private readonly GameGrid _grid;

        public GameGridLoader(
            string levelContentRaw,
            GameGrid gameGrid)
        {
            if (string.IsNullOrEmpty(levelContentRaw))
                throw new InvalidFileException("File vuoto");

            _lines = levelContentRaw
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(s => !string.IsNullOrWhiteSpace(s));

            if (_lines.Count() != gameGrid.Rows * gameGrid.Columns)
                throw new InvalidFileException("Dimensione della mappa non compatibile");

            _grid = gameGrid;
        }

        public void LoadGrid()
        {
            int i = 0;
            int j = 0;

            foreach (var riga in _lines)
            {
                var rigaParts = riga.Split(':');

                _grid.SetGroundType(j, i, GroundTypes.Terra);
                if (rigaParts[0] == "NULL")
                {
                    _grid.SetGroundType(j, i, GroundTypes.Buio);
                }
                else if (rigaParts[1] == "MACERIE")
                {
                    _grid.SetGroundType(j, i, GroundTypes.Macerie);
                }
                // Considerando che faccio il Contains, vanno SEMPRE prima i caratteri multipli
                else if (rigaParts[1].Contains("GT"))
                {
                    _grid.SetGroundType(j, i, GroundTypes.TriggerGoblin);
                }
                else if (rigaParts[1].Contains("DT"))
                {
                    _grid.SetGroundType(j, i, GroundTypes.TrappolaMorteIstantanea);
                }
                else if (rigaParts[1].Contains("BT"))
                {
                    _grid.SetGroundType(j, i, GroundTypes.TrappolaTemporizzata);
                }
                else if (rigaParts[1].Contains("P"))
                {
                    _grid.SetObjectType(j, i, ObjectTypes.Giocatore);
                }
                else if (rigaParts[1].Contains("E"))
                {
                    _grid.SetObjectType(j, i, ObjectTypes.Uscita);
                }
                else if (rigaParts[1].Contains("W"))
                {
                    _grid.SetObjectType(j, i, ObjectTypes.Werewolf);
                }
                else if (rigaParts[1].Contains("Z"))
                {
                    _grid.SetObjectType(j, i, ObjectTypes.Zombie, rigaParts[1]);
                }
                else if (rigaParts[1].Contains("S"))
                {
                    _grid.SetObjectType(j, i, ObjectTypes.Scheletro, rigaParts[1]);
                }
                else if (rigaParts[1].Contains("T"))
                {
                    _grid.SetObjectType(j, i, ObjectTypes.Taurus, rigaParts[1]);
                }
                else if (rigaParts[1].Contains("G"))
                {
                    _grid.SetObjectType(j, i, ObjectTypes.Goblin);
                }
                else if (rigaParts[1].Contains("C"))
                {
                    var numberOfCandles = rigaParts[1].Split('-')[1];
                    _grid.SetObjectType(j, i, ObjectTypes.Candele, numberOfCandles);
                }
                else if (rigaParts[1].Contains("K"))
                {
                    _grid.SetObjectType(j, i, ObjectTypes.Chiave);
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
                                        _grid.SetWallType(j, i, WallTypes.Fragile, WallPositions.Top);
                                    else
                                        _grid.SetWallType(j, i, WallTypes.Occludente, WallPositions.Top);
                                    break;

                                case 1:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        _grid.SetWallType(j, i, WallTypes.Fragile, WallPositions.Right);
                                    else
                                        _grid.SetWallType(j, i, WallTypes.Occludente, WallPositions.Right);
                                    break;

                                case 2:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        _grid.SetWallType(j, i, WallTypes.Fragile, WallPositions.Bottom);
                                    else
                                        _grid.SetWallType(j, i, WallTypes.Occludente, WallPositions.Bottom);
                                    break;

                                case 3:
                                    if (rigaParts[0][c] == 'D') // Destroyable
                                        _grid.SetWallType(j, i, WallTypes.Fragile, WallPositions.Left);
                                    else
                                        _grid.SetWallType(j, i, WallTypes.Occludente, WallPositions.Left);
                                    break;
                            }
                        }
                    }
                }

                // Gioco con gli indici della griglia per far scorrere verticalmente
                // la griglia al momento dell'inserimento
                // (Perchè ho fatto esportare da Excel (vecchio editor) in questo ordine)
                if (j == _grid.Rows - 1)
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
    }
}
