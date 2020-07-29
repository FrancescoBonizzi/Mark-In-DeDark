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

                case GroundTypes.Terra1:
                    return App.Current.Resources["ColoreTerra"] as SolidColorBrush;

                case GroundTypes.Terra2:
                    return App.Current.Resources["ColoreTerra2"] as SolidColorBrush;

                case GroundTypes.Terra3:
                    return App.Current.Resources["ColoreTerra3"] as SolidColorBrush;

                case GroundTypes.Terra4:
                    return App.Current.Resources["ColoreTerra4"] as SolidColorBrush;

                case GroundTypes.Terra5:
                    return App.Current.Resources["ColoreTerra5"] as SolidColorBrush;

                case GroundTypes.Terra6:
                    return App.Current.Resources["ColoreTerra6"] as SolidColorBrush;

                case GroundTypes.Terra7:
                    return App.Current.Resources["ColoreTerra7"] as SolidColorBrush;

                case GroundTypes.Terra8:
                    return App.Current.Resources["ColoreTerra8"] as SolidColorBrush;

                case GroundTypes.Terra9:
                    return App.Current.Resources["ColoreTerra9"] as SolidColorBrush;

                case GroundTypes.Terra10:
                    return App.Current.Resources["ColoreTerra10"] as SolidColorBrush;

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
