using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace Surveying.ViewModels
{
    public partial class FilterPopupViewModel : ObservableObject
    {
        // Events
        public event EventHandler CloseRequested;
        public event EventHandler<FilterResult> FiltersApplied;

        // Customer filtering
        [ObservableProperty]
        private ObservableCollection<CustomerFilterItem> availableCustomers = new();

        [ObservableProperty]
        private ObservableCollection<CustomerFilterItem> filteredAvailableCustomers = new();

        [ObservableProperty]
        private string customerSearchText = "";

        // Status filtering
        [ObservableProperty]
        private bool showPendingStatus = true;

        [ObservableProperty]
        private bool showApprovedStatus = true;

        // Cleaning required filtering
        [ObservableProperty]
        private bool showCleaningRequired = true;

        // UI properties
        [ObservableProperty]
        private string activeFiltersText = "No filters applied";

        public FilterPopupViewModel(List<CustomerFilterItem> customers, FilterResult currentFilters = null)
        {
            // Initialize available customers
            foreach (var customer in customers)
            {
                AvailableCustomers.Add(customer);
            }

            // Apply current filters if provided
            if (currentFilters != null)
            {
                ApplyCurrentFilters(currentFilters);
            }

            // Initialize filtered list
            UpdateFilteredCustomers();
            UpdateActiveFiltersText();
        }

        partial void OnCustomerSearchTextChanged(string value)
        {
            UpdateFilteredCustomers();
        }

        private void UpdateFilteredCustomers()
        {
            FilteredAvailableCustomers.Clear();

            var filtered = string.IsNullOrWhiteSpace(CustomerSearchText)
                ? AvailableCustomers
                : AvailableCustomers.Where(c => c.CustomerCode.ToLower().Contains(CustomerSearchText.ToLower()));

            foreach (var customer in filtered.OrderBy(c => c.CustomerCode))
            {
                FilteredAvailableCustomers.Add(customer);
            }
        }

        private void ApplyCurrentFilters(FilterResult filters)
        {
            // Apply customer selections
            foreach (var customer in AvailableCustomers)
            {
                customer.IsSelected = filters.SelectedCustomers.Contains(customer.CustomerCode);
            }

            // Apply status selections
            ShowPendingStatus = filters.ShowPendingStatus;
            ShowApprovedStatus = filters.ShowApprovedStatus;

            // Apply cleaning required selection
            ShowCleaningRequired = filters.ShowCleaningRequired;
        }

        [RelayCommand]
        void SelectAllCustomers()
        {
            foreach (var customer in FilteredAvailableCustomers)
            {
                customer.IsSelected = true;
            }
            UpdateActiveFiltersText();
        }

        [RelayCommand]
        void ClearAllCustomers()
        {
            foreach (var customer in AvailableCustomers)
            {
                customer.IsSelected = false;
            }
            UpdateActiveFiltersText();
        }

        [RelayCommand]
        void ToggleCustomer(CustomerFilterItem customer)
        {
            if (customer != null)
            {
                customer.IsSelected = !customer.IsSelected;
                UpdateActiveFiltersText();
            }
        }

        [RelayCommand]
        void ClearAllFilters()
        {
            // Clear customer selections
            foreach (var customer in AvailableCustomers)
            {
                customer.IsSelected = false;
            }

            // Reset status filters
            ShowPendingStatus = true;
            ShowApprovedStatus = true;

            // Reset cleaning required filter
            ShowCleaningRequired = true;

            UpdateActiveFiltersText();
        }

        [RelayCommand]
        void ApplyFilters()
        {
            var result = new FilterResult
            {
                SelectedCustomers = AvailableCustomers.Where(c => c.IsSelected).Select(c => c.CustomerCode).ToList(),
                ShowPendingStatus = ShowPendingStatus,
                ShowApprovedStatus = ShowApprovedStatus,
                ShowCleaningRequired = ShowCleaningRequired
            };

            FiltersApplied?.Invoke(this, result);
        }

        private void UpdateActiveFiltersText()
        {
            var activeCount = 0;
            var filterParts = new List<string>();

            // Count customer filters
            var selectedCustomers = AvailableCustomers.Count(c => c.IsSelected);
            var totalCustomers = AvailableCustomers.Count;

            if (selectedCustomers > 0 && selectedCustomers < totalCustomers)
            {
                activeCount++;
                filterParts.Add($"{selectedCustomers} customer(s)");
            }

            // Count status filters
            var statusCount = 0;
            if (ShowPendingStatus) statusCount++;
            if (ShowApprovedStatus) statusCount++;

            if (statusCount > 0 && statusCount < 2)
            {
                activeCount++;
                filterParts.Add("status");
            }

            // Count cleaning required filter
            if (!ShowCleaningRequired)
            {
                activeCount++;
                filterParts.Add("cleaning");
            }

            if (activeCount == 0)
            {
                ActiveFiltersText = "No filters applied";
            }
            else
            {
                ActiveFiltersText = $"{activeCount} filter(s): {string.Join(", ", filterParts)}";
            }
        }

        // Update active filters text when properties change
        partial void OnShowPendingStatusChanged(bool value) => UpdateActiveFiltersText();
        partial void OnShowApprovedStatusChanged(bool value) => UpdateActiveFiltersText();
        partial void OnShowCleaningRequiredChanged(bool value) => UpdateActiveFiltersText();
    }

    // Customer filter item model
    public partial class CustomerFilterItem : ObservableObject
    {
        [ObservableProperty]
        private string customerCode;

        [ObservableProperty]
        private int containerCount;

        [ObservableProperty]
        private bool isSelected;

        public CustomerFilterItem(string customerCode, int containerCount)
        {
            CustomerCode = customerCode;
            ContainerCount = containerCount;
            IsSelected = false;
        }
    }

    // Filter result model
    public class FilterResult
    {
        public List<string> SelectedCustomers { get; set; } = new();
        public bool ShowPendingStatus { get; set; } = true;
        public bool ShowApprovedStatus { get; set; } = true;
        public bool ShowCleaningRequired { get; set; } = true;

        public bool HasAnyFilters()
        {
            return SelectedCustomers.Any() || !ShowPendingStatus || !ShowApprovedStatus || !ShowCleaningRequired;
        }

        public int GetActiveFilterCount()
        {
            int count = 0;
            if (SelectedCustomers.Any()) count++;
            if (!ShowPendingStatus || !ShowApprovedStatus) count++;
            if (!ShowCleaningRequired) count++;
            return count;
        }
    }
}