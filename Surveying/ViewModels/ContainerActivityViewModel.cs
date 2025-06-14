using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Services;
using System.Collections.ObjectModel;


namespace Surveying.ViewModels
{
    /// <summary>
    /// UNIFIED CONTAINER ACTIVITY VIEWMODEL
    /// Replaces: RepairViewModel + EnhancedCleaningViewModel + PeriodicViewModel + SurveyorViewModel + PhotoUploadViewModel
    /// Uses Strategy pattern for activity-specific behavior
    /// </summary>
    public partial class ContainerActivityViewModel : BaseViewModel
    {
        private readonly IContainerApiService _containerApiService;
        private readonly ICleaningCriteriaService _cleaningCriteriaService;
        private readonly IActivityStrategy _currentStrategy;

        // ===== CORE PROPERTIES =====
        public Order Order { get; }
        public Container Container { get; }

        [ObservableProperty]
        private ActivityType activityType;

        [ObservableProperty]
        private string pageTitle;

        [ObservableProperty]
        private string containerNumber;

        // ===== ACTIVITY-SPECIFIC DATA =====
        [ObservableProperty]
        private DateTime activityDate = DateTime.Today;

        [ObservableProperty]
        private DateTime endDate = DateTime.Today.AddDays(1);

        [ObservableProperty]
        private string activityDescription = "";

        [ObservableProperty]
        private string activityNotes = "";

        // ===== STATUS TRACKING =====
        [ObservableProperty]
        private bool isActivityApproved;

        [ObservableProperty]
        private DateTime? approvalDate;

        [ObservableProperty]
        private string approvedBy = "";

        [ObservableProperty]
        private string approvalStatus = "";

        [ObservableProperty]
        private string approvalStatusColor = "#FFC107";

        [ObservableProperty]
        private bool showApprovalInfo;

        // ===== LOADING STATES =====
        [ObservableProperty]
        private bool isLoadingData = true;

        [ObservableProperty]
        private string loadingMessage = "Loading...";

        [ObservableProperty]
        private bool hasActivityData;

        [ObservableProperty]
        private string debugInfo = "";

        [ObservableProperty]
        private bool showDebugInfo = true; // Set to false in production

        // ===== ACTIVITY-SPECIFIC COLLECTIONS =====
        public ObservableCollection<RepairCode> RepairCodes { get; } = new();
        public ObservableCollection<RepairCode> CleaningRequirements { get; } = new();

        // ===== INTEGRATED PHOTO MANAGEMENT =====
        [ObservableProperty]
        private int maxAllowedPhotos = 4;

        [ObservableProperty]
        private int photoColumnCount = 2;

        [ObservableProperty]
        private bool isUploading = false;

        [ObservableProperty]
        private string uploadStatusMessage = "";

        [ObservableProperty]
        private int selectedSegmentIndex = 0;

        [ObservableProperty]
        private string currentSegmentLabel = "";

        [ObservableProperty]
        private ObservableCollection<string> photoSegments = new();

        public Dictionary<string, ObservableCollection<Photo>> PhotosBySegment { get; set; } = new();
        public ObservableCollection<Photo> Photos { get; set; } = new();

        [ObservableProperty]
        private ObservableCollection<Photo> currentSegmentPhotos = new();

        // ===== REVIEW/APPROVAL PROPERTIES (for Survey activity) =====
        [ObservableProperty]
        private bool canReviewActivity;

        [ObservableProperty]
        private bool activityAccept;

        [ObservableProperty]
        private bool activityReject;

        [ObservableProperty]
        private string rejectionRemark = "";

        // ===== CONSTRUCTOR =====
        public ContainerActivityViewModel(Order order, Container container, ActivityType activityType)
            : this(order, container, activityType, new ContainerApiService(), new CleaningCriteriaService())
        {
        }

