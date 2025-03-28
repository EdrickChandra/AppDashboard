using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System;
using System.Collections.ObjectModel;

namespace Surveying.ViewModels
{
    public partial class AddPageViewModel : ObservableObject
    {
        // Input properties for the SurveyModel fields
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
        private DateTime orderDate = DateTime.Today;

        [ObservableProperty]
        private DateTime surveyDate = DateTime.Today;

        [ObservableProperty]
        private DateTime pickupDate = DateTime.Today;

        [ObservableProperty]
        private string condition;

        // Temporary collection to hold survey entries
        public ObservableCollection<SurveyModel> SurveyEntries { get; } = new ObservableCollection<SurveyModel>();

        // Callback action that will be set by the AddPage and invoked on submit.
        public Action<ObservableCollection<SurveyModel>> OnSubmitCompleted { get; set; }

        public ObservableCollection<string> ConditionList => ConditionData.ConditionList;

        [RelayCommand]
        void AddSurveyEntry()
        {
            // Ensure that required fields are provided.
            if (!string.IsNullOrWhiteSpace(OrderNumber) &&
                !string.IsNullOrWhiteSpace(Surveyor) &&
                !string.IsNullOrWhiteSpace(ContNumber) &&
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

                // Optionally clear some fields for the next entry
                ContNumber = string.Empty;
                Condition = string.Empty;
                OnPropertyChanged(nameof(ContNumber));
                OnPropertyChanged(nameof(Condition));
            }
        }

        [RelayCommand]
        void Submit()
        {
            // Invoke the callback to pass the survey entries to MainPage.
            OnSubmitCompleted?.Invoke(SurveyEntries);
        }
    }
}
