using Surveying.ViewModels;
using Surveying.Services;

namespace Surveying.Views
{
    public partial class CleaningList : ContentPage
    {
        private CleaningListViewModel _viewModel;

        public CleaningList()
        {
            InitializeComponent();

            // Create ViewModel with proper dependency injection
            var containerApiService = new ContainerApiService();
            _viewModel = new CleaningListViewModel(containerApiService);
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Refresh data when page appears
            if (_viewModel != null && !_viewModel.IsLoading)
            {
                await _viewModel.LoadCleaningDataFromApiCommand.ExecuteAsync(null);
            }
        }
    }
}