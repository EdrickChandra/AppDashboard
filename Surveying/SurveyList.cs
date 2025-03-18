using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surveying
{
    public class SurveyList : INotifyPropertyChanged
    {
        private static int _orderCounter = 1;

        private string _principal = string.Empty;
        private string _surveyor = string.Empty;
        private string _shipper = string.Empty;
        private string _tankNo = string.Empty;
        private string _condition = string.Empty;

        private DateTime _orderDate;
        private DateTime _surveyDate;
        private DateTime _pickupDate;

        public int OrderNumber { get; private set; }

        public string Principal
        {
            get => _principal;
            set
            {
                if (_principal != value)
                {
                    _principal = value;
                    OnPropertyChanged(nameof(Principal));
                }
            }
        }

        public string Surveyor
        {
            get => _surveyor;
            set
            {
                if (_surveyor != value)
                {
                    _surveyor = value;
                    OnPropertyChanged(nameof(Surveyor));
                }
            }
        }

        public string Shipper
        {
            get => _shipper;
            set
            {
                if (_shipper != value)
                {
                    _shipper = value;
                    OnPropertyChanged(nameof(Shipper));
                }
            }
        }

        public string TankNo
        {
            get => _tankNo;
            set
            {
                if (_tankNo != value)
                {
                    _tankNo = value;
                    OnPropertyChanged(nameof(TankNo));
                }
            }
        }

        public DateTime OrderDate
        {
            get => _orderDate;
            set
            {
                if (_orderDate != value)
                {
                    _orderDate = value;
                    OnPropertyChanged(nameof(OrderDate));
                }
            }
        }

        public DateTime SurveyDate
        {
            get => _surveyDate;
            set
            {
                if (_surveyDate != value)
                {
                    _surveyDate = value;
                    OnPropertyChanged(nameof(SurveyDate));
                }
            }
        }

        public DateTime PickupDate
        {
            get => _pickupDate;
            set
            {
                if (_pickupDate != value)
                {
                    _pickupDate = value;
                    OnPropertyChanged(nameof(PickupDate));
                }
            }
        }

        public string Condition
        {
            get => _condition;
            set
            {
                if (_condition != value)
                {
                    _condition = value;
                    OnPropertyChanged(nameof(Condition));
                }
            }
        }

        public SurveyList(string principal,
                          string surveyor,
                          string shipper,
                          string tankNo,
                          DateTime orderDate,
                          DateTime surveyDate,
                          DateTime pickupDate,
                          string condition)
        {
            OrderNumber = _orderCounter++;
            Principal = principal;
            Surveyor = surveyor;
            Shipper = shipper;
            TankNo = tankNo;
            OrderDate = orderDate;
            SurveyDate = surveyDate;
            PickupDate = pickupDate;
            Condition = condition;
        }


        public static void ResetOrderCounter()
        {
            _orderCounter = 1;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}