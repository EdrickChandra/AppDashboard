using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Views;
using System;

namespace Surveying.ViewModels
{
    public partial class CleaningViewModel : BaseViewModel
    {
        public SurveyModel Survey { get; }
        public ContainerDetailModel Container { get; private set; }
        public string ContainerNumber => Container?.ContNumber ?? Survey.ContNumber;
        public DateTime StartCleanDate { get; set; }
        public DateTime EndCleanDate { get; set; }

        // Use factory method for cleaning-specific segments OR keep existing for backward compatibility
        public PhotoUploadViewModel PhotoUploader { get; }

        // Constructor with just Survey (for backward compatibility)
        public CleaningViewModel(SurveyModel survey)
        {
            Survey = survey;
            StartCleanDate = DateTime.Today;
            EndCleanDate = DateTime.Today.AddDays(1);

            // Choose between new segmented approach or old approach
            PhotoUploader = PhotoUploadViewModel.CreateForCleaning(); // New approach
            // PhotoUploader = new PhotoUploadViewModel(4); // Old approach for backward compatibility
        }

        // New constructor with Survey and Container
        public CleaningViewModel(SurveyModel survey, ContainerDetailModel container) : this(survey)
        {
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
        async void SubmitCleaning()
        {
            bool isValid = true;
            string errorMessage = "";

            // Check if all cleaning segments have exactly one photo
            var requiredSegments = new[] { "Top Outside", "Front Upper Half", "Front Lower Half", "Back Upper Half", "Back Lower Half" };
            var missingSegments = requiredSegments.Where(segment =>
                PhotoUploader.GetPhotoCountForSegment(segment) == 0).ToList();

            if (missingSegments.Any())
            {
                isValid = false;
                errorMessage += $"Please upload photos for: {string.Join(", ", missingSegments)}\n";
            }

            if (EndCleanDate < StartCleanDate)
            {
                isValid = false;
                errorMessage += "End date cannot be before start date.\n";
            }

            if (!isValid)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", errorMessage, "OK");
                return;
            }

            // Update the container if available, otherwise update the survey
            if (Container != null)
            {
                Container.CleaningStatus = StatusType.OnReview;
            }

            // Always update the survey for backward compatibility
            Survey.CleaningStatus = StatusType.OnReview;

            await Application.Current.MainPage.DisplayAlert("Success",
                "Cleaning data has been submitted for review.", "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}
