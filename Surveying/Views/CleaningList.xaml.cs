using Surveying.ViewModels;
using Surveying.Services;
using Surveying.Models;
using Surveying.Views;

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

            System.Diagnostics.Debug.WriteLine("CleaningList constructor completed - using ViewModel-calculated row numbers");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                System.Diagnostics.Debug.WriteLine("CleaningList OnAppearing started");

                // Simple data loading - no complex pagination handling needed
                if (_viewModel != null)
                {
                    System.Diagnostics.Debug.WriteLine($"ViewModel state - IsLoading: {_viewModel.IsLoading}, HasError: {_viewModel.HasError}");
                    System.Diagnostics.Debug.WriteLine($"CleaningList count: {_viewModel.CleaningList?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"FilteredCleaningList count: {_viewModel.FilteredCleaningList?.Count ?? 0}");

                    // Load the data - row numbers are calculated automatically in ViewModel
                    await _viewModel.LoadCleaningDataFromApiCommand.ExecuteAsync(null);

                    System.Diagnostics.Debug.WriteLine($"After API call - CleaningList count: {_viewModel.CleaningList?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"After API call - FilteredCleaningList count: {_viewModel.FilteredCleaningList?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"After API call - HasError: {_viewModel.HasError}, ErrorMessage: {_viewModel.ErrorMessage}");

                  
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: _viewModel is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                await DisplayAlert("Loading Error", $"Failed to load page: {ex.Message}", "OK");
            }
        }

        private async void OnGoToCleaningClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("OnGoToCleaningClicked started");

                // Updated to handle the extended model
                if (sender is Button button && button.CommandParameter is ContainerWithRepairCodesModelExtended container)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigating to cleaning for container: {container.ContNumber} (Row: {container.RowNumber})");

                    // Show loading indicator while navigating
                    button.IsEnabled = false;
                    button.Text = "Loading...";

                    // Create a survey model for the cleaning page
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

                    // Convert extended model back to base model for navigation
                    var baseContainer = new ContainerWithRepairCodesModel
                    {
                        Id = container.Id,
                        ContNumber = container.ContNumber,
                        DtmIn = container.DtmIn,
                        CustomerCode = container.CustomerCode,
                        RepairCodes = container.RepairCodes,
                        IsRepairApproved = container.IsRepairApproved,
                        ApprovalDate = container.ApprovalDate,
                        ApprovedBy = container.ApprovedBy,
                        Commodity = container.Commodity
                    };

                    // Navigate to enhanced cleaning page
                    var cleaningPage = new EnhancedCleaning(survey, containerDetail, baseContainer);
                    await Navigation.PushAsync(cleaningPage);

                    System.Diagnostics.Debug.WriteLine("Navigation completed successfully");
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Navigation Error", $"Failed to open cleaning page: {ex.Message}", "OK");
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
    }
}