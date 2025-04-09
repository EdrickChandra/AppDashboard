using Microsoft.Maui.Controls;

namespace Surveying.Views
{
    public partial class ImageViewerPage : ContentPage
    {
        public ImageViewerPage(ImageSource imageSource)
        {
            InitializeComponent();
            FullScreenImage.Source = imageSource;
        }

        private async void OnImageTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}