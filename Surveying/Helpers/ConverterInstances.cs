using Surveying.Models;

namespace Surveying.Helpers
{
    /// <summary>
    /// STATIC CONVERTER INSTANCES
    /// Provides easy access to commonly used converter configurations
    /// Reduces XAML code and ensures consistency
    /// </summary>
    public static class Converters
    {
        // ===== BOOLEAN CONVERTERS =====
        public static UniversalBoolConverter BoolDirect { get; } = new() { ConversionType = BoolConversionType.Direct };
        public static UniversalBoolConverter BoolInverse { get; } = new() { ConversionType = BoolConversionType.Inverse };
        public static UniversalBoolConverter StringNotEmpty { get; } = new() { ConversionType = BoolConversionType.StringNotEmpty };
        public static UniversalBoolConverter StringEmpty { get; } = new() { ConversionType = BoolConversionType.StringEmpty };
        public static UniversalBoolConverter ZeroToTrue { get; } = new() { ConversionType = BoolConversionType.ZeroToTrue };
        public static UniversalBoolConverter GreaterThanOne { get; } = new() { ConversionType = BoolConversionType.GreaterThanOne };

        // ===== NUMERIC CONVERTERS =====
        public static UniversalNumericConverter CountDirect { get; } = new() { ConversionType = NumericConversionType.Direct };
        public static UniversalNumericConverter CountIsZero { get; } = new() { ConversionType = NumericConversionType.IsZero };
        public static UniversalNumericConverter CountNotZero { get; } = new() { ConversionType = NumericConversionType.IsNotZero };
        public static UniversalNumericConverter CountGreaterThanOne { get; } = new() { ConversionType = NumericConversionType.GreaterThan, ComparisonValue = 1 };
        public static UniversalNumericConverter PhotoCount { get; } = new() { ConversionType = NumericConversionType.PhotoCount };

        // ===== STATUS CONVERTERS =====
        public static UniversalStatusConverter StatusText { get; } = new() { ConversionType = StatusConversionType.Text };
        public static UniversalStatusConverter StatusColor { get; } = new() { ConversionType = StatusConversionType.Color };
        public static UniversalStatusConverter StatusColorHex { get; } = new() { ConversionType = StatusConversionType.ColorHex };
        public static UniversalStatusConverter StatusSymbol { get; } = new() { ConversionType = StatusConversionType.Symbol };

        // ===== ACTIVITY CONVERTERS =====
        public static UniversalActivityConverter ActivityText { get; } = new() { ConversionType = ActivityConversionType.Text };
        public static UniversalActivityConverter ActivityShowEndDate { get; } = new() { ConversionType = ActivityConversionType.ShowEndDate };
        public static UniversalActivityConverter ActivityPageTitle { get; } = new() { ConversionType = ActivityConversionType.PageTitle };
        public static UniversalActivityConverter ActivitySubmitButton { get; } = new() { ConversionType = ActivityConversionType.SubmitButtonText };
        public static UniversalActivityConverter ActivityMaxPhotos { get; } = new() { ConversionType = ActivityConversionType.MaxPhotos };

        // ===== OTHER CONVERTERS =====
        public static UniversalOpacityConverter OpacityDefault { get; } = new() { EnabledOpacity = 1.0, DisabledOpacity = 0.3 };
        public static UniversalOpacityConverter OpacitySubtle { get; } = new() { EnabledOpacity = 1.0, DisabledOpacity = 0.6 };

        public static UniversalFilterConverter FilterButton { get; } = new() { ConversionType = FilterConversionType.ButtonColor };
        public static UniversalFilterConverter FilterText { get; } = new() { ConversionType = FilterConversionType.ButtonText };

        public static ActivityTypeVisibilityConverter ActivityTypeToCleaningVisibility { get; } =
        new() { TargetActivityType = ActivityType.Cleaning };

        public static ActivityTypeVisibilityConverter ActivityTypeToRepairVisibility { get; } =
            new() { TargetActivityType = ActivityType.Repair };

        public static ActivityTypeVisibilityConverter ActivityTypeToPeriodicVisibility { get; } =
            new() { TargetActivityType = ActivityType.Periodic };

        public static ActivityTypeVisibilityConverter ActivityTypeToSurveyVisibility { get; } =
            new() { TargetActivityType = ActivityType.Survey };
    }
}