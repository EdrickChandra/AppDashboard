using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surveying.ViewModels
{
    public partial class AddPageViewModel : ObservableObject
    {
        private readonly IContainerApiService _containerApiService;

        [ObservableProperty]
        private string orderNumber;

        [ObservableProperty]
        private long principalId;

        [ObservableProperty]
        private string surveyor;

        [ObservableProperty]
        private long shipperId;

        [ObservableProperty]
        private string contNumber;

        [ObservableProperty]
        private string contNumberError;

        [ObservableProperty]
        private bool isValidatingContainer;

        [ObservableProperty]
        private bool isContainerValid;

        [ObservableProperty]
        private DateTime orderDate = DateTime.Today;

        [ObservableProperty]
        private DateTime surveyDate = DateTime.Today;

        [ObservableProperty]
        private DateTime pickupDate = DateTime.Today;

        [ObservableProperty]
        private string condition;

        public ObservableCollection<SurveyModel> SurveyEntries { get; } = new ObservableCollection<SurveyModel>();

        public Action<ObservableCollection<SurveyModel>> OnSubmitCompleted { get; set; }

        public ObservableCollection<string> ConditionList => ConditionData.ConditionList;

        public AddPageViewModel() : this(new ContainerApiService())
        {
        }

        public AddPageViewModel(IContainerApiService containerApiService)
        {
            _containerApiService = containerApiService;
        }

        partial void OnContNumberChanged(string value)
        {
            _ = ValidateContainerNumberAsync();
        }

        // Replace the ValidateContainerNumberAsync method in AddPageViewModel.cs

        private async Task ValidateContainerNumberAsync()
        {
            // Clear previous errors
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

            // Skip complex check digit validation - just check with API
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

  

        [RelayCommand]
        async Task AddSurveyEntry()
        {
            // Wait for any ongoing validation to complete
            if (IsValidatingContainer)
            {
                await Application.Current.MainPage.DisplayAlert("Please Wait",
                    "Container validation in progress...", "OK");
                return;
            }

            // Validate container number first
            await ValidateContainerNumberAsync();

            if (!string.IsNullOrWhiteSpace(OrderNumber) &&
                !string.IsNullOrWhiteSpace(Surveyor) &&
                !string.IsNullOrWhiteSpace(ContNumber) &&
                string.IsNullOrWhiteSpace(ContNumberError) && // Make sure there's no validation error
                !string.IsNullOrWhiteSpace(Condition))
            {
                var survey = new SurveyModel
                {
                    OrderNumber = OrderNumber,
                    PrincipalId = PrincipalId,
                    Surveyor = Surveyor,
                    ShipperId = ShipperId,
                    ContNumber = ContNumber,
                    OrderDate = OrderDate,
                    SurveyDate = SurveyDate,
                    PickupDate = PickupDate,
                    Condition = Condition
                };

                SurveyEntries.Add(survey);
                ContNumber = string.Empty;
                Condition = string.Empty;
                OnPropertyChanged(nameof(ContNumber));
                OnPropertyChanged(nameof(Condition));
            }
            else
            {
                // Show an error message if validation fails
                await Application.Current.MainPage.DisplayAlert("Validation Error",
                    "Please correct all errors before adding the entry.", "OK");
            }
        }

        [RelayCommand]
        void Submit()
        {
            OnSubmitCompleted?.Invoke(SurveyEntries);
        }
    }
}