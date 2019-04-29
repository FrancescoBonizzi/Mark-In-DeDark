using MarkInDeDark.ViewModel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mark.Engine
{
    public class Level
    {
        #region Dichiarazioni

        private String folder_ = "Levels/";
        public String Filename { get; set; }
        public String TextToShow { get; private set; }

        public ObservableSingleObject<bool> Completed { get; set; }
        public ObservableSingleObject<bool> HasSecret { get; set; }
        public int StartingCandlesNumber { get; set; }
        public ObservableSingleObject<int> RecordUsedCandles { get; set; }
        public ObservableSingleObject<int> RecordCompletionMinutes { get; set; }

        public Rectangle CellRectangle { get; private set; }
        public Rectangle MaceriaRectangle { get; private set; }
        public Rectangle DirtyRectangle { get; private set; }
        public double DirtyProbability { get; private set; }
        public Rectangle WallRectangle { get; private set; }
        public Rectangle AngleRectangle { get; private set; }
        public Rectangle SecretRectangle { get; private set; }

        #endregion

        #region Costruttore

        public Level(
            String Filename,
            String TextToShow,
            bool IsCompleted,
            bool HasSecret,
            Rectangle CellRectangle,
            Rectangle DirtyTextureRectangle,
            Rectangle WallRectangle,
            Rectangle AngleRectangle,
            Rectangle MaceriaRectangle,
            Rectangle SecretRectangle,
            double DirtyProbability,
            int StartingCandlesNumber,
            int RecordUsedCandles,
            int RecordCompletionMinutes)
        {
            this.Filename = Filename;
            this.TextToShow = TextToShow;

            this.Completed = new ObservableSingleObject<bool>();
            this.Completed.Value = IsCompleted;

            this.HasSecret = new ObservableSingleObject<bool>();
            this.HasSecret.Value = HasSecret;

            this.RecordUsedCandles = new ObservableSingleObject<int>();
            this.RecordUsedCandles.Value = RecordUsedCandles;
            this.RecordCompletionMinutes = new ObservableSingleObject<int>();
            this.RecordCompletionMinutes.Value = RecordCompletionMinutes;

            this.StartingCandlesNumber = StartingCandlesNumber;

            this.CellRectangle = CellRectangle;
            this.DirtyRectangle = DirtyTextureRectangle;
            this.DirtyProbability = DirtyProbability;
            this.WallRectangle = WallRectangle;
            this.AngleRectangle = AngleRectangle;
            this.MaceriaRectangle = MaceriaRectangle;
            this.SecretRectangle = SecretRectangle;
        }

        #endregion

        #region Metodi

        public String Path()
        {
            return folder_ + Filename;
        }

        public override string ToString()
        {
            return TextToShow;
        }

        #endregion
    }
}
