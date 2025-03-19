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
            var containernumber = _viewModel.SelectedSurvey.ContNumber;
            await Navigation.PushAsync(new Cleaning(containernumber));
        }
        else
        {
            await DisplayAlert("No Selection", "Please select a survey first.", "OK");
        }
    }

    private async void SfButton_Clicked_1(object sender, EventArgs e)
    {
        if (_viewModel.SelectedSurvey != null)
        {
            var containernumber = _viewModel.SelectedSurvey.ContNumber;
            await Navigation.PushAsync(new Repair());
        }
        else
        {
            await DisplayAlert("No Selection", "Please select a survey first.", "OK");
        }
    }

    private async void SfButton_Clicked_2(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Periodic());
    }

    private async void SfButton_Clicked_3(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Survey());
    }
}
