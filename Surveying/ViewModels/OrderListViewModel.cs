using CommunityToolkit.Mvvm.ComponentModel;
using Surveying.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Surveying.ViewModels
{
    /// <summary>
    /// SIMPLIFIED: OrderListViewModel using direct Order objects
    /// Removed: Complex data conversion and lookups
    /// </summary>
    public partial class OrderListViewModel : BaseViewModel
    {
        [ObservableProperty]
        private Order selectedOrder;

        // ===== SIMPLIFIED COLLECTIONS =====
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

        // ===== SIMPLIFIED CONSTRUCTOR =====
        public OrderListViewModel()
        {
            // MUCH SIMPLER: Just use the orders directly from DummyData
            Orders = DummyData.Orders;
            FilteredOrderList = new ObservableCollection<Order>(Orders);

            // Default to desktop mode - will be updated by MainPage
            IsDesktopMode = true;
            IsMobileMode = false;
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
    }
}