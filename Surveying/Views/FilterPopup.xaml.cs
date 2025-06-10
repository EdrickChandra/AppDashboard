using Microsoft.Maui.Storage;
using Surveying.ViewModels;

namespace Surveying.Views
{
    public partial class FilterPopup : ContentPage
    {
        public FilterPopupViewModel ViewModel { get; }
        public TaskCompletionSource<FilterResult> TaskCompletionSource { get; set; }

        public FilterPopup(FilterPopupViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;

            // Subscribe to close events
            ViewModel.CloseRequested += OnCloseRequested;
            ViewModel.FiltersApplied += OnFiltersApplied;
        }

        private void OnBackgroundTapped(object sender, EventArgs e)
        {
            // Close popup when background is tapped
            ClosePopup(null);
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            // Close popup without applying filters
            ClosePopup(null);
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            ClosePopup(null);
        }

        private void OnFiltersApplied(object sender, FilterResult e)
        {
            ClosePopup(e);
        }

        private async void ClosePopup(FilterResult result)
        {
            // Unsubscribe from events
            ViewModel.CloseRequested -= OnCloseRequested;
            ViewModel.FiltersApplied -= OnFiltersApplied;

            // Set result and close
            TaskCompletionSource?.SetResult(result);
            await Navigation.PopModalAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            // Handle Android back button
            ClosePopup(null);
            return true;
        }
    }
}