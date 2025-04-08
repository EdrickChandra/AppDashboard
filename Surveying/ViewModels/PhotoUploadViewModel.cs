using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Surveying.ViewModels
{
    public partial class PhotoUploadViewModel : ObservableObject
    {
        public ObservableCollection<PhotoItem> Photos { get; set; } = new ObservableCollection<PhotoItem>();

       
        public IAsyncRelayCommand UploadPhotoAsyncCommand => new AsyncRelayCommand(UploadPhotoAsync);
        public IRelayCommand DeletePhotoCommand => new RelayCommand<PhotoItem>(DeletePhoto);

        public async Task UploadPhotoAsync()
        {
            try
            {
         
                await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    System.Diagnostics.Debug.WriteLine("Calling MediaPicker.PickPhotoAsync...");
                    var photoResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                    {
                        Title = "Select a Photo"
                    });
                    System.Diagnostics.Debug.WriteLine("MediaPicker returned: " + (photoResult != null ? "Photo Selected" : "Null"));

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
