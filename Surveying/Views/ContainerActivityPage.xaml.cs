using Surveying.Models;
using Surveying.ViewModels;
using Windows.Devices.Sensors;

namespace Surveying.Views
{
    /// <summary>
    /// UNIFIED CONTAINER ACTIVITY PAGE
    /// Replaces: Cleaning.xaml + Repair.xaml + Periodic.xaml + Survey.xaml
    /// Uses ContainerActivityViewModel with ActivityType to determine behavior
    /// </summary>
    public partial class ContainerActivityPage : ContentPage
    {
        public ContainerActivityPage(Order order, Container container, ActivityType activityType)
        {
            InitializeComponent();
            BindingContext = new ContainerActivityViewModel(order, container, activityType);
        }

        // Alternative constructor for dependency injection in testing
        public ContainerActivityPage(ContainerActivityViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Any page-specific initialization can go here
            if (BindingContext is ContainerActivityViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"ContainerActivityPage appeared for {viewModel.ActivityType} on container {viewModel.ContainerNumber}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Cleanup if needed
            if (BindingContext is ContainerActivityViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"ContainerActivityPage disappeared for {viewModel.ActivityType}");
            }
        }
    }
}