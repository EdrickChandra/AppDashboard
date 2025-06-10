using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.Collections;

namespace Surveying.Helpers
{
    public class StaticRowNumberConverter : IValueConverter
    {
        private static int _currentPageIndex = 0;
        private static int _pageSize = 10;

        public static void UpdatePageInfo(int pageIndex, int pageSize)
        {
            _currentPageIndex = pageIndex;
            _pageSize = pageSize;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter is IList pagedSource)
            {
                // Find the index of the current item in the current page (0-based)
                int indexInPage = pagedSource.IndexOf(value);

                if (indexInPage >= 0)
                {
                    // Calculate static row number based on page info
                    // Page 0 (first page): items 1-10 (indexInPage 0-9 → row numbers 1-10)
                    // Page 1 (second page): items 11-20 (indexInPage 0-9 → row numbers 11-20)
                    int staticRowNumber = (_currentPageIndex * _pageSize) + indexInPage + 1;
                    return staticRowNumber.ToString();
                }
            }

            // Fallback: if we can't find the item, just use a simple counter
            return "1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}