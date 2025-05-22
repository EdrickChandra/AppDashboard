using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Views;
using System;

namespace Surveying.ViewModels
{
    public partial class RepairViewModel : BaseViewModel
    {
        public SurveyModel Survey { get; }
        public ContainerDetailModel Container { get; }

        public string ContainerNumber => Container.ContNumber;
        public PhotoUploadViewModel PhotoUploader { get; } = new PhotoUploadViewModel();

        [ObservableProperty]
        private DateTime repairDate = DateTime.Today;

        [ObservableProperty]
        private string repairDescription = "";

        public RepairViewModel(SurveyModel survey, ContainerDetailModel container)
        {
            Survey = survey;
            Container = container;
        }

        [RelayCommand]
        async void ViewFullImage(PhotoItem photo)
        {
            if (photo != null && photo.ImageSource != null)
            {
                var imageViewerPage = new ImageViewerPage(photo.ImageSource);
                await Application.Current.MainPage.Navigation.PushAsync(imageViewerPage);
            }
        }

        [RelayCommand]
        async void SubmitRepair()
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
                Container.RepairStatus = StatusType.Finished;   
                Container.UpdateActivities();
            }

            // Also update survey for backward compatibility
            Survey.RepairStatus = StatusType.Finished; 

            await Application.Current.MainPage.DisplayAlert("Success",
                "Repair data has been submitted and marked as Finished.", "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}