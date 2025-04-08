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
        public ObservableCollection<PhotoItem> Photos { get; set; } = new ObservableCollection<PhotoItem>();

        // Property for column count that will be used in XAML
        [ObservableProperty]
        private int _photoColumnCount;

        // The threshold width in device-independent pixels
        private const double MobileWidthThreshold = 768;

        public IAsyncRelayCommand UploadPhotoAsyncCommand => new AsyncRelayCommand(UploadPhotoAsync);
        public IRelayCommand DeletePhotoCommand => new RelayCommand<PhotoItem>(DeletePhoto);

        public PhotoUploadViewModel()
        {
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

        
            PhotoColumnCount = screenWidth <= MobileWidthThreshold ? 1 : 3;

            System.Diagnostics.Debug.WriteLine($"Screen width: {screenWidth}, Column count: {PhotoColumnCount}");
        }

        public async Task UploadPhotoAsync()
        {
            try
            {
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

        public void DeletePhoto(PhotoItem photo)
        {
            if (Photos.Contains(photo))
            {
                Photos.Remove(photo);
            }
        }
    }
}