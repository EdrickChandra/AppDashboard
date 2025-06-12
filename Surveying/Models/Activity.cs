using CommunityToolkit.Mvvm.ComponentModel;

namespace Surveying.Models
{
    /// <summary>
    /// SIMPLIFIED ACTIVITY CLASS
    /// Replaces: ActivityModel (from ActivityModel.cs)
    /// Simplifies: Removes complex update logic - activities are now computed from Container status
    /// </summary>
    public partial class Activity : ObservableObject
    {
        // ===== CORE PROPERTIES (from ActivityModel) =====
        public string Name { get; set; } = string.Empty;        // Was: ActivityName
        public string Type { get; set; } = string.Empty;        // Was: ActivityType

        [ObservableProperty]
        private StatusType status;

        // ===== CONSTRUCTORS =====
        public Activity()
        {
        }

        public Activity(string name, string type, StatusType status)
        {
            Name = name;
            Type = type;
            Status = status;
        }

        // ===== COMPUTED PROPERTIES FOR UI =====
        public string DisplayText => $"Go to {Type}";

        public bool IsActionable => Status != StatusType.Finished;

        public string StatusDescription => Status switch
        {
            StatusType.NotFilled => "Not Started",
            StatusType.OnReview => "In Review",
            StatusType.Finished => "Completed",
            StatusType.Rejected => "Rejected",
            _ => "Unknown"
        };
    }
}