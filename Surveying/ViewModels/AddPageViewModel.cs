﻿using CommunityToolkit.Mvvm.ComponentModel;
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

        private async Task ValidateContainerNumberAsync()
        {
            // Clear previous errors
            ContNumberError = string.Empty;

            if (string.IsNullOrWhiteSpace(ContNumber))
            {
                ContNumberError = "Container number is required";
                return;
            }

            // Trim and convert to uppercase
            ContNumber = ContNumber.Trim().ToUpper();

            // Check format: 4 letters followed by 6 digits and 1 check digit
            if (!Regex.IsMatch(ContNumber, @"^[A-Z]{4}\d{7}$"))
            {
                ContNumberError = "Invalid format. Required: 4 letters + 6 digits + 1 check digit";
                return;
            }

            // Calculate the check digit and verify it matches
            char lastDigit = ContNumber[10];
            if (!char.IsDigit(lastDigit))
            {
                ContNumberError = "Last character must be a digit";
                return;
            }

            int providedCheckDigit = int.Parse(lastDigit.ToString());
            int calculatedCheckDigit = CalculateCheckDigit(ContNumber.Substring(0, 10));

            if (providedCheckDigit != calculatedCheckDigit)
            {
                ContNumberError = $"Invalid check digit. Should be {calculatedCheckDigit}";
                return;
            }

            // If format validation passes, check with API
            IsValidatingContainer = true;
            IsContainerValid = false;
            ContNumberError = "Checking container in depot...";

            try
            {
                var apiResponse = await _containerApiService.CheckContainerExists(ContNumber);

                if (apiResponse.IsSuccess)
                {
                    ContNumberError = string.Empty; // Clear error - container is valid
                    IsContainerValid = true;
                    System.Diagnostics.Debug.WriteLine($"Container {ContNumber} found in depot");
                }
                else
                {
                    ContNumberError = apiResponse.Message ?? "Container not found in depot or already left";
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

        private int CalculateCheckDigit(string containerPrefix)
        {
            // Ensure we have exactly 10 characters (4 letters + 6 digits)
            if (containerPrefix.Length != 10)
                return -1;

            int sum = 0;
            int[] multipliers = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 };

            for (int i = 0; i < 10; i++)
            {
                char c = containerPrefix[i];
                int value;

                if (char.IsLetter(c))
                {
                    // Convert letter to corresponding value (A=10, B=11, ..., Z=35)
                    value = c - 'A' + 10;
                }
                else if (char.IsDigit(c))
                {
                    // Convert digit character to numeric value
                    value = c - '0';
                }
                else
                {
                    // Invalid character
                    return -1;
                }

                sum += value * multipliers[i];
            }

            // The check digit is the remainder when dividing by 11
            // If the remainder is 10, the check digit is 0
            int checkDigit = sum % 11;
            if (checkDigit == 10)
                checkDigit = 0;

            return checkDigit;
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