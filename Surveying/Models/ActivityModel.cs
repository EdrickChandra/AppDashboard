using System;
using System.Collections.ObjectModel;

namespace Surveying.Models
{
    // Activity class for the third level of the master-details view
    public class ActivityModel
    {
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public StatusType Status { get; set; }

        public ActivityModel(string name, string type, StatusType status)
        {
            ActivityName = name;
            ActivityType = type;
            Status = status;
        }
    }

    // Enhanced ContainerModel for the second level of the master-details view
    public class ContainerDetailModel : ContModel
    {
        public string Condition { get; set; }
        public StatusType CleaningStatus { get; set; } = StatusType.NotFilled;
        public StatusType RepairStatus { get; set; } = StatusType.NotFilled;
        public StatusType PeriodicStatus { get; set; } = StatusType.NotFilled;
        public StatusType SurveyStatus { get; set; } = StatusType.NotFilled;

        // Collection for the third level (activities)
        public ObservableCollection<ActivityModel> Activities { get; set; }

        public ContainerDetailModel() : base()
        {
            Activities = new ObservableCollection<ActivityModel>();
        }

        // Method to update activities based on status
        public void UpdateActivities()
        {
            Activities.Clear();

            Activities.Add(new ActivityModel("Cleaning", "Cleaning", CleaningStatus));
            Activities.Add(new ActivityModel("Repair", "Repair", RepairStatus));
            Activities.Add(new ActivityModel("Periodic", "Periodic", PeriodicStatus));
            Activities.Add(new ActivityModel("Survey", "Survey", SurveyStatus));
        }
    }
}