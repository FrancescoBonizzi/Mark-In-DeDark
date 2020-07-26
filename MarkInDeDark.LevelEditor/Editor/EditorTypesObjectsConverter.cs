using MarkInDeDark.GameGridCommon;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarkInDeDark.LevelEditor.Editor
{
    public static class EditorTypesObjectsConverter
    {
        public static void ManageObjectType(
            Border canvasCell,
            ObjectTypes? type,
            GroundTypes groundType,
            string quantityInfo)
        {
            if (!(canvasCell.Child is Grid container))
            {
                container = new Grid();
                canvasCell.Child = container;
            }

            canvasCell.Background = EditorTypesColorConverter.Convert(groundType);

            // Per convenzione le textblock sono per il numero di candele ecc
            TextBlock textBlock = null;
            foreach (var c in container.Children)
            {
                if (c is TextBlock thisGridTextLabel)
                {
                    textBlock = thisGridTextLabel;
                    break;
                }
            }

            if (quantityInfo != null)
            {
                if (textBlock == null)
                {
                    container.Children.Add(new TextBlock()
                    {
                        Text = quantityInfo,
                        FontFamily = new FontFamily("SEGOE UI"),
                        FontWeight = FontWeights.SemiBold,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0),
                        Padding = new Thickness(0),
                        FontSize = 10
                    });
                }
                else
                {
                    textBlock.Text = quantityInfo;
                }
            }
            else
            {
                container.Children.Remove(textBlock);
            }

            Image image = null;
            foreach (var i in container.Children)
            {
                if (i is Image thisImage)
                {
                    image = thisImage;
                    break;
                }
            }

            if (image == null)
            {
                if (type != null)
                    container.Children.Add(GetImage(type.Value));
            }
            else
            {
                container.Children.Remove(image);

                if (type != null)
                    container.Children.Add(GetImage(type.Value));
            }

            ReorderControls(container);
        }

        public static void ManageWallType(
            Border canvasCell,
            WallTypes type,
            WallPositions wallPosition)
        {
            if (!(canvasCell.Child is Grid container))
            {
                container = new Grid();
                canvasCell.Child = container;
            }

            var currentBorderThickness = canvasCell.BorderThickness;
            int strokeWidth = type == WallTypes.Niente ? 0 : 4;

            canvasCell.BorderThickness = new Thickness(
                left: wallPosition == WallPositions.Left ? strokeWidth : currentBorderThickness.Left,
                top: wallPosition == WallPositions.Top ? strokeWidth : currentBorderThickness.Top,
                right: wallPosition == WallPositions.Right ? strokeWidth : currentBorderThickness.Right,
                bottom: wallPosition == WallPositions.Bottom ? strokeWidth : currentBorderThickness.Bottom);

            // Textblock per indicare il numero del muro
            string textblockTag = $"Wall{wallPosition}";
            TextBlock oldWallNameTextblock = null;
            foreach (var c in container.Children)
            {
                if (c is TextBlock thisGridTextLabel
                    && thisGridTextLabel.Tag is string tag
                    && !string.IsNullOrEmpty(tag)
                    && tag == textblockTag)
                {
                    oldWallNameTextblock = thisGridTextLabel;
                    break;
                }
            }

            string thisWallCode = GetWallCode(type);

            var margins = new Thickness();
            var horizontalAlignment = HorizontalAlignment.Left;
            var verticalAlignment = VerticalAlignment.Top;

            switch (wallPosition)
            {
                case WallPositions.Top:
                    margins.Top = -7;
                    horizontalAlignment = HorizontalAlignment.Center;
                    verticalAlignment = VerticalAlignment.Top;
                    break;

                case WallPositions.Right:
                    margins.Right = -7;
                    horizontalAlignment = HorizontalAlignment.Right;
                    verticalAlignment = VerticalAlignment.Center;
                    break;

                case WallPositions.Left:
                    margins.Left = -7;
                    horizontalAlignment = HorizontalAlignment.Left;
                    verticalAlignment = VerticalAlignment.Center;
                    break;

                case WallPositions.Bottom:
                    margins.Bottom = -7;
                    horizontalAlignment = HorizontalAlignment.Center;
                    verticalAlignment = VerticalAlignment.Bottom;
                    break;
            }

            var wallNameTextblock = new TextBlock()
            {
                Text = thisWallCode,
                Tag = textblockTag,
                FontFamily = new FontFamily("SEGOE UI"),
                Foreground = new SolidColorBrush(Colors.White),
                Margin = margins,
                Padding = new Thickness(0),
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment,
                FontSize = 8
            };

            if (oldWallNameTextblock == null)
            {
                container.Children.Add(wallNameTextblock);
            }
            else
            {
                container.Children.Remove(oldWallNameTextblock);
                container.Children.Add(wallNameTextblock);
            }

            //Panel.SetZIndex(wallNameTextblock, 10);

            if (type != WallTypes.Niente)
            {
                canvasCell.BorderBrush = EditorTypesColorConverter.Convert(type);
            }
        }

        public static void ReorderControls(Grid grid)
        {
            Image image = null;
            TextBlock textBlock = null;
            Label label = null;

            foreach (var control in grid.Children)
            {
                if (control is Image)
                    image = (Image)control;
                else if (control is TextBlock)
                    textBlock = (TextBlock)control;
                else if (control is Label)
                    label = (Label)control;
            }

#warning Non si può sistemare con lo Zindex?
            grid.Children.Clear();
            if (image != null)
                grid.Children.Add(image);
            if (label != null)
                grid.Children.Add(label);
            if (textBlock != null)
                grid.Children.Add(textBlock);
        }

        public static string GetWallCode(WallTypes wallType)
        {
            string wallTypeString = wallType.ToString();
            if (wallTypeString.Contains("Decal") || wallTypeString.Contains("Speciale"))
            {
                return $"{wallTypeString[0]}{wallTypeString[wallTypeString.Length - 1]}";
            }

            switch (wallType)
            {
                case WallTypes.Niente:
                    return string.Empty;

                case WallTypes.Occludente:
                    return "M0";

                case WallTypes.Fragile:
                    return "MR";

                case WallTypes.Porta:
                    return "P";

                case WallTypes.Speciale1:
                    return "M1";

                case WallTypes.Ingresso:
                    return "I";

                case WallTypes.Uscita:
                    return "U";
            }

            throw new Exception($"Tipo di muro {wallType} non gestito");
        }

        public static Image GetImage(ObjectTypes type)
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

                case ObjectTypes.Lucina:
                    imagePath = "Assets/icons/brazier_lit_1.png";
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

                case ObjectTypes.Bubu:
                    imagePath = "Assets/icons/bubu_front_walking_1.png";
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

                case ObjectTypes.OggettoNonPassabile1:
                    imagePath = "Assets/icons/object_unpassable_1.png";
                    break;

                case ObjectTypes.OggettoNonPassabile2:
                    imagePath = "Assets/icons/object_unpassable_2.png";
                    break;

                case ObjectTypes.OggettoPassabile1:
                    imagePath = "Assets/icons/object_passable_1.png";
                    break;
            }

            return new Image()
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Margin = new Thickness(2)
            };
        }

    }
}