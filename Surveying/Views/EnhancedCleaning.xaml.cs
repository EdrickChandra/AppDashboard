using Surveying.Models;
using Surveying.ViewModels;

namespace Surveying.Views
{
    public partial class EnhancedCleaning : ContentPage
    {
        public EnhancedCleaning(SurveyModel survey, ContainerDetailModel container, ContainerWithRepairCodesModel containerFromApi)
        {
            InitializeComponent();
            BindingContext = new EnhancedCleaningViewModel(survey, container, containerFromApi);
        }
    }
}