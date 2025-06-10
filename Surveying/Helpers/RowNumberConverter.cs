using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.Collections;
using System.Linq;

namespace Surveying.Helpers
{
    public class RowNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value = current data item
            // parameter = PagedSource collection from DataPager

            if (value != null && parameter is IList pagedSource)
            {
                // Find the index of the current item in the current page
                int indexInPage = -1;

                // Use a more reliable way to find the item
                var currentItem = value;
                for (int i = 0; i < pagedSource.Count; i++)
                {
                    var pageItem = pagedSource[i];
                    if (pageItem != null &&
                        pageItem.GetType().GetProperty("ContNumber") != null &&
                        currentItem.GetType().GetProperty("ContNumber") != null)
                    {
                        var pageContNumber = pageItem.GetType().GetProperty("ContNumber").GetValue(pageItem)?.ToString();
                        var currentContNumber = currentItem.GetType().GetProperty("ContNumber").GetValue(currentItem)?.ToString();

                        if (pageContNumber == currentContNumber)
                        {
                            indexInPage = i;
                            break;
                        }
                    }
                }

                if (indexInPage >= 0)
                {
                    // We need to get the current page index and page size to calculate the actual row number
                    // For now, we'll show the index within the current page (1-based)
                    // This will show 1,2,3... for each page
                    return (indexInPage + 1).ToString();
                }
            }

            return "1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Enhanced converter that works with actual pagination
    public class PaginatedRowNumberConverter : IValueConverter
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
                // Find the index of the current item in the current page
                int indexInPage = -1;

                var currentItem = value;
                for (int i = 0; i < pagedSource.Count; i++)
                {
                    var pageItem = pagedSource[i];
                    if (pageItem != null &&
                        pageItem.GetType().GetProperty("ContNumber") != null &&
                        currentItem.GetType().GetProperty("ContNumber") != null)
                    {
                        var pageContNumber = pageItem.GetType().GetProperty("ContNumber").GetValue(pageItem)?.ToString();
                        var currentContNumber = currentItem.GetType().GetProperty("ContNumber").GetValue(currentItem)?.ToString();

                        if (pageContNumber == currentContNumber)
                        {
                            indexInPage = i;
                            break;
                        }
                    }
                }

                if (indexInPage >= 0)
                {
                    // Calculate actual row number across all pages
                    int actualRowNumber = (_currentPageIndex * _pageSize) + indexInPage + 1;
                    return actualRowNumber.ToString();
                }
            }

            return "1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}