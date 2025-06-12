using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Services;
using Surveying.Views;
using System.Collections.ObjectModel;
using System.Linq;

namespace Surveying.ViewModels
{
    // ===== MIGRATED: CleaningListViewModel using simplified models =====
    public partial class CleaningListViewModel : BaseViewModel
    {
        private readonly IContainerApiService _containerApiService;
        private readonly ICleaningCriteriaService _cleaningCriteriaService;

        // ===== SIMPLIFIED COLLECTIONS (single Container type!) =====
        // OLD: ObservableCollection<ContainerWithRepairCodesModelExtended> cleaningList
        // NEW: Single Container class for everything
        [ObservableProperty]
        private ObservableCollection<Container> cleaningList;

        [ObservableProperty]
        private ObservableCollection<Container> filteredCleaningList;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string errorMessage = "";

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private int totalContainers;

        [ObservableProperty]
        private string searchText = "";

        // ===== SIMPLIFIED FILTER PROPERTIES (no more complex FilterResult!) =====
        // OLD: FilterResult currentFilters = new FilterResult();
        // NEW: Direct properties - much simpler!
        [ObservableProperty]
        private List<string> selectedCustomers = new();

        [ObservableProperty]
        private List<string> selectedCleaningRequirements = new();

        [ObservableProperty]
        private bool showPendingStatus = true;

        [ObservableProperty]
        private bool showApprovedStatus = true;

        [ObservableProperty]
        private bool showAllCleaningContainers = true;

        [ObservableProperty]
        private string filterButtonText = "Filter";

        [ObservableProperty]
        private bool hasActiveFilters = false;

        // ===== CLEANING CRITERIA - UNCHANGED =====
        [ObservableProperty]
        private string cleaningCriteriaText = "Loading...";

        [ObservableProperty]
        private List<string> cleaningCriteriaList = new List<string>();

        [ObservableProperty]
        private string footerCriteriaText = "Loading cleaning criteria...";

        // ===== CONSTRUCTORS =====
        public CleaningListViewModel() : this(new ContainerApiService(), new CleaningCriteriaService())
        {
        }

        public CleaningListViewModel(IContainerApiService containerApiService, ICleaningCriteriaService cleaningCriteriaService)
        {
            _containerApiService = containerApiService;
            _cleaningCriteriaService = cleaningCriteriaService;
            Title = "Cleaning List";

            // SIMPLIFIED: Only need two collections instead of multiple types
            CleaningList = new ObservableCollection<Container>();
            FilteredCleaningList = new ObservableCollection<Container>();

            // Load cleaning criteria and data
            _ = InitializeAsync();
        }

        // ===== INITIALIZATION - SIMPLIFIED =====
        private async Task InitializeAsync()
        {
            await LoadCleaningCriteriaAsync();
            await LoadCleaningDataFromApiAsync();
        }

        private async Task LoadCleaningCriteriaAsync()
        {
            try
            {
                var criteria = await _cleaningCriteriaService.GetCleaningCriteriaAsync();
                var formattedCriteria = _cleaningCriteriaService.GetFormattedCleaningCriteria();
                var criteriaList = _cleaningCriteriaService.GetFormattedCriteriaList();

                CleaningCriteriaText = formattedCriteria;
                CleaningCriteriaList = criteriaList;
                FooterCriteriaText = $"Filtered by cleaning criteria: {formattedCriteria}";

                System.Diagnostics.Debug.WriteLine($"Loaded cleaning criteria: {formattedCriteria}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cleaning criteria: {ex.Message}");
                CleaningCriteriaText = "YXT • 1101 • APNN"; // Fallback
                CleaningCriteriaList = new List<string> { "YXT • 1101 • APNN" };
                FooterCriteriaText = "Filtered by cleaning criteria: YXT • 1101 • APNN";
            }
        }

