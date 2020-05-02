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

                case GroundTypes.CellaMuroSpeciale:
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
                    return new SolidColorBrush(Colors.Green);

                case WallTypes.Fragile:
                    return new SolidColorBrush(Colors.Red);

                case WallTypes.Porta:
                    return new SolidColorBrush(Colors.Blue);

                case WallTypes.Speciale1:
                    return new SolidColorBrush(Colors.Yellow);

                case WallTypes.Speciale2:
                    return new SolidColorBrush(Colors.Gold);

                case WallTypes.Speciale3:
                    return new SolidColorBrush(Colors.Brown);

                case WallTypes.Speciale4:
                    return new SolidColorBrush(Colors.Coral);

                case WallTypes.Speciale5:
                    return new SolidColorBrush(Colors.DarkKhaki);

                case WallTypes.Ingresso:
                    return new SolidColorBrush(Colors.White);

                case WallTypes.Uscita:
                    return new SolidColorBrush(Colors.Aqua);

                case WallTypes.Decal1:
                    return new SolidColorBrush(Colors.RosyBrown);

                case WallTypes.Decal2:
                    return new SolidColorBrush(Colors.MistyRose); 

                case WallTypes.Decal3:
                    return new SolidColorBrush(Colors.CornflowerBlue); 

                case WallTypes.Decal4:
                    return new SolidColorBrush(Colors.BlueViolet); 

                case WallTypes.Decal5:
                    return new SolidColorBrush(Colors.Violet);

            }

            throw new NotImplementedException();
        }
    }
}
