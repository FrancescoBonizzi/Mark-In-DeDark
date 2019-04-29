using System.ComponentModel;

namespace MarkInDeDark.ViewModel
{
    public class ObservableSingleObject<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyPropertyChanged("Value");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
