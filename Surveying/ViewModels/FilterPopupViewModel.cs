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

        // NEW: Cleaning requirements filtering
        [ObservableProperty]
        private ObservableCollection<CleaningRequirementFilterItem> availableCleaningRequirements = new();

        [ObservableProperty]
        private ObservableCollection<CleaningRequirementFilterItem> filteredCleaningRequirements = new();

        [ObservableProperty]
        private string cleaningRequirementsSearchText = "";

        // Status filtering
        [ObservableProperty]
        private bool showPendingStatus = true;

        [ObservableProperty]
        private bool showApprovedStatus = true;

        // Display options
        [ObservableProperty]
        private bool showAllCleaningContainers = true;

        // UI properties
        [ObservableProperty]
        private string activeFiltersText = "No filters applied";

        // Dynamic cleaning criteria display
        [ObservableProperty]
        private string cleaningCriteriaText = "YXT • 1101 • APNN"; // Default fallback

        public FilterPopupViewModel(List<CustomerFilterItem> customers,
                                  List<CleaningRequirementFilterItem> cleaningRequirements = null,
                                  FilterResult currentFilters = null,
                                  string cleaningCriteria = null)
        {
            // Set dynamic cleaning criteria text
            if (!string.IsNullOrEmpty(cleaningCriteria))
            {
                CleaningCriteriaText = cleaningCriteria;
            }

            // Initialize available customers
            foreach (var customer in customers)
            {
                AvailableCustomers.Add(customer);
            }

            // Initialize available cleaning requirements
            if (cleaningRequirements != null)
            {
                foreach (var requirement in cleaningRequirements)
                {
                    AvailableCleaningRequirements.Add(requirement);
                }
            }

            // Apply current filters if provided
            if (currentFilters != null)
            {
                ApplyCurrentFilters(currentFilters);
            }

            // Initialize filtered lists
            UpdateFilteredCustomers();
            UpdateFilteredCleaningRequirements();
            UpdateActiveFiltersText();
        }

        partial void OnCustomerSearchTextChanged(string value)
        {
            UpdateFilteredCustomers();
        }

        partial void OnCleaningRequirementsSearchTextChanged(string value)
        {
            UpdateFilteredCleaningRequirements();
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

        private void UpdateFilteredCleaningRequirements()
        {
            FilteredCleaningRequirements.Clear();

            var filtered = string.IsNullOrWhiteSpace(CleaningRequirementsSearchText)
                ? AvailableCleaningRequirements
                : AvailableCleaningRequirements.Where(c =>
                    c.CleaningCode.ToLower().Contains(CleaningRequirementsSearchText.ToLower()) ||
                    c.Description.ToLower().Contains(CleaningRequirementsSearchText.ToLower()));

            foreach (var requirement in filtered.OrderBy(c => c.CleaningCode))
            {
                FilteredCleaningRequirements.Add(requirement);
            }
        }

        private void ApplyCurrentFilters(FilterResult filters)
        {
            // Apply customer selections
            foreach (var customer in AvailableCustomers)
            {
                customer.IsSelected = filters.SelectedCustomers.Contains(customer.CustomerCode);
            }

            // Apply cleaning requirement selections
            foreach (var requirement in AvailableCleaningRequirements)
            {
                requirement.IsSelected = filters.SelectedCleaningRequirements.Contains(requirement.CleaningCode);
            }

            // Apply status selections
            ShowPendingStatus = filters.ShowPendingStatus;
            ShowApprovedStatus = filters.ShowApprovedStatus;

            // Apply display options
            ShowAllCleaningContainers = filters.ShowAllCleaningContainers;
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
        void SelectAllCleaningRequirements()
        {
            foreach (var requirement in FilteredCleaningRequirements)
            {
                requirement.IsSelected = true;
            }
            UpdateActiveFiltersText();
        }

        [RelayCommand]
        void ClearAllCleaningRequirements()
        {
            foreach (var requirement in AvailableCleaningRequirements)
            {
                requirement.IsSelected = false;
            }
            UpdateActiveFiltersText();
        }

        [RelayCommand]
        void ToggleCleaningRequirement(CleaningRequirementFilterItem requirement)
        {
            if (requirement != null)
            {
                requirement.IsSelected = !requirement.IsSelected;
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

            // Clear cleaning requirement selections
            foreach (var requirement in AvailableCleaningRequirements)
            {
                requirement.IsSelected = false;
            }

            // Reset status filters
            ShowPendingStatus = true;
            ShowApprovedStatus = true;

            // Reset display options
            ShowAllCleaningContainers = true;

            UpdateActiveFiltersText();
        }

        [RelayCommand]
        void ApplyFilters()
        {
            var result = new FilterResult
            {
                SelectedCustomers = AvailableCustomers.Where(c => c.IsSelected).Select(c => c.CustomerCode).ToList(),
                SelectedCleaningRequirements = AvailableCleaningRequirements.Where(c => c.IsSelected).Select(c => c.CleaningCode).ToList(),
                ShowPendingStatus = ShowPendingStatus,
                ShowApprovedStatus = ShowApprovedStatus,
                ShowAllCleaningContainers = ShowAllCleaningContainers
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

            // Count cleaning requirement filters
            var selectedRequirements = AvailableCleaningRequirements.Count(c => c.IsSelected);
            var totalRequirements = AvailableCleaningRequirements.Count;

            if (selectedRequirements > 0 && selectedRequirements < totalRequirements)
            {
                activeCount++;
                filterParts.Add($"{selectedRequirements} cleaning code(s)");
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

            // Count display options
            if (!ShowAllCleaningContainers)
            {
                activeCount++;
                filterParts.Add("display");
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
        partial void OnShowAllCleaningContainersChanged(bool value) => UpdateActiveFiltersText();
    }

    // Customer filter item model (existing)
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

    // NEW: Cleaning requirement filter item model
    public partial class CleaningRequirementFilterItem : ObservableObject
    {
        [ObservableProperty]
        private string cleaningCode;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private int containerCount;

        [ObservableProperty]
        private bool isSelected;

        public CleaningRequirementFilterItem(string cleaningCode, string description, int containerCount)
        {
            CleaningCode = cleaningCode;
            Description = description;
            ContainerCount = containerCount;
            IsSelected = false;
        }
    }

    // Updated filter result model
    public class FilterResult
    {
        public List<string> SelectedCustomers { get; set; } = new();
        public List<string> SelectedCleaningRequirements { get; set; } = new(); // NEW
        public bool ShowPendingStatus { get; set; } = true;
        public bool ShowApprovedStatus { get; set; } = true;
        public bool ShowAllCleaningContainers { get; set; } = true; // NEW

        public bool HasAnyFilters()
        {
            return SelectedCustomers.Any() ||
                   SelectedCleaningRequirements.Any() ||
                   !ShowPendingStatus ||
                   !ShowApprovedStatus ||
                   !ShowAllCleaningContainers;
        }

        public int GetActiveFilterCount()
        {
            int count = 0;
            if (SelectedCustomers.Any()) count++;
            if (SelectedCleaningRequirements.Any()) count++;
            if (!ShowPendingStatus || !ShowApprovedStatus) count++;
            if (!ShowAllCleaningContainers) count++;
            return count;
        }
    }
}