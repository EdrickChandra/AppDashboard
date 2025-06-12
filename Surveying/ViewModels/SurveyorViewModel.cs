using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Surveying.ViewModels
{
    public partial class SurveyorViewModel : BaseViewModel
    {
        // ===== SIMPLIFIED MODELS =====
        // OLD: SurveyModel survey, ContainerDetailModel container
        // NEW: Order order, Container container (unified models)
        public Order Order { get; }
        public Container Container { get; }

        public string ContainerNumber => Container.ContNumber;

        // ===== SIMPLIFIED CLEANING REVIEW PROPERTIES =====
        // Checkboxes for cleaning status
        [ObservableProperty]
        private bool cleaningAccept;

        [ObservableProperty]
        private bool cleaningReject;

        [ObservableProperty]
        private string cleaningRejectionRemark = "";

        // ===== SIMPLIFIED STATUS TRACKING =====
        // OLD: Complex property change events and manual syncing
        // NEW: Direct access to Container.CleaningStatus
        public bool CleaningReadyForReview => Container.CleaningStatus == StatusType.OnReview;

        // ===== SIMPLIFIED PHOTO UPLOADER =====
        // Photo upload functionality - single Photo type
        public PhotoUploadViewModel PhotoUploader { get; } = new PhotoUploadViewModel(4);

        // ===== CONSTRUCTOR - SIMPLIFIED =====
        // OLD: Took SurveyModel + ContainerDetailModel with complex event handling
        // NEW: Takes Order + Container (unified models)
        public SurveyorViewModel(Order order, Container container)
        {
            Order = order;
            Container = container;

            // Initialize checkbox states from container
            UpdateStateFromContainer();

            // Subscribe to container property changes (simplified)
            Container.PropertyChanged += Container_PropertyChanged;
        }

        // ===== SIMPLIFIED STATE MANAGEMENT =====
        private void Container_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Container.CleaningStatus))
            {
                UpdateStateFromContainer();
                OnPropertyChanged(nameof(CleaningReadyForReview));
            }
        }

        private void UpdateStateFromContainer()
        {
            // Update cleaning checkbox and review status directly from Container
            CleaningAccept = Container.CleaningStatus == StatusType.Finished;
            CleaningReject = Container.CleaningStatus == StatusType.Rejected;
        }

        // ===== CHECKBOX LOGIC - SIMPLIFIED =====
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

            // SIMPLIFIED: Direct status update (no manual syncing needed)
            if (CleaningAccept)
            {
                Container.CleaningStatus = StatusType.Finished;
                Container.UpdateActivities();

                await Application.Current.MainPage.DisplayAlert("Status Updated",
                    "Cleaning has been marked as Finished.", "OK");
            }
            else if (CleaningReject)
            {
                Container.CleaningStatus = StatusType.Rejected;
                Container.UpdateActivities();

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
