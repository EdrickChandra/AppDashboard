using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Views;
using System.ComponentModel;

namespace Surveying.ViewModels
{
    public partial class SurveyorViewModel : BaseViewModel
    {
        public SurveyModel Survey { get; }
        public ContainerDetailModel Container { get; }

        public string ContainerNumber => Container?.ContNumber ?? Survey.ContNumber;

        // Checkboxes for cleaning status
        [ObservableProperty]
        private bool cleaningAccept;

        [ObservableProperty]
        private bool cleaningReject;

        [ObservableProperty]
        private string cleaningRejectionRemark = "";

        // Tracking if cleaning is ready for review
        [ObservableProperty]
        private bool cleaningReadyForReview;

        // Photo upload functionality - keep using default constructor for backward compatibility
        public PhotoUploadViewModel PhotoUploader { get; } = new PhotoUploadViewModel(4);

        public SurveyorViewModel(SurveyModel survey, ContainerDetailModel container)
        {
            Survey = survey;
            Container = container;

            // Initialize checkbox states and review status
            UpdateStateFromContainer();

            if (Container != null)
            {
                // Subscribe to container property changes
                Container.PropertyChanged += Container_PropertyChanged;
            }
            else
            {
                // If no container, use survey for backward compatibility
                UpdateStateFromSurvey();
                Survey.PropertyChanged += Survey_PropertyChanged;
            }
        }

        private void Container_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ContainerDetailModel.CleaningStatus))
            {
                UpdateStateFromContainer();
            }
        }

        private void Survey_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SurveyModel.CleaningStatus))
            {
                UpdateStateFromSurvey();
            }
        }

        private void UpdateStateFromContainer()
        {
            if (Container == null) return;

            // Update cleaning checkbox and review status
            CleaningReadyForReview = Container.CleaningStatus == StatusType.OnReview;
            CleaningAccept = Container.CleaningStatus == StatusType.Finished;
            CleaningReject = Container.CleaningStatus == StatusType.Rejected;
        }

        private void UpdateStateFromSurvey()
        {
            // Update cleaning checkbox and review status
            CleaningReadyForReview = Survey.CleaningStatus == StatusType.OnReview;
            CleaningAccept = Survey.CleaningStatus == StatusType.Finished;
            CleaningReject = Survey.CleaningStatus == StatusType.Rejected;
        }

        partial void OnCleaningAcceptChanged(bool value)
        {
            if (value)
                CleaningReject = false;
        }

        partial void OnCleaningRejectChanged(bool value)
        {
            if (value)
            {
                CleaningAccept = false;
            }
            else
            {
                CleaningRejectionRemark = string.Empty;
            }
        }

        [RelayCommand]
        async void ViewFullImage(PhotoItem photo) // Keep as PhotoItem for backward compatibility
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
            // Only allow submission if status is OnReview
            if (!CleaningReadyForReview)
            {
                await Application.Current.MainPage.DisplayAlert("Not Ready",
                    "This item is not ready for review. Make sure Cleaning has been submitted first.", "OK");
                return;
            }

            // Require at least one photo
            if (PhotoUploader.Photos == null || PhotoUploader.Photos.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Photo Required",
                    "Please upload at least one photo before submitting.", "OK");
                return;
            }

            if (CleaningAccept)
            {
                if (Container != null)
                {
                    Container.CleaningStatus = StatusType.Finished;
                    Container.UpdateActivities();
                }
                // Also update survey
                Survey.CleaningStatus = StatusType.Finished;

                await Application.Current.MainPage.DisplayAlert("Status Updated",
                    "Cleaning has been marked as Finished.", "OK");
            }
            else if (CleaningReject)
            {
                if (Container != null)
                {
                    Container.CleaningStatus = StatusType.Rejected;
                    Container.UpdateActivities();
                }
                // Also update survey
                Survey.CleaningStatus = StatusType.Rejected;

                await Application.Current.MainPage.DisplayAlert("Status Updated",
                    "Cleaning has been rejected.", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Selection Required",
                    "Please select either Finish or Reject before submitting.", "OK");
                return;
            }

            // Return to main page after successful submission
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}
