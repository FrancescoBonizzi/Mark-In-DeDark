using MarkInDeDark.LevelEditor.Domain;
using System;
using System.Windows.Media;

namespace MarkInDeDark.LevelEditor.Editor
{
    public static class CellTypeColorConverter
    {
        public static SolidColorBrush Convert(CellTypes type)
        {
            switch (type)
            {
                case CellTypes.Buio:
                    return new SolidColorBrush(Colors.Black);

                case CellTypes.Terra:
                    return new SolidColorBrush(Colors.Brown);

                case CellTypes.Macerie:
                    return new SolidColorBrush(Colors.White);
            }

            throw new NotImplementedException();
        }
    }
}
