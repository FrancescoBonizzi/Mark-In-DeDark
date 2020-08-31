using MarkInDeDark.GameGridCommon;
using System;
using System.Linq;
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

                case GroundTypes.Terra1:
                case GroundTypes.Terra2:
                case GroundTypes.Terra3:
                case GroundTypes.Terra4:
                case GroundTypes.Terra5:
                case GroundTypes.Terra6:
                case GroundTypes.Terra7:
                case GroundTypes.Terra8:
                case GroundTypes.Terra9:
                    return App.Current.Resources[$"ColoreTerra{type.ToString().Last()}"] as SolidColorBrush;

                case GroundTypes.Terra10:
                    return App.Current.Resources["ColoreTerra10"] as SolidColorBrush;

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
                    return App.Current.Resources["ColoreRiempimento"] as SolidColorBrush;

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
                case WallTypes.Muro1:
                case WallTypes.Muro2:
                case WallTypes.Muro3:
                case WallTypes.Muro4:
                case WallTypes.Muro5:
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
                case WallTypes.Decal6:
                case WallTypes.Decal7:
                case WallTypes.Decal8:
                    return new SolidColorBrush(Colors.Violet);
            }

            throw new NotImplementedException();
        }
    }
}
