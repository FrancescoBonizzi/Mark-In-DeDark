using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace MarkInDeDark.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Dichiarazioni

        public event PropertyChangedEventHandler PropertyChanged;
        public LevelsStorage _levels;

        private IsolatedStorageSettings _settings;
        private string _soundEntry = "SoundEnabled";
        public Nullable<bool> _soundEnabled;

        #endregion

        public MainViewModel()
        {
            Levels = new LevelsStorage();

            #region Sound

            _settings = IsolatedStorageSettings.ApplicationSettings;

            if (!_settings.Contains(_soundEntry))
            {
                SoundEnabled = true;
            }
            else
            {
                SoundEnabled = (Nullable<bool>)_settings[_soundEntry];
            }

            #endregion
        }

        #region Properties

        public Nullable<bool> SoundEnabled
        {
            get { return _soundEnabled; }
            set
            {
                if (_soundEnabled == value)
                    return;

                _soundEnabled = value;

                if (!_settings.Contains(_soundEntry))
                {
                    _settings.Add(_soundEntry, value);
                    _settings.Save();
                }
                else
                {
                    _settings[_soundEntry] = value;
                }

                Mark.Engine.GameReferencesLocator.SoundManagerReference.SetSound(value.Value);

                RaisePropertyChanged("SoundEnabled");
            }
        }

        public LevelsStorage Levels
        {
            get { return _levels; }
            private set
            {
                if (_levels == value)
                    return;

                _levels = value;
                RaisePropertyChanged("Levels");
            }
        }

        public void RaisePropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        #endregion
    }
}
