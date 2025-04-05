using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Surveying.ViewModels
{
    public partial class AddPageViewModel : ObservableObject
    {
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
        private string contNumberError; // Error message for validation

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

        partial void OnContNumberChanged(string value)
        {
            // Example: 4 letters + 7 digits
            var regex = new Regex(@"^[A-Za-z]{4}[0-9]{7}$");
            if (!string.IsNullOrWhiteSpace(value) && !regex.IsMatch(value))
            {
                ContNumberError = "Container number must be AAAANNNNNNN (4 letters, 7 digits).";
            }
            else
            {
                ContNumberError = "";
            }
        }

        [RelayCommand]
        void AddSurveyEntry()
        {
            // Ensure required fields are provided and no validation error exists.
            if (!string.IsNullOrWhiteSpace(OrderNumber) &&
                !string.IsNullOrWhiteSpace(Surveyor) &&
                !string.IsNullOrWhiteSpace(ContNumber) &&
                string.IsNullOrWhiteSpace(ContNumberError) &&
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
        }

        [RelayCommand]
        void Submit()
        {
            OnSubmitCompleted?.Invoke(SurveyEntries);
        }
    }
}
