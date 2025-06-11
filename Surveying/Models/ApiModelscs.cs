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

    // Container with repair codes - UPDATED: Now includes individual cleaning requirements
    public class ContainerWithRepairCodesModel
    {
        public long Id { get; set; }
        public string ContNumber { get; set; } = string.Empty;
        public DateTime? DtmIn { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public List<RepairCodeModel> RepairCodes { get; set; } = new List<RepairCodeModel>();

        // Approval properties
        public bool IsRepairApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;

        // Additional properties for cleaning
        public DateTime? CleaningStartDate { get; set; }
        public DateTime? CleaningCompleteDate { get; set; }

        // Commodity information
        public string Commodity { get; set; } = string.Empty;

        // NEW: Individual container cleaning requirements
        public string CleaningRequirementsText { get; set; } = string.Empty;
        public string CleaningRequirementsJson { get; set; } = string.Empty;
    }

    public partial class RepairCodeModel : ObservableObject
    {
        // Your existing properties - fixed property names
        public string RepairCode { get; set; } = string.Empty;
        public string ComponentCode { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string ComponentDescription { get; set; } = string.Empty; // This is the correct property name
        public string ComponentCategory { get; set; } = string.Empty;

        // NEW: Additional description properties to match API
        public string RepairCodeDescription { get; set; } = string.Empty;
        public string ComponentCodeDescription { get; set; } = string.Empty;
        public string RepairDetailDescription { get; set; } = string.Empty;

        [ObservableProperty]
        private bool isCompleted;

        [ObservableProperty]
        private DateTime? completedDate;

        [ObservableProperty]
        private string completedBy = string.Empty;

        [ObservableProperty]
        private string repairNotes = string.Empty;

        [ObservableProperty]
        private ImageSource repairPhoto;

        [ObservableProperty]
        private bool hasPhoto;

        public string StatusText => IsCompleted ? "COMPLETED" : "PENDING";

        public string StatusColor => IsCompleted ? "#28A745" : "#FFC107";

        public string ButtonText => IsCompleted ? "Mark Pending" : "Mark Done";

        public string ButtonColor => IsCompleted ? "#6C757D" : "#007BFF";

        // Backward compatibility properties
        public string RepairCodeDescription_Legacy
        {
            get => RepairCodeDescription;
            set => RepairCodeDescription = value;
        }

        public string ComponentCodeDescription_Legacy
        {
            get => ComponentCodeDescription;
            set => ComponentCodeDescription = value;
        }

        public string RepairDetailDescription_Legacy
        {
            get => RepairDetailDescription;
            set => RepairDetailDescription = value;
        }

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