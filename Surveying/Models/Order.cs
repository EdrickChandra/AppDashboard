using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Surveying.Models
{
    /// <summary>
    /// UNIFIED ORDER CLASS  
    /// Replaces: SurveyModel (from SurveyModel.cs)
    /// Simplifies: Removes Principal/Shipper ID lookups
    /// Better naming: "Order" is clearer than "Survey"
    /// </summary>
    public partial class Order : ObservableObject
    {
        // ===== BASIC ORDER PROPERTIES (from SurveyModel) =====
        public string OrderNumber { get; set; } = string.Empty;
        public string Surveyor { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime SurveyDate { get; set; }
        public DateTime PickupDate { get; set; }

        // ===== PRINCIPAL INFO - SIMPLIFIED (no more ID lookups!) =====
        // OLD: Had PrincipalId + complex lookup in PrincipalCode/PrincipalName properties
        // NEW: Direct string properties - much simpler!
        public string PrincipalCode { get; set; } = string.Empty;
        public string PrincipalName { get; set; } = string.Empty;

        // ===== SHIPPER INFO - SIMPLIFIED (no more ID lookups!) =====  
        // OLD: Had ShipperId + complex lookup in ShipperCode/ShipperName properties
        // NEW: Direct string properties - much simpler!
        public string ShipperCode { get; set; } = string.Empty;
        public string ShipperName { get; set; } = string.Empty;

        // ===== MASTER-DETAIL RELATIONSHIP (from SurveyModel) =====
        // OLD: Had both individual ContNumber + Containers collection
        // NEW: Just the containers collection - cleaner
        public ObservableCollection<Container> Containers { get; set; } = new ObservableCollection<Container>();

        // ===== COMPUTED PROPERTIES (for display) =====
        public int ContainerCount => Containers?.Count ?? 0;

        public string ContainerSummary => ContainerCount switch
        {
            0 => "No containers",
            1 => "1 container",
            _ => $"{ContainerCount} containers"
        };
    }
}