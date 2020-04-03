using System.Windows.Controls;
using System.Windows.Media;

namespace MarkInDeDark.LevelEditor.Editor
{
    public static class EditorCellTextConverer
    {
        public static void ManageCellText(Border canvasCell, string text)
        {
            if (!(canvasCell.Child is Grid container))
            {
                container = new Grid();
                canvasCell.Child = container;
            }

            // Per convenzione le label sono per il testo da far vedere 
            // (le textblock sono per il numero di candele ecc)
            Label label = null;
            foreach (var c in container.Children)
            {
                if (c is Label textLabel)
                {
                    label = textLabel;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                if (label == null)
                {
                    container.Children.Add(new Label()
                    {
                        Content = "A",
                        FontFamily = new FontFamily("Times New Roman"),
                        FontWeight = System.Windows.FontWeights.SemiBold,
                        FontStyle = System.Windows.FontStyles.Italic,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalAlignment = System.Windows.VerticalAlignment.Top,
                        Margin = new System.Windows.Thickness(4),
                        Padding = new System.Windows.Thickness(0),
                        FontSize = 10
                    });
                }
            }
            else if (label != null)
            {
                container.Children.Remove(label);
            }

            EditorTypesObjectsConverter.ReorderControls(container);
        }
    }
}
