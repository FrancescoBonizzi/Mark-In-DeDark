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
                case CellTypes.Dark:
                    return new SolidColorBrush(Colors.Black);
                case CellTypes.Ground:

                    return new SolidColorBrush(Colors.Brown);

                    // TODO Many other...
            }

            throw new NotImplementedException();
        }
    }
}