        // ===== SEARCH/FILTER - SIMPLIFIED =====
        partial void OnSearchTextChanged(string value)
        {
            PerformFiltering();
        }

        [RelayCommand]
        void Search()
        {
            PerformFiltering();
        }

        [RelayCommand]
        async Task ShowFilterPopup()
        {
            try
            {
                // SIMPLIFIED: Direct operations on Container model (no wrapper classes!)
                var customerCounts = CleaningList
                    .GroupBy(c => c.CustomerCode)
                    .Select(g => new CustomerFilterItem(g.Key, g.Count()))
                    .OrderBy(c => c.CustomerCode)
                    .ToList();

                var cleaningRequirementCounts = CleaningList
                    .Where(c => !string.IsNullOrEmpty(c.CleaningRequirementsText) && c.CleaningRequirementsText != "No requirements")
                    .SelectMany(c => c.CleaningRequirementsText.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    .GroupBy(req => req.Trim())
                    .Select(g => new CleaningRequirementFilterItem(
                        g.Key,
                        GetCleaningRequirementDescription(g.Key),
                        g.Count()))
                    .OrderBy(c => c.CleaningCode)
                    .ToList();

                // SIMPLIFIED: Create simple object instead of complex FilterResult
                var currentFilters = new
                {
                    SelectedCustomers = SelectedCustomers,
                    SelectedCleaningRequirements = SelectedCleaningRequirements,
                    ShowPendingStatus = ShowPendingStatus,
                    ShowApprovedStatus = ShowApprovedStatus,
                    ShowAllCleaningContainers = ShowAllCleaningContainers
                };

                var filterViewModel = new FilterPopupViewModel(customerCounts, cleaningRequirementCounts, currentFilters, CleaningCriteriaText);
                var filterPopup = new FilterPopup(filterViewModel);

                filterPopup.TaskCompletionSource = new TaskCompletionSource<FilterResult>();
                await Application.Current.MainPage.Navigation.PushModalAsync(filterPopup);

                var result = await filterPopup.TaskCompletionSource.Task;

                if (result != null)
                {
                    // SIMPLIFIED: Direct property assignment instead of complex object mapping
                    SelectedCustomers = result.SelectedCustomers;
                    SelectedCleaningRequirements = result.SelectedCleaningRequirements;
                    ShowPendingStatus = result.ShowPendingStatus;
                    ShowApprovedStatus = result.ShowApprovedStatus;
                    ShowAllCleaningContainers = result.ShowAllCleaningContainers;

                    UpdateFilterButtonText();
                    PerformFiltering();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing filter popup: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to open filter options.", "OK");
            }
        }

        private string GetCleaningRequirementDescription(string cleaningCode)
        {
            var descriptions = new Dictionary<string, string>
            {
                { "YXT • 1101 • APNN", "Exterior Water Wash - Tank Shell" },
                { "YNT • 1112 • APNN", "Interior Chemical Wash - Tank Interior" },
                { "YXT • 1112 • APNN", "Exterior Chemical Wash - Tank Shell" },
                { "YNT • 1101 • APNN", "Interior Water Wash - Tank Interior" }
            };

            return descriptions.ContainsKey(cleaningCode) ? descriptions[cleaningCode] : "Standard Cleaning Procedure";
        }

        private void UpdateFilterButtonText()
        {
            // SIMPLIFIED: Direct counting instead of complex FilterResult.GetActiveFilterCount()
            var activeCount = 0;
            if (SelectedCustomers.Any()) activeCount++;
            if (SelectedCleaningRequirements.Any()) activeCount++;
            if (!ShowPendingStatus || !ShowApprovedStatus) activeCount++;
            if (!ShowAllCleaningContainers) activeCount++;

            HasActiveFilters = activeCount > 0;
            FilterButtonText = activeCount == 0 ? "Filter" : $"Filter ({activeCount})";
        }

        private void PerformFiltering()
        {
            if (CleaningList == null)
                return;

            var filtered = CleaningList.AsEnumerable();

            // SIMPLIFIED: Direct property access instead of wrapper properties
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchTerm = SearchText.ToLower().Trim();
                filtered = filtered.Where(container =>
                    container.ContNumber.ToLower().Contains(searchTerm) ||
                    container.CustomerCode.ToLower().Contains(searchTerm) ||
                    container.Id.ToString().Contains(searchTerm) ||
                    (container.CleaningRequirementsText?.ToLower().Contains(searchTerm) ?? false));
            }

            if (SelectedCustomers.Any())
            {
                filtered = filtered.Where(container =>
                    SelectedCustomers.Contains(container.CustomerCode));
            }

            if (SelectedCleaningRequirements.Any())
            {
                filtered = filtered.Where(container =>
                {
                    if (string.IsNullOrEmpty(container.CleaningRequirementsText))
                        return false;

                    var containerRequirements = container.CleaningRequirementsText
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(req => req.Trim());

                    return SelectedCleaningRequirements
                        .Any(selectedReq => containerRequirements.Contains(selectedReq));
                });
            }

            if (!ShowPendingStatus || !ShowApprovedStatus)
            {
                filtered = filtered.Where(container =>
                {
                    if (ShowPendingStatus && !container.IsRepairApproved)
                        return true;
                    if (ShowApprovedStatus && container.IsRepairApproved)
                        return true;
                    return false;
                });
            }

            if (!ShowAllCleaningContainers)
            {
                filtered = filtered.Where(container =>
                    !string.IsNullOrEmpty(container.CleaningRequirementsText) &&
                    container.CleaningRequirementsText != "No requirements" &&
                    container.CleaningRequirementsText != "No specific requirements");
            }

            var filteredList = filtered.ToList();

            // SIMPLIFIED: Row numbers are now just a property on Container
            for (int i = 0; i < filteredList.Count; i++)
            {
                filteredList[i].RowNumber = i + 1;
            }

            // Update the filtered collection
            FilteredCleaningList.Clear();
            foreach (var item in filteredList)
            {
                FilteredCleaningList.Add(item);
            }

            System.Diagnostics.Debug.WriteLine($"Filtering applied: {filteredList.Count} containers shown (from {CleaningList.Count} total)");
        }

        // ===== API LOADING - SIMPLIFIED =====
        [RelayCommand]
        async Task LoadCleaningDataFromApiAsync()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = "";

                System.Diagnostics.Debug.WriteLine("Loading cleaning data from API...");

                var response = await _containerApiService.GetContainersForCleaning();

                if (response.IsSuccess && response.Content != null)
                {
                    CleaningList.Clear();

                    // SIMPLIFIED: No more complex type conversion - API returns Container objects
                    if (response.Content is IEnumerable<Container> containerList)
                    {
                        var containerArray = containerList.ToArray();
                        for (int i = 0; i < containerArray.Length; i++)
                        {
                            var container = containerArray[i];
                            container.RowNumber = i + 1; // Set row number directly
                            CleaningList.Add(container);
                        }
                    }
                    else if (response.Content is Container singleContainer)
                    {
                        singleContainer.RowNumber = 1;
                        CleaningList.Add(singleContainer);
                    }

                    TotalContainers = CleaningList.Count;
                    System.Diagnostics.Debug.WriteLine($"Loaded {TotalContainers} containers with calculated row numbers");

                    // Initialize filtered list
                    PerformFiltering();
                }
                else
                {
                    HasError = true;
                    ErrorMessage = response.Message ?? "Failed to load cleaning data";
                    System.Diagnostics.Debug.WriteLine($"API Error: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Error loading cleaning data: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception in LoadCleaningDataFromApiAsync: {ex}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadCleaningDataFromApiAsync();
        }

        [RelayCommand]
        async Task RetryAsync()
        {
            await LoadCleaningDataFromApiAsync();
        }
    }
}
