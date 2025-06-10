using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using System.Collections;

namespace Surveying.Helpers
{
    public class SimpleRowNumberConverter : IValueConverter
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
                System.Diagnostics.Debug.WriteLine($"SimpleRowNumberConverter: Updated page info - PageIndex={pageIndex}, PageSize={pageSize}");
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                {
                    System.Diagnostics.Debug.WriteLine("SimpleRowNumberConverter: value is null");
                    return "1";
                }

                if (parameter == null)
                {
                    System.Diagnostics.Debug.WriteLine("SimpleRowNumberConverter: parameter (PagedSource) is null - DataPager not ready yet");
                    return "1";
                }

                if (!(parameter is IList pagedSource))
                {
                    System.Diagnostics.Debug.WriteLine($"SimpleRowNumberConverter: parameter is not IList, type: {parameter.GetType().Name}");
                    return "1";
                }

                if (pagedSource.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("SimpleRowNumberConverter: pagedSource is empty");
                    return "1";
                }

                // Use IndexOf instead of ReferenceEquals - much more reliable
                int indexInPage = pagedSource.IndexOf(value);

                if (indexInPage >= 0)
                {
                    int actualRowNumber;
                    lock (_lock)
                    {
                        // Calculate actual row number: (pageIndex * pageSize) + indexInPage + 1
                        actualRowNumber = (_currentPageIndex * _pageSize) + indexInPage + 1;
                    }

                    System.Diagnostics.Debug.WriteLine($"SimpleRowNumberConverter: SUCCESS - Item at index {indexInPage} = row number {actualRowNumber} (Page {_currentPageIndex}, PageSize {_pageSize})");
                    return actualRowNumber.ToString();
                }

                System.Diagnostics.Debug.WriteLine($"SimpleRowNumberConverter: IndexOf returned -1 for item of type {value.GetType().Name}");
                return "1";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SimpleRowNumberConverter: {ex.Message}");
                return "1";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}