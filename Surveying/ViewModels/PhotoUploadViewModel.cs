using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Surveying.ViewModels
{
    public partial class PhotoUploadViewModel : ObservableObject
    {
        private int _maxPhotoCount = 4;

        [ObservableProperty]
        private int maxAllowedPhotos;

        [ObservableProperty]
        private int _photoColumnCount;

        [ObservableProperty]
        private bool isUploading = false;

        [ObservableProperty]
        private string uploadStatusMessage = "";

        [ObservableProperty]
        private int selectedSegmentIndex = 0;

        [ObservableProperty]
        private string currentSegmentLabel = "";

        // ===== SIMPLIFIED PHOTO SEGMENTS =====
        [ObservableProperty]
        private ObservableCollection<string> photoSegments;

        // ===== SIMPLIFIED PHOTO STORAGE =====
        // OLD: Dictionary<string, ObservableCollection<FlexiblePhotoItem>> PhotosBySegment + PhotoItem collection
        // NEW: Dictionary<string, ObservableCollection<Photo>> PhotosBySegment + Photo collection
        public Dictionary<string, ObservableCollection<Photo>> PhotosBySegment { get; set; }

        // SIMPLIFIED: Single Photo collection (no more PhotoItem vs FlexiblePhotoItem confusion)
        public ObservableCollection<Photo> Photos { get; set; }

        // Current segment's photos for binding
        [ObservableProperty]
        private ObservableCollection<Photo> currentSegmentPhotos;

        private const double MobileWidthThreshold = 768;

        // ===== SIMPLIFIED COMMANDS =====
        public IAsyncRelayCommand UploadPhotoAsyncCommand => new AsyncRelayCommand(UploadPhotoAsync);
        public IAsyncRelayCommand<Photo> DeletePhotoCommand => new AsyncRelayCommand<Photo>(DeletePhoto);  // SIMPLIFIED: Single Photo type
        public IAsyncRelayCommand ShowPhotoOptionsCommand => new AsyncRelayCommand(ShowPhotoOptions);

        // ===== CONSTRUCTORS - SIMPLIFIED =====
        public PhotoUploadViewModel() : this(4, new List<string> { "General" }) { }

        public PhotoUploadViewModel(int maxPhotoCount) : this(maxPhotoCount, new List<string> { "General" }) { }

        public PhotoUploadViewModel(int maxPhotoCount, List<string> segments)
        {
            _maxPhotoCount = maxPhotoCount;
            MaxAllowedPhotos = maxPhotoCount;

            PhotoSegments = new ObservableCollection<string>(segments);
            PhotosBySegment = new Dictionary<string, ObservableCollection<Photo>>();  // SIMPLIFIED: Single Photo type
            Photos = new ObservableCollection<Photo>();  // SIMPLIFIED: Single Photo type

            // Initialize each segment with empty collection
            foreach (var segment in segments)
            {
                PhotosBySegment[segment] = new ObservableCollection<Photo>();  // SIMPLIFIED: Single Photo type
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

        // Factory methods - SIMPLIFIED return types
        public static PhotoUploadViewModel CreateForCleaning()
        {
            return new PhotoUploadViewModel(1, new List<string>
            {
                "Top Outside", "Front Upper Half", "Front Lower Half", "Back Upper Half", "Back Lower Half"
            });
        }

        public static PhotoUploadViewModel CreateForRepair()
        {
            return new PhotoUploadViewModel(4, new List<string> { "General Photos" });
        }

        public static PhotoUploadViewModel CreateForPeriodic()
        {
            return new PhotoUploadViewModel(1, new List<string> { "CSC Plate" });
        }

        // ===== SIMPLIFIED PHOTO PROCESSING =====
        private async Task ProcessPhotoResult(FileResult photoResult)
        {
            try
            {
                UploadStatusMessage = "Processing photo...";

                // SIMPLIFIED: Create single Photo object (no more FlexiblePhotoItem complexity)
                var photo = new Photo(photoResult, null, CurrentSegmentLabel);

                using var stream = await photoResult.OpenReadAsync();
                photo.ImageSource = ImageSource.FromStream(() => stream);

                // Calculate file size
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
                await HandlePhotoError(ex);
            }
        }

        // ===== SIMPLIFIED DELETE =====
        public async Task DeletePhoto(Photo photo)  // SIMPLIFIED: Single Photo type parameter
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

                    // Remove from segment dictionary
                    if (PhotosBySegment.ContainsKey(photo.Segment))
                    {
                        PhotosBySegment[photo.Segment].Remove(photo);
                    }

                    // Clean up resources
                    photo.Dispose();
                }
            }
        }

        // Get photo count for specific segment
        public int GetPhotoCountForSegment(string segmentName)
        {
            return PhotosBySegment.ContainsKey(segmentName) ? PhotosBySegment[segmentName].Count : 0;
        }

        // Get first photo for a segment
        public Photo GetFirstPhotoForSegment(string segmentName)  // SIMPLIFIED: Return Photo directly
        {
            return PhotosBySegment.ContainsKey(segmentName) && PhotosBySegment[segmentName].Any()
                ? PhotosBySegment[segmentName].First()
                : null;
        }

        // Other methods stay the same but use Photo instead of PhotoItem/FlexiblePhotoItem
        // ... (simplified implementations)
    }
}