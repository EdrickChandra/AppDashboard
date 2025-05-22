using Surveying.Models;
using System;
using System.Collections.ObjectModel;

namespace Surveying;

public static class DummyData
{
    public static ObservableCollection<PrincipalModel> Principals = new ObservableCollection<PrincipalModel>
    {
        new PrincipalModel(1, "P001", "Principal 1"),
        new PrincipalModel(2, "P002", "Principal 2"),
        new PrincipalModel(3, "P003", "Principal 3"),
        new PrincipalModel(4, "P004", "Principal 4"),
        new PrincipalModel(5, "P005", "Principal 5"),
    };

    public static ObservableCollection<ShipperModel> Shippers = new ObservableCollection<ShipperModel>
    {
        new ShipperModel(1, "S001", "Shipper 1"),
        new ShipperModel(2, "S002", "Shipper 2"),
        new ShipperModel(3, "S003", "Shipper 3"),
        new ShipperModel(4, "S004", "Shipper 4"),
        new ShipperModel(5, "S005", "Shipper 5"),
    };

    public static ObservableCollection<ContModel> Containers = new ObservableCollection<ContModel>
    {
        new ContModel("ABCD0000001", "20", "Tank"),
        new ContModel("ABCD0000002", "20", "Tank"),
        new ContModel("ABCD0000003", "40", "Tank"),
        new ContModel("EFGH0000001", "20", "Tank"),
        new ContModel("EFGH0000002", "40", "Tank"),
        new ContModel("IJKL0000001", "20", "Tank"),
        new ContModel("IJKL0000002", "20", "Tank"),
        new ContModel("MNOP0000001", "40", "Tank"),
    };

    public static ObservableCollection<SurveyModel> Surveys = new ObservableCollection<SurveyModel>
    {
        // Order ABC001 with multiple containers
        new SurveyModel("ABC001", 1, "John Doe", 1, "ABCD0000001", DateTime.Now, DateTime.Now.AddDays(2), DateTime.Now.AddDays(5), "Clean"),
        new SurveyModel("ABC001", 1, "John Doe", 1, "ABCD0000002", DateTime.Now, DateTime.Now.AddDays(2), DateTime.Now.AddDays(5), "Dirty"),
        new SurveyModel("ABC001", 1, "John Doe", 1, "ABCD0000003", DateTime.Now, DateTime.Now.AddDays(2), DateTime.Now.AddDays(5), "Mty Clean"),
        
        // Order ABC002 with multiple containers
        new SurveyModel("ABC002", 2, "Jane Smith", 2, "EFGH0000001", DateTime.Now, DateTime.Now.AddDays(3), DateTime.Now.AddDays(6), "Clean"),
        new SurveyModel("ABC002", 2, "Jane Smith", 2, "EFGH0000002", DateTime.Now, DateTime.Now.AddDays(3), DateTime.Now.AddDays(6), "Dirty"),
        
        // Order ABC003 with multiple containers
        new SurveyModel("ABC003", 3, "Mike Johnson", 3, "IJKL0000001", DateTime.Now, DateTime.Now.AddDays(4), DateTime.Now.AddDays(7), "Clean"),
        new SurveyModel("ABC003", 3, "Mike Johnson", 3, "IJKL0000002", DateTime.Now, DateTime.Now.AddDays(4), DateTime.Now.AddDays(7), "Mty Clean"),
        
        // Order ABC004 with one container
        new SurveyModel("ABC004", 4, "Sarah Wilson", 4, "MNOP0000001", DateTime.Now, DateTime.Now.AddDays(5), DateTime.Now.AddDays(8), "Dirty")
    };

    // Initialize some status values to demonstrate status displays
    static DummyData()
    {
        // Set some statuses for demonstration
        Surveys[0].CleaningStatus = StatusType.Finished;
        Surveys[0].RepairStatus = StatusType.OnReview;
        Surveys[0].PeriodicStatus = StatusType.Finished;

        Surveys[1].CleaningStatus = StatusType.Rejected;
        Surveys[1].RepairStatus = StatusType.Finished;

        Surveys[2].CleaningStatus = StatusType.OnReview;
        Surveys[2].PeriodicStatus = StatusType.OnReview;

        Surveys[3].RepairStatus = StatusType.Finished;
        Surveys[3].PeriodicStatus = StatusType.Finished;

        Surveys[4].CleaningStatus = StatusType.OnReview;
        Surveys[4].RepairStatus = StatusType.Rejected;

        Surveys[5].CleaningStatus = StatusType.Finished;
        Surveys[5].RepairStatus = StatusType.Finished;
        Surveys[5].PeriodicStatus = StatusType.Finished;
        Surveys[5].SurveyStatus = StatusType.Finished;
    }
}