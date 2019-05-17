using MarkInDeDark.LevelEditor.Domain;
using System;
using System.Windows.Media;

namespace MarkInDeDark.LevelEditor.Editor
{
    public static class EditorTypesColorConverter
    {
        public static SolidColorBrush Convert(GroundTypes type)
        {
            switch (type)
            {
                case GroundTypes.Buio:
                    return new SolidColorBrush(Colors.Black);

                case GroundTypes.Terra:
                    return new SolidColorBrush(Colors.Brown);

                case GroundTypes.Macerie:
                    return new SolidColorBrush(Colors.White);
            }

            throw new NotImplementedException();
        }

        public static SolidColorBrush Convert(WallTypes type)
        {
            switch (type)
            {
                case WallTypes.Niente:
                    return new SolidColorBrush(Colors.Black);

                case WallTypes.Occludente:
                    return new SolidColorBrush(Colors.Green);

                case WallTypes.Fragile:
                    return new SolidColorBrush(Colors.Red);
            }

            throw new NotImplementedException();
        }
    }
}
