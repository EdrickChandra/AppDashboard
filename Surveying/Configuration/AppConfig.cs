// Updated Surveying/Configuration/AppConfig.cs with better debugging

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
            ? "http://10.0.2.2:5196"  // Android emulator
            : "http://localhost:5196"; // iOS simulator and other platforms

        // Alternative for physical device testing (replace with your actual IP)
        // public static string ApiBaseUrl = "http://192.168.1.100:5196";
#else
        // Production URL
        public static string ApiBaseUrl = "https://your-production-api.com";
#endif

        // Test method to verify API connectivity
        public static async Task<bool> TestApiConnectivity()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.GetAsync($"{ApiBaseUrl}/api/container/GetTkContainersInDepot");

                    System.Diagnostics.Debug.WriteLine($"API Test - Status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"API Test - URL: {ApiBaseUrl}");

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Test Failed: {ex.Message}");
                return false;
            }
        }
    }
}

