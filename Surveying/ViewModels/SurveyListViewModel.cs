using CommunityToolkit.Mvvm.ComponentModel;
using Surveying.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Surveying.ViewModels;

public partial class SurveyListViewModel : BaseViewModel
{
    [ObservableProperty]
    private SurveyModel selectedSurvey;

    public ObservableCollection<SurveyModel> SurveyListCollection { get; set; }

    // A filtered collection that the DataGrid will bind to.
    public ObservableCollection<SurveyModel> FilteredSurveyList { get; set; }

    // The search text that the SearchBar binds to.
    [ObservableProperty]
    private string searchText;

    public SurveyListViewModel()
    {
        SurveyListCollection = new ObservableCollection<SurveyModel>(DummyData.Surveys);
        FilteredSurveyList = new ObservableCollection<SurveyModel>(SurveyListCollection);
    }

    // Called automatically whenever SearchText changes.
    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            // Show all if search text is empty.
            FilteredSurveyList.Clear();
            foreach (var item in SurveyListCollection)
                FilteredSurveyList.Add(item);
        }
        else
        {
            var lowerVal = value.ToLower();
            var filtered = SurveyListCollection.Where(s =>
                s.OrderNumber.ToLower().Contains(lowerVal) ||
                s.Surveyor.ToLower().Contains(lowerVal) ||
                s.ContNumber.ToLower().Contains(lowerVal));

            FilteredSurveyList.Clear();
            foreach (var item in filtered)
                FilteredSurveyList.Add(item);
        }
    }

    public ObservableCollection<string> ConditionList => ConditionData.ConditionList;
}