        public ContainerActivityViewModel(Order order, Container container, ActivityType activityType,
            IContainerApiService containerApiService, ICleaningCriteriaService cleaningCriteriaService)
        {
            Order = order;
            Container = container;
            ActivityType = activityType;
            ContainerNumber = container.ContNumber;

            _containerApiService = containerApiService;
            _cleaningCriteriaService = cleaningCriteriaService;

            // Set up activity-specific configuration
            ConfigureForActivityType();

            // Initialize strategy pattern
            _currentStrategy = CreateActivityStrategy();

            // Initialize photo management
            InitializePhotoManagement();

            // Load activity data
            _ = LoadActivityDataAsync();
        }

        // ===== ACTIVITY TYPE CONFIGURATION =====
        private void ConfigureForActivityType()
        {
            switch (ActivityType)
            {
                case ActivityType.Cleaning:
                    PageTitle = "Enhanced Cleaning";
                    MaxAllowedPhotos = 1; // One per segment
                    PhotoSegments = new ObservableCollection<string>
                    {
                        "Top Outside", "Front Upper Half", "Front Lower Half", "Back Upper Half", "Back Lower Half"
                    };
                    LoadingMessage = "Loading cleaning requirements...";
                    break;

                case ActivityType.Repair:
                    PageTitle = "Repair Details";
                    MaxAllowedPhotos = 4;
                    PhotoSegments = new ObservableCollection<string> { "General Photos" };
                    LoadingMessage = "Loading repair codes...";
                    break;

                case ActivityType.Periodic:
                    PageTitle = "Periodic Maintenance";
                    MaxAllowedPhotos = 1;
                    PhotoSegments = new ObservableCollection<string> { "CSC Plate" };
                    LoadingMessage = "Loading periodic requirements...";
                    EndDate = DateTime.Today.AddYears(2).AddMonths(6); // Next due date
                    break;

                case ActivityType.Survey:
                    PageTitle = "Survey Review";
                    MaxAllowedPhotos = 4;
                    PhotoSegments = new ObservableCollection<string> { "Evidence Photos" };
                    LoadingMessage = "Loading review data...";
                    CanReviewActivity = Container.CleaningStatus == StatusType.OnReview; // Can review if submitted
                    break;
            }
        }

        // ===== STRATEGY PATTERN FOR ACTIVITY-SPECIFIC LOGIC =====
        private IActivityStrategy CreateActivityStrategy()
        {
            return ActivityType switch
            {
                ActivityType.Cleaning => new CleaningStrategy(_containerApiService, _cleaningCriteriaService),
                ActivityType.Repair => new RepairStrategy(_containerApiService),
                ActivityType.Periodic => new PeriodicStrategy(),
                ActivityType.Survey => new SurveyStrategy(),
                _ => throw new ArgumentException($"Unknown activity type: {ActivityType}")
            };
        }

        // ===== PHOTO MANAGEMENT (integrated from PhotoUploadViewModel) =====
        private void InitializePhotoManagement()
        {
            // Initialize photo segments
            foreach (var segment in PhotoSegments)
            {
                PhotosBySegment[segment] = new ObservableCollection<Photo>();
            }

            // Set initial segment
            if (PhotoSegments.Any())
            {
                CurrentSegmentLabel = PhotoSegments[0];
                CurrentSegmentPhotos = PhotosBySegment[CurrentSegmentLabel];
            }

            UpdateColumnCount();
            DeviceDisplay.MainDisplayInfoChanged += OnDisplayInfoChanged;
        }

        private void UpdateColumnCount()
        {
            var width = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            PhotoColumnCount = width switch
            {
                < 600 => 1,
                < 900 => 2,
                < 1200 => 3,
                _ => 4
            };
        }

        private void OnDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            UpdateColumnCount();
        }

        // ===== SEGMENT MANAGEMENT =====
        partial void OnSelectedSegmentIndexChanged(int value)
        {
            if (value >= 0 && value < PhotoSegments.Count)
            {
                CurrentSegmentLabel = PhotoSegments[value];
                CurrentSegmentPhotos = PhotosBySegment[CurrentSegmentLabel];
            }
        }

