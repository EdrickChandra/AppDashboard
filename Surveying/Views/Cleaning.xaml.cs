using Surveying.Models;
using Surveying.ViewModels;
using System.Collections.ObjectModel;

namespace Surveying.Views
{
    public partial class Cleaning : ContentPage
    {
        CleaningViewModel viewModel;
        public ObservableCollection<PhotoItem> Photos { get; set; } = new ObservableCollection<PhotoItem>();

        public Cleaning(string containerNumber)
        {
            InitializeComponent();

            viewModel = new CleaningViewModel(containerNumber);

            BindingContext = viewModel;
        }
        private async void OnUploadPhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var photoResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a Photo"
                });

                if (photoResult == null)
                    return;

                using var stream = await photoResult.OpenReadAsync();
                var imageSource = ImageSource.FromFile(photoResult.FullPath);


                // Now add the photo to the VM's Photos
                viewModel.Photos.Add(new PhotoItem
                {
                    FileResult = photoResult,
                    ImageSource = imageSource
                });
           

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PhotoItem photo)
            {
                viewModel.Photos.Remove(photo);
            }
        }

        private void OnSubmitClicked(object sender, EventArgs e)
        {

            var start = StartCleanDatePicker.Date;
            var end = EndCleanDatePicker.Date;

  
            viewModel.StartCleanDate = start;
            viewModel.EndCleanDate = end;

        }
    }
}

