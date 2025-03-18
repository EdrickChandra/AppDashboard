using Syncfusion.Maui.DataGrid;

namespace Surveying;
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

        if (e.RowData is SurveyList selected)
        {
            _viewModel.SelectedSurvey = selected;
          
        }
    }

    private async void SfButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Cleaning());

    }

    private async void SfButton_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Repair());
    }

    private async void SfButton_Clicked_2(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Periodic());
    }
}
