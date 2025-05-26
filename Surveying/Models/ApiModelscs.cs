using System;
using System.Collections.Generic;

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
    }

    // Container with repair codes
    public class ContainerWithRepairCodesModel
    {
        public long Id { get; set; }
        public string ContNumber { get; set; }
        public DateTime? DtmIn { get; set; }
        public string CustomerCode { get; set; }
        public List<RepairCodeModel> RepairCodes { get; set; } = new List<RepairCodeModel>();
    }

    // Repair code details
    public class RepairCodeModel
    {
        public string RepairCode { get; set; }
        public string ComponentCode { get; set; }
        public string LocationCode { get; set; }
    }
}