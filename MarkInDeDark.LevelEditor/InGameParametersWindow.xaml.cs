using MarkInDeDark.Common;
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

            _normalMapParameters = new NormalMapParameters()
            {
                LightColor = new Vector3((float)LightColorX.Value, (float)LightColorY.Value, (float)LightColorZ.Value),
                AmbienceColor = new Vector3((float)AmbienceColorX.Value, (float)AmbienceColorY.Value, (float)AmbienceColorZ.Value),
                LightDirectionZ = (float)LightDirectionZ.Value,
                LightDistance = (float)LightDistance.Value,
                LightOpacity = (float)LightOpacity.Value
            };

            LightColorX.ValueChanged += (obj, args) => _normalMapParameters.LightColor.X = (float)args.NewValue;
            LightColorY.ValueChanged += (obj, args) => _normalMapParameters.LightColor.Y = (float)args.NewValue;
            LightColorZ.ValueChanged += (obj, args) => _normalMapParameters.LightColor.Z = (float)args.NewValue;

            AmbienceColorX.ValueChanged += (obj, args) => _normalMapParameters.AmbienceColor.X = (float)args.NewValue;
            AmbienceColorY.ValueChanged += (obj, args) => _normalMapParameters.AmbienceColor.Y = (float)args.NewValue;
            AmbienceColorZ.ValueChanged += (obj, args) => _normalMapParameters.AmbienceColor.Z = (float)args.NewValue;

            LightDirectionZ.ValueChanged += (obj, args) => _normalMapParameters.LightDirectionZ = (float)args.NewValue;
            LightDistance.ValueChanged += (obj, args) => _normalMapParameters.LightDistance = (float)args.NewValue;
            LightOpacity.ValueChanged += (obj, args) => _normalMapParameters.LightOpacity = (float)args.NewValue;
        }

        private void btnMandaCambiamentiAlGioco_Click(object sender, RoutedEventArgs e)
        {
            var serializedConfiguration = JsonConvert.SerializeObject(_normalMapParameters);
            File.WriteAllText(_tempConfigurationPath, serializedConfiguration);
        }
    }
}
