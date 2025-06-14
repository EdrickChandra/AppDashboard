using Surveying.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Surveying
{
    /// <summary>
    /// SIMPLIFIED DUMMY DATA
    /// Removed: Complex lookups, separate collections for Principals/Shippers/Containers
    /// Added: Direct Order objects with embedded Containers
    /// </summary>
    public static class DummyData
    {
        /// <summary>
        /// Sample orders with containers - ready to use directly
        /// </summary>
        public static ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>
        {
            // Order 1: Multiple containers
            new Order
            {
                OrderNumber = "ORD-001",
                Surveyor = "John Smith",
                PrincipalCode = "P001",
                PrincipalName = "Newport Shipping",
                ShipperCode = "S001",
                ShipperName = "Musim Mas",
                OrderDate = DateTime.Today.AddDays(-5),
                SurveyDate = DateTime.Today.AddDays(-3),
                PickupDate = DateTime.Today.AddDays(2),
                Containers = new ObservableCollection<Container>
                {
                    new Container
                    {
                        ContNumber = "ABCD0000001",
                        ContSize = "20",
                        ContType = "Tank",
                        CustomerCode = "CUST001",
                        Commodity = "Palm Oil",
                        CleaningStatus = StatusType.Finished,
                        RepairStatus = StatusType.OnReview,
                        PeriodicStatus = StatusType.Finished,
                        SurveyStatus = StatusType.NotFilled
                    },
                    new Container
                    {
                        ContNumber = "ABCD0000002",
                        ContSize = "20",
                        ContType = "Tank",
                        CustomerCode = "CUST001",
                        Commodity = "Cooking Oil",
                        CleaningStatus = StatusType.Rejected,
                        RepairStatus = StatusType.Finished,
                        PeriodicStatus = StatusType.NotFilled,
                        SurveyStatus = StatusType.NotFilled
                    },
                    new Container
                    {
                        ContNumber = "ABCD0000003",
                        ContSize = "40",
                        ContType = "Tank",
                        CustomerCode = "CUST001",
                        Commodity = "Vegetable Oil",
                        CleaningStatus = StatusType.OnReview,
                        RepairStatus = StatusType.NotFilled,
                        PeriodicStatus = StatusType.OnReview,
                        SurveyStatus = StatusType.NotFilled
                    }
                }
            },

            // Order 2: Multiple containers
            new Order
            {
                OrderNumber = "ORD-002",
                Surveyor = "Jane Doe",
                PrincipalCode = "P002",
                PrincipalName = "Global Logistics",
                ShipperCode = "S002",
                ShipperName = "Asia Pacific Oils",
                OrderDate = DateTime.Today.AddDays(-3),
                SurveyDate = DateTime.Today.AddDays(-1),
                PickupDate = DateTime.Today.AddDays(3),
                Containers = new ObservableCollection<Container>
                {
                    new Container
                    {
                        ContNumber = "EFGH0000001",
                        ContSize = "20",
                        ContType = "Tank",
                        CustomerCode = "CUST002",
                        Commodity = "Soybean Oil",
                        CleaningStatus = StatusType.Finished,
                        RepairStatus = StatusType.Finished,
                        PeriodicStatus = StatusType.Finished,
                        SurveyStatus = StatusType.Finished
                    },
                    new Container
                    {
                        ContNumber = "EFGH0000002",
                        ContSize = "40",
                        ContType = "Tank",
                        CustomerCode = "CUST002",
                        Commodity = "Sunflower Oil",
                        CleaningStatus = StatusType.OnReview,
                        RepairStatus = StatusType.Rejected,
                        PeriodicStatus = StatusType.NotFilled,
                        SurveyStatus = StatusType.NotFilled
                    }
                }
            },

            // Order 3: Single container
            new Order
            {
                OrderNumber = "ORD-003",
                Surveyor = "Mike Johnson",
                PrincipalCode = "P003",
                PrincipalName = "Maritime Express",
                ShipperCode = "S003",
                ShipperName = "Pacific Traders",
                OrderDate = DateTime.Today.AddDays(-2),
                SurveyDate = DateTime.Today,
                PickupDate = DateTime.Today.AddDays(4),
                Containers = new ObservableCollection<Container>
                {
                    new Container
                    {
                        ContNumber = "IJKL0000001",
                        ContSize = "20",
                        ContType = "Tank",
                        CustomerCode = "CUST003",
                        Commodity = "Coconut Oil",
                        CleaningStatus = StatusType.Finished,
                        RepairStatus = StatusType.Finished,
                        PeriodicStatus = StatusType.OnReview,
                        SurveyStatus = StatusType.OnReview
                    }
                }
            },

            // Order 4: Multiple containers with different statuses
            new Order
            {
                OrderNumber = "ORD-004",
                Surveyor = "Sarah Wilson",
                PrincipalCode = "P004",
                PrincipalName = "Ocean Freight",
                ShipperCode = "S004",
                ShipperName = "Eastern Oils",
                OrderDate = DateTime.Today.AddDays(-1),
                SurveyDate = DateTime.Today.AddDays(1),
                PickupDate = DateTime.Today.AddDays(5),
                Containers = new ObservableCollection<Container>
                {
                    new Container
                    {
                        ContNumber = "MNOP0000001",
                        ContSize = "40",
                        ContType = "Tank",
                        CustomerCode = "CUST004",
                        Commodity = "Canola Oil",
                        CleaningStatus = StatusType.NotFilled,
                        RepairStatus = StatusType.NotFilled,
                        PeriodicStatus = StatusType.NotFilled,
                        SurveyStatus = StatusType.NotFilled
                    },
                    new Container
                    {
                        ContNumber = "MNOP0000002",
                        ContSize = "20",
                        ContType = "Tank",
                        CustomerCode = "CUST004",
                        Commodity = "Olive Oil",
                        CleaningStatus = StatusType.OnReview,
                        RepairStatus = StatusType.Finished,
                        PeriodicStatus = StatusType.NotFilled,
                        SurveyStatus = StatusType.NotFilled
                    }
                }
            }
        };

        /// <summary>
        /// Initialize activities for all containers
        /// Called automatically when data is accessed
        /// </summary>
        static DummyData()
        {
            // Initialize activities for all containers
            foreach (var order in Orders)
            {
                foreach (var container in order.Containers)
                {
                    container.UpdateActivities();
                }
            }
        }

        /// <summary>
        /// Get sample containers for cleaning list (mimics API response)
        /// </summary>
        public static ObservableCollection<Container> GetContainersForCleaning()
        {
            var cleaningContainers = new ObservableCollection<Container>();

            // Add containers that need cleaning
            foreach (var order in Orders)
            {
                foreach (var container in order.Containers)
                {
                    // Add containers that are not finished cleaning
                    if (container.CleaningStatus != StatusType.Finished)
                    {
                        // Add some cleaning requirements
                        container.CleaningRequirementsText = "YXT • 1101 • APNN";
                        container.DtmIn = DateTime.Today.AddDays(-Random.Shared.Next(1, 10));

                        cleaningContainers.Add(container);
                    }
                }
            }

            return cleaningContainers;
        }

        /// <summary>
        /// Get a specific container by number
        /// </summary>
        public static Container GetContainerByNumber(string contNumber)
        {
            return Orders.SelectMany(o => o.Containers)
                        .FirstOrDefault(c => c.ContNumber.Equals(contNumber, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get order containing a specific container
        /// </summary>
        public static Order GetOrderByContainerNumber(string contNumber)
        {
            return Orders.FirstOrDefault(o => o.Containers.Any(c =>
                c.ContNumber.Equals(contNumber, StringComparison.OrdinalIgnoreCase)));
        }
    }
}