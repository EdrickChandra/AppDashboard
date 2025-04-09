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

        private int MaxPhotoCount = 4;

        public ObservableCollection<PhotoItem> Photos { get; set; } = new ObservableCollection<PhotoItem>();


        [ObservableProperty]
        private int _photoColumnCount;


        private double MobileWidthThreshold = 768;

        public IAsyncRelayCommand UploadPhotoAsyncCommand => new AsyncRelayCommand(UploadPhotoAsync);
        public IRelayCommand DeletePhotoCommand => new RelayCommand<PhotoItem>(DeletePhoto);

        public PhotoUploadViewModel()
        {

            UpdateColumnCount();

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
                if (Photos.Count >= MaxPhotoCount)
                {
                    await Application.Current.MainPage.DisplayAlert("Limit Reached",
                        $"Maximum of {MaxPhotoCount} photos allowed.", "OK");
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

        public async void DeletePhoto(PhotoItem photo)
        {
            if (Photos.Contains(photo))
            {

                bool confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Confirm Delete",
                    "Are you sure you want to delete this photo?",
                    "Yes", "No");


                if (confirmed)
                {
                    Photos.Remove(photo);
                }
            }
        }
    }
}