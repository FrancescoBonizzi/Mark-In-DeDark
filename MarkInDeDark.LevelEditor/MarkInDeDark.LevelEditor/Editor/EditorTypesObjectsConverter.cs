using MarkInDeDark.GameGrid;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarkInDeDark.LevelEditor.Editor
{
    public static class EditorTypesObjectsConverter
    {
        public static Grid Convert(ObjectTypes type, string quantityInfo)
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

                case ObjectTypes.Zombie:
                    imagePath = "Assets/icons/zombie_front_walking_1.png";
                    break;

                case ObjectTypes.Ghost:
                    imagePath = "Assets/icons/ghost_back_attack_1.png";
                    break;

                case ObjectTypes.Scheletro:
                    imagePath = "Assets/icons/skeleton_front_iddle.png";
                    break;

                case ObjectTypes.Goblin:
                    imagePath = "Assets/icons/goblin_front_dying_1.png";
                    break;

                case ObjectTypes.Taurus:
                    imagePath = "Assets/icons/taurus_front_iddle.png";
                    break;
            }

            var container = new Grid();

            container.Children.Add(new Image()
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Margin = new System.Windows.Thickness(2)
            });

            if (quantityInfo != null)
            {
                container.Children.Add(new TextBlock()
                {
                    Text = quantityInfo,
                    FontFamily = new FontFamily("SEGOE UI"),
                    FontWeight = System.Windows.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Colors.White),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                    Margin = new System.Windows.Thickness(0),
                    Padding = new System.Windows.Thickness(0),
                    FontSize = 10
                });
            }

            return container;
        }
    }
}
