using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Surveying
{
    public class SurveyListViewModel : INotifyPropertyChanged
    {
        private SurveyList _selectedSurvey;
        public SurveyList SelectedSurvey
        {
            get => _selectedSurvey;
            set
            {
                if (_selectedSurvey != value)
                {
                    _selectedSurvey = value;
                    OnPropertyChanged(nameof(SelectedSurvey));
                }
            }
        }

        public ObservableCollection<SurveyList> SurveyListCollection { get; set; }

        public SurveyListViewModel()
        {            SurveyListCollection = new ObservableCollection<SurveyList>
            {
                new SurveyList("Company A", "John Doe", "Shipper A", "Tank-101", DateTime.Now, DateTime.Now.AddDays(2), DateTime.Now.AddDays(5), "Good"),
                new SurveyList("Company B", "Jane Smith", "Shipper B", "Tank-102", DateTime.Now, DateTime.Now.AddDays(3), DateTime.Now.AddDays(6), "Needs Repair"),
                new SurveyList("Company C", "Mike Johnson", "Shipper C", "Tank-103", DateTime.Now, DateTime.Now.AddDays(4), DateTime.Now.AddDays(7), "Damaged")
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
