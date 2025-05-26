using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Surveying.Helpers
{
    // Converter to check if count is zero and return true for visibility
    public class ZeroToVisibilityConverter : IValueConverter
    {
        private static ZeroToVisibilityConverter _instance;
        public static ZeroToVisibilityConverter Instance => _instance ??= new ZeroToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Add Instance property to InverseBoolConverter for easier XAML usage
    public partial class InverseBoolConverter
    {
        private static InverseBoolConverter _instance;
        public static InverseBoolConverter Instance => _instance ??= new InverseBoolConverter();
    }
}