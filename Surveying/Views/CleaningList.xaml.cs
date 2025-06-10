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

            // ✅ FIXED: Only create ViewModel once, in code-behind
            var containerApiService = new ContainerApiService();
            _viewModel = new CleaningListViewModel(containerApiService);
            BindingContext = _viewModel;

            // ✅ RESTORED: Subscribe to pagination events with proper null checking
            if (dataPager != null)
            {
                dataPager.PageChanged += OnPageChanged;
                System.Diagnostics.Debug.WriteLine("Successfully subscribed to dataPager.PageChanged");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("WARNING: dataPager is null during constructor");
            }

            System.Diagnostics.Debug.WriteLine("CleaningList constructor completed");
        }

        private void OnPageChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataPager == null)
                {
                    System.Diagnostics.Debug.WriteLine("dataPager is null in OnPageChanged");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Page changing to: {dataPager.PageIndex}, PageSize: {dataPager.PageSize}");
                System.Diagnostics.Debug.WriteLine($"PagedSource is null: {dataPager.PagedSource == null}");
                System.Diagnostics.Debug.WriteLine($"PagedSource count: {dataPager.PagedSource?.Count ?? 0}");

                // DON'T update page info immediately - wait for PagedSource to be ready
                // Instead, use a small delay to let the DataPager finish its internal updates
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // Wait a short moment for the DataPager to update its PagedSource
                        await Task.Delay(50);

                        // Now check if PagedSource is ready
                        if (dataPager.PagedSource != null && dataPager.PagedSource.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"PagedSource ready with {dataPager.PagedSource.Count} items");

                            // Now it's safe to update the page info for row numbering
                            PaginatedRowNumberConverter.UpdatePageInfo(dataPager.PageIndex, dataPager.PageSize);

                            System.Diagnostics.Debug.WriteLine("Page info updated successfully");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("PagedSource still not ready after delay");

                            // Fallback: try again after a longer delay
                            await Task.Delay(100);
                            if (dataPager.PagedSource != null)
                            {
                                PaginatedRowNumberConverter.UpdatePageInfo(dataPager.PageIndex, dataPager.PageSize);
                                System.Diagnostics.Debug.WriteLine("Page info updated after longer delay");
                            }
                        }
                    }
                    catch (Exception innerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in delayed page update: {innerEx.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnPageChanged: {ex.Message}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                System.Diagnostics.Debug.WriteLine("CleaningList OnAppearing started");

                // Initialize page info for row numbering
                if (dataPager != null)
                {
                    PaginatedRowNumberConverter.UpdatePageInfo(dataPager.PageIndex, dataPager.PageSize);
                    System.Diagnostics.Debug.WriteLine($"Initialized pagination: PageIndex={dataPager.PageIndex}, PageSize={dataPager.PageSize}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: dataPager is null in OnAppearing");
                }

                // ✅ FIXED: Force data load and check if it's working
                if (_viewModel != null)
                {
                    System.Diagnostics.Debug.WriteLine($"ViewModel state - IsLoading: {_viewModel.IsLoading}, HasError: {_viewModel.HasError}");
                    System.Diagnostics.Debug.WriteLine($"CleaningList count: {_viewModel.CleaningList?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"FilteredCleaningList count: {_viewModel.FilteredCleaningList?.Count ?? 0}");

                    // Force load the data
                    await _viewModel.LoadCleaningDataFromApiCommand.ExecuteAsync(null);

                    System.Diagnostics.Debug.WriteLine($"After API call - CleaningList count: {_viewModel.CleaningList?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"After API call - FilteredCleaningList count: {_viewModel.FilteredCleaningList?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"After API call - HasError: {_viewModel.HasError}, ErrorMessage: {_viewModel.ErrorMessage}");

                    // Check pagination after data load
                    if (dataPager != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"After data load - PagedSource count: {dataPager.PagedSource?.Count ?? 0}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: _viewModel is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                await DisplayAlert("Loading Error", $"Failed to load page: {ex.Message}", "OK");
            }
        }

        private async void OnGoToCleaningClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("OnGoToCleaningClicked started");

                if (sender is Button button && button.CommandParameter is ContainerWithRepairCodesModel container)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigating to cleaning for container: {container.ContNumber}");

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

                    System.Diagnostics.Debug.WriteLine("Navigation completed successfully");
                }
                else
                {
            
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
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

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            try
            {
                // Unsubscribe from events to prevent memory leaks
                if (dataPager != null)
                {
                    dataPager.PageChanged -= OnPageChanged;
                    System.Diagnostics.Debug.WriteLine("Successfully unsubscribed from dataPager.PageChanged");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnDisappearing: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine("CleaningList OnDisappearing completed");
        }
    }
}