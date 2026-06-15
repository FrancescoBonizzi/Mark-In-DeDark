using MarkInDeDark.Common;
using MarkInDeDark.LevelEditor.Converters;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace MarkInDeDark.LevelEditor
{
    public partial class InGameParametersWindow : Window
    {
        private readonly string _tempConfigurationPath;
        private readonly NormalMapParameters _normalMapParameters;

        public InGameParametersWindow(string gameExecutablePath)
        {
            InitializeComponent();

            _tempConfigurationPath = Path.Combine(Path.GetDirectoryName(gameExecutablePath), NormalMapParameters.FileName);
            PenumbraAmbienceColor.SelectedColor = new System.Windows.Media.Color()
            {
                R = 104,
                G = 104,
                B = 104,
                A = 255
            };

            _normalMapParameters = new NormalMapParameters()
            {
                PlayerLightColor = new Vector3((float)LightColorX.Value, (float)LightColorY.Value, (float)LightColorZ.Value),
                AmbienceColor = new Vector3((float)AmbienceColorX.Value, (float)AmbienceColorY.Value, (float)AmbienceColorZ.Value),
                PlayerLightDirectionZ = (float)LightDirectionZ.Value,
                PlayerLightDistance = (float)LightDistance.Value,
                PlayerLightOpacity = (float)LightOpacity.Value,
                EditAmbienceLights = false,
                PenumbraAmbienceColor = PenumbraAmbienceColor.SelectedColor.Value.ToXnaColor()
            };

            LightColorX.ValueChanged += (obj, args) => _normalMapParameters.PlayerLightColor.X = (float)args.NewValue;
            LightColorY.ValueChanged += (obj, args) => _normalMapParameters.PlayerLightColor.Y = (float)args.NewValue;
            LightColorZ.ValueChanged += (obj, args) => _normalMapParameters.PlayerLightColor.Z = (float)args.NewValue;

            AmbienceColorX.ValueChanged += (obj, args) => _normalMapParameters.AmbienceColor.X = (float)args.NewValue;
            AmbienceColorY.ValueChanged += (obj, args) => _normalMapParameters.AmbienceColor.Y = (float)args.NewValue;
            AmbienceColorZ.ValueChanged += (obj, args) => _normalMapParameters.AmbienceColor.Z = (float)args.NewValue;

            LightDirectionZ.ValueChanged += (obj, args) => _normalMapParameters.PlayerLightDirectionZ = (float)args.NewValue;
            LightDistance.ValueChanged += (obj, args) => _normalMapParameters.PlayerLightDistance = (float)args.NewValue;
            LightOpacity.ValueChanged += (obj, args) => _normalMapParameters.PlayerLightOpacity = (float)args.NewValue;

            PenumbraAmbienceColor.SelectedColorChanged += PenumbraAmbienceColor_SelectedColorChanged;
        }

        private void PenumbraAmbienceColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            _normalMapParameters.PenumbraAmbienceColor = e.NewValue.Value.ToXnaColor();
        }

        private void btnMandaCambiamentiAlGiocoSoloPlayer_Click(object sender, RoutedEventArgs e)
        {
            _normalMapParameters.EditAmbienceLights = false;
            _normalMapParameters.EditPlayerLights = true;
            var serializedConfiguration = JsonConvert.SerializeObject(_normalMapParameters);
            File.WriteAllText(_tempConfigurationPath, serializedConfiguration);
        }

        private void btnMandaCambiamentiAlGiocoSoloLuceAmbientale_Click(object sender, RoutedEventArgs e)
        {
            _normalMapParameters.EditAmbienceLights = true;
            _normalMapParameters.EditPlayerLights = false;
            var serializedConfiguration = JsonConvert.SerializeObject(_normalMapParameters);
            File.WriteAllText(_tempConfigurationPath, serializedConfiguration);
        }
    }
}
