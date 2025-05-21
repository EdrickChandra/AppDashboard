using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System.ComponentModel;

namespace Surveying.ViewModels
{
    public partial class SurveyorViewModel : BaseViewModel
    {
        public SurveyModel Survey { get; }
        public ContainerDetailModel Container { get; }

        public string ContainerNumber => Container?.ContNumber ?? Survey.ContNumber;

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
        private string cleaningRejectionRemark = "";

        [ObservableProperty]
        private string repairRejectionRemark = "";

        [ObservableProperty]
        private string periodicRejectionRemark = "";

        // New properties to track if items are ready for review
        [ObservableProperty]
        private bool cleaningReadyForReview;

        [ObservableProperty]
        private bool repairReadyForReview;

        [ObservableProperty]
        private bool periodicReadyForReview;

        // Constructor with survey and container
        public SurveyorViewModel(SurveyModel survey, ContainerDetailModel container)
        {
            Survey = survey;
            Container = container;

            // Initialize checkbox states and review status
            UpdateStatesFromContainer();

            if (Container != null)
            {
                // Subscribe to container property changes
                Container.PropertyChanged += Container_PropertyChanged;
            }
            else
            {
                // If no container, use survey for backward compatibility
                UpdateStatesFromSurvey();
                Survey.PropertyChanged += Survey_PropertyChanged;
            }
        }

        private void Container_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ContainerDetailModel.CleaningStatus) ||
                e.PropertyName == nameof(ContainerDetailModel.RepairStatus) ||
                e.PropertyName == nameof(ContainerDetailModel.PeriodicStatus))
            {
                UpdateStatesFromContainer();
            }
        }

        private void Survey_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SurveyModel.CleaningStatus) ||
                e.PropertyName == nameof(SurveyModel.RepairStatus) ||
                e.PropertyName == nameof(SurveyModel.PeriodicStatus))
            {
                UpdateStatesFromSurvey();
            }
        }

        private void UpdateStatesFromContainer()
        {
            if (Container == null) return;

            // Update cleaning checkboxes and review status
            CleaningReadyForReview = Container.CleaningStatus == StatusType.OnReview;
            CleaningAccept = Container.CleaningStatus == StatusType.Accepted;
            CleaningReject = Container.CleaningStatus == StatusType.Rejected;

            // Update repair checkboxes and review status
            RepairReadyForReview = Container.RepairStatus == StatusType.OnReview;
            RepairAccept = Container.RepairStatus == StatusType.Accepted;
            RepairReject = Container.RepairStatus == StatusType.Rejected;

            // Update periodic checkboxes and review status
            PeriodicReadyForReview = Container.PeriodicStatus == StatusType.OnReview;
            PeriodicAccept = Container.PeriodicStatus == StatusType.Accepted;
            PeriodicReject = Container.PeriodicStatus == StatusType.Rejected;
        }

        private void UpdateStatesFromSurvey()
        {
            // Update cleaning checkboxes and review status
            CleaningReadyForReview = Survey.CleaningStatus == StatusType.OnReview;
            CleaningAccept = Survey.CleaningStatus == StatusType.Accepted;
            CleaningReject = Survey.CleaningStatus == StatusType.Rejected;

            // Update repair checkboxes and review status
            RepairReadyForReview = Survey.RepairStatus == StatusType.OnReview;
            RepairAccept = Survey.RepairStatus == StatusType.Accepted;
            RepairReject = Survey.RepairStatus == StatusType.Rejected;

            // Update periodic checkboxes and review status
            PeriodicReadyForReview = Survey.PeriodicStatus == StatusType.OnReview;
            PeriodicAccept = Survey.PeriodicStatus == StatusType.Accepted;
            PeriodicReject = Survey.PeriodicStatus == StatusType.Rejected;
        }

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
            // Only allow submission if status is OnReview
            if (!CleaningReadyForReview)
            {
                Application.Current.MainPage.DisplayAlert("Not Ready",
                    "This item is not ready for review. Make sure Cleaning has been submitted first.", "OK");
                return;
            }

            if (CleaningAccept)
            {
                if (Container != null)
                {
                    Container.CleaningStatus = StatusType.Accepted;
                }
                // Also update survey
                Survey.CleaningStatus = StatusType.Accepted;
            }
            else if (CleaningReject)
            {
                if (Container != null)
                {
                    Container.CleaningStatus = StatusType.Rejected;
                    // Store the rejection remark in the container
                    // Container.CleaningRejectionRemark = CleaningRejectionRemark;
                }
                // Also update survey
                Survey.CleaningStatus = StatusType.Rejected;
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Selection Required",
                    "Please select either Accept or Reject before submitting.", "OK");
            }
        }

        [RelayCommand]
        void SubmitRepair()
        {
            // Only allow submission if status is OnReview
            if (!RepairReadyForReview)
            {
                Application.Current.MainPage.DisplayAlert("Not Ready",
                    "This item is not ready for review. Make sure Repair has been submitted first.", "OK");
                return;
            }

            if (RepairAccept)
            {
                if (Container != null)
                {
                    Container.RepairStatus = StatusType.Accepted;
                }
                // Also update survey
                Survey.RepairStatus = StatusType.Accepted;
            }
            else if (RepairReject)
            {
                if (Container != null)
                {
                    Container.RepairStatus = StatusType.Rejected;
                    // Store the rejection remark
                    // Container.RepairRejectionRemark = RepairRejectionRemark;
                }
                // Also update survey
                Survey.RepairStatus = StatusType.Rejected;
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Selection Required",
                    "Please select either Accept or Reject before submitting.", "OK");
            }
        }

        [RelayCommand]
        void SubmitPeriodic()
        {
            // Only allow submission if status is OnReview
            if (!PeriodicReadyForReview)
            {
                Application.Current.MainPage.DisplayAlert("Not Ready",
                    "This item is not ready for review. Make sure Periodic Maintenance has been submitted first.", "OK");
                return;
            }

            if (PeriodicAccept)
            {
                if (Container != null)
                {
                    Container.PeriodicStatus = StatusType.Accepted;
                }
                // Also update survey
                Survey.PeriodicStatus = StatusType.Accepted;
            }
            else if (PeriodicReject)
            {
                if (Container != null)
                {
                    Container.PeriodicStatus = StatusType.Rejected;
                    // Store the rejection remark
                    // Container.PeriodicRejectionRemark = PeriodicRejectionRemark;
                }
                // Also update survey
                Survey.PeriodicStatus = StatusType.Rejected;
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Selection Required",
                    "Please select either Accept or Reject before submitting.", "OK");
            }
        }
    }
}