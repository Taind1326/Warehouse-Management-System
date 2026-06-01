using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Do_An.Helper
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string hex)
                    return (Color)ColorConverter.ConvertFromString(hex);
            }
            catch
            {
            }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}