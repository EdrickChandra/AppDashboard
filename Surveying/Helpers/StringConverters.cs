using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Surveying.Helpers
{
    // Converter to check if string is not empty and return true for visibility
    public class StringIsNotEmptyConverter : IValueConverter
    {
        private static StringIsNotEmptyConverter _instance;
        public static StringIsNotEmptyConverter Instance => _instance ??= new StringIsNotEmptyConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return !string.IsNullOrWhiteSpace(stringValue);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter to check if string is empty and return true for visibility
    public class StringIsEmptyConverter : IValueConverter
    {
        private static StringIsEmptyConverter _instance;
        public static StringIsEmptyConverter Instance => _instance ??= new StringIsEmptyConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return string.IsNullOrWhiteSpace(stringValue);
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}