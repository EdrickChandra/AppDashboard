using System.Globalization;
using Microsoft.Maui.Controls;
using Surveying.ViewModels;

namespace Surveying.Helpers
{
    public class PhotoButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasPhoto)
            {
                return hasPhoto ? "View" : "Photo";
            }
            return "Photo";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter for photo button color
    public class PhotoButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasPhoto)
            {
                return hasPhoto ? "#28A745" : "#6C757D"; // Green if has photo, Gray if not
            }
            return "#6C757D";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter to get photo count for a specific segment
    public class PhotoCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PhotoUploadViewModel photoUploader && parameter is string segmentName)
            {
                var count = photoUploader.GetPhotoCountForSegment(segmentName);
                return count > 0 ? $"✓ {count} photo(s)" : "❌ No photos";
            }
            return "❌ No photos";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}