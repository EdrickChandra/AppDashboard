using Surveying.Models;
using Surveying.ViewModels;

namespace Surveying.Views
{
    public partial class Survey : ContentPage
    {
        // Create a constructor that accepts the SurveyModel
        public Survey(SurveyModel survey)
        {
            InitializeComponent();
            // Pass the SurveyModel into the view model
            BindingContext = new CheckboxViewModel(survey);
        }
    }
}
