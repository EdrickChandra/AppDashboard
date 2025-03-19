using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Surveying.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Surveying.ViewModels;

public partial class SurveyListViewModel : BaseViewModel {
    [ObservableProperty]
    SurveyModel _selectedSurvey;
    
    public ObservableCollection<SurveyModel> SurveyListCollection { get; set; }

    public SurveyListViewModel()
    {
        SurveyListCollection = new ObservableCollection<SurveyModel>(DummyData.Surveys);
    }

    public ObservableCollection<string> ConditionList { get; set; } = new ObservableCollection<string>
        {
            "Mty Clean", "Clean", "Dirty"
        };

}

