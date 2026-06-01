using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Do_An.Helper
{
    public class RatioToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double ratio = value is double d ? d : 0.0;
            ratio = Math.Max(0, Math.Min(1, ratio));

            bool invert = parameter?.ToString()?.ToLower() == "invert";
            double stars = invert ? 1.0 - ratio : ratio;

            if (stars <= 0)
                stars = 0.001;

            return new GridLength(stars, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}