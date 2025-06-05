using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Views;
using System;

namespace Surveying.ViewModels
{
    public partial class PeriodicViewModel : BaseViewModel
    {
        public SurveyModel Survey { get; }
        public ContainerDetailModel Container { get; }

        public string ContainerNumber => Container.ContNumber;

        // Use factory method for periodic (CSC Plate only)
        public PhotoUploadViewModel PhotoUploader { get; } = PhotoUploadViewModel.CreateForPeriodic();

        [ObservableProperty]
        private DateTime inspectionDate = DateTime.Today;

        [ObservableProperty]
        private DateTime nextDueDate = DateTime.Today.AddYears(2).AddMonths(6);

        public PeriodicViewModel(SurveyModel survey, ContainerDetailModel container)
        {
            Survey = survey;
            Container = container;
        }

        [RelayCommand]
        async void ViewFullImage(PhotoItem photo) // Keep as PhotoItem
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

            if (PhotoUploader.Photos == null || PhotoUploader.Photos.Count == 0)
            {
                isValid = false;
                errorMessage += "Please upload at least one photo.\n";
            }

            if (!isValid)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", errorMessage, "OK");
                return;
            }

            // Skip OnReview status and go directly to Finished
            if (Container != null)
            {
                Container.PeriodicStatus = StatusType.Finished;
                Container.UpdateActivities();
            }

            // Also update survey for backward compatibility
            Survey.PeriodicStatus = StatusType.Finished;

            await Application.Current.MainPage.DisplayAlert("Success",
                "Periodic maintenance data has been submitted and marked as Finished.", "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}