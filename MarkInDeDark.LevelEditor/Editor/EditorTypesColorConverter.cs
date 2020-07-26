using MarkInDeDark.GameGridCommon;
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
                case GroundTypes.NonCella:
                    return new SolidColorBrush(Colors.Black);

                case GroundTypes.Buio:
                    return new SolidColorBrush(Colors.Gray);

                case GroundTypes.Terra:
                    return new SolidColorBrush(Colors.Brown);

                case GroundTypes.Terra2:
                    return new SolidColorBrush(Colors.SandyBrown);

                case GroundTypes.Terra3:
                    return new SolidColorBrush(Colors.RosyBrown);

                case GroundTypes.Terra4:
                    return new SolidColorBrush(Colors.LightCoral);

                case GroundTypes.Terra5:
                    return new SolidColorBrush(Colors.BlanchedAlmond);

                case GroundTypes.Riempimento:
                    return new SolidColorBrush(Colors.Navy);

                case GroundTypes.TrappolaMorteIstantanea:
                    return new SolidColorBrush(Colors.Fuchsia);

                case GroundTypes.TrappolaTemporizzata:
                    return new SolidColorBrush(Colors.Aqua);

                case GroundTypes.TriggerGoblin:
                    return new SolidColorBrush(Colors.Lime);
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
                case WallTypes.Speciale1:
                case WallTypes.Speciale2:
                case WallTypes.Speciale3:
                case WallTypes.Speciale4:
                case WallTypes.Speciale5:
                    return new SolidColorBrush(Colors.Green);

                case WallTypes.Fragile:
                    return new SolidColorBrush(Colors.Red);

                case WallTypes.Porta:
                    return new SolidColorBrush(Colors.Blue);

                case WallTypes.Ingresso:
                case WallTypes.Uscita:
                    return new SolidColorBrush(Colors.Aqua);

                case WallTypes.Decal1:
                case WallTypes.Decal2:
                case WallTypes.Decal3:
                case WallTypes.Decal4:
                case WallTypes.Decal5:
                    return new SolidColorBrush(Colors.Violet);
            }

            throw new NotImplementedException();
        }
    }
}
