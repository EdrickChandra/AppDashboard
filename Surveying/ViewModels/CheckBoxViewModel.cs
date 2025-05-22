using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System.ComponentModel;

namespace Surveying.ViewModels
{
    public partial class CheckboxViewModel : ObservableObject
    {
        public SurveyModel Survey { get; }

        public CheckboxViewModel()
        {

            Survey = new SurveyModel();
        }

        public CheckboxViewModel(SurveyModel survey)
        {
            Survey = survey;
            Survey.PropertyChanged += Survey_PropertyChanged;
            UpdateCleaningCheckboxes();
            UpdateRepairCheckboxes();
            UpdatePeriodicCheckboxes();
        }

        [ObservableProperty]
        private bool cleaningAccept;
        [ObservableProperty]
        private bool cleaningReject;

   
        [ObservableProperty]
        private bool repairAccept;
        [ObservableProperty]
        private bool repairReject;

        [ObservableProperty]
        private bool periodicAccept;
        [ObservableProperty]
        private bool periodicReject;

        [ObservableProperty]
        private string cleaningRejectionRemark;
        [ObservableProperty]
        private string repairRejectionRemark;
        [ObservableProperty]
        private string periodicRejectionRemark;

        partial void OnCleaningAcceptChanged(bool value)
        {
            if (value)
                CleaningReject = false;
        }

        partial void OnRepairAcceptChanged(bool value)
        {
            if (value)
                RepairReject = false;
        }

        partial void OnPeriodicAcceptChanged(bool value)
        {
            if (value)
                PeriodicReject = false;
        }

        partial void OnCleaningRejectChanged(bool value)
        {
            if (value)
            {
                CleaningAccept = false;
            }
            else
            {
                CleaningRejectionRemark = string.Empty;
            }
        }

        partial void OnRepairRejectChanged(bool value)
        {
            if (value)
            {
                RepairAccept = false;
            }
            else
            {
                RepairRejectionRemark = string.Empty;
            }
        }

        partial void OnPeriodicRejectChanged(bool value)
        {
            if (value)
            {
                PeriodicAccept = false;
            }
            else
            {
                PeriodicRejectionRemark = string.Empty;
            }
        }


        [RelayCommand]
        void SubmitCleaning()
        {
            if (CleaningAccept)
                Survey.CleaningStatus = StatusType.Finished;
            if (CleaningReject)
                Survey.CleaningStatus = StatusType.Rejected;
        }

        [RelayCommand]
        void SubmitRepair()
        {
            if (RepairAccept)
                Survey.RepairStatus = StatusType.Finished;
            if (RepairReject)
                Survey.RepairStatus = StatusType.Rejected;
        }

        [RelayCommand]
        void SubmitPeriodic()
        {
            if (PeriodicAccept)
                Survey.PeriodicStatus = StatusType.Finished;
            if (PeriodicReject)
                Survey.PeriodicStatus = StatusType.Rejected;
        }

        private void Survey_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SurveyModel.CleaningStatus))
            {
                UpdateCleaningCheckboxes();
            }
            else if (e.PropertyName == nameof(SurveyModel.RepairStatus))
            {
                UpdateRepairCheckboxes();
            }
            else if (e.PropertyName == nameof(SurveyModel.PeriodicStatus))
            {
                UpdatePeriodicCheckboxes();
            }
        }

        private void UpdateCleaningCheckboxes()
        {
            bool accepted = Survey.CleaningStatus == StatusType.Finished;
            bool rejected = Survey.CleaningStatus == StatusType.Rejected;
            if (CleaningAccept != accepted)
                CleaningAccept = accepted;
            if (CleaningReject != rejected)
                CleaningReject = rejected;
        }

        private void UpdateRepairCheckboxes()
        {
            bool accepted = Survey.RepairStatus == StatusType.Finished;
            bool rejected = Survey.RepairStatus == StatusType.Rejected;
            if (RepairAccept != accepted)
                RepairAccept = accepted;
            if (RepairReject != rejected)
                RepairReject = rejected;
        }

        private void UpdatePeriodicCheckboxes()
        {
            bool accepted = Survey.PeriodicStatus == StatusType.Finished;
            bool rejected = Survey.PeriodicStatus == StatusType.Rejected;
            if (PeriodicAccept != accepted)
                PeriodicAccept = accepted;
            if (PeriodicReject != rejected)
                PeriodicReject = rejected;
        }
    }
}
