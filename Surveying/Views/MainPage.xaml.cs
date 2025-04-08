using Syncfusion.Maui.DataGrid;
using Surveying.ViewModels;
using Surveying.Models;
using System;

namespace Surveying.Views
{
    public partial class MainPage : ContentPage
    {
        private SurveyListViewModel _viewModel;
        private bool isInitialized = false;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = new SurveyListViewModel();
            BindingContext = _viewModel;

            dataGrid.ColumnWidthMode = ColumnWidthMode.Fill;

            SizeChanged += OnSizeChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!isInitialized)
            {
                AdjustLayoutForDeviceAndOrientation();
                isInitialized = true;
            }
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            AdjustLayoutForDeviceAndOrientation();
        }

        private void AdjustLayoutForDeviceAndOrientation()
        {
            bool isLandscape = Width > Height;
            double screenWidth = Width;


            contentLayout.RowDefinitions.Clear();
            contentLayout.ColumnDefinitions.Clear();

            Grid.SetRow(dataGrid, 0);
            Grid.SetColumn(dataGrid, 0);
            Grid.SetRow(detailsScrollView, 0);
            Grid.SetColumn(detailsScrollView, 0);

            if (isLandscape || screenWidth > 768)
            {
                // Set horizontal layout (side by side)
                contentLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                contentLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                contentLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Grid.SetRow(dataGrid, 0);
                Grid.SetColumn(dataGrid, 0);
                Grid.SetRowSpan(dataGrid, 1);
                Grid.SetColumnSpan(dataGrid, 1);

                Grid.SetRow(detailsScrollView, 0);
                Grid.SetColumn(detailsScrollView, 1);
                Grid.SetRowSpan(detailsScrollView, 1);
                Grid.SetColumnSpan(detailsScrollView, 1);
            }
      
            else
            {
                // Set vertical layout (stacked)
                contentLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
                contentLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) });
                contentLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Grid.SetRow(dataGrid, 0);
                Grid.SetColumn(dataGrid, 0);
                Grid.SetRowSpan(dataGrid, 1);
                Grid.SetColumnSpan(dataGrid, 1);

                Grid.SetRow(detailsScrollView, 1);
                Grid.SetColumn(detailsScrollView, 0);
                Grid.SetRowSpan(detailsScrollView, 1);
                Grid.SetColumnSpan(detailsScrollView, 1);
            }

       
            AdjustDataGridForScreenSize(screenWidth);
        }

        private void AdjustDataGridForScreenSize(double screenWidth)
        {
            dataGrid.ColumnWidthMode = ColumnWidthMode.Fill;

            if (screenWidth < 400)
            {
              
                foreach (var column in dataGrid.Columns)
                {
                    if (column is DataGridColumn dgColumn)
                    {
                        dgColumn.MinimumWidth = 80;
                    }
                }
            }
            else
            {
                foreach (var column in dataGrid.Columns)
                {
                    if (column is DataGridColumn dgColumn)
                    {
                        dgColumn.MinimumWidth = 80;
                    }
                }
            }

            dataGrid.InvalidateMeasure();
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
              
                await Navigation.PushAsync(new Cleaning(_viewModel.SelectedSurvey));
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
    
                await Navigation.PushAsync(new Survey(_viewModel.SelectedSurvey));
            }
            else
            {
                await DisplayAlert("No Selection", "Please select a survey first.", "OK");
            }
        }
    }
}