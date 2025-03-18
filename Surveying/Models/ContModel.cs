using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surveying.Models;

public class ContModel : ObservableObject {
    public string ContNumber { get; set; }
    public string ContSize {  get; set; }
    public string ContType { get; set; }

    public ContModel() { }

    public ContModel(string contNumber, string contSize, string contType) {
        ContNumber = contNumber;
        ContSize = contSize;
        ContType = contType;
    }
}