        // ===== DATA LOADING =====
        private async Task LoadActivityDataAsync()
        {
            try
            {
                IsLoadingData = true;
                DebugInfo = $"Starting {ActivityType} data load for {ContainerNumber}";

                // Use strategy to load activity-specific data
                var activityData = await _currentStrategy.LoadActivityDataAsync(Container);

                if (activityData != null)
                {
                    await ApplyActivityDataAsync(activityData);
                    HasActivityData = true;
                    LoadingMessage = $"{ActivityType} data loaded successfully";
                }
                else
                {
                    HasActivityData = false;
                    LoadingMessage = $"No {ActivityType} data found";
                }

                DebugInfo += $"\n{ActivityType} data load completed";
            }
            catch (Exception ex)
            {
                HasActivityData = false;
                LoadingMessage = $"Error loading {ActivityType} data: {ex.Message}";
                DebugInfo += $"\nException: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception in LoadActivityDataAsync: {ex}");
            }
            finally
            {
                IsLoadingData = false;
            }
        }

        private async Task ApplyActivityDataAsync(ActivityData activityData)
        {
            // Apply common data
            IsActivityApproved = activityData.IsApproved;
            ApprovalDate = activityData.ApprovalDate;
            ApprovedBy = activityData.ApprovedBy;
            UpdateApprovalDisplay();

            // Apply activity-specific data using strategy
            await _currentStrategy.ApplyActivitySpecificDataAsync(this, activityData);

            // Update container status
            UpdateContainerStatus();
        }

        private void UpdateApprovalDisplay()
        {
            if (IsActivityApproved)
            {
                ApprovalStatus = "Approved";
                ApprovalStatusColor = "#28A745"; // Green
                ShowApprovalInfo = true;
            }
            else
            {
                ApprovalStatus = "Pending Approval";
                ApprovalStatusColor = "#FFC107"; // Yellow/Orange
                ShowApprovalInfo = false;
            }
        }

        private void UpdateContainerStatus()
        {
            // Update container status based on activity type and completion
            switch (ActivityType)
            {
                case ActivityType.Cleaning:
                    // Status updated by strategy
                    break;
                case ActivityType.Repair:
                    // Status updated by strategy
                    break;
                case ActivityType.Periodic:
                    Container.PeriodicStatus = HasActivityData ? StatusType.OnReview : StatusType.NotFilled;
                    break;
                case ActivityType.Survey:
                    // Survey status based on review state
                    break;
            }

            Container.UpdateActivities();
        }

        // ===== PHOTO COMMANDS =====
        [RelayCommand]
        async Task UploadPhotoAsync()
        {
            try
            {
                IsUploading = true;
                UploadStatusMessage = "Opening camera...";

                var photoResult = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = $"Photo for {CurrentSegmentLabel}"
                });

                if (photoResult != null)
                {
                    await ProcessPhotoResultAsync(photoResult);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Failed to capture photo: {ex.Message}", "OK");
            }
            finally
            {
                IsUploading = false;
                UploadStatusMessage = "";
            }
        }

        private async Task ProcessPhotoResultAsync(FileResult photoResult)
        {
            try
            {
                UploadStatusMessage = "Processing photo...";

                var photo = new Photo(photoResult, null, CurrentSegmentLabel);

                using var stream = await photoResult.OpenReadAsync();
                photo.ImageSource = ImageSource.FromStream(() => stream);

                await photo.CalculateFileSizeAsync();

                // Add to current segment and main collection
                CurrentSegmentPhotos.Add(photo);
                Photos.Add(photo);

                UploadStatusMessage = $"Photo added to {CurrentSegmentLabel}";
                await Task.Delay(2000);
                if (UploadStatusMessage.Contains("added to"))
                {
                    UploadStatusMessage = "";
                }
            }
            catch (Exception ex)
            {
                UploadStatusMessage = $"Error processing photo: {ex.Message}";
                await Task.Delay(3000);
                UploadStatusMessage = "";
            }
        }

        [RelayCommand]
        async Task DeletePhoto(Photo photo)
        {
            if (CurrentSegmentPhotos.Contains(photo))
            {
                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Delete Photo",
                    $"Delete this photo from {photo.Segment}?",
                    "Delete", "Cancel");

                if (confirmed)
                {
                    CurrentSegmentPhotos.Remove(photo);
                    Photos.Remove(photo);

                    if (PhotosBySegment.ContainsKey(photo.Segment))
                    {
                        PhotosBySegment[photo.Segment].Remove(photo);
                    }

                    photo.Dispose();
                }
            }
        }

        // ===== ACTIVITY COMMANDS =====
        [RelayCommand]
        async Task ViewFullImage(Photo photo)
        {
            if (photo?.ImageSource != null)
            {
                // Navigate to image viewer - you'll need to create this page
                // var imageViewerPage = new ImageViewerPage(photo.ImageSource);
                // await Application.Current.MainPage.Navigation.PushAsync(imageViewerPage);

                // For now, show alert
                await Application.Current.MainPage.DisplayAlert("Image Viewer",
                    "Image viewer functionality would open here", "OK");
            }
        }

        [RelayCommand]
        async Task RefreshActivityData()
        {
            await LoadActivityDataAsync();
        }

        [RelayCommand]
        async Task ShowDebugDetails()
        {
            await Application.Current.MainPage.DisplayAlert("Debug Info", DebugInfo, "OK");
        }

        [RelayCommand]
        async Task SubmitActivity()
        {
            try
            {
                // Validate using strategy
                var validation = await _currentStrategy.ValidateSubmissionAsync(this);

                if (!validation.IsValid)
                {
                    await Application.Current.MainPage.DisplayAlert("Validation Error", validation.ErrorMessage, "OK");
                    return;
                }

                // Submit using strategy
                var success = await _currentStrategy.SubmitActivityAsync(this);

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Success",
                        $"{ActivityType} data has been submitted successfully.", "OK");

                    await Application.Current.MainPage.Navigation.PopToRootAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        $"Failed to submit {ActivityType} data.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Error submitting {ActivityType}: {ex.Message}", "OK");
            }
        }

        // ===== REVIEW COMMANDS (for Survey activity) =====
        [RelayCommand]
        async Task SubmitReview()
        {
            if (ActivityType != ActivityType.Survey)
                return;

            if (!CanReviewActivity)
            {
                await Application.Current.MainPage.DisplayAlert("Not Ready",
                    "This item is not ready for review.", "OK");
                return;
            }

            if (Photos.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Photo Required",
                    "Please upload at least one photo before submitting.", "OK");
                return;
            }

            if (ActivityAccept)
            {
                Container.CleaningStatus = StatusType.Finished;
            }
            else if (ActivityReject)
            {
                Container.CleaningStatus = StatusType.Rejected;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Selection Required",
                    "Please select either Finish or Reject before submitting.", "OK");
                return;
            }

            Container.UpdateActivities();

            await Application.Current.MainPage.DisplayAlert("Status Updated",
                $"Review has been submitted.", "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }

        // ===== UTILITY METHODS =====
        public int GetPhotoCountForSegment(string segmentName)
        {
            return PhotosBySegment.ContainsKey(segmentName) ? PhotosBySegment[segmentName].Count : 0;
        }

        public Photo GetFirstPhotoForSegment(string segmentName)
        {
            return PhotosBySegment.ContainsKey(segmentName) && PhotosBySegment[segmentName].Any()
                ? PhotosBySegment[segmentName].First()
                : null;
        }

        // ===== CLEANUP =====
        protected void OnDisposing()
        {
            DeviceDisplay.MainDisplayInfoChanged -= OnDisplayInfoChanged;

            foreach (var photo in Photos)
            {
                photo.Dispose();
            }
        }
    }
}