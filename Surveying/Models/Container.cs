using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Surveying.Models
{
    /// <summary>
    /// UNIFIED CONTAINER CLASS
    /// Replaces: ContModel, ContainerApiModel, ContainerWithRepairCodesModel, ContainerDetailModel, ContainerWithRepairCodesModelExtended
    /// </summary>
    public partial class Container : ObservableObject
    {
        // ===== FROM ContainerApiModel (API properties) =====
        public long Id { get; set; }
        public string ContNumber { get; set; } = string.Empty;
        public DateTime? DtmIn { get; set; }
        public string CustomerCode { get; set; } = string.Empty;

        // ===== FROM ContModel (basic container info) =====
        public string ContSize { get; set; } = "20";
        public string ContType { get; set; } = "Tank";

        // ===== FROM ContainerWithRepairCodesModel (repair/cleaning info) =====
        public bool IsRepairApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? CleaningStartDate { get; set; }
        public DateTime? CleaningCompleteDate { get; set; }
        public string Commodity { get; set; } = string.Empty;
        public string CleaningRequirementsText { get; set; } = string.Empty;
        public List<RepairCode> RepairCodes { get; set; } = new List<RepairCode>();

        // ===== FROM ContainerWithRepairCodesModelExtended (UI display) =====
        [JsonIgnore] // Don't send to API
        [ObservableProperty]
        private int rowNumber;

        // ===== FROM ContainerDetailModel (status tracking - UI only) =====
        [JsonIgnore] // Don't send to API
        [ObservableProperty]
        private StatusType cleaningStatus = StatusType.NotFilled;

        [JsonIgnore] // Don't send to API
        [ObservableProperty]
        private StatusType repairStatus = StatusType.NotFilled;

        [JsonIgnore] // Don't send to API
        [ObservableProperty]
        private StatusType periodicStatus = StatusType.NotFilled;

        [JsonIgnore] // Don't send to API
        [ObservableProperty]
        private StatusType surveyStatus = StatusType.NotFilled;

        // ===== FROM ContainerDetailModel (activities collection - UI only) =====
        [JsonIgnore] // Don't send to API
        public ObservableCollection<Activity> Activities { get; set; } = new ObservableCollection<Activity>();

        // ===== COMPUTED PROPERTIES (for UI display) =====
        [JsonIgnore]
        public string StatusDisplay => $"{GetSymbol(CleaningStatus)}|{GetSymbol(RepairStatus)}|{GetSymbol(PeriodicStatus)}|{GetSymbol(SurveyStatus)}";

        [JsonIgnore]
        public string ApprovalStatusText => IsRepairApproved ? "Approved" : "Pending";

        [JsonIgnore]
        public string ApprovalStatusColor => IsRepairApproved ? "#28A745" : "#FFC107";

        // ===== METHODS (from ContainerDetailModel) =====
        private string GetSymbol(StatusType status) => status switch
        {
            StatusType.NotFilled => "*",
            StatusType.OnReview => "R",
            StatusType.Finished => "A",
            StatusType.Rejected => "X",
            _ => ""
        };

        public void UpdateActivities()
        {
            Activities.Clear();
            Activities.Add(new Activity("Cleaning", "Cleaning", CleaningStatus));
            Activities.Add(new Activity("Repair", "Repair", RepairStatus));
            Activities.Add(new Activity("Periodic", "Periodic", PeriodicStatus));
            Activities.Add(new Activity("Survey", "Survey", SurveyStatus));
        }

        // Auto-update overall status when individual statuses change
        partial void OnCleaningStatusChanged(StatusType value) => UpdateOverallStatus();
        partial void OnRepairStatusChanged(StatusType value) => UpdateOverallStatus();
        partial void OnPeriodicStatusChanged(StatusType value) => UpdateOverallStatus();

        private void UpdateOverallStatus()
        {
            if (CleaningStatus != StatusType.NotFilled &&
                RepairStatus != StatusType.NotFilled &&
                PeriodicStatus != StatusType.NotFilled)
            {
                SurveyStatus = (CleaningStatus == StatusType.Finished &&
                               RepairStatus == StatusType.Finished &&
                               PeriodicStatus == StatusType.Finished)
                               ? StatusType.Finished
                               : StatusType.OnReview;
            }
        }
    }

    // Status enum - keep this unchanged
    public enum StatusType
    {
        NotFilled,
        OnReview,
        Finished,
        Rejected
    }
}