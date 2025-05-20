namespace Surveying
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzg2Nzc1MkAzMjM5MmUzMDJlMzAzYjMyMzkzYmMrSXV1VHg2WTBEeDNxUXljOHZ6UU13YnlRYytVM2Yza1dWRC91emF1MFU9");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}