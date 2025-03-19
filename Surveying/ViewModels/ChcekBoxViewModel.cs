using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Surveying.ViewModels
{
    public partial class CheckboxViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool accept;

        [ObservableProperty]
        private bool reject;


        partial void OnAcceptChanged(bool value)
        {
            if (value)
                Reject = false;
        }

 
        partial void OnRejectChanged(bool value)
        {
            if (value)
                Accept = false;
        }
    }
}
