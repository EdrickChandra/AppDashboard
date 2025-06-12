using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Surveying.Models
{
    /// <summary>
    /// UNIFIED REPAIR CODE CLASS
    /// Replaces: RepairCodeModel (from ApiModelscs.cs)
    /// Removes: All "_Legacy" duplicate properties
    /// </summary>
    public partial class RepairCode : ObservableObject
    {
        // ===== CORE API PROPERTIES (from original RepairCodeModel) =====
        public string Code { get; set; } = string.Empty;  // Was: RepairCode
        public string ComponentCode { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;

        // ===== DESCRIPTION PROPERTIES (consolidated from duplicates) =====
        // OLD had: RepairCodeDescription + RepairCodeDescription_Legacy
        public string Description { get; set; } = string.Empty;  // Was: RepairCodeDescription

        // OLD had: ComponentCodeDescription + ComponentCodeDescription_Legacy  
        public string ComponentDescription { get; set; } = string.Empty;  // Was: ComponentCodeDescription

        // OLD had: RepairDetailDescription + RepairDetailDescription_Legacy
        public string DetailDescription { get; set; } = string.Empty;  // Was: RepairDetailDescription

        // Keep this existing property for backward compatibility if needed
        public string ComponentCategory { get; set; } = string.Empty;

        // ===== STATUS TRACKING PROPERTIES (from original RepairCodeModel) =====
        [ObservableProperty]
        private bool isCompleted;

        [ObservableProperty]
        private DateTime? completedDate;

        [ObservableProperty]
        private string completedBy = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;  // Was: repairNotes

        // ===== PHOTO PROPERTIES (from original RepairCodeModel) =====
        [ObservableProperty]
        private ImageSource photo;  // Was: repairPhoto

        [ObservableProperty]
        private bool hasPhoto;

        // ===== UI COMPUTED PROPERTIES (from original RepairCodeModel - but cleaner) =====
        [JsonIgnore]
        public string StatusText => IsCompleted ? "COMPLETED" : "PENDING";

        [JsonIgnore]
        public string StatusColor => IsCompleted ? "#28A745" : "#FFC107";

        [JsonIgnore]
        public string ButtonText => IsCompleted ? "Mark Pending" : "Mark Done";

        [JsonIgnore]
        public string ButtonColor => IsCompleted ? "#6C757D" : "#007BFF";

        // ===== FORMATTED DISPLAY PROPERTIES (improved from original) =====
        [JsonIgnore]
        public string FormattedCode => string.IsNullOrEmpty(Description)
            ? Code
            : $"{Code} - {Description}";

        [JsonIgnore]
        public string FormattedComponent => string.IsNullOrEmpty(ComponentDescription)
            ? ComponentCode
            : $"{ComponentCode} - {ComponentDescription}";

        [JsonIgnore]
        public string FullDescription
        {
            get
            {
                var parts = new List<string>();

                if (!string.IsNullOrEmpty(Code)) parts.Add($"Code: {Code}");
                if (!string.IsNullOrEmpty(Description)) parts.Add($"Type: {Description}");
                if (!string.IsNullOrEmpty(ComponentDescription)) parts.Add($"Component: {ComponentDescription}");
                if (!string.IsNullOrEmpty(DetailDescription)) parts.Add($"Details: {DetailDescription}");

                return string.Join(" | ", parts);
            }
        }
    }
}