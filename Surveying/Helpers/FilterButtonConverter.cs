using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Surveying.Helpers
{
    public class FilterButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasActiveFilters)
            {
                return hasActiveFilters ? "#FF6B35" : "#007BFF"; // Orange when active, blue when inactive
            }
            return "#007BFF";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}