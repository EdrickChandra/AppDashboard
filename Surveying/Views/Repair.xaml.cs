using Surveying.Models;
using Surveying.ViewModels;

namespace Surveying.Views
{
    public partial class Repair : ContentPage
    {
        public Repair(SurveyModel survey)
        {
            InitializeComponent();
            BindingContext = new RepairViewModel(survey);
        }
    }
}
