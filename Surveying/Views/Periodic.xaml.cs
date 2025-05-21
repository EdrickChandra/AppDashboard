using Surveying.Models;
using Surveying.ViewModels;

namespace Surveying.Views
{
    public partial class Periodic : ContentPage
    {
        public Periodic(SurveyModel survey, ContainerDetailModel container)
        {
            InitializeComponent();
            BindingContext = new PeriodicViewModel(survey, container);
        }
    }
}