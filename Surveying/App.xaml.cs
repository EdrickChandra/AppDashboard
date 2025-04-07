namespace Surveying
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzgwMDM3M0AzMjM5MmUzMDJlMzAzYjMyMzkzYm9hQkoxcXdyRVAzRFBrR1A5L01nUDFXKzFFczd5YU9lSkoySmthNm9kcm89");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}