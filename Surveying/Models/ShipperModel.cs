using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surveying.Models;

public class ShipperModel : ObservableObject {
    public long Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }

    public ShipperModel() { }

    public ShipperModel(long id, string code, string name) {
        Id = id;
        Code = code;
        Name = name;
    }
}
