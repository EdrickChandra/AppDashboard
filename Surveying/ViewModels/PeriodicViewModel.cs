using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Views;
using System;

namespace Surveying.ViewModels
{
    // ===== MIGRATED: PeriodicViewModel using simplified models =====
    public partial class PeriodicViewModel : BaseViewModel
    {
        // ===== SIMPLIFIED MODELS =====
        // OLD: SurveyModel survey, ContainerDetailModel container
        // NEW: Order order, Container container (unified models)
        public Order Order { get; }
        public Container Container { get; }

        public string ContainerNumber => Container.ContNumber;

        // ===== SIMPLIFIED PHOTO UPLOADER =====
        // Use factory method for periodic (CSC Plate only) - single Photo type
        public PhotoUploadViewModel PhotoUploader { get; } = PhotoUploadViewModel.CreateForPeriodic();

        [ObservableProperty]
        private DateTime inspectionDate = DateTime.Today;

        [ObservableProperty]
        private DateTime nextDueDate = DateTime.Today.AddYears(2).AddMonths(6);

        // ===== CONSTRUCTOR - SIMPLIFIED =====
        // OLD: Took SurveyModel + ContainerDetailModel
        // NEW: Takes Order + Container (unified models)
        public PeriodicViewModel(Order order, Container container)
        {
            Order = order;
            Container = container;
        }

        // ===== UI COMMANDS - SIMPLIFIED =====
        [RelayCommand]
        async void ViewFullImage(Photo photo)  // SIMPLIFIED: Single Photo type instead of PhotoItem
        {
            if (photo != null && photo.ImageSource != null)
            {
                var imageViewerPage = new ImageViewerPage(photo.ImageSource);
                await Application.Current.MainPage.Navigation.PushAsync(imageViewerPage);
            }
        }

        [RelayCommand]
        async void SubmitPeriodic()
        {
            bool isValid = true;
            string errorMessage = "";

            // Check if CSC Plate photo is uploaded
            if (PhotoUploader.Photos == null || PhotoUploader.Photos.Count == 0)
            {
                isValid = false;
                errorMessage += "Please upload at least one photo of the CSC Plate.\n";
            }

            if (!isValid)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", errorMessage, "OK");
                return;
            }

            // SIMPLIFIED: Direct status update on Container (no manual syncing needed)
            Container.PeriodicStatus = StatusType.Finished;
            Container.UpdateActivities();

            // Set periodic maintenance dates
            Container.CleaningStartDate = InspectionDate;  // Reuse existing property for inspection date
            Container.CleaningCompleteDate = NextDueDate;  // Reuse existing property for next due date

            await Application.Current.MainPage.DisplayAlert("Success",
                "Periodic maintenance data has been submitted and marked as Finished.", "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}

}