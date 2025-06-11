
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Services;
using Surveying.Views;
using System.Collections.ObjectModel;
using System.Linq;


namespace Surveying.ViewModels
{
    public partial class CleaningListViewModel : BaseViewModel
    {
        private readonly IContainerApiService _containerApiService;
        private readonly ICleaningCriteriaService _cleaningCriteriaService;

        [ObservableProperty]
        private ObservableCollection<ContainerWithRepairCodesModelExtended> cleaningList;

        [ObservableProperty]
        private ObservableCollection<ContainerWithRepairCodesModelExtended> filteredCleaningList;

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

        // Filter properties
        [ObservableProperty]
        private FilterResult currentFilters = new FilterResult();

        [ObservableProperty]
        private string filterButtonText = "Filter";

        [ObservableProperty]
        private bool hasActiveFilters = false;

        // Dynamic cleaning criteria display
        [ObservableProperty]
        private string cleaningCriteriaText = "Loading...";

        [ObservableProperty]
        private List<string> cleaningCriteriaList = new List<string>();

        [ObservableProperty]
        private string footerCriteriaText = "Loading cleaning criteria...";

        public CleaningListViewModel() : this(new ContainerApiService(), new CleaningCriteriaService())
        {
        }

        public CleaningListViewModel(IContainerApiService containerApiService, ICleaningCriteriaService cleaningCriteriaService)
        {
            _containerApiService = containerApiService;
            _cleaningCriteriaService = cleaningCriteriaService;
            Title = "Cleaning List";
            CleaningList = new ObservableCollection<ContainerWithRepairCodesModelExtended>();
            FilteredCleaningList = new ObservableCollection<ContainerWithRepairCodesModelExtended>();

            // Load cleaning criteria and data
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // Load cleaning criteria first
            await LoadCleaningCriteriaAsync();

            // Then load container data
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
                System.Diagnostics.Debug.WriteLine($"Criteria groups: {string.Join(", ", criteriaList)}");
                System.Diagnostics.Debug.WriteLine($"Components: {string.Join(", ", criteria.ComponentCodes)}");
                System.Diagnostics.Debug.WriteLine($"Repair Codes: {string.Join(", ", criteria.RepairCodes)}");
                System.Diagnostics.Debug.WriteLine($"Location Codes: {string.Join(", ", criteria.LocationCodes)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cleaning criteria: {ex.Message}");
                CleaningCriteriaText = "YXT • 1101 • APNN"; // Fallback
                CleaningCriteriaList = new List<string> { "YXT • 1101 • APNN" };
                FooterCriteriaText = "Filtered by cleaning criteria: YXT • 1101 • APNN";
            }
        }

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
                // Create list of available customers with counts
                var customerCounts = CleaningList
                    .GroupBy(c => c.CustomerCode)
                    .Select(g => new CustomerFilterItem(g.Key, g.Count()))
                    .OrderBy(c => c.CustomerCode)
                    .ToList();

                // Create and show filter popup
                var filterViewModel = new FilterPopupViewModel(customerCounts, CurrentFilters, CleaningCriteriaText);
                var filterPopup = new FilterPopup(filterViewModel);

                // Show popup and wait for result
                filterPopup.TaskCompletionSource = new TaskCompletionSource<FilterResult>();
                await Application.Current.MainPage.Navigation.PushModalAsync(filterPopup);

                var result = await filterPopup.TaskCompletionSource.Task;

