using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DirectoryInfo.App.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        const string inverted = "invert";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                var isInverted = false;
                if(parameter is string)
                    isInverted = parameter.ToString().Equals(inverted);

                var isVisible = isInverted ? !(bool)value : (bool)value;

                return isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                var isInverted = false;
                if (parameter is string)
                    isInverted = parameter.ToString().Equals(inverted);

                var isVisible = isInverted ? (Visibility)value  != Visibility.Visible: (Visibility)value == Visibility.Visible;

                return isVisible;
            }
            else
                return false;
        }
    }
}
