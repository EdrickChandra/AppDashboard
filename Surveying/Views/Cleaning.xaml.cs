using Surveying.Models;
using System.Collections.ObjectModel;

namespace Surveying.Views;

public partial class Cleaning : ContentPage
{

    public ObservableCollection<PhotoItem> Photos { get; set; } = new ObservableCollection<PhotoItem>();

    public Cleaning()
	{
        InitializeComponent();

        BindingContext = this;
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
            var imageSource = ImageSource.FromStream(() => stream);

            Photos.Add(new PhotoItem
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
        if (sender is Button button && button.BindingContext is PhotoItem photo)
        {

            Photos.Remove(photo);
        }
    }
}
