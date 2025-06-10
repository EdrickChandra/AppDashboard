using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.Collections;
using System.Linq;

namespace Surveying.Helpers
{
    // Enhanced converter that works with actual pagination - FINAL VERSION
    public class PaginatedRowNumberConverter : IValueConverter
    {
        private static int _currentPageIndex = 0;
        private static int _pageSize = 10;
        private static readonly object _lock = new object();

        public static void UpdatePageInfo(int pageIndex, int pageSize)
        {
            lock (_lock)
            {
                _currentPageIndex = pageIndex;
                _pageSize = pageSize;
                System.Diagnostics.Debug.WriteLine($"PaginatedRowNumberConverter: Updated page info - PageIndex={pageIndex}, PageSize={pageSize}");
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                {
                    return "1";
                }

                // CRITICAL FIX: Handle null parameter (PagedSource) gracefully
                if (parameter == null)
                {
                    System.Diagnostics.Debug.WriteLine("PaginatedRowNumberConverter: parameter is null - page is changing");
                    return "1";
                }

                if (!(parameter is IList pagedSource))
                {
                    System.Diagnostics.Debug.WriteLine("PaginatedRowNumberConverter: parameter is not IList");
                    return "1";
                }

                if (pagedSource.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("PaginatedRowNumberConverter: pagedSource is empty - page is changing");
                    return "1";
                }

                // Find the index of the current item in the current page
                int indexInPage = -1;

                var currentItem = value;

                // Try to get ContNumber property safely
                var contNumberProperty = currentItem.GetType().GetProperty("ContNumber");
                if (contNumberProperty == null)
                {
                    // Fallback: try to use the index in the collection
                    for (int i = 0; i < pagedSource.Count; i++)
                    {
                        if (pagedSource[i] != null && pagedSource[i] == currentItem)
                        {
                            indexInPage = i;
                            break;
                        }
                    }
                }
                else
                {
                    var currentContNumber = contNumberProperty.GetValue(currentItem)?.ToString();
                    if (!string.IsNullOrEmpty(currentContNumber))
                    {
                        for (int i = 0; i < pagedSource.Count; i++)
                        {
                            var pageItem = pagedSource[i];
                            if (pageItem == null) continue;

                            var pageContNumberProperty = pageItem.GetType().GetProperty("ContNumber");
                            if (pageContNumberProperty == null) continue;

                            var pageContNumber = pageContNumberProperty.GetValue(pageItem)?.ToString();
                            if (string.IsNullOrEmpty(pageContNumber)) continue;

                            if (pageContNumber == currentContNumber)
                            {
                                indexInPage = i;
                                break;
                            }
                        }
                    }
                }

                if (indexInPage >= 0)
                {
                    int actualRowNumber;
                    lock (_lock)
                    {
                        // Calculate actual row number across all pages
                        actualRowNumber = (_currentPageIndex * _pageSize) + indexInPage + 1;
                    }

                    return actualRowNumber.ToString();
                }

                // Fallback: if we can't find the item, just show 1
                return "1";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PaginatedRowNumberConverter: {ex.Message}");
                return "1";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}