using Syncfusion.Maui.DataGrid;
using Surveying.ViewModels;
using Surveying.Models;
using System;
using Syncfusion.Maui.Data;

namespace Surveying.Views
{
    public partial class MainPage : ContentPage
    {
        private SurveyListViewModel _viewModel;
        private bool isInitialized = false;
        private bool isMobileView = false;
        private double panelHeight = 0;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = new SurveyListViewModel();
            BindingContext = _viewModel;

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

            mainLayout.Margin = new Thickness(0);
            mainLayout.Padding = new Thickness(0);
            contentLayout.Margin = new Thickness(0);
            contentLayout.Padding = new Thickness(0);
            dataGrid.Margin = new Thickness(0);

            contentLayout.RowDefinitions.Clear();
            contentLayout.ColumnDefinitions.Clear();

           
            panelHeight = Height * 0.7; 

            isMobileView = screenWidth <= 768 && !isLandscape;

            if (isMobileView)
            {
                // Mobile view - full screen datagrid with sliding details panel
                contentLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                contentLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Grid.SetRow(dataGrid, 0);
                Grid.SetColumn(dataGrid, 0);
                Grid.SetRowSpan(dataGrid, 1);
                Grid.SetColumnSpan(dataGrid, 1);

                Grid.SetRow(mobileDetailsPanel, 0);
                Grid.SetColumn(mobileDetailsPanel, 0);

                detailsScrollView.IsVisible = false;
                mobileDetailsPanel.IsVisible = true;
                mobileDetailsPanel.TranslationY = panelHeight;

               
                if (dataGrid.Columns.Count >= 4)
                {
                    dataGrid.Columns[3].Visible = false;
                }
            }
            else
            {
                // Desktop/landscape view - side by side layout
                contentLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                contentLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                contentLayout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Grid.SetRow(dataGrid, 0);
                Grid.SetColumn(dataGrid, 0);
                Grid.SetRow(detailsScrollView, 0);
                Grid.SetColumn(detailsScrollView, 1);

                
                detailsScrollView.IsVisible = true;
                mobileDetailsPanel.IsVisible = false;

                if (dataGrid.Columns.Count >= 4)
                {
                    dataGrid.Columns[3].Visible = true;
                }
            }

            AdjustDataGridForScreenSize(screenWidth);
        }

        private void AdjustDataGridForScreenSize(double screenWidth)
        {
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

           
                if (isMobileView)
                {
                    ShowMobileDetailPanel();
                }
            }
        }

        private async void ShowMobileDetailPanel()
        {
    
            mobileDetailsPanel.IsVisible = true;
            mobileDetailsPanel.HeightRequest = panelHeight;

            mobileDetailsPanel.TranslationY = panelHeight;

            await mobileDetailsPanel.TranslateTo(0, 0, 250, Easing.Linear);
        }

        private async void HideMobileDetailPanel()
        {
          
            await mobileDetailsPanel.TranslateTo(0, panelHeight, 200, Easing.Linear);
            mobileDetailsPanel.IsVisible = false;
        }

        private void OnCloseDetailPanel(object sender, EventArgs e)
        {
            HideMobileDetailPanel();
        }



        private async void NavigateToPage(object sender, EventArgs e)
        {
            if (_viewModel.SelectedSurvey == null)
            {
                await DisplayAlert("No Selection", "Please select a survey first.", "OK");
                return;
            }

            Button button = sender as Button;
            string pageType = button?.CommandParameter?.ToString();

            if (string.IsNullOrEmpty(pageType))
            {
                return;
            }

            Page destinationPage = null;

            switch (pageType)
            {
                case "Cleaning":
                    destinationPage = new Cleaning(_viewModel.SelectedSurvey);
                    break;
                case "Repair":
                    destinationPage = new Repair(_viewModel.SelectedSurvey);
                    break;
                case "Periodic":
                    destinationPage = new Periodic(_viewModel.SelectedSurvey);
                    break;
                case "Survey":
                    destinationPage = new Survey(_viewModel.SelectedSurvey);
                    break;
            }

            if (destinationPage != null)
            {
                // If in mobile view, hide the panel before navigating
                if (isMobileView)
                {
                    await mobileDetailsPanel.TranslateTo(0, panelHeight, 200, Easing.Linear);
                }

                await Navigation.PushAsync(destinationPage);
            }
        }
    }
}