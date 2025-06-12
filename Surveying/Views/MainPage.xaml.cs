using Syncfusion.Maui.DataGrid;
using Surveying.ViewModels;
using Surveying.Models;
using Surveying.Views;
using System;
using System.Collections.ObjectModel;

namespace Surveying.Views
{
    public partial class MainPage : ContentPage
    {
        private SurveyListViewModel _viewModel;
        private bool isInitialized = false;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = new SurveyListViewModel();
            BindingContext = _viewModel;

            SizeChanged += OnSizeChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!isInitialized)
            {
                AdjustLayoutForDeviceAndOrientation();
                isInitialized = true;
            }
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            AdjustLayoutForDeviceAndOrientation();
        }

        private void AdjustLayoutForDeviceAndOrientation()
        {
            bool isLandscape = Width > Height;
            double screenWidth = Width;

            // Update ViewModel display mode
            _viewModel.UpdateDisplayMode(screenWidth, isLandscape);

            // Modify DataGrid columns and layout based on screen size
            if (screenWidth <= 768 && !isLandscape)
            {
                // We're in mobile mode - handled by the ViewModel IsMobileMode property
                // This ensures the appropriate view is visible
            }
            else
            {
                // Desktop/Tablet view
                if (dataGrid.Columns.Count >= 5)
                {
                    // Only hide date columns in landscape tablet view if needed
                    bool isTablet = screenWidth < 1024;

                    dataGrid.Columns[4].Visible = !isTablet; // Order Date
                    dataGrid.Columns[5].Visible = !isTablet; // Survey Date
                    dataGrid.Columns[6].Visible = !isTablet; // Pickup Date
                }
            }
        }
        private async void OnAddSurveyClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddPage((newOrder) =>
            {
                // Add the new order to the ViewModel
                _viewModel.AddOrder(newOrder);

                // Navigate back
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                });
            }));
        }



        private void DataGrid_CellTapped(object sender, DataGridCellTappedEventArgs e)
        {
            if (e.RowData is SurveyModel selected)
            {
                _viewModel.SelectedSurvey = selected;
            }
        }

        private void MobileView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is SurveyModel selected)
            {
                _viewModel.SelectedSurvey = selected;
            }
        }

        // Updated NavigateToPage method in MainPage.xaml.cs

        private async void NavigateToPage(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                string pageType = button.CommandParameter?.ToString();

                if (string.IsNullOrEmpty(pageType))
                {
                    return;
                }

                // Find the container from the button context
                Container container = null;
                Order order = null;

                if (button.BindingContext is Activity activity)
                {
                    // Find the container that contains this activity
                    foreach (var orderItem in _viewModel.Orders)
                    {
                        foreach (var cont in orderItem.Containers)
                        {
                            if (cont.Activities.Contains(activity))
                            {
                                container = cont;
                                order = orderItem;
                                break;
                            }
                        }

                        if (container != null)
                            break;
                    }
                }
                else if (button.BindingContext is Container directContainer)
                {
                    container = directContainer;
                    // Find the order that contains this container
                    order = _viewModel.Orders.FirstOrDefault(o => o.Containers.Contains(container));
                }

                if (container == null || order == null)
                {
                    await DisplayAlert("Error", "Cannot find the container or order for this action.", "OK");
                    return;
                }

                // ===== NEW UNIFIED NAVIGATION =====
                // Instead of separate pages, use one ContainerActivityPage with different ActivityTypes

                try
                {
                    Page destinationPage = null;
                    ActivityType activityType;

                    switch (pageType)
                    {
                        case "Cleaning":
                            activityType = ActivityType.Cleaning;
                            destinationPage = new ContainerActivityPage(order, container, activityType);
                            break;
                        case "Repair":
                            activityType = ActivityType.Repair;
                            destinationPage = new ContainerActivityPage(order, container, activityType);
                            break;
                        case "Periodic":
                            activityType = ActivityType.Periodic;
                            destinationPage = new ContainerActivityPage(order, container, activityType);
                            break;
                        case "Survey":
                            activityType = ActivityType.Survey;
                            destinationPage = new ContainerActivityPage(order, container, activityType);
                            break;
                        default:
                            await DisplayAlert("Error", $"Unknown activity type: {pageType}", "OK");
                            return;
                    }

                    if (destinationPage != null)
                    {
                        await Navigation.PushAsync(destinationPage);
                        System.Diagnostics.Debug.WriteLine($"Successfully navigated to {pageType} for container {container.ContNumber}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                    await DisplayAlert("Navigation Error", $"Failed to open {pageType} page: {ex.Message}", "OK");
                }
            }
        }

        