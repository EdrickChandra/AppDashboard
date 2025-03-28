using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surveying.Models
{
    public static class ConditionData
    {
        public static ObservableCollection<string> ConditionList { get; } =
            new ObservableCollection<string>
            {
                "Mty Clean",
                "Clean",
                "Dirty"
            };
    }
}