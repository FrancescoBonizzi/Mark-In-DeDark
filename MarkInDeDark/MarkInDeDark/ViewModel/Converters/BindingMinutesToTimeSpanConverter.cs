using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace MarkInDeDark.ViewModel.Converters
{
    public class BindingMinutesToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan s = TimeSpan.FromSeconds((int)value);
            return string.Format("{0}:{1:00}", (int)s.TotalMinutes, s.Seconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
