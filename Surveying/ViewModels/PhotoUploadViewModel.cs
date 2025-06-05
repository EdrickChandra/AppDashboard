using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

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

        // Flexible photo segments - can be customized per page
        [ObservableProperty]
        private ObservableCollection<string> photoSegments;

        // Dictionary to store photos by segment
        public Dictionary<string, ObservableCollection<FlexiblePhotoItem>> PhotosBySegment { get; set; }

        // Backward compatibility - this is what your existing code expects
        public ObservableCollection<PhotoItem> Photos { get; set; }

        // Current segment's photos for binding
        [ObservableProperty]
        private ObservableCollection<FlexiblePhotoItem> currentSegmentPhotos;

        private const double MobileWidthThreshold = 768;

        public IAsyncRelayCommand UploadPhotoAsyncCommand => new AsyncRelayCommand(UploadPhotoAsync);
        public IAsyncRelayCommand<PhotoItem> DeletePhotoCommand => new AsyncRelayCommand<PhotoItem>(DeletePhoto);
        public IAsyncRelayCommand ShowPhotoOptionsCommand => new AsyncRelayCommand(ShowPhotoOptions);

        // Default constructor with standard segments
        public PhotoUploadViewModel() : this(4, new List<string> { "General" }) { }

        // Constructor with custom max photos
        public PhotoUploadViewModel(int maxPhotoCount) : this(maxPhotoCount, new List<string> { "General" }) { }

        // Constructor with custom segments (most flexible)
        public PhotoUploadViewModel(int maxPhotoCount, List<string> segments)
        {
            _maxPhotoCount = maxPhotoCount;
            MaxAllowedPhotos = maxPhotoCount;

            // Initialize photo segments
            PhotoSegments = new ObservableCollection<string>(segments);
            PhotosBySegment = new Dictionary<string, ObservableCollection<FlexiblePhotoItem>>();
            Photos = new ObservableCollection<PhotoItem>(); // Backward compatibility

            // Initialize each segment with empty collection
            foreach (var segment in segments)
            {
                PhotosBySegment[segment] = new ObservableCollection<FlexiblePhotoItem>();
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

        // Factory methods for different page types
        public static PhotoUploadViewModel CreateForCleaning()
        {
            return new PhotoUploadViewModel(1, new List<string>
            {
                "Top Outside",
                "Front Upper Half",
                "Front Lower Half",
                "Back Upper Half",
                "Back Lower Half"
            });
        }

        public static PhotoUploadViewModel CreateForRepair()
        {
            // Repair uses existing flexible system for individual repair codes
            return new PhotoUploadViewModel(4, new List<string>
            {
                "General Photos"
            });
        }

        public static PhotoUploadViewModel CreateForPeriodic()
        {
            return new PhotoUploadViewModel(1, new List<string>
            {
                "CSC Plate"
            });
        }

        public static PhotoUploadViewModel CreateForSurvey()
        {
            // Survey will be implemented later - use backward compatible approach
            return new PhotoUploadViewModel(4, new List<string>
            {
                "General"
            });
        }

        private void OnDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            UpdateColumnCount();
        }

        private void UpdateColumnCount()
        {
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            PhotoColumnCount = screenWidth <= MobileWidthThreshold ? 1 : 4;
        }

        // Segment selection changed handler
        partial void OnSelectedSegmentIndexChanged(int value)
        {
            if (value >= 0 && value < PhotoSegments.Count)
            {
                CurrentSegmentLabel = PhotoSegments[value];
                CurrentSegmentPhotos = PhotosBySegment[CurrentSegmentLabel];
                OnPropertyChanged(nameof(CurrentSegmentPhotos));
            }
        }

        // Method to programmatically change segment
        public void SelectSegment(string segmentName)
        {
            var index = PhotoSegments.IndexOf(segmentName);
            if (index >= 0)
            {
                SelectedSegmentIndex = index;
            }
        }

        // Get photo count for specific segment
        public int GetPhotoCountForSegment(string segmentName)
        {
            return PhotosBySegment.ContainsKey(segmentName) ? PhotosBySegment[segmentName].Count : 0;
        }

        // Get first photo for a segment (useful for repair preview)
        public FlexiblePhotoItem GetFirstPhotoForSegment(string segmentName)
        {
            return PhotosBySegment.ContainsKey(segmentName) && PhotosBySegment[segmentName].Any()
                ? PhotosBySegment[segmentName].First()
                : null;
        }

        // Check if current segment has reached max photos (1 photo per segment)
        public bool IsCurrentSegmentFull => CurrentSegmentPhotos?.Count >= 1;

        // Enhanced photo options with better UX
        public async Task ShowPhotoOptions()
        {
            // If segment is full, ask user if they want to replace
            if (IsCurrentSegmentFull)
            {
                bool replace = await Application.Current.MainPage.DisplayAlert("Replace Photo",
                    $"This segment already has a photo. Do you want to replace it?", "Replace", "Cancel");

                if (!replace) return;

                // Remove existing photo first
                var existingPhoto = CurrentSegmentPhotos.FirstOrDefault();
                if (existingPhoto != null)
                {
                    CurrentSegmentPhotos.Remove(existingPhoto);
                    Photos.Remove(existingPhoto);
                    if (PhotosBySegment.ContainsKey(existingPhoto.Segment))
                    {
                        PhotosBySegment[existingPhoto.Segment].Remove(existingPhoto);
                    }
                    existingPhoto.Dispose();
                }
            }

            try
            {
                string action = await Application.Current.MainPage.DisplayActionSheet(
                    $"Add Photo to {CurrentSegmentLabel}", "Cancel", null, "Take Photo", "Choose from Gallery");

                switch (action)
                {
                    case "Take Photo":
                        await CapturePhotoAsync();
                        break;
                    case "Choose from Gallery":
                        await PickPhotoAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                await HandlePhotoError(ex);
            }
        }

        // Improved photo capture with better error handling
        public async Task CapturePhotoAsync()
        {
            try
            {
                IsUploading = true;
                UploadStatusMessage = "Opening camera...";

                var photoResult = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Take a Photo for Survey"
                });

                if (photoResult != null)
                {
                    await ProcessPhotoResult(photoResult);
                }
            }
            catch (FeatureNotSupportedException)
            {
                await Application.Current.MainPage.DisplayAlert("Not Supported",
                    "Camera is not supported on this device.", "OK");
            }
            catch (Exception ex)
            {
                await HandlePhotoError(ex);
            }
            finally
            {
                IsUploading = false;
                UploadStatusMessage = "";
            }
        }

        // Improved gallery picker
        public async Task PickPhotoAsync()
        {
            try
            {
                IsUploading = true;
                UploadStatusMessage = "Opening gallery...";

                var photoResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select Photo for Survey"
                });

                if (photoResult != null)
                {
                    await ProcessPhotoResult(photoResult);
                }
            }
            catch (FeatureNotSupportedException)
            {
                await Application.Current.MainPage.DisplayAlert("Not Supported",
                    "Photo gallery is not supported on this device.", "OK");
            }
            catch (Exception ex)
            {
                await HandlePhotoError(ex);
            }
            finally
            {
                IsUploading = false;
                UploadStatusMessage = "";
            }
        }

        // Enhanced photo processing with segment assignment
        private async Task ProcessPhotoResult(FileResult photoResult)
        {
            try
            {
                UploadStatusMessage = "Processing photo...";

                var photoItem = new FlexiblePhotoItem
                {
                    FileResult = photoResult,
                    Segment = CurrentSegmentLabel,
                    CaptureDate = DateTime.Now,
                    FileSize = await GetFileSize(photoResult),
                    OriginalFileName = photoResult.FileName
                };

                using var stream = await photoResult.OpenReadAsync();
                photoItem.ImageSource = ImageSource.FromStream(() => stream);

                // Add to current segment
                CurrentSegmentPhotos.Add(photoItem);

                // Also add to backward compatibility collection
                Photos.Add(photoItem);

                var currentCount = CurrentSegmentPhotos.Count;
                UploadStatusMessage = $"Photo added to {CurrentSegmentLabel}";

                // Auto-clear status message after 2 seconds
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

        // Get file size helper
        private async Task<long> GetFileSize(FileResult fileResult)
        {
            try
            {
                using var stream = await fileResult.OpenReadAsync();
                return stream.Length;
            }
            catch
            {
                return 0;
            }
        }

        // Enhanced error handling
        private async Task HandlePhotoError(Exception ex)
        {
            string errorMessage = ex switch
            {
                PermissionException => "Permission denied. Please enable camera/photo permissions in settings.",
                FeatureNotSupportedException => "This feature is not supported on your device.",
                InvalidOperationException => "Please try again. Make sure you have available storage space.",
                _ => $"An error occurred: {ex.Message}"
            };

            await Application.Current.MainPage.DisplayAlert("Photo Error", errorMessage, "OK");
            System.Diagnostics.Debug.WriteLine($"Photo upload error: {ex}");
        }

        // Backward compatibility method
        public async Task UploadPhotoAsync()
        {
            await ShowPhotoOptions();
        }

        // Enhanced delete with confirmation and better UX - backward compatible signature
        public async Task DeletePhoto(PhotoItem photo)
        {
            if (photo is FlexiblePhotoItem flexPhoto)
            {
                if (CurrentSegmentPhotos.Contains(flexPhoto))
                {
                    bool confirmed = await Application.Current.MainPage.DisplayAlert(
                        "Delete Photo",
                        $"Delete this photo from {flexPhoto.Segment}?",
                        "Delete", "Cancel");

                    if (confirmed)
                    {
                        CurrentSegmentPhotos.Remove(flexPhoto);
                        Photos.Remove(photo);

                        // Remove from segment dictionary as well
                        if (PhotosBySegment.ContainsKey(flexPhoto.Segment))
                        {
                            PhotosBySegment[flexPhoto.Segment].Remove(flexPhoto);
                        }

                        // Clean up resources
                        flexPhoto.Dispose();
                    }
                }
            }
            else if (Photos.Contains(photo))
            {
                // Handle regular PhotoItem for backward compatibility
                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Delete Photo",
                    "Are you sure you want to delete this photo?",
                    "Delete", "Cancel");

                if (confirmed)
                {
                    Photos.Remove(photo);
                }
            }
        }

        // Clean up resources
        public void Dispose()
        {
            DeviceDisplay.MainDisplayInfoChanged -= OnDisplayInfoChanged;

            foreach (var photo in Photos.OfType<FlexiblePhotoItem>())
            {
                photo.Dispose();
            }
        }
    }

    // Flexible photo item model for segmented approach
    public class FlexiblePhotoItem : PhotoItem, IDisposable
    {
        public string Segment { get; set; }
        public DateTime CaptureDate { get; set; }
        public long FileSize { get; set; }
        public string OriginalFileName { get; set; }
        public bool IsCompressed { get; set; }
        public string Description { get; set; }

        public string FileSizeFormatted => FormatFileSize(FileSize);

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
            return $"{bytes / (1024 * 1024):F1} MB";
        }

        public void Dispose()
        {
            // Clean up any unmanaged resources if needed
        }
    }
}