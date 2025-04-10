using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Surveying.ViewModels
{
    public partial class PhotoUploadViewModel : ObservableObject
    {
        // Default max photos if not specified
        private int _maxPhotoCount = 4;

        // Property for maximum photo count with notification
        [ObservableProperty]
        private int maxAllowedPhotos;

        public ObservableCollection<PhotoItem> Photos { get; set; } = new ObservableCollection<PhotoItem>();

        // Property for column count that will be used in XAML
        [ObservableProperty]
        private int _photoColumnCount;

        // The threshold width in device-independent pixels
        private const double MobileWidthThreshold = 768;

        public IAsyncRelayCommand UploadPhotoAsyncCommand => new AsyncRelayCommand(UploadPhotoAsync);
        public IAsyncRelayCommand<PhotoItem> DeletePhotoCommand => new AsyncRelayCommand<PhotoItem>(DeletePhoto);

        // Constructor with default max photo count (4)
        public PhotoUploadViewModel() : this(4)
        {
        }

        // Constructor with custom max photo count
        public PhotoUploadViewModel(int maxPhotoCount)
        {
            _maxPhotoCount = maxPhotoCount;
            MaxAllowedPhotos = maxPhotoCount;

            // Set initial column count based on current screen size
            UpdateColumnCount();

            // Listen for orientation or size changes
            DeviceDisplay.MainDisplayInfoChanged += OnDisplayInfoChanged;
        }

        private void OnDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            UpdateColumnCount();
        }

        private void UpdateColumnCount()
        {

            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;


            PhotoColumnCount = screenWidth <= MobileWidthThreshold ? 1 : 4;

            System.Diagnostics.Debug.WriteLine($"Screen width: {screenWidth}, Column count: {PhotoColumnCount}");
        }

        public async Task UploadPhotoAsync()
        {
            try
            {
                // Check if maximum photo count has been reached
                if (Photos.Count >= MaxAllowedPhotos)
                {
                    await Application.Current.MainPage.DisplayAlert("Limit Reached",
                        $"Maximum of {MaxAllowedPhotos} photos allowed.", "OK");
                    return;
                }

                await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(async () =>
                {

                    double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

                    FileResult photoResult;

                    // If screen width is less than or equal to 768, use camera
                    if (screenWidth <= MobileWidthThreshold)
                    {
                        System.Diagnostics.Debug.WriteLine("Using camera for mobile device...");
                        photoResult = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                        {
                            Title = "Take a Photo"
                        });
                        System.Diagnostics.Debug.WriteLine("Camera result: " + (photoResult != null ? "Photo Captured" : "Null"));
                    }
                    // Otherwise use gallery picker
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Using gallery picker for larger screen...");
                        photoResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                        {
                            Title = "Select a Photo"
                        });
                        System.Diagnostics.Debug.WriteLine("Gallery result: " + (photoResult != null ? "Photo Selected" : "Null"));
                    }

                    if (photoResult == null)
                        return;

                    using var stream = await photoResult.OpenReadAsync();
                    var imageSource = ImageSource.FromFile(photoResult.FullPath);

                    Photos.Add(new PhotoItem
                    {
                        FileResult = photoResult,
                        ImageSource = imageSource
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in UploadPhotoAsync: " + ex);
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public async Task DeletePhoto(PhotoItem photo)
        {
            if (Photos.Contains(photo))
            {
                // Show confirmation dialog
                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Confirm Delete",
                    "Are you sure you want to delete this photo?",
                    "Yes", "No");

                // Only remove the photo if user confirmed
                if (confirmed)
                {
                    Photos.Remove(photo);
                }
            }
        }
    }
}