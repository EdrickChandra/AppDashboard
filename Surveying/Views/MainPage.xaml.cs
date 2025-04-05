using Syncfusion.Maui.DataGrid;
using Surveying.ViewModels;
using Surveying.Models;

namespace Surveying.Views;
public partial class MainPage : ContentPage
{
    private SurveyListViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new SurveyListViewModel();
        BindingContext = _viewModel;
  
    }

    private async void OnAddSurveyClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddPage((surveyEntries) =>
        {
            foreach (var survey in surveyEntries)
            {
                _viewModel.SurveyListCollection.Add(survey);
            }
       
            _viewModel.SearchText = string.Empty;
        }));
    }

    private void DataGrid_CellTapped(object sender, Syncfusion.Maui.DataGrid.DataGridCellTappedEventArgs e)
    {

        if (e.RowData is SurveyModel selected)
        {
            _viewModel.SelectedSurvey = selected;
          
        }
    }

    private async void SfButton_Clicked(object sender, EventArgs e)
    {
        if (_viewModel.SelectedSurvey != null)
        {
            // Navigate to Cleaning page with the selected survey.
            await Navigation.PushAsync(new Cleaning(_viewModel.SelectedSurvey));
        }
        else
        {
            await DisplayAlert("No Selection", "Please select a survey first.", "OK");
        }
    }

    // Similarly for Repair and Periodic:
    private async void SfButton_Clicked_1(object sender, EventArgs e)
    {
        if (_viewModel.SelectedSurvey != null)
        {
            await Navigation.PushAsync(new Repair(_viewModel.SelectedSurvey));
        }
        else
        {
            await DisplayAlert("No Selection", "Please select a survey first.", "OK");
        }
    }

    private async void SfButton_Clicked_2(object sender, EventArgs e)
    {
        if (_viewModel.SelectedSurvey != null)
        {
            await Navigation.PushAsync(new Periodic(_viewModel.SelectedSurvey));
        }
        else
        {
            await DisplayAlert("No Selection", "Please select a survey first.", "OK");
        }
    }

    private async void SfButton_Clicked_3(object sender, EventArgs e)
    {
        if (_viewModel.SelectedSurvey != null)
        {
            // Pass the selected SurveyModel to the Survey page
            await Navigation.PushAsync(new Survey(_viewModel.SelectedSurvey));
        }
        else
        {
            await DisplayAlert("No Selection", "Please select a survey first.", "OK");
        }
    }



}
