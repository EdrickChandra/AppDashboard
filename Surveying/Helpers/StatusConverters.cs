using Surveying.Models;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace Surveying.Helpers
{
    // Converts StatusType enum to user-friendly string
    public class StatusTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusType status)
            {
                switch (status)
                {
                    case StatusType.NotFilled:
                        return "Pending";
                    case StatusType.OnReview:
                        return "Review";
                    case StatusType.Finished:
                        return "Finished";
                    case StatusType.Rejected:
                        return "Rejected";
                    default:
                        return "Unknown";
                }
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converts StatusType enum to background color
    public class StatusTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusType status)
            {
                switch (status)
                {
                    case StatusType.NotFilled:
                        return Colors.Gray;
                    case StatusType.OnReview:
                        return Colors.Orange;
                    case StatusType.Finished:
                        return Colors.Green;
                    case StatusType.Rejected:
                        return Colors.Red;
                    default:
                        return Colors.Gray;
                }
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}