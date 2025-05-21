using Surveying.Models;
using Surveying.ViewModels;

namespace Surveying.Views
{
    public partial class Cleaning : ContentPage
    {
        public Cleaning(SurveyModel survey, ContainerDetailModel container)
        {
            InitializeComponent();
            BindingContext = new CleaningViewModel(survey, container);
        }
    }
}