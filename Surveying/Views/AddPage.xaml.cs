using Surveying.ViewModels;
using Surveying.Models;
using System.Collections.ObjectModel;

namespace Surveying.Views
{
    public partial class AddPage : ContentPage
    {
        // CHANGE FROM:
        // public AddPage(Action<ObservableCollection<SurveyModel>> onSubmitCallback)
        
        // CHANGE TO:
        public AddPage(Action<Order> onSubmitCallback)
        {
            InitializeComponent();
            var viewModel = new AddPageViewModel();

            viewModel.OnSubmitCompleted = (newOrder) =>  // CHANGED: was surveyEntries, now newOrder
            {
                onSubmitCallback?.Invoke(newOrder);  // CHANGED: Pass the Order object
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                });
            };

            // Check for screen size changes
            DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
            AdjustLayoutForScreenSize();

            BindingContext = viewModel;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
        }

        private void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            AdjustLayoutForScreenSize();
        }

        private void AdjustLayoutForScreenSize()
        {
            // Get current screen density and size
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            var width = mainDisplayInfo.Width / mainDisplayInfo.Density;

            // For very small screens, adjust the grid to use a single column
            if (width < 600)
            {
                // This would require adding x:Name attributes to the main input grid in XAML
                // If you want to implement this logic, add the name and uncomment below
                // mainInputGrid.ColumnDefinitions.Clear();
                // mainInputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                // 
                // And adjust the Grid.Row and Grid.Column properties of children
                // You'd need to handle this with additional code or binding
            }
        }
    }
}