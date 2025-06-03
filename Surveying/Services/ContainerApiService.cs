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
                PropertyNameCaseInsensitive = true,
                WriteIndented = true // For better debugging
            };

            System.Diagnostics.Debug.WriteLine($"API Base URL: {_baseUrl}");
        }

        public async Task<ApiResponse> CheckContainerExists(string contNumber)
        {
            try
            {
                string formattedContNumber = FormatContainerNumber(contNumber);
                string url = $"{_baseUrl}/api/container/GetByContNumber/{formattedContNumber}";

                System.Diagnostics.Debug.WriteLine($"Checking container: {contNumber} -> {formattedContNumber}");
                System.Diagnostics.Debug.WriteLine($"API URL: {url}");

                var response = await _httpClient.GetAsync(url);
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API Response: {json}");

                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, _jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"Deserialized IsSuccess: {apiResponse?.IsSuccess}");

                    return apiResponse;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = $"API call failed with status: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in CheckContainerExists: {ex}");
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
                string formattedContNumber = FormatContainerNumber(contNumber);
                string url = $"{_baseUrl}/api/container/GetContainerWithRepairCodes/{formattedContNumber}";

                System.Diagnostics.Debug.WriteLine($"Getting repair codes for: {contNumber} -> {formattedContNumber}");
                System.Diagnostics.Debug.WriteLine($"API URL: {url}");

                var response = await _httpClient.GetAsync(url);
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Raw API Response: {json}");

                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, _jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"API Response IsSuccess: {apiResponse?.IsSuccess}");
                    System.Diagnostics.Debug.WriteLine($"API Response Message: {apiResponse?.Message}");

                    if (apiResponse?.IsSuccess == true && apiResponse.Content != null)
                    {
                        // Deserialize the Content property to ContainerWithRepairCodesModel
                        var contentJson = JsonSerializer.Serialize(apiResponse.Content, _jsonOptions);
                        System.Diagnostics.Debug.WriteLine($"Content JSON: {contentJson}");

                        var container = JsonSerializer.Deserialize<ContainerWithRepairCodesModel>(contentJson, _jsonOptions);

                        System.Diagnostics.Debug.WriteLine($"Deserialized Container:");
                        System.Diagnostics.Debug.WriteLine($"  - ContNumber: {container?.ContNumber}");
                        System.Diagnostics.Debug.WriteLine($"  - IsRepairApproved: {container?.IsRepairApproved}");
                        System.Diagnostics.Debug.WriteLine($"  - ApprovalDate: {container?.ApprovalDate}");
                        System.Diagnostics.Debug.WriteLine($"  - RepairCodes Count: {container?.RepairCodes?.Count ?? 0}");

                        if (container?.RepairCodes != null)
                        {
                            for (int i = 0; i < Math.Min(3, container.RepairCodes.Count); i++)
                            {
                                var code = container.RepairCodes[i];
                                System.Diagnostics.Debug.WriteLine($"  RepairCode {i + 1}:");
                                System.Diagnostics.Debug.WriteLine($"    - RepairCode: {code.RepairCode}");
                                System.Diagnostics.Debug.WriteLine($"    - ComponentCode: {code.ComponentCode}");
                                System.Diagnostics.Debug.WriteLine($"    - LocationCode: {code.LocationCode}");
                                System.Diagnostics.Debug.WriteLine($"    - ComponentCategory: {code.ComponentCategory}");
                                System.Diagnostics.Debug.WriteLine($"    - DisplayDescription: {code.ComponentDescription}");
                            }
                        }

                        return container;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {response.ReasonPhrase}");
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetContainerWithRepairCodes: {ex}");
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
                string formatted = $"{contNumber.Substring(0, 4)} {contNumber.Substring(4, 3)} {contNumber.Substring(7, 3)} {contNumber.Substring(10, 1)}";
                System.Diagnostics.Debug.WriteLine($"Formatted container number: {contNumber} -> {formatted}");
                return formatted;
            }

            return contNumber;
        }
    }
}