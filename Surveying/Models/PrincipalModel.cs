using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surveying.Models;

public class PrincipalModel : ObservableObject {
    public long Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }

    public PrincipalModel() { }

    public PrincipalModel(long id, string code, string name) {
        Id = id;
        Code = code;
        Name = name;
    }
}
