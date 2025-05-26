using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Surveying.Models;
using Surveying.Configuration;


namespace Surveying.Services
{
    public interface IContainerApiService
    {
        Task<ApiResponse> CheckContainerExists(string contNumber);
        Task<ContainerWithRepairCodesModel> GetContainerWithRepairCodes(string contNumber);
    }

    public class ContainerApiService : IContainerApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ContainerApiService()
        {
            _httpClient = new HttpClient();
            _baseUrl = AppConfig.ApiBaseUrl;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse> CheckContainerExists(string contNumber)
        {
            try
            {
                // Format the container number to match API format (XXXX XXX XXX X)
                string formattedContNumber = FormatContainerNumber(contNumber);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/container/GetByContNumber/{formattedContNumber}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, _jsonOptions);
                    return apiResponse;
                }
                else
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = $"API call failed with status: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Error connecting to API: {ex.Message}"
                };
            }
        }

        public async Task<ContainerWithRepairCodesModel> GetContainerWithRepairCodes(string contNumber)
        {
            try
            {
                // Format the container number to match API format
                string formattedContNumber = FormatContainerNumber(contNumber);

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/container/GetContainerWithRepairCodes/{formattedContNumber}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, _jsonOptions);

                    if (apiResponse.IsSuccess && apiResponse.Content != null)
                    {
                        // Deserialize the Content property to ContainerWithRepairCodesModel
                        var contentJson = JsonSerializer.Serialize(apiResponse.Content);
                        var container = JsonSerializer.Deserialize<ContainerWithRepairCodesModel>(contentJson, _jsonOptions);
                        return container;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting container with repair codes: {ex.Message}");
                return null;
            }
        }

        private string FormatContainerNumber(string contNumber)
        {
            // Remove any spaces first
            contNumber = contNumber.Replace(" ", "").ToUpper();

            // Format as XXXX XXX XXX X (matching the API's BwctsContNumFormat)
            if (contNumber.Length >= 11)
            {
                return $"{contNumber.Substring(0, 4)} {contNumber.Substring(4, 3)} {contNumber.Substring(7, 3)} {contNumber.Substring(10, 1)}";
            }

            return contNumber;
        }
    }
}