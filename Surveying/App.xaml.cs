namespace Surveying
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzc3NjQzMUAzMjM5MmUzMDJlMzAzYjMyMzkzYkFYSmJ4c3BTVW9ibHA4TVNOODJYam5FSlZtMU94T3UzUGlUT29sdXBFbXc9");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}