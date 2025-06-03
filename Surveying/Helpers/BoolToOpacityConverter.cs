using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Surveying.Helpers
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1.0 : 0.3; // Full opacity (1.0) if true, 30% opacity (0.3) if false
            }
            return 1.0; // Default to full opacity if value is not a boolean
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack not supported for BoolToOpacityConverter");
        }
    }
}