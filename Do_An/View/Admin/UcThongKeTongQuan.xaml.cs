using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Do_An.View.Admin
{
    /// <summary>
    /// Code-behind tối giản theo MVVM – chỉ set DataContext.
    /// </summary>
    public partial class UcThongKeTongQuan : System.Windows.Controls.UserControl
    {
        public UcThongKeTongQuan()
        {
            InitializeComponent();
        }
    }

    // =========================================================
    //  Converter 1: double Ratio  →  GridLength (Star)
    //  ConverterParameter = "Invert"  →  trả về (1 - Ratio)*
    // =========================================================
    public class RatioToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double ratio = value is double d ? d : 0.0;
            ratio = Math.Max(0, Math.Min(1, ratio));

            bool invert = parameter?.ToString()?.ToLower() == "invert";
            double stars = invert ? (1.0 - ratio) : ratio;

            // Tránh GridLength = 0* (WPF không render đẹp)
            if (stars <= 0) stars = 0.001;

            return new GridLength(stars, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // =========================================================
    //  Converter 2: string hex "#FFrrggbb"  →  Color
    // =========================================================
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string hex)
                    return (Color)ColorConverter.ConvertFromString(hex);
            }
            catch { /* fallback */ }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
