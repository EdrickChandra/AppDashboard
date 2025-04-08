using Surveying.Models;
using Surveying.ViewModels;

namespace Surveying.Views
{
    public partial class Survey : ContentPage
    {
 
        public Survey(SurveyModel survey)
        {
            InitializeComponent();
            
            BindingContext = new CheckboxViewModel(survey);
        }
    }
}
