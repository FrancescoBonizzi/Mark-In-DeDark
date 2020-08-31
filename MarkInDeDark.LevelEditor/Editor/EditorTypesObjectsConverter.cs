using MarkInDeDark.GameGridCommon;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarkInDeDark.LevelEditor.Editor
{
    public static class EditorTypesObjectsConverter
    {
        public static void RemoveGroundTypeImage(Border canvasCell)
        {
            if (!(canvasCell.Child is Grid container))
                return;

            Image image = null;
            foreach (var i in container.Children)
            {
                if (i is Image thisImage)
                {
                    image = thisImage;
                    break;
                }
            }

            if (image != null)
            {
                container.Children.Remove(image);
            }

            ReorderControls(container);
        }

        public static void ManageGroundTypeWithImage(
            Border canvasCell,
            GroundTypes groundType)
        {
            if (!GridObjectsHelpers.IsRiempimento(groundType))
            {
                throw new Exception("GroundType with image is only for 'riempimento'");
            }

            if (!(canvasCell.Child is Grid container))
            {
                container = new Grid();
                canvasCell.Child = container;
            }

            canvasCell.Background = EditorTypesColorConverter.Convert(groundType);

            // Per convenzione le textblock senza Tag sono per il numero di candele ecc
            TextBlock textBlock = null;
            foreach (var c in container.Children)
            {
                if (c is TextBlock thisGridTextLabel
                    && string.IsNullOrWhiteSpace(thisGridTextLabel.Tag as string))
                {
                    textBlock = thisGridTextLabel;
                    break;
                }
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
                container.Children.Add(GetImage(groundType));
            }
            else
            {
                container.Children.Remove(image);
                container.Children.Add(GetImage(groundType));
            }

            ReorderControls(container);
        }

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

            if (GridObjectsHelpers.IsRiempimento(groundType))
            {
                if (type != null)
                {
                    groundType = GroundTypes.Terra1;
                    RemoveGroundTypeImage(canvasCell);
                }
                else
                {
                    ManageGroundTypeWithImage(canvasCell, groundType);
                    return;
                }
            }

            canvasCell.Background = EditorTypesColorConverter.Convert(groundType);

            // Per convenzione le textblock senza Tag sono per il numero di candele ecc
            TextBlock textBlock = null;
            foreach (var c in container.Children)
            {
                if (c is TextBlock thisGridTextLabel
                    && string.IsNullOrWhiteSpace(thisGridTextLabel.Tag as string))
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
                    margins.Top = -4;
                    horizontalAlignment = HorizontalAlignment.Center;
                    verticalAlignment = VerticalAlignment.Top;
                    break;

                case WallPositions.Right:
                    margins.Right = -4;
                    horizontalAlignment = HorizontalAlignment.Right;
                    verticalAlignment = VerticalAlignment.Center;
                    break;

                case WallPositions.Left:
                    margins.Left = -4;
                    horizontalAlignment = HorizontalAlignment.Left;
                    verticalAlignment = VerticalAlignment.Center;
                    break;

                case WallPositions.Bottom:
                    margins.Bottom = -4;
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

            if (type != WallTypes.Niente)
            {
                canvasCell.BorderBrush = EditorTypesColorConverter.Convert(type);
            }

            ReorderControls(container);
        }

        public static void ReorderControls(Grid grid)
        {
            Image image = null;
            TextBlock textBlockObjectQuantity = null;
            List<TextBlock> textBlocksWallType = new List<TextBlock>();
            Label label = null;

            foreach (var control in grid.Children)
            {
                if (control is Image)
                    image = (Image)control;
                else if (control is Label)
                    label = (Label)control;
                else if (control is TextBlock t)
                {
                    if (t.Tag is string tag && string.IsNullOrEmpty(tag))
                        textBlockObjectQuantity = (TextBlock)control;
                    else textBlocksWallType.Add(t);
                }

            }

            // Con lo Zindex purtroppo non si riesce
            grid.Children.Clear();
            if (image != null)
                grid.Children.Add(image);
            if (textBlockObjectQuantity != null)
                grid.Children.Add(textBlockObjectQuantity);
            if (label != null)
                grid.Children.Add(label);
            foreach (var tWallType in textBlocksWallType)
                grid.Children.Add(tWallType);
        }

        public static string GetWallCode(WallTypes wallType)
        {
            string wallTypeString = wallType.ToString();
            if (wallTypeString.Contains("Decal") || wallTypeString.Contains("Muro"))
            {
                return $"{wallTypeString[0]}{wallTypeString[wallTypeString.Length - 1]}";
            }

            return wallType switch
            {
                WallTypes.Niente => string.Empty,
                WallTypes.Occludente => "M0",
                WallTypes.Fragile => "MB",
                WallTypes.Porta => "P",
                WallTypes.Ingresso => "I",
                WallTypes.Uscita => "U",
                _ => throw new Exception($"Tipo di muro {wallType} non gestito"),
            };
        }

        public static Image GetImage(GroundTypes groundType)
        {
            switch (groundType)
            {
                case GroundTypes.Riempimento1:
                case GroundTypes.Riempimento2:
                case GroundTypes.Riempimento3:
                case GroundTypes.Riempimento4:
                case GroundTypes.Riempimento5:
                case GroundTypes.Riempimento6:
                case GroundTypes.Riempimento7:
                case GroundTypes.Riempimento8:
                case GroundTypes.Riempimento9:
                case GroundTypes.Riempimento10:
                case GroundTypes.Riempimento11:
                case GroundTypes.Riempimento12:
                case GroundTypes.Riempimento13:

                    string number = groundType.ToString().Replace("Riempimento", string.Empty);
                    return new Image()
                    {
                        Source = new BitmapImage(
                            new Uri($"Assets/icons/Riempimento/riempimento_{number}.png", 
                            UriKind.RelativeOrAbsolute)),
                        Margin = new Thickness(2)
                    };
            }

            throw new Exception($"Image not found for GroundType {groundType}");
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