using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Surveying.Models
{
    // Activity class for the third level of the master-details view
    public class ActivityModel : ObservableObject
    {
        private string _activityName;
        private string _activityType;
        private StatusType _status;

        public string ActivityName
        {
            get => _activityName;
            set => SetProperty(ref _activityName, value);
        }

        public string ActivityType
        {
            get => _activityType;
            set => SetProperty(ref _activityType, value);
        }

        public StatusType Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public ActivityModel(string name, string type, StatusType status)
        {
            _activityName = name;
            _activityType = type;
            _status = status;
        }
    }

    // Enhanced ContainerModel for the second level of the master-details view
    public class ContainerDetailModel : ContModel, INotifyPropertyChanged
    {
        public string Condition { get; set; }

        private StatusType _cleaningStatus = StatusType.NotFilled;
        public StatusType CleaningStatus
        {
            get => _cleaningStatus;
            set
            {
                if (_cleaningStatus != value)
                {
                    _cleaningStatus = value;
                    OnPropertyChanged(nameof(CleaningStatus));
                    UpdateActivityStatus("Cleaning", value);
                }
            }
        }

        private StatusType _repairStatus = StatusType.NotFilled;
        public StatusType RepairStatus
        {
            get => _repairStatus;
            set
            {
                if (_repairStatus != value)
                {
                    _repairStatus = value;
                    OnPropertyChanged(nameof(RepairStatus));
                    UpdateActivityStatus("Repair", value);
                }
            }
        }

        private StatusType _periodicStatus = StatusType.NotFilled;
        public StatusType PeriodicStatus
        {
            get => _periodicStatus;
            set
            {
                if (_periodicStatus != value)
                {
                    _periodicStatus = value;
                    OnPropertyChanged(nameof(PeriodicStatus));
                    UpdateActivityStatus("Periodic", value);
                }
            }
        }

        private StatusType _surveyStatus = StatusType.NotFilled;
        public StatusType SurveyStatus
        {
            get => _surveyStatus;
            set
            {
                if (_surveyStatus != value)
                {
                    _surveyStatus = value;
                    OnPropertyChanged(nameof(SurveyStatus));
                    UpdateActivityStatus("Survey", value);
                }
            }
        }

        // Collection for the third level (activities)
        public ObservableCollection<ActivityModel> Activities { get; set; }

        public ContainerDetailModel() : base()
        {
            Activities = new ObservableCollection<ActivityModel>();

            // Initialize activities with current status values
            InitializeActivities();
        }

        // Explicitly implement INotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        // Update a specific activity's status
        private void UpdateActivityStatus(string activityType, StatusType status)
        {
            var activity = Activities.FirstOrDefault(a => a.ActivityType == activityType);
            if (activity != null)
            {
                activity.Status = status;
                System.Diagnostics.Debug.WriteLine($"Updated {activityType} activity status to {status}");
            }
        }

        // Initialize the activities collection
        private void InitializeActivities()
        {
            Activities.Clear();
            Activities.Add(new ActivityModel("Cleaning", "Cleaning", CleaningStatus));
            Activities.Add(new ActivityModel("Repair", "Repair", RepairStatus));
            Activities.Add(new ActivityModel("Periodic", "Periodic", PeriodicStatus));
            Activities.Add(new ActivityModel("Survey", "Survey", SurveyStatus));
        }

        // Method maintained for backward compatibility
        public void UpdateActivities()
        {
            // Check if activities are empty
            if (Activities.Count == 0)
            {
                InitializeActivities();
                return;
            }

            // Directly update specific activities without clearing the collection
            UpdateActivityStatus("Cleaning", CleaningStatus);
            UpdateActivityStatus("Repair", RepairStatus);
            UpdateActivityStatus("Periodic", PeriodicStatus);
            UpdateActivityStatus("Survey", SurveyStatus);
        }
    }
}