using Surveying.ViewModels;
using Surveying.Models;
using System.Collections.ObjectModel;

namespace Surveying.Views
{
    public partial class AddPage : ContentPage
    {
        public AddPage(Action<ObservableCollection<SurveyModel>> onSubmitCallback)
        {
            InitializeComponent();
            var viewModel = new AddPageViewModel();
       
            viewModel.OnSubmitCompleted = (surveyEntries) =>
            {
                onSubmitCallback?.Invoke(surveyEntries);
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                });
            };
            BindingContext = viewModel;
        }
    }
}
