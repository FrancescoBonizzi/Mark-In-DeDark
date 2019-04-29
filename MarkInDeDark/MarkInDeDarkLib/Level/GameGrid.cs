#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Diagnostics;

#endregion

namespace Mark.Engine
{
    public class GameGrid
    {
        #region Dichiarazioni

        private Vector2 position_ = Vector2.Zero;
        private const int n_horizontal_cells_ = 45;
        private const int n_vertical_cells_ = 25;

        private int total_width_;
        private int total_height_;

        private int cell_width_;
        private int cell_heigth_;
        private Cell[,] cells_;

        private Vector2 player_starting_position_ = Vector2.Zero;
        private Vector2 win_position_ = Vector2.Zero;
        private Vector2 key_position_ = Vector2.Zero;
        private Vector2 goblin_starting_position_ = Vector2.Zero;
        private List<List<Cell>> skeletron_path_ = new List<List<Cell>>();
        private List<List<Cell>> zombie_path_ = new List<List<Cell>>();
        private Vector2 werewolf_starting_position_ = Vector2.Zero;
        private Vector2 secret_object_position_ = Vector2.Zero;
        private Vector2 armor_position_ = Vector2.Zero;
        private List<Vector2> speedPotions_position_ = new List<Vector2>();
        private List<Vector2> nightvisionPotions_position_ = new List<Vector2>();

        #endregion

        #region Costruttore

