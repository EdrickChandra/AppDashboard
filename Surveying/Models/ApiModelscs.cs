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

    public partial class RepairCodeModel : ObservableObject
    {
        // Your existing properties - fixed property names
        public string RepairCode { get; set; }
        public string ComponentCode { get; set; }
        public string LocationCode { get; set; }
        public string ComponentDescription { get; set; } // This is the correct property name
        public string ComponentCategory { get; set; }

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


        public string RepairCodeDescription
        {
            get => RepairCode;
            set => RepairCode = value;
        }

        public string ComponentCodeDescription
        {
            get => ComponentCode;
            set => ComponentCode = value;
        }

        public string RepairDetailDescription
        {
            get => ComponentDescription;
            set => ComponentDescription = value;
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