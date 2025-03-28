using Surveying.Models;
using Surveying.ViewModels;

namespace Surveying.Views
{
    public partial class Periodic : ContentPage
    {
        public Periodic(SurveyModel survey)
        {
            InitializeComponent();
            BindingContext = new PeriodicViewModel(survey);
        }
    }
}
