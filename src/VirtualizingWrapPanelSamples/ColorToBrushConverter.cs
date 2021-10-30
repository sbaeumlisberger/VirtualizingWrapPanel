using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VirtualizingWrapPanelSamples
{
    class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Color color ? new SolidColorBrush(color) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as SolidColorBrush)?.Color;
        }
    }
}
