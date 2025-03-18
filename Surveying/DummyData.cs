using Surveying.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surveying;

public static class DummyData {
    public static List<PrincipalModel> Principals = new List<PrincipalModel> {
        new PrincipalModel(1, "P001", "Principal 1"),
        new PrincipalModel(2, "P002", "Principal 2"),
        new PrincipalModel(3, "P003", "Principal 3"),
        new PrincipalModel(4, "P004", "Principal 4"),
        new PrincipalModel(5, "P005", "Principal 5"),
    };

    public static List<ShipperModel> Shippers = new List<ShipperModel> {
        new ShipperModel(1, "S001", "Shipper 1"),
        new ShipperModel(2, "S002", "Shipper 2"),
        new ShipperModel(3, "S003", "Shipper 3"),
        new ShipperModel(4, "S004", "Shipper 4"),
        new ShipperModel(5, "S005", "Shipper 5"),
    };

    public static List<ContModel> Containers = new List<ContModel> {
        new ContModel("C001", "20", "Tank"),
        new ContModel("C002", "20", "Tank"),
        new ContModel("C003", "20", "Tank"),
        new ContModel("C004", "20", "Tank"),
        new ContModel("C005", "20", "Tank"),
    };

    public static List<SurveyModel> Surveys = new List<SurveyModel> {
        new SurveyModel("ABC001", 1, "John Doe", 1, "C001", DateTime.Now, DateTime.Now.AddDays(2), DateTime.Now.AddDays(5), "Good"),
        new SurveyModel("ABC002", 2, "Jane Smith", 2, "C002", DateTime.Now, DateTime.Now.AddDays(3), DateTime.Now.AddDays(6), "Needs Repair"),
        new SurveyModel("ABC003", 3, "Mike Johnson", 3, "C003", DateTime.Now, DateTime.Now.AddDays(4), DateTime.Now.AddDays(7), "Damaged")
    };
}
