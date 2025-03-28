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
            // Set the callback so that when Submit is invoked, the data is passed back.
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
