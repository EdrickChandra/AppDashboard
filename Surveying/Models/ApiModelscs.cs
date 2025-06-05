using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Surveying.Models
{
    // API Response wrapper
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Content { get; set; }
    }

    // Container model from API
    public class ContainerApiModel
    {
        public long Id { get; set; }
        public string ContNumber { get; set; }
        public DateTime? DtmIn { get; set; }
        public string CustomerCode { get; set; }

        // Approval properties
        public bool IsRepairApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
    }

    // Container with repair codes
    public class ContainerWithRepairCodesModel
    {
        public long Id { get; set; }
        public string ContNumber { get; set; }
        public DateTime? DtmIn { get; set; }
        public string CustomerCode { get; set; }
        public List<RepairCodeModel> RepairCodes { get; set; } = new List<RepairCodeModel>();

        // Approval properties
        public bool IsRepairApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
    }

    // Enhanced repair code details with individual status tracking
    public partial class RepairCodeModel : ObservableObject
    {
        // Your existing properties
        public string RepairCode { get; set; }
        public string ComponentCode { get; set; }
        public string LocationCode { get; set; }
        public string ComponentDescription { get; set; }
        public string ComponentCategory { get; set; }

        // NEW: Add the repair detail description
        public string RepairDetailDescription { get; set; } = string.Empty;

        // NEW: Add repair code description
        public string RepairCodeDescription { get; set; } = string.Empty;

        // NEW: Add component code description  
        public string ComponentCodeDescription { get; set; } = string.Empty;

        [ObservableProperty]
        private bool isCompleted;

        [ObservableProperty]
        private DateTime? completedDate;

        [ObservableProperty]
        private string completedBy = string.Empty;

        [ObservableProperty]
        private string repairNotes = string.Empty;

        // Computed properties for UI binding
        public string StatusText => IsCompleted ? "COMPLETED" : "PENDING";

        public string StatusColor => IsCompleted ? "#28A745" : "#FFC107"; // Green : Orange

        public string ButtonText => IsCompleted ? "Mark Pending" : "Mark Done";

        public string ButtonColor => IsCompleted ? "#6C757D" : "#007BFF"; // Gray : Blue

        // Override OnPropertyChanged to notify UI when dependent properties change
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // When IsCompleted changes, notify UI about dependent properties
            if (e.PropertyName == nameof(IsCompleted))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(ButtonText));
                OnPropertyChanged(nameof(ButtonColor));
            }
        }
    }
}