        public GameGrid(Level LevelToLoad)
        {
            Rectangle cell_texture = LevelToLoad.CellRectangle;
            Rectangle wall_texture = LevelToLoad.WallRectangle;
            Rectangle dirty_texture = LevelToLoad.DirtyRectangle;
            Rectangle angle = LevelToLoad.AngleRectangle;
            double dirty_probability = LevelToLoad.DirtyProbability;

            cell_width_ = cell_texture.Width;
            cell_heigth_ = cell_width_;
            cells_ = new Cell[n_horizontal_cells_, n_vertical_cells_];

            Vector2 next_avaible_pos = position_;

            for (int i = 0; i < n_horizontal_cells_; ++i)
            {
                for (int j = 0; j < n_vertical_cells_; ++j)
                {
                    cells_[i, j] = new Cell(
                        next_avaible_pos, cell_texture, wall_texture,
                        angle,
                        dirty_texture,
                        dirty_probability);

                    next_avaible_pos.Y += cell_heigth_;
                }
                next_avaible_pos.X += cell_width_;
                next_avaible_pos.Y = position_.Y;
            }

            LoadLabyrinth(LevelToLoad.Path(), LevelToLoad.MaceriaRectangle);
            total_width_ = cell_width_ * n_horizontal_cells_;
            total_height_ = cell_width_ * n_vertical_cells_;

            // Elimino le tutti i muri doppi gli angoli doppi
            // Per ogni angolo devo vedere se quell'angolo o quel muro esiste già uguale vicino
     
            foreach(Cell cell in cells_)
            {
                if (cell != null)
                {
                    foreach (Cell.Wall wall in cell.walls_)
                    {
                        foreach (Cell nearCell in cell.Links)
                        {
                            if (nearCell != null)
                            {
                                if (nearCell.walls_.Count != 0)
                                {
                                    for (int w = nearCell.walls_.Count - 1; w >= 0; --w)
                                    {
                                        if (wall.location == nearCell.walls_[w].location)
                                        {
                                            nearCell.walls_.RemoveAt(w);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (Vector2 cellAngle in cell.angle_positions_)
                        {
                            foreach (Cell nearCell in cell.Links)
                            {
                                if (nearCell != null)
                                {
                                    if (nearCell.angle_positions_.Count != 0)
                                    {
                                        for (int w = nearCell.angle_positions_.Count - 1; w >= 0; --w)
                                        {
                                            if (cellAngle == nearCell.angle_positions_[w])
                                            {
                                                nearCell.angle_positions_.RemoveAt(w);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }


        }

        #endregion

        #region Get/Set

        public int TotalWidth
        {
            get { return total_width_; }
        }

        public int TotalHeigth
        {
            get { return total_height_; }
        }

        public Vector2 PlayerStartingPosition
        {
            get { return player_starting_position_; }
        }

        public Vector2 WinPosition
        {
            get { return win_position_; }
        }

        public Vector2 SecretObjectPosition
        {
            get { return secret_object_position_; }
        }

        public Vector2 ArmorPosition
        {
            get { return armor_position_; }
        }

        public Vector2 GoblinPosition
        {
            get { return goblin_starting_position_; }
        }

        public Vector2 WerewolfPosition
        {
            get { return werewolf_starting_position_; }
        }

        public List<List<Cell>> ZombiePath
        {
            get { return zombie_path_; }
        }

        public List<List<Cell>> SkeletronPath
        {
            get { return skeletron_path_; }
        }

        public List<Vector2> SpeedPotionsPositions
        {
            get { return speedPotions_position_; }
        }

        public List<Vector2> NightvisionPotionsPositions
        {
            get { return nightvisionPotions_position_; }
        }

        public int GetNhorizontalCells
        {
            get { return n_horizontal_cells_; }
        }

        public int GetNvericalCells
        {
            get { return n_vertical_cells_; }
        }

        public Rectangle GetGridRectangle()
        {
            return new Rectangle(
                (int)position_.X,
                (int)position_.Y,
                cell_width_ * n_horizontal_cells_,
                cell_width_ * n_vertical_cells_);
        }

        private Vector2 GetPixelBy_ij(int i, int j)
        {
            return new Vector2(
                i * cell_width_ + position_.X,
                j * cell_width_ + position_.Y);
        }

        public int GetCellByPixelX(int X)
        {
            X -= (int)position_.X; // Se traslo la griglia
            return X / cell_width_;
        }

        public int GetCellByPixelY(int Y)
        {
            Y -= (int)position_.Y; // Se traslo la griglia
            return Y / cell_heigth_;
        }

        public Vector2 GetCellIndexByPixel(Vector2 pixelLocation)
        {
            return new Vector2(
                GetCellByPixelX((int)pixelLocation.X),
                GetCellByPixelY((int)pixelLocation.Y));
        }

        public Rectangle CellWorldRectangle(int cellX, int cellY)
        {
            return new Rectangle(
                cellX * cell_width_,
                cellY * cell_heigth_,
                cell_width_,
                cell_heigth_);
        }

        public Rectangle CellWorldRectangle(Vector2 cell)
        {
            return CellWorldRectangle(
                (int)cell.X,
                (int)cell.Y);
        }

        public Rectangle CellScreenRectangle(int cellX, int cellY)
        {
            return Camera.WorldToScreen(CellWorldRectangle(cellX, cellY));
        }

        public Rectangle CellSreenRectangle(Vector2 cell)
        {
            return CellScreenRectangle((int)cell.X, (int)cell.Y);
        }

        public Cell GetCellByIndex(int cellX, int cellY)
        {
            if ((cellX >= 0) && (cellX < n_horizontal_cells_) &&
                (cellY >= 0) && (cellY < n_vertical_cells_))
            {
                return cells_[cellX, cellY];
            }
            else
            {
                return null;
            }
        }

        public Cell GetCellAtPixel(int pixelX, int pixelY)
        {
            return GetCellByIndex(
                GetCellByPixelX(pixelX),
                GetCellByPixelY(pixelY));
        }

        public Cell GetCellAtPixel(Vector2 pixelLocation)
        {
            return GetCellAtPixel(
                (int)pixelLocation.X,
                (int)pixelLocation.Y);
        }

        public Vector2 KeyPosition
        {
            get { return key_position_; }
        }


        #endregion

        #region Metodi

        public void LoadLabyrinth(string Filename, Rectangle Maceria)
        {
            // La sintassi è: NESW (TOP-RIGHT-SUD-WEST) -> 1 se c'è, 0 se non c'è
            // Per gli altri comandi vedi il level editor in Excel

            string riga;

            // Gli indici della matrice delle celle
            int i = 0;
            int j = 0;

            int riga_count = 0;

            using (var stream = TitleContainer.OpenStream(Filename))
            {
                StreamReader file = new StreamReader(stream);

                while ((riga = file.ReadLine()) != null)
                {
                    ++riga_count;
                    if (riga != string.Empty)
                    {
                        // Normalizzo la stringa perchè da excel mi viene con gli apici doppi
                        riga = riga.Replace('"', ' ').Trim();

                        #region Oggetti

                        if (riga.Contains("NULL"))
                        {
                            cells_[j, i] = null;
                        }
                        else if (riga.Contains("MACERIE"))
                        {
                            cells_[j, i].SkipForGraph = true;
                            cells_[j, i].CellTexture = Maceria;
                        }
                        else if (riga.Contains("P"))
                        {
                            // Imposto la posizione iniziale del player
                            player_starting_position_ = GetPixelBy_ij(j, i);
                        }
                        else if (riga.Contains("U"))
                        {
                            // Imposto il punto di uscita
                            win_position_ = GetPixelBy_ij(j, i);
                        }
                        else if (riga.Contains("L"))
                        {
                            // Lupo
                            werewolf_starting_position_ = GetPixelBy_ij(j, i);
                        }
                        else if (riga.Contains("Z"))
                        {
                            // Zombie

                            // Vedo che numero di zombie è:
                            int monster_n = (int)Char.GetNumericValue(riga[riga.IndexOf("Z") + 1]);

                            if (monster_n <= zombie_path_.Count)
                            {
                                zombie_path_[monster_n - 1].Add(GetCellByIndex(j, i));
                            }
                            else
                            {
                                // Li aggiungo per dopo
                                int needed_slots_ = monster_n - zombie_path_.Count;
                                for (int m = 0; m < needed_slots_; ++m)
                                    zombie_path_.Add(new List<Cell>());

                                zombie_path_[monster_n - 1].Add(GetCellByIndex(j, i));
                            }
                        }
                        else if (riga.Contains("K"))
                        {
                            // Vedo che numero di skeletro è:
                            int monster_n = (int)Char.GetNumericValue(riga[riga.IndexOf("K") + 1]);

                            if (monster_n <= skeletron_path_.Count)
                            {
                                skeletron_path_[monster_n - 1].Add(GetCellByIndex(j, i));
                            }
                            else
                            {
                                // Li aggiungo per dopo
                                int needed_slots_ = monster_n - skeletron_path_.Count;
                                for (int m = 0; m < needed_slots_; ++m)
                                    skeletron_path_.Add(new List<Cell>());

                                skeletron_path_[monster_n - 1].Add(GetCellByIndex(j, i));
                            }
                        }
                        else if (riga.Contains("G"))
                        {
                            // Goblin
                            goblin_starting_position_ = GetPixelBy_ij(j, i);
                        }
                        else if (riga.Contains("S"))
                        {
                            // Oggetto segreto
                            secret_object_position_ = GetPixelBy_ij(j, i);
                        }
                        else if (riga.Contains("C"))
                        {
                            key_position_ = GetPixelBy_ij(j, i);
                        }

                        #endregion

                        // Ho messo direttamente 4 per escludere l'ultimo carattere se c'è,
                        // che ho già controllato sopra
                        if (cells_[j, i] != null)
                        {
                            for (int c = 0; c < 4; c++)
                            {
                                if (riga[c] == '1')
                                {
                                    switch (c)
                                    {
                                        case 0:
                                            cells_[j, i].IsNorthPassable = false;
                                            break;
                                        case 1:
                                            cells_[j, i].IsEastPassable = false;
                                            break;
                                        case 2:
                                            cells_[j, i].IsSouthPassable = false;
                                            break;
                                        case 3:
                                            cells_[j, i].IsWestPassable = false;
                                            break;
                                    }
                                }
                            }

                            cells_[j, i].GenerateWalls();
                            cells_[j, i].SetSimpleAngles();
                        }

                        // Gioco con gli indici della griglia per far scorrere verticalmente
                        // la griglia al momento dell'inserimento
                        // (Perchè ho fatto esportare da Excel in questo ordine)
                        if (i == n_vertical_cells_ - 1)
                        {
                            i = 0;
                            ++j;
                        }
                        else
                        {
                            ++i;
                        }
                    }
                }
            }

            GridAsGraph();
        }

        private void GridAsGraph()
        {
            for (int i = 0; i < n_horizontal_cells_; ++i)
            {
                for (int j = 0; j < n_vertical_cells_; ++j)
                {
                    Cell cell = cells_[i, j];

                    if (cell != null && !cell.SkipForGraph) //&& cell.WorldLocation != win_position_)
                    {
                        // Non può andare out of range l'indice perchè se è passable l'altra è nella griglia
                        if (cell.IsNorthPassable)
                        {
                            cell.AddLink(cells_[i, j - 1], 0);
                        }
                        else
                            cell.AddLink(null, 0);

                        if (cell.IsEastPassable)
                        {
                            cell.AddLink(cells_[i + 1, j], 1);
                        }
                        else
                            cell.AddLink(null, 1);

                        if (cell.IsSouthPassable)
                        {
                            cell.AddLink(cells_[i, j + 1], 2);
                        }
                        else
                            cell.AddLink(null, 2);

                        if (cell.IsWestPassable)
                        {
                            cell.AddLink(cells_[i - 1, j], 3);
                        }
                        else
                            cell.AddLink(null, 3);

                        // Aggiungo altri muretti. Posso farlo solo qua perchè prima non ho i link alle celle intorno!
                        // Ho dovuto aggiungere queste funzioni perchè avendo messo alcune celle a null, non riesco disegnerebbe
                        // alcuni angoli con il metodo semplice

                        // Diagonale alto a destra
                        if (!cell.IsEastPassable)
                            if (cell.Links[0] != null)
                                if (cell.Links[0].Links[1] != null)
                                    if (!cell.Links[0].Links[1].IsSouthPassable)
                                        cell.AddNEAngle();

                        // Diagonale alto a sinistra
                        if (!cell.IsWestPassable)
                            if (cell.Links[0] != null)
                                if (cell.Links[0].Links[3] != null)
                                    if (!cell.Links[0].Links[3].IsSouthPassable)
                                        cell.AddNWAngle();

                        // Sotto e destra
                        if (cell.Links[0] != null)
                            if (!cell.Links[0].IsEastPassable)
                                if (cell.Links[1] != null)
                                    if (!cell.Links[1].IsNorthPassable)
                                        cell.AddNEAngle();

                        // Sotto e a sinistra
                        if (cell.Links[0] != null)
                            if (!cell.Links[0].IsWestPassable)
                                if (cell.Links[3] != null)
                                    if (!cell.Links[3].IsNorthPassable)
                                        cell.AddNWAngle();

                        // Per disegnare i pilastri in cui i muri non sono da soli (solo delle righe senza angoli)
                        if (!cell.IsNorthPassable)
                            if (cell.Links[1] != null)
                                if (cell.Links[1].IsNorthPassable)
                                    cell.AddNEAngle();

                        if (!cell.IsNorthPassable)
                            if (cell.Links[3] != null)
                                if (cell.Links[3].IsNorthPassable)
                                    cell.AddNWAngle();

                        if (!cell.IsEastPassable)
                            if (cell.Links[2] != null)
                                if (cell.Links[2].IsEastPassable)
                                    cell.AddSEAngle();

                        if (!cell.IsEastPassable)
                            if (cell.Links[0] != null)
                                if (cell.Links[0].IsEastPassable)
                                    cell.AddNEAngle();

                    }

                    // Aggiungo i null perchè così ho sempre un'ordine in cui andare e so (per le direzioni dopo)
                    // in quale direzione andare... 0N, 1E, 2S, 3W
                    // Una cosa furba potrebbe essere creare la classe Link che associ il link ad una direzione
                }
            }
        }

        // E' una BFS greedy 
        // TODO euristica
        // Da Ottimizzare enormemente
        // 1. Non creare le liste nuove ma tenerele lì e svuotarle
        private List<Cell> FindPath(Cell A, Cell B)
        {
            var parent = new Dictionary<Cell, Cell>();

            Queue<Cell> queue = new Queue<Cell>();
            List<Cell> visited = new List<Cell>();

            queue.Enqueue(A);
            parent.Add(A, null);

            while (queue.Count != 0)
            {
                Cell c = queue.Dequeue();

                visited.Add(c);

                if (c == B)
                    break;

                foreach (Cell near in c.Links)
                {
                    if (near != null)
                    {
                        if (!visited.Contains(near))
                        {
                            parent.Add(near, c);
                            visited.Add(near);
                            queue.Enqueue(near);
                        }
                    }
                }
            }

            List<Cell> path = new List<Cell>();

            if (parent.ContainsKey(B))
            {
                Cell backTrack = B;
                do
                {
                    path.Add(backTrack);
                    backTrack = parent[backTrack];
                }
                while (backTrack != null);
                path.Reverse();
            }

            return path;
        }

        public List<InputManager.InputAction> PathToDirections(List<Cell> Path)
        {
            List<InputManager.InputAction> directions = new List<InputManager.InputAction>();

            // Ps. Il Null è fondamentale qui perchè ho sempre assegnate le 4 possibilità. Se è null 
            // allora non c'è un percorso a quella cella
            for (int i = 0; i < Path.Count - 1; ++i)
            {
                if (Path[i].Links[0] == Path[i + 1])
                {
                    directions.Add(InputManager.InputAction.Su);
                }
                else if (Path[i].Links[1] == Path[i + 1])
                {
                    directions.Add(InputManager.InputAction.Destra);
                }
                else if (Path[i].Links[2] == Path[i + 1])
                {
                    directions.Add(InputManager.InputAction.Giu);
                }
                else if (Path[i].Links[3] == Path[i + 1])
                {
                    directions.Add(InputManager.InputAction.Sinistra);
                }
            }

            return directions;
        }

        public List<InputManager.InputAction> FindPath(Vector2 A, Vector2 B)
        {
            return PathToDirections(
                FindPath(
                    GetCellAtPixel((int)A.X, (int)A.Y),
                    GetCellAtPixel((int)B.X, (int)B.Y)));
        }

        public void ApplyBloodDecal(Vector2 Where)
        {
            Cell c = GetCellAtPixel(Where);

            c.ApplyBloodDecal();

            // Su
            if (c.Links[0] != null)
                c.Links[0].ApplyBloodDecal();

            // Su-Destra
            if (c.Links[0] != null)
                if (c.Links[0].Links[1] != null)
                    c.Links[0].Links[1].ApplyBloodDecal();

            // Destra
            if (c.Links[1] != null)
                c.Links[1].ApplyBloodDecal();

            // Destra-Giu
            if (c.Links[1] != null)
                if (c.Links[1].Links[2] != null)
                    c.Links[1].Links[2].ApplyBloodDecal();

            // Sotto
            if (c.Links[2] != null)
                c.Links[2].ApplyBloodDecal();

            // Sotto-Sinitra
            if (c.Links[2] != null)
                if (c.Links[2].Links[3] != null)
                    c.Links[2].Links[3].ApplyBloodDecal();

            // Sinistra
            if (c.Links[3] != null)
                c.Links[3].ApplyBloodDecal();

            // Sinistra-Su
            if (c.Links[3] != null)
                if (c.Links[3].Links[0] != null)
                    c.Links[3].Links[0].ApplyBloodDecal();
        }

        #endregion

        #region Update / Draw

        public void Update(float dt)
        {
            int startX = GetCellByPixelX((int)Camera.Position.X);
            int endX = GetCellByPixelX((int)Camera.Position.X + Camera.ViewPortWidth);
            int startY = GetCellByPixelY((int)Camera.Position.Y);
            int endY = GetCellByPixelY((int)Camera.Position.Y + Camera.ViewPortHeight);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if ((x >= 0) && (y >= 0) && (x < n_horizontal_cells_) && (y < n_vertical_cells_))
                    {
                        if (cells_[x, y] != null)
                            cells_[x, y].Update(dt);
                    }
                }
            }
        }

        public void DrawCells(SpriteBatch spriteBatch)
        {
            // Disegno solo le celle presenti nel range visivo della telecamera

            int startX = GetCellByPixelX((int)Camera.Position.X);
            int endX = GetCellByPixelX((int)Camera.Position.X + Camera.ViewPortWidth);
            int startY = GetCellByPixelY((int)Camera.Position.Y);
            int endY = GetCellByPixelY((int)Camera.Position.Y + Camera.ViewPortHeight);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if ((x >= 0) && (y >= 0) && (x < n_horizontal_cells_) && (y < n_vertical_cells_))
                    {
                        if (cells_[x, y] != null)
                            cells_[x, y].DrawCell(spriteBatch);
                    }
                }
            }
        }

        public void DrawWalls(SpriteBatch spriteBatch)
        {

            int startX = GetCellByPixelX((int)Camera.Position.X);
            int endX = GetCellByPixelX((int)Camera.Position.X + Camera.ViewPortWidth);
            int startY = GetCellByPixelY((int)Camera.Position.Y);
            int endY = GetCellByPixelY((int)Camera.Position.Y + Camera.ViewPortHeight);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if ((x >= 0) && (y >= 0) && (x < n_horizontal_cells_) && (y < n_vertical_cells_))
                    {
                        if (cells_[x, y] != null)
                            cells_[x, y].DrawWalls(spriteBatch);
                    }
                }
            }
        }

        public void DrawAngles(SpriteBatch spriteBatch)
        {
            int startX = GetCellByPixelX((int)Camera.Position.X);
            int endX = GetCellByPixelX((int)Camera.Position.X + Camera.ViewPortWidth);
            int startY = GetCellByPixelY((int)Camera.Position.Y);
            int endY = GetCellByPixelY((int)Camera.Position.Y + Camera.ViewPortHeight);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if ((x >= 0) && (y >= 0) && (x < n_horizontal_cells_) && (y < n_vertical_cells_))
                    {
                        if (cells_[x, y] != null)
                            cells_[x, y].DrawAngles(spriteBatch);
                    }
                }
            }
        }

        #endregion
    }
}
