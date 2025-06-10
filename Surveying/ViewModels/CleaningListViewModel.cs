using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace Surveying.ViewModels
{
    public partial class CleaningListViewModel : BaseViewModel
    {
        private readonly IContainerApiService _containerApiService;

        [ObservableProperty]
        private ObservableCollection<ContainerWithRepairCodesModel> cleaningList;

        [ObservableProperty]
        private ObservableCollection<ContainerWithRepairCodesModel> filteredCleaningList;

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

        public CleaningListViewModel() : this(new ContainerApiService())
        {
        }

        public CleaningListViewModel(IContainerApiService containerApiService)
        {
            _containerApiService = containerApiService;
            Title = "Cleaning List";
            CleaningList = new ObservableCollection<ContainerWithRepairCodesModel>();
            FilteredCleaningList = new ObservableCollection<ContainerWithRepairCodesModel>();

            // Load data when ViewModel is created
            _ = LoadCleaningDataFromApiAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            PerformSearch();
        }

        [RelayCommand]
        void Search()
        {
            PerformSearch();
        }

        private void PerformSearch()
        {
            if (CleaningList == null)
                return;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Show all items when search is empty
                FilteredCleaningList.Clear();
                foreach (var item in CleaningList)
                {
                    FilteredCleaningList.Add(item);
                }
            }
            else
            {
                var searchTerm = SearchText.ToLower().Trim();
                var filtered = CleaningList.Where(container =>
                    container.ContNumber.ToLower().Contains(searchTerm) ||
                    container.CustomerCode.ToLower().Contains(searchTerm) ||
                    container.Id.ToString().Contains(searchTerm)
                ).ToList();

                FilteredCleaningList.Clear();
                foreach (var item in filtered)
                {
                    FilteredCleaningList.Add(item);
                }
            }

            // Update total count
            OnPropertyChanged(nameof(FilteredCleaningList));
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

                    // Handle single container or list of containers
                    if (response.Content is IEnumerable<ContainerWithRepairCodesModel> containerList)
                    {
                        foreach (var container in containerList)
                        {
                            CleaningList.Add(container);
                        }
                    }
                    else if (response.Content is ContainerWithRepairCodesModel singleContainer)
                    {
                        CleaningList.Add(singleContainer);
                    }

                    TotalContainers = CleaningList.Count;
                    System.Diagnostics.Debug.WriteLine($"Loaded {TotalContainers} containers for cleaning");

                    // Initialize filtered list
                    PerformSearch();
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