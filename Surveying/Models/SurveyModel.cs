using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surveying.Models;

public partial class SurveyModel : ObservableObject {
    //private static int _orderCounter = 1;
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
                      DateTime orderDate, DateTime surveyDate, DateTime pickupDate, string condition) {
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

    public string PrincipalCode { 
        get {
            return DummyData.Principals.Where(w=>w.Id == PrincipalId).FirstOrDefault().Code;
        }
    }

    public string PrincipalName {
        get {
            return DummyData.Principals.Where(w => w.Id == PrincipalId).FirstOrDefault().Name;
        }
    }

    public string ShipperCode {
        get {
            return DummyData.Shippers.Where(w => w.Id == ShipperId).FirstOrDefault().Code;
        }
    }

    public string ShipperName {
        get {
            return DummyData.Shippers.Where(w => w.Id == ShipperId).FirstOrDefault().Name;
        }
    }

    public string ContSize {
        get {
            return DummyData.Containers.Where(w => w.ContNumber == ContNumber).FirstOrDefault().ContSize;
        }
    }

    public string ContType {
        get {
            return DummyData.Containers.Where(w => w.ContNumber == ContNumber).FirstOrDefault().ContType;
        }
    }

}