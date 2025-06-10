using Surveying.ViewModels;
using Surveying.Services;
using Surveying.Models;
using Surveying.Views;
using Surveying.Helpers;

namespace Surveying.Views
{
    public partial class CleaningList : ContentPage
    {
        private CleaningListViewModel _viewModel;

        public CleaningList()
        {
            InitializeComponent();

            // Create ViewModel with proper dependency injection
            var containerApiService = new ContainerApiService();
            _viewModel = new CleaningListViewModel(containerApiService);
            BindingContext = _viewModel;

            // Subscribe to page change events
            dataPager.PageChanged += OnPageChanged;
        }

        private void OnPageChanged(object sender, EventArgs e)
        {
            // Update static page info for row numbering
            if (dataPager != null)
            {
                StaticRowNumberConverter.UpdatePageInfo(dataPager.PageIndex, dataPager.PageSize);

                // Force refresh of DataGrid to update row numbers using modern MAUI approach
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (cleaningDataGrid != null)
                    {
                        // Simple ItemsSource refresh - this is the most reliable method
                        var currentSource = cleaningDataGrid.ItemsSource;
                        cleaningDataGrid.ItemsSource = null;
                        cleaningDataGrid.ItemsSource = currentSource;
                    }
                });
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Initialize page info
            StaticRowNumberConverter.UpdatePageInfo(dataPager.PageIndex, dataPager.PageSize);

            // Refresh data when page appears
            if (_viewModel != null && !_viewModel.IsLoading)
            {
                await _viewModel.LoadCleaningDataFromApiCommand.ExecuteAsync(null);
            }
        }

        private async void OnGoToCleaningClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is ContainerWithRepairCodesModel container)
                {
                    // Show loading indicator while navigating
                    button.IsEnabled = false;
                    button.Text = "Loading...";

                    // Create a survey model for the cleaning page (required for backward compatibility)
                    var survey = new SurveyModel
                    {
                        OrderNumber = $"CLN-{container.ContNumber}",
                        ContNumber = container.ContNumber,
                        Surveyor = "Cleaning Crew",
                        PrincipalId = 1, // Default principal
                        ShipperId = 1,   // Default shipper
                        OrderDate = DateTime.Today,
                        SurveyDate = DateTime.Today,
                        PickupDate = DateTime.Today.AddDays(1),
                        Condition = "Dirty" // Since this is for cleaning
                    };

                    // Create a container detail model
                    var containerDetail = new ContainerDetailModel
                    {
                        ContNumber = container.ContNumber,
                        ContSize = "20", // Default, could be enhanced to get from API
                        ContType = "Tank",
                        Condition = "Dirty",
                        CleaningStatus = StatusType.NotFilled,
                        RepairStatus = StatusType.NotFilled,
                        PeriodicStatus = StatusType.NotFilled,
                        SurveyStatus = StatusType.NotFilled
                    };

                    // Initialize activities
                    containerDetail.UpdateActivities();

                    // Navigate to enhanced cleaning page
                    var cleaningPage = new EnhancedCleaning(survey, containerDetail, container);
                    await Navigation.PushAsync(cleaningPage);
                }
                else
                {
                    await DisplayAlert("Error", "Unable to load container information.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Navigation Error", $"Failed to open cleaning page: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex}");
            }
            finally
            {
                // Reset button state
                if (sender is Button button)
                {
                    button.IsEnabled = true;
                    button.Text = "Go to Cleaning";
                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Unsubscribe from events to prevent memory leaks
            if (dataPager != null)
            {
                dataPager.PageChanged -= OnPageChanged;
            }
        }
    }
}