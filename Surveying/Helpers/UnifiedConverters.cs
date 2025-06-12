using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Surveying.Models;
using Surveying.ViewModels;

namespace Surveying.Helpers
{
    // ===== UNIVERSAL BOOLEAN CONVERTER =====
    /// <summary>
    /// Universal boolean converter that can handle multiple scenarios
    /// Replaces: InverseBoolConverter, StringToBoolConverter, StringIsNotEmptyConverter, StringIsEmptyConverter
    /// </summary>
    public class UniversalBoolConverter : IValueConverter
    {
        public BoolConversionType ConversionType { get; set; } = BoolConversionType.Direct;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConversionType switch
            {
                BoolConversionType.Direct => ConvertDirect(value),
                BoolConversionType.Inverse => ConvertInverse(value),
                BoolConversionType.StringNotEmpty => ConvertStringNotEmpty(value),
                BoolConversionType.StringEmpty => ConvertStringEmpty(value),
                BoolConversionType.ZeroToTrue => ConvertZeroToTrue(value),
                BoolConversionType.GreaterThanOne => ConvertGreaterThanOne(value),
                _ => false
            };
        }

        private bool ConvertDirect(object value) => value is bool boolValue && boolValue;
        private bool ConvertInverse(object value) => value is bool boolValue && !boolValue;
        private bool ConvertStringNotEmpty(object value) => !string.IsNullOrWhiteSpace(value?.ToString());
        private bool ConvertStringEmpty(object value) => string.IsNullOrWhiteSpace(value?.ToString());
        private bool ConvertZeroToTrue(object value) => value is int intValue && intValue == 0;
        private bool ConvertGreaterThanOne(object value) => value is int intValue && intValue > 1;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ConversionType == BoolConversionType.Inverse)
                return value is bool boolValue && !boolValue;

            throw new NotImplementedException($"ConvertBack not supported for {ConversionType}");
        }
    }

    public enum BoolConversionType
    {
        Direct,
        Inverse,
        StringNotEmpty,
        StringEmpty,
        ZeroToTrue,
        GreaterThanOne
    }

    // ===== UNIVERSAL NUMERIC CONVERTER =====
    /// <summary>
    /// Universal numeric converter for counts, comparisons, and calculations
    /// Replaces: ZeroToVisibilityConverter, GreaterThanOneConverter, PhotoCountConverter
    /// </summary>
    public class UniversalNumericConverter : IValueConverter
    {
        public NumericConversionType ConversionType { get; set; } = NumericConversionType.Direct;
        public int ComparisonValue { get; set; } = 1;
        public string Format { get; set; } = "{0}";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var numericValue = GetNumericValue(value, parameter);

            return ConversionType switch
            {
                NumericConversionType.Direct => numericValue,
                NumericConversionType.IsZero => numericValue == 0,
                NumericConversionType.IsNotZero => numericValue != 0,
                NumericConversionType.GreaterThan => numericValue > ComparisonValue,
                NumericConversionType.GreaterThanOrEqual => numericValue >= ComparisonValue,
                NumericConversionType.LessThan => numericValue < ComparisonValue,
                NumericConversionType.PhotoCount => GetPhotoCountText(numericValue),
                NumericConversionType.Formatted => string.Format(Format, numericValue),
                _ => numericValue
            };
        }

        private int GetNumericValue(object value, object parameter)
        {
            // Handle PhotoUploadViewModel photo count
            if (value is PhotoUploadViewModel photoUploader && parameter is string segmentName)
            {
                return photoUploader.GetPhotoCountForSegment(segmentName);
            }

            // Handle ContainerActivityViewModel photo count  
            if (value is ContainerActivityViewModel activityVM && parameter is string segment)
            {
                return activityVM.GetPhotoCountForSegment(segment);
            }

            // Handle direct numeric values
            if (value is int intValue) return intValue;
            if (value is double doubleValue) return (int)doubleValue;
            if (value is float floatValue) return (int)floatValue;

            // Handle collections
            if (value is System.Collections.ICollection collection) return collection.Count;

            return 0;
        }

        private string GetPhotoCountText(int count)
        {
            return count > 0 ? $"✓ {count} photo(s)" : "❌ No photos";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum NumericConversionType
    {
        Direct,
        IsZero,
        IsNotZero,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        PhotoCount,
        Formatted
    }

    // ===== UNIVERSAL STATUS CONVERTER =====
    /// <summary>
    /// Universal status converter for StatusType enum
    /// Replaces: StatusTypeToStringConverter, StatusTypeToColorConverter
    /// </summary>
    public class UniversalStatusConverter : IValueConverter
    {
        public StatusConversionType ConversionType { get; set; } = StatusConversionType.Text;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not StatusType status) return GetDefault();

            return ConversionType switch
            {
                StatusConversionType.Text => GetStatusText(status),
                StatusConversionType.Color => GetStatusColor(status),
                StatusConversionType.ColorHex => GetStatusColorHex(status),
                StatusConversionType.Symbol => GetStatusSymbol(status),
                _ => GetDefault()
            };
        }

        private string GetStatusText(StatusType status) => status switch
        {
            StatusType.NotFilled => "Pending",
            StatusType.OnReview => "Review",
            StatusType.Finished => "Finished",
            StatusType.Rejected => "Rejected",
            _ => "Unknown"
        };

        private Color GetStatusColor(StatusType status) => status switch
        {
            StatusType.NotFilled => Colors.Gray,
            StatusType.OnReview => Colors.Orange,
            StatusType.Finished => Colors.Green,
            StatusType.Rejected => Colors.Red,
            _ => Colors.Gray
        };

        private string GetStatusColorHex(StatusType status) => status switch
        {
            StatusType.NotFilled => "#6C757D",
            StatusType.OnReview => "#FFC107",
            StatusType.Finished => "#28A745",
            StatusType.Rejected => "#DC3545",
            _ => "#6C757D"
        };

        private string GetStatusSymbol(StatusType status) => status switch
        {
            StatusType.NotFilled => "*",
            StatusType.OnReview => "R",
            StatusType.Finished => "A",
            StatusType.Rejected => "X",
            _ => ""
        };

        private object GetDefault() => ConversionType switch
        {
            StatusConversionType.Text => "Unknown",
            StatusConversionType.Color => Colors.Gray,
            StatusConversionType.ColorHex => "#6C757D",
            StatusConversionType.Symbol => "",
            _ => "Unknown"
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum StatusConversionType
    {
        Text,
        Color,
        ColorHex,
        Symbol
    }

    // ===== UNIVERSAL ACTIVITY CONVERTER =====
    /// <summary>
    /// Universal activity converter for ActivityType enum and activity-related properties
    /// Replaces: ActivityTypeToStringConverter, ActivityTypeToEndDateVisibilityConverter
    /// </summary>
    public class UniversalActivityConverter : IValueConverter
    {
        public ActivityConversionType ConversionType { get; set; } = ActivityConversionType.Text;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ActivityType activityType) return GetDefault();

            return ConversionType switch
            {
                ActivityConversionType.Text => GetActivityText(activityType),
                ActivityConversionType.ShowEndDate => GetShowEndDate(activityType),
                ActivityConversionType.PageTitle => GetPageTitle(activityType),
                ActivityConversionType.SubmitButtonText => GetSubmitButtonText(activityType),
                ActivityConversionType.MaxPhotos => GetMaxPhotos(activityType),
                _ => GetDefault()
            };
        }

        private string GetActivityText(ActivityType activityType) => activityType switch
        {
            ActivityType.Cleaning => "Cleaning",
            ActivityType.Repair => "Repair",
            ActivityType.Periodic => "Periodic",
            ActivityType.Survey => "Survey",
            _ => "Unknown"
        };

        private bool GetShowEndDate(ActivityType activityType) =>
            activityType == ActivityType.Cleaning || activityType == ActivityType.Periodic;

        private string GetPageTitle(ActivityType activityType) => activityType switch
        {
            ActivityType.Cleaning => "Enhanced Cleaning",
            ActivityType.Repair => "Repair Details",
            ActivityType.Periodic => "Periodic Maintenance",
            ActivityType.Survey => "Survey Review",
            _ => "Activity"
        };

        private string GetSubmitButtonText(ActivityType activityType) =>
            $"Submit {GetActivityText(activityType)}";

        private int GetMaxPhotos(ActivityType activityType) => activityType switch
        {
            ActivityType.Cleaning => 1, // One per segment
            ActivityType.Repair => 4,
            ActivityType.Periodic => 1,
            ActivityType.Survey => 4,
            _ => 4
        };

        private object GetDefault() => ConversionType switch
        {
            ActivityConversionType.Text => "Unknown",
            ActivityConversionType.ShowEndDate => false,
            ActivityConversionType.PageTitle => "Activity",
            ActivityConversionType.SubmitButtonText => "Submit",
            ActivityConversionType.MaxPhotos => 4,
            _ => "Unknown"
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum ActivityConversionType
    {
        Text,
        ShowEndDate,
        PageTitle,
        SubmitButtonText,
        MaxPhotos
    }

    // ===== UNIVERSAL OPACITY CONVERTER =====
    /// <summary>
    /// Universal opacity converter for enabling/disabling UI elements
    /// Replaces: BoolToOpacityConverter
    /// </summary>
    public class UniversalOpacityConverter : IValueConverter
    {
        public double EnabledOpacity { get; set; } = 1.0;
        public double DisabledOpacity { get; set; } = 0.3;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? EnabledOpacity : DisabledOpacity;
            }
            return EnabledOpacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ===== UNIVERSAL FILTER CONVERTER =====
    /// <summary>
    /// Universal filter converter for filter button states
    /// Replaces: FilterButtonColorConverter
    /// </summary>
    public class UniversalFilterConverter : IValueConverter
    {
        public FilterConversionType ConversionType { get; set; } = FilterConversionType.ButtonColor;
        public string ActiveColor { get; set; } = "#FF6B35";
        public string InactiveColor { get; set; } = "#007BFF";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hasActiveFilters = value is bool boolValue && boolValue;

            return ConversionType switch
            {
                FilterConversionType.ButtonColor => hasActiveFilters ? ActiveColor : InactiveColor,
                FilterConversionType.ButtonText => hasActiveFilters ? "Clear Filters" : "Filter",
                FilterConversionType.IsActive => hasActiveFilters,
                _ => InactiveColor
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum FilterConversionType
    {
        ButtonColor,
        ButtonText,
        IsActive
    }
}