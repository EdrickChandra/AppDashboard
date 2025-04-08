using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
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
        Accepted,   // Will be displayed as "A"
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

        public SurveyModel() { }

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
                case StatusType.Accepted:
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
                if (CleaningStatus == StatusType.Accepted &&
                    RepairStatus == StatusType.Accepted &&
                    PeriodicStatus == StatusType.Accepted)
                {
                    SurveyStatus = StatusType.Accepted;
                }
                else
                {
                    SurveyStatus = StatusType.OnReview;
                }
            }
        }
    }
}
