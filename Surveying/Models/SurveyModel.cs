using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Surveying.Models
{
    public enum StatusType
    {
        NotFilled,  // Will be displayed as "*"
        OnReview,   // Will be displayed as "R"
        Finished,   // Will be displayed as "A"
        Rejected    // Will be displayed as "X"
    }

    public partial class SurveyModel : ObservableObject
    {
        public string OrderNumber { get; set; }
        public long PrincipalId { get; set; }
        public string Surveyor { get; set; }
        public long ShipperId { get; set; }
        public string ContNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime SurveyDate { get; set; }
        public DateTime PickupDate { get; set; }
        public string Condition { get; set; }

        // New property to support containers for the master-detail view
        [ObservableProperty]
        private ObservableCollection<ContainerDetailModel> containers;

        public SurveyModel()
        {
            Containers = new ObservableCollection<ContainerDetailModel>();
        }

        public SurveyModel(string orderNumber, long principalId, string surveyor, long shipperId, string contNumber,
                           DateTime orderDate, DateTime surveyDate, DateTime pickupDate, string condition)
        {
            OrderNumber = orderNumber;
            PrincipalId = principalId;
            Surveyor = surveyor;
            ShipperId = shipperId;
            ContNumber = contNumber;
            OrderDate = orderDate;
            SurveyDate = surveyDate;
            PickupDate = pickupDate;
            Condition = condition;

            // Initialize Containers collection
            Containers = new ObservableCollection<ContainerDetailModel>();

            // Find the container in DummyData and add it to the Containers collection
            var containerInfo = DummyData.Containers.FirstOrDefault(c => c.ContNumber == contNumber);
            if (containerInfo != null)
            {
                // Create a new ContainerDetailModel with enhanced properties for the detail view
                var container = new ContainerDetailModel
                {
                    ContNumber = contNumber,
                    ContSize = containerInfo.ContSize,
                    ContType = containerInfo.ContType,
                    Condition = condition,
                    CleaningStatus = CleaningStatus,
                    RepairStatus = RepairStatus,
                    PeriodicStatus = PeriodicStatus,
                    SurveyStatus = SurveyStatus
                };

                // Set up property change notifications
                PropertyChanged += (s, e) => {
                    if (e.PropertyName == nameof(CleaningStatus))
                    {
                        container.CleaningStatus = CleaningStatus;
                        container.UpdateActivities();
                    }
                    else if (e.PropertyName == nameof(RepairStatus))
                    {
                        container.RepairStatus = RepairStatus;
                        container.UpdateActivities();
                    }
                    else if (e.PropertyName == nameof(PeriodicStatus))
                    {
                        container.PeriodicStatus = PeriodicStatus;
                        container.UpdateActivities();
                    }
                    else if (e.PropertyName == nameof(SurveyStatus))
                    {
                        container.SurveyStatus = SurveyStatus;
                        container.UpdateActivities();
                    }
                };

                // Initialize activities
                container.UpdateActivities();

                Containers.Add(container);
            }
        }

        // Helper method to add a container to this survey
        public void AddContainer(string contNumber, string condition)
        {
            var containerInfo = DummyData.Containers.FirstOrDefault(c => c.ContNumber == contNumber);
            if (containerInfo != null)
            {
                var container = new ContainerDetailModel
                {
                    ContNumber = contNumber,
                    ContSize = containerInfo.ContSize,
                    ContType = containerInfo.ContType,
                    Condition = condition,
                    CleaningStatus = StatusType.NotFilled,
                    RepairStatus = StatusType.NotFilled,
                    PeriodicStatus = StatusType.NotFilled,
                    SurveyStatus = StatusType.NotFilled
                };

                // Initialize activities
                container.UpdateActivities();

                Containers.Add(container);
            }
        }

        public string PrincipalCode
        {
            get { return DummyData.Principals.Where(w => w.Id == PrincipalId).FirstOrDefault()?.Code ?? ""; }
        }

        public string PrincipalName
        {
            get { return DummyData.Principals.Where(w => w.Id == PrincipalId).FirstOrDefault()?.Name ?? ""; }
        }

        public string ShipperCode
        {
            get { return DummyData.Shippers.Where(w => w.Id == ShipperId).FirstOrDefault()?.Code ?? ""; }
        }

        public string ShipperName
        {
            get { return DummyData.Shippers.Where(w => w.Id == ShipperId).FirstOrDefault()?.Name ?? ""; }
        }

        public string ContSize
        {
            get { return DummyData.Containers.Where(w => w.ContNumber == ContNumber).FirstOrDefault()?.ContSize ?? ""; }
        }

        public string ContType
        {
            get { return DummyData.Containers.Where(w => w.ContNumber == ContNumber).FirstOrDefault()?.ContType ?? ""; }
        }

        [ObservableProperty]
        private StatusType cleaningStatus = StatusType.NotFilled;
        partial void OnCleaningStatusChanged(StatusType value)
        {
            OnPropertyChanged(nameof(Status));
            UpdateOverallStatus();
        }

        [ObservableProperty]
        private StatusType repairStatus = StatusType.NotFilled;
        partial void OnRepairStatusChanged(StatusType value)
        {
            OnPropertyChanged(nameof(Status));
            UpdateOverallStatus();
        }

        [ObservableProperty]
        private StatusType periodicStatus = StatusType.NotFilled;
        partial void OnPeriodicStatusChanged(StatusType value)
        {
            OnPropertyChanged(nameof(Status));
            UpdateOverallStatus();
        }

        [ObservableProperty]
        private StatusType surveyStatus = StatusType.NotFilled;
        partial void OnSurveyStatusChanged(StatusType value)
        {
            OnPropertyChanged(nameof(Status));
        }

        public string Status => $"{GetSymbol(CleaningStatus)}|{GetSymbol(RepairStatus)}|{GetSymbol(PeriodicStatus)}|{GetSymbol(SurveyStatus)}";

        private string GetSymbol(StatusType status)
        {
            switch (status)
            {
                case StatusType.NotFilled:
                    return "*";
                case StatusType.OnReview:
                    return "R";
                case StatusType.Finished:
                    return "A";
                case StatusType.Rejected:
                    return "X";
                default:
                    return "";
            }
        }

        private void UpdateOverallStatus()
        {
            if (CleaningStatus != StatusType.NotFilled &&
                RepairStatus != StatusType.NotFilled &&
                PeriodicStatus != StatusType.NotFilled)
            {
                if (CleaningStatus == StatusType.Finished &&
                    RepairStatus == StatusType.Finished &&
                    PeriodicStatus == StatusType.Finished)
                {
                    SurveyStatus = StatusType.Finished;
                }
                else
                {
                    SurveyStatus = StatusType.OnReview;
                }
            }
        }
    }
}