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
        private bool isRefreshing;

        [ObservableProperty]
        private string errorMessage = "";

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private int totalContainers;

        public CleaningListViewModel() : this(new ContainerApiService())
        {
        }

        public CleaningListViewModel(IContainerApiService containerApiService)
        {
            _containerApiService = containerApiService;
            Title = "Cleaning List";
            CleaningList = new ObservableCollection<ContainerWithRepairCodesModel>();

            // Load data when ViewModel is created
            _ = LoadCleaningDataFromApiAsync();
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