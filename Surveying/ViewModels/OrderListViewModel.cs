using CommunityToolkit.Mvvm.ComponentModel;
using Surveying.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Surveying.ViewModels
{
     MIGRATED: SurveyListViewModel using simplified models =====
    // RENAMED: SurveyListViewModel → OrderListViewModel (better naming)
    public partial class OrderListViewModel : BaseViewModel
    {
        [ObservableProperty]
        private Order selectedOrder;

        // ===== SIMPLIFIED COLLECTIONS =====
        // OLD: ObservableCollection<SurveyModel> orderGroups (complex grouped surveys)
        // NEW: ObservableCollection<Order> orders (clean Order objects with Containers)
        [ObservableProperty]
        private ObservableCollection<Order> orders;

        private ObservableCollection<Order> filteredOrderList;
        public ObservableCollection<Order> FilteredOrderList
        {
            get => filteredOrderList;
            set
            {
                filteredOrderList = value;
                OnPropertyChanged(nameof(FilteredOrderList));
            }
        }

        [ObservableProperty]
        private string searchText;

        // Properties for responsive design
        [ObservableProperty]
        private bool isMobileMode;

        [ObservableProperty]
        private bool isDesktopMode;

        // ===== CONSTRUCTOR - SIMPLIFIED =====
        public OrderListViewModel()
        {
            // SIMPLIFIED: Convert DummyData to unified Order objects
            Orders = ConvertDummyDataToOrders();
            FilteredOrderList = new ObservableCollection<Order>(Orders);

            // Default to desktop mode - will be updated by MainPage
            IsDesktopMode = true;
            IsMobileMode = false;
        }

        // ===== SIMPLIFIED DATA CONVERSION =====
        // OLD: Complex GroupSurveysByOrder method with inheritance and lookups
        // NEW: Simple conversion to unified Order objects
        private ObservableCollection<Order> ConvertDummyDataToOrders()
        {
            // Group old surveys by order number
            var groupedSurveys = DummyData.Surveys.GroupBy(s => s.OrderNumber)
                                                 .ToDictionary(g => g.Key, g => g.ToList());

            var orders = new ObservableCollection<Order>();

            foreach (var grouping in groupedSurveys)
            {
                string orderNumber = grouping.Key;
                var surveysInGroup = grouping.Value;

                if (surveysInGroup.Count > 0)
                {
                    var firstSurvey = surveysInGroup[0];

                    // SIMPLIFIED: Create Order with direct string properties (no more ID lookups!)
                    var order = new Order
                    {
                        OrderNumber = orderNumber,
                        Surveyor = firstSurvey.Surveyor,
                        OrderDate = firstSurvey.OrderDate,
                        SurveyDate = firstSurvey.SurveyDate,
                        PickupDate = firstSurvey.PickupDate,

                        // SIMPLIFIED: Get Principal/Shipper info directly (no more DummyData lookups!)
                        PrincipalCode = DummyData.Principals.FirstOrDefault(p => p.Id == firstSurvey.PrincipalId)?.Code ?? $"P{firstSurvey.PrincipalId:000}",
                        PrincipalName = DummyData.Principals.FirstOrDefault(p => p.Id == firstSurvey.PrincipalId)?.Name ?? "Unknown Principal",
                        ShipperCode = DummyData.Shippers.FirstOrDefault(s => s.Id == firstSurvey.ShipperId)?.Code ?? $"S{firstSurvey.ShipperId:000}",
                        ShipperName = DummyData.Shippers.FirstOrDefault(s => s.Id == firstSurvey.ShipperId)?.Name ?? "Unknown Shipper",

                        Containers = new ObservableCollection<Container>()
                    };

                    // SIMPLIFIED: Add containers to order
                    foreach (var survey in surveysInGroup)
                    {
                        var containerInfo = DummyData.Containers.FirstOrDefault(c => c.ContNumber == survey.ContNumber);
                        if (containerInfo != null)
                        {
                            // SIMPLIFIED: Create unified Container object
                            var container = new Container
                            {
                                ContNumber = survey.ContNumber,
                                ContSize = containerInfo.ContSize,
                                ContType = containerInfo.ContType,
                                Condition = survey.Condition,

                                // Status directly from survey
                                CleaningStatus = survey.CleaningStatus,
                                RepairStatus = survey.RepairStatus,
                                PeriodicStatus = survey.PeriodicStatus,
                                SurveyStatus = survey.SurveyStatus,

                                // Initialize other properties
                                CustomerCode = "TBD",
                                DtmIn = DateTime.Now,
                                Commodity = "Tank Container"
                            };

                            // Update activities automatically
                            container.UpdateActivities();

                            order.Containers.Add(container);
                        }
                    }

                    orders.Add(order);
                }
            }

            return orders;
        }

        // ===== RESPONSIVE DESIGN - UNCHANGED =====
        public void UpdateDisplayMode(double screenWidth, bool isLandscape)
        {
            IsMobileMode = screenWidth < 768 && !isLandscape;
            IsDesktopMode = !IsMobileMode;

            OnPropertyChanged(nameof(IsMobileMode));
            OnPropertyChanged(nameof(IsDesktopMode));
        }

        // ===== SEARCH/FILTER - SIMPLIFIED =====
        partial void OnSearchTextChanged(string value)
        {
            UpdateFilteredOrderList();
        }

        public void UpdateFilteredOrderList()
        {
            if (Orders == null)
                return;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredOrderList = new ObservableCollection<Order>(Orders);
            }
            else
            {
                var lowerVal = SearchText.ToLower();

                // SIMPLIFIED: Filter orders with direct property access
                var filtered = Orders.Where(order =>
                    order.OrderNumber.ToLower().Contains(lowerVal) ||
                    order.Surveyor.ToLower().Contains(lowerVal) ||
                    order.PrincipalName.ToLower().Contains(lowerVal) ||
                    order.ShipperName.ToLower().Contains(lowerVal) ||
                    order.Containers.Any(c => c.ContNumber.ToLower().Contains(lowerVal)));

                FilteredOrderList = new ObservableCollection<Order>(filtered);
            }
        }

        // ===== ADD NEW ORDER - SIMPLIFIED =====
        public void AddOrder(Order newOrder)
        {
            Orders.Add(newOrder);
            UpdateFilteredOrderList();
        }

        // Condition list - unchanged
        public ObservableCollection<string> ConditionList => ConditionData.ConditionList;
    }
}