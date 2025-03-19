using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Surveying.Models
{
    public class PhotoItem : ObservableObject
    {
        public FileResult FileResult { get; set; }
        public ImageSource ImageSource { get; set; }
    }

}