                if (result != null)
                {
                    // Apply the new filters
                    CurrentFilters = result;
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

        private void UpdateFilterButtonText()
        {
            var activeCount = CurrentFilters.GetActiveFilterCount();
            HasActiveFilters = activeCount > 0;

            if (activeCount == 0)
            {
                FilterButtonText = "Filter";
            }
            else
            {
                FilterButtonText = $"Filter ({activeCount})";
            }
        }

        private void PerformFiltering()
        {
            if (CleaningList == null)
                return;

            var filtered = CleaningList.AsEnumerable();

            // Apply search text filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchTerm = SearchText.ToLower().Trim();
                filtered = filtered.Where(container =>
                    container.ContNumber.ToLower().Contains(searchTerm) ||
                    container.CustomerCode.ToLower().Contains(searchTerm) ||
                    container.Id.ToString().Contains(searchTerm));
            }

            // Apply customer filter
            if (CurrentFilters.SelectedCustomers.Any())
            {
                filtered = filtered.Where(container =>
                    CurrentFilters.SelectedCustomers.Contains(container.CustomerCode));
            }

            // Apply status filter
            if (!CurrentFilters.ShowPendingStatus || !CurrentFilters.ShowApprovedStatus)
            {
                filtered = filtered.Where(container =>
                {
                    if (CurrentFilters.ShowPendingStatus && !container.IsRepairApproved)
                        return true;
                    if (CurrentFilters.ShowApprovedStatus && container.IsRepairApproved)
                        return true;
                    return false;
                });
            }

            // Apply cleaning required filter
            if (!CurrentFilters.ShowCleaningRequired)
            {
                // Filter logic for cleaning required - you can customize this
                // For now, we'll assume all containers in this list require cleaning
                // So if ShowCleaningRequired is false, we show nothing
                filtered = Enumerable.Empty<ContainerWithRepairCodesModelExtended>();
            }

            var filteredList = filtered.ToList();

            // Recalculate row numbers for filtered results
            RecalculateRowNumbers(filteredList);

            // Update the filtered collection
            FilteredCleaningList.Clear();
            foreach (var item in filteredList)
            {
                FilteredCleaningList.Add(item);
            }

            // Update total count
            OnPropertyChanged(nameof(FilteredCleaningList));

            System.Diagnostics.Debug.WriteLine($"Filtering applied: {filteredList.Count} containers shown (from {CleaningList.Count} total)");
        }

        /// <summary>
        /// Calculate sequential row numbers for the provided list
        /// </summary>
        private void RecalculateRowNumbers(List<ContainerWithRepairCodesModelExtended> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].RowNumber = i + 1;
            }
        }

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
                    // Clear existing data
                    CleaningList.Clear();

                    // Convert API models to extended models with row numbers
                    var containers = new List<ContainerWithRepairCodesModelExtended>();

                    // Handle single container or list of containers
                    if (response.Content is IEnumerable<ContainerWithRepairCodesModel> containerList)
                    {
                        var containerArray = containerList.ToArray();
                        for (int i = 0; i < containerArray.Length; i++)
                        {
                            var container = containerArray[i];
                            var extendedContainer = new ContainerWithRepairCodesModelExtended
                            {
                                Id = container.Id,
                                ContNumber = container.ContNumber,
                                DtmIn = container.DtmIn,
                                CustomerCode = container.CustomerCode,
                                RepairCodes = container.RepairCodes,
                                IsRepairApproved = container.IsRepairApproved,
                                ApprovalDate = container.ApprovalDate,
                                ApprovedBy = container.ApprovedBy,
                                CleaningStartDate = container.CleaningStartDate,
                                CleaningCompleteDate = container.CleaningCompleteDate,
                                Commodity = container.Commodity ?? "Not Specified",
                                CleaningRequirementsText = container.CleaningRequirementsText ?? "No requirements",
                                RowNumber = i + 1 // Calculate sequential row number
                            };
                            containers.Add(extendedContainer);
                        }
                    }
                    else if (response.Content is ContainerWithRepairCodesModel singleContainer)
                    {
                        var extendedContainer = new ContainerWithRepairCodesModelExtended
                        {
                            Id = singleContainer.Id,
                            ContNumber = singleContainer.ContNumber,
                            DtmIn = singleContainer.DtmIn,
                            CustomerCode = singleContainer.CustomerCode,
                            RepairCodes = singleContainer.RepairCodes,
                            IsRepairApproved = singleContainer.IsRepairApproved,
                            ApprovalDate = singleContainer.ApprovalDate,
                            ApprovedBy = singleContainer.ApprovedBy,
                            CleaningStartDate = singleContainer.CleaningStartDate,
                            CleaningCompleteDate = singleContainer.CleaningCompleteDate,
                            Commodity = singleContainer.Commodity ?? "Not Specified",
                            CleaningRequirementsText = singleContainer.CleaningRequirementsText ?? "No requirements",
                            RowNumber = 1
                        };
                        containers.Add(extendedContainer);
                    }

                    // Add to collections
                    foreach (var container in containers)
                    {
                        CleaningList.Add(container);
                    }

                    TotalContainers = CleaningList.Count;
                    System.Diagnostics.Debug.WriteLine($"Loaded {TotalContainers} containers with calculated row numbers");

                    // Initialize filtered list (this will also set up row numbers)
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

    /// <summary>
    /// Extended model that includes calculated row number
    /// </summary>
    public class ContainerWithRepairCodesModelExtended : ContainerWithRepairCodesModel
    {
        public int RowNumber { get; set; }

        // Override to ensure we use the container-specific requirements
        public new string CleaningRequirementsText { get; set; } = "Not Specified";
    }
}