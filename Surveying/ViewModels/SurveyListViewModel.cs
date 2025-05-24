using CommunityToolkit.Mvvm.ComponentModel;
using Surveying.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Surveying.ViewModels
{
    public partial class SurveyListViewModel : BaseViewModel
    {
        [ObservableProperty]
        private SurveyModel selectedSurvey;

        // New property for pre-grouped orders
        [ObservableProperty]
        private ObservableCollection<SurveyModel> orderGroups;

        private ObservableCollection<SurveyModel> filteredSurveyList;
        public ObservableCollection<SurveyModel> FilteredSurveyList
        {
            get => filteredSurveyList;
            set
            {
                filteredSurveyList = value;
                OnPropertyChanged(nameof(FilteredSurveyList));
            }
        }

        [ObservableProperty]
        private string searchText;

        // Properties for responsive design
        [ObservableProperty]
        private bool isMobileMode;

        [ObservableProperty]
        private bool isDesktopMode;

        public SurveyListViewModel()
        {
            // Pre-group the surveys by order for cleaner display
            OrderGroups = GroupSurveysByOrder(DummyData.Surveys);
            FilteredSurveyList = new ObservableCollection<SurveyModel>(OrderGroups);

            // Default to desktop mode - will be updated by MainPage
            IsDesktopMode = true;
            IsMobileMode = false;
        }

        // Update display mode based on screen size
        public void UpdateDisplayMode(double screenWidth, bool isLandscape)
        {
            // Consider mobile if width is less than 768 and not landscape
            IsMobileMode = screenWidth < 768 && !isLandscape;
            IsDesktopMode = !IsMobileMode;

            OnPropertyChanged(nameof(IsMobileMode));
            OnPropertyChanged(nameof(IsDesktopMode));
        }

        // Group surveys by order number to avoid duplicate rows
        private ObservableCollection<SurveyModel> GroupSurveysByOrder(ObservableCollection<SurveyModel> allSurveys)
        {
            // Group surveys by order number
            var groupedSurveys = allSurveys.GroupBy(s => s.OrderNumber)
                                            .ToDictionary(g => g.Key, g => g.ToList());

            var orderGroups = new ObservableCollection<SurveyModel>();

            foreach (var grouping in groupedSurveys)
            {
                string orderNumber = grouping.Key;
                var surveysInGroup = grouping.Value;

                if (surveysInGroup.Count > 0)
                {
                    // Take the first survey as the "parent" for this order
                    var firstSurvey = surveysInGroup[0];

                    // Create an order group record
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

                    // Add each container to this order
                    foreach (var survey in surveysInGroup)
                    {
                        // Find the container information
                        var containerInfo = DummyData.Containers.FirstOrDefault(c => c.ContNumber == survey.ContNumber);
                        if (containerInfo != null)
                        {
                            // Create a container with enhanced details
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

                            // Initialize the activities collection
                            containerDetail.Activities = new ObservableCollection<ActivityModel>
                            {
                                new ActivityModel("Cleaning", "Cleaning", survey.CleaningStatus),
                                new ActivityModel("Repair", "Repair", survey.RepairStatus),
                                new ActivityModel("Periodic", "Periodic", survey.PeriodicStatus),
                                new ActivityModel("Survey", "Survey", survey.SurveyStatus)
                            };

                            orderGroup.Containers.Add(containerDetail);
                        }
                    }

                    // Add the order group to our collection
                    orderGroups.Add(orderGroup);
                }
            }

            return orderGroups;
        }

        partial void OnSearchTextChanged(string value)
        {
            UpdateFilteredSurveyList();
        }

        public void UpdateFilteredSurveyList()
        {
            if (OrderGroups == null)
                return;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredSurveyList = new ObservableCollection<SurveyModel>(OrderGroups);
            }
            else
            {
                var lowerVal = SearchText.ToLower();

                // Filter orders based on search text
                var filtered = OrderGroups.Where(s =>
                    s.OrderNumber.ToLower().Contains(lowerVal) ||
                    s.Surveyor.ToLower().Contains(lowerVal) ||
                    s.PrincipalName.ToLower().Contains(lowerVal) ||
                    s.ShipperName.ToLower().Contains(lowerVal) ||
                    s.Containers.Any(c => c.ContNumber.ToLower().Contains(lowerVal)));

                FilteredSurveyList = new ObservableCollection<SurveyModel>(filtered);
            }
        }

        public ObservableCollection<string> ConditionList => ConditionData.ConditionList;
    }
}