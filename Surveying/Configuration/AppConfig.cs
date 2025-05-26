namespace Surveying.Configuration
{
    public static class AppConfig
    {
        // Update this to your actual API URL
        // For Android emulator, use 10.0.2.2 instead of localhost
        // For iOS simulator, localhost should work
        // For physical devices, use your machine's IP address

#if DEBUG
        public static string ApiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5196"
            : "http://localhost:5196";
#else
        // Production URL
        public static string ApiBaseUrl = "https://your-production-api.com";
#endif
    }
}