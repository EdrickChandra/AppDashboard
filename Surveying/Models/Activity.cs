using CommunityToolkit.Mvvm.ComponentModel;

namespace Surveying.Models

{

    public enum ActivityType
    {
        Cleaning,
        Repair,
        Periodic,
        Survey
    }

  
    public partial class Activity : ObservableObject
    {
        // ===== CORE PROPERTIES (from ActivityModel) =====
        public string Name { get; set; } = string.Empty;       
        public string Type { get; set; } = string.Empty;      

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