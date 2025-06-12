using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Surveying.ViewModels;

    public partial class AddPageViewModel : ObservableObject
    {
        private readonly IContainerApiService _containerApiService;

        [ObservableProperty]
        private string orderNumber = string.Empty;

        [ObservableProperty]
        private string surveyor = string.Empty;

        [ObservableProperty]
        private string principalCode = string.Empty;

        [ObservableProperty]
        private string principalName = string.Empty;

        [ObservableProperty]
        private string shipperCode = string.Empty;

        [ObservableProperty]
        private string shipperName = string.Empty;

        // ===== CONTAINER INFO - SIMPLIFIED =====
        [ObservableProperty]
        private string contNumber = string.Empty;

        [ObservableProperty]
        private string contNumberError = string.Empty;

        [ObservableProperty]
        private bool isValidatingContainer;

        [ObservableProperty]
        private bool isContainerValid;

        [ObservableProperty]
        private string condition = string.Empty;

        // ===== DATE INFO - UNCHANGED =====
        [ObservableProperty]
        private DateTime orderDate = DateTime.Today;

        [ObservableProperty]
        private DateTime surveyDate = DateTime.Today;

        [ObservableProperty]
        private DateTime pickupDate = DateTime.Today;

        /
        public Order CurrentOrder { get; set; } = new Order();

        // For display in the grid - show individual containers being added
        public ObservableCollection<Container> ContainerEntries { get; } = new ObservableCollection<Container>();

        // Callback when submission is complete
        public Action<Order> OnSubmitCompleted { get; set; }

        // Condition list - unchanged
        public ObservableCollection<string> ConditionList => ConditionData.ConditionList;

        // ===== CONSTRUCTORS =====
        public AddPageViewModel() : this(new ContainerApiService())
        {
        }

        public AddPageViewModel(IContainerApiService containerApiService)
        {
            _containerApiService = containerApiService;
        }

        // ===== VALIDATION - SIMPLIFIED =====
        partial void OnContNumberChanged(string value)
        {
            _ = ValidateContainerNumberAsync();
        }

        private async Task ValidateContainerNumberAsync()
        {
            ContNumberError = string.Empty;

            if (string.IsNullOrWhiteSpace(ContNumber))
            {
                ContNumberError = "Container number is required";
                return;
            }

            // Clean up the input
            ContNumber = ContNumber.Trim().ToUpper().Replace(" ", "");

            // Simple format check: 4 letters + 7 digits
            if (ContNumber.Length != 11)
            {
                ContNumberError = "Must be 11 characters (4 letters + 7 digits)";
                return;
            }

            // Check first 4 are letters
            if (!ContNumber.Substring(0, 4).All(char.IsLetter))
            {
                ContNumberError = "First 4 characters must be letters";
                return;
            }

            // Check last 7 are digits
            if (!ContNumber.Substring(4, 7).All(char.IsDigit))
            {
                ContNumberError = "Last 7 characters must be digits";
                return;
            }

            // Check with API
            IsValidatingContainer = true;
            IsContainerValid = false;
            ContNumberError = "Checking container in depot...";

            try
            {
                var apiResponse = await _containerApiService.CheckContainerExists(ContNumber);

                if (apiResponse.IsSuccess)
                {
                    ContNumberError = string.Empty;
                    IsContainerValid = true;
                }
                else
                {
                    ContNumberError = apiResponse.Message ?? "Container not found in depot";
                    IsContainerValid = false;
                }
            }
            catch (Exception ex)
            {
                ContNumberError = $"Error checking container: {ex.Message}";
                IsContainerValid = false;
            }
            finally
            {
                IsValidatingContainer = false;
            }
        }

        // ===== ADD CONTAINER - SIMPLIFIED =====
        [RelayCommand]
        async Task AddContainerEntry()
        {
            // Wait for any ongoing validation
            if (IsValidatingContainer)
            {
                await Application.Current.MainPage.DisplayAlert("Please Wait",
                    "Container validation in progress...", "OK");
                return;
            }

            // Validate all required fields
            await ValidateContainerNumberAsync();

            if (!IsValidContainer() || !string.IsNullOrWhiteSpace(ContNumberError))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error",
                    "Please correct all errors before adding the container.", "OK");
                return;
            }

            // Create new container with simplified model
            var container = new Container
            {
                ContNumber = ContNumber,
                Condition = Condition,
                ContSize = "20", // Default - could be enhanced later
                ContType = "Tank",
                CustomerCode = "TBD", // Will be populated from API if needed
                DtmIn = DateTime.Now,

                // Initialize statuses
                CleaningStatus = StatusType.NotFilled,
                RepairStatus = StatusType.NotFilled,
                PeriodicStatus = StatusType.NotFilled,
                SurveyStatus = StatusType.NotFilled
            };

            // Update activities
            container.UpdateActivities();

            // Add to current order
            CurrentOrder.Containers.Add(container);

            // Also add to display collection
            ContainerEntries.Add(container);

            // Clear input fields
            ContNumber = string.Empty;
            Condition = string.Empty;
            OnPropertyChanged(nameof(ContNumber));
            OnPropertyChanged(nameof(Condition));
        }

        // ===== SUBMIT ORDER - SIMPLIFIED =====
        [RelayCommand]
        void Submit()
        {
            if (!IsValidOrder())
            {
                return;
            }

            // Update order with current info
            CurrentOrder.OrderNumber = OrderNumber;
            CurrentOrder.Surveyor = Surveyor;
            CurrentOrder.PrincipalCode = PrincipalCode;
            CurrentOrder.PrincipalName = PrincipalName;
            CurrentOrder.ShipperCode = ShipperCode;
            CurrentOrder.ShipperName = ShipperName;
            CurrentOrder.OrderDate = OrderDate;
            CurrentOrder.SurveyDate = SurveyDate;
            CurrentOrder.PickupDate = PickupDate;

            // Invoke callback with completed order
            OnSubmitCompleted?.Invoke(CurrentOrder);
        }

        // ===== VALIDATION HELPERS =====
        private bool IsValidContainer()
        {
            return !string.IsNullOrWhiteSpace(ContNumber) &&
                   !string.IsNullOrWhiteSpace(Condition) &&
                   IsContainerValid;
        }

        private bool IsValidOrder()
        {
            if (string.IsNullOrWhiteSpace(OrderNumber))
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Order number is required.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Surveyor))
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "Surveyor is required.", "OK");
                return false;
            }

            if (!ContainerEntries.Any())
            {
                Application.Current.MainPage.DisplayAlert("Validation Error", "At least one container is required.", "OK");
                return false;
            }

            return true;
        }
    }
}