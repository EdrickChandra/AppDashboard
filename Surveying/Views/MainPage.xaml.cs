using Syncfusion.Maui.DataGrid;
using Surveying.ViewModels;
using Surveying.Models;
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
            await Navigation.PushAsync(new AddPage((surveyEntries) =>
            {
                // First add all new surveys to DummyData
                foreach (var survey in surveyEntries)
                {
                    // Add the container to DummyData if it doesn't exist
                    if (!DummyData.Containers.Any(c => c.ContNumber == survey.ContNumber))
                    {
                        // Extract size and type from the survey or use defaults
                        var newContainer = new ContModel
                        {
                            ContNumber = survey.ContNumber,
                            ContSize = "20", // Default size, you might want to add this to AddPage
                            ContType = "Tank" // Default type, you might want to add this to AddPage
                        };
                        DummyData.Containers.Add(newContainer);
                    }

                    // Add the survey to DummyData
                    DummyData.Surveys.Add(survey);
                }

                // We need to handle adding new surveys differently now
                // Group them by order number and add to OrderGroups
                var newOrderNumbers = surveyEntries.Select(s => s.OrderNumber).Distinct();

                foreach (var orderNumber in newOrderNumbers)
                {
                    var surveysForOrder = surveyEntries.Where(s => s.OrderNumber == orderNumber).ToList();
                    if (surveysForOrder.Any())
                    {
                        var firstSurvey = surveysForOrder.First();
                        var orderGroup = new SurveyModel
                        {
                            OrderNumber = orderNumber,
                            PrincipalId = firstSurvey.PrincipalId,
                            Surveyor = firstSurvey.Surveyor,
                            ShipperId = firstSurvey.ShipperId,
                            OrderDate = firstSurvey.OrderDate,
                            SurveyDate = firstSurvey.SurveyDate,
                            PickupDate = firstSurvey.PickupDate,
                            Containers = new ObservableCollection<ContainerDetailModel>()
                        };

                        foreach (var survey in surveysForOrder)
                        {
                            var containerInfo = DummyData.Containers.FirstOrDefault(c => c.ContNumber == survey.ContNumber);
                            if (containerInfo != null)
                            {
                                var containerDetail = new ContainerDetailModel
                                {
                                    ContNumber = survey.ContNumber,
                                    ContSize = containerInfo.ContSize,
                                    ContType = containerInfo.ContType,
                                    Condition = survey.Condition,
                                    CleaningStatus = survey.CleaningStatus,
                                    RepairStatus = survey.RepairStatus,
                                    PeriodicStatus = survey.PeriodicStatus,
                                    SurveyStatus = survey.SurveyStatus
                                };

                                // Initialize activities using UpdateActivities method
                                containerDetail.UpdateActivities();

                                orderGroup.Containers.Add(containerDetail);
                            }
                        }

                        _viewModel.OrderGroups.Add(orderGroup);
                    }
                }

                // Refresh the filtered list
                _viewModel.UpdateFilteredSurveyList();
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
                ContainerDetailModel container = null;

                if (button.BindingContext is ActivityModel activity)
                {
                    // Find the container that contains this activity
                    foreach (var surveyItem in _viewModel.OrderGroups)
                    {
                        foreach (var cont in surveyItem.Containers)
                        {
                            if (cont.Activities.Contains(activity))
                            {
                                container = cont;
                                break;
                            }
                        }

                        if (container != null)
                            break;
                    }
                }
                else if (button.BindingContext is ContainerDetailModel directContainer)
                {
                    container = directContainer;
                }

                if (container == null)
                {
                    await DisplayAlert("Error", "Cannot find the container for this action.", "OK");
                    return;
                }

                // Find the survey that contains this container
                var survey = _viewModel.OrderGroups.FirstOrDefault(s =>
                    s.Containers.Contains(container));

                if (survey == null)
                {
                    await DisplayAlert("Error", "Cannot find the survey for this container.", "OK");
                    return;
                }

                _viewModel.SelectedSurvey = survey;

                // Navigate to the appropriate page - passing BOTH survey and container
                Page destinationPage = null;

                switch (pageType)
                {
                    case "Cleaning":
                        destinationPage = new Cleaning(survey, container);
                        break;
                    case "Repair":
                        destinationPage = new Repair(survey, container);
                        break;
                    case "Periodic":
                        destinationPage = new Periodic(survey, container);
                        break;
                    case "Survey":
                        destinationPage = new Survey(survey, container);
                        break;
                }

                if (destinationPage != null)
                {
                    await Navigation.PushAsync(destinationPage);
                }
            }
        }
    }
}