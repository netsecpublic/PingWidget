using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace PingWidget
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return b ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v) return v == Visibility.Collapsed;
            return false;
        }
    }

    public partial class App : Application
    {
    }
}