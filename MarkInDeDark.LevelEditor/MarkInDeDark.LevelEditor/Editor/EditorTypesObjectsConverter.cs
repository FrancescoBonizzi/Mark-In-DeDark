using MarkInDeDark.LevelEditor.Domain;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarkInDeDark.LevelEditor.Editor
{
    public static class EditorTypesObjectsConverter
    {
        public static Image Convert(ObjectTypes type)
        {
            string imagePath = string.Empty;
            switch (type)
            {
                case ObjectTypes.Giocatore:
                    imagePath = "Assets/icons/player_front_iddle.png";
                    break;

                case ObjectTypes.Chiave:
                    imagePath = "Assets/icons/key.png";
                    break;

                case ObjectTypes.Uscita:
                    imagePath = "Assets/icons/win_closed.png";
                    break;

                case ObjectTypes.Candele:
                    imagePath = "Assets/icons/candle6.png";
                    break;
            }

            return new Image()
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Width = 20,
                Height = 20
            };
        }
    }
}
