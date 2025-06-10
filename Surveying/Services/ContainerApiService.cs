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
        Task<ContainerWithRepairCodesModel> GetContainerCleaningDetails(string contNumber);
        Task<ApiResponse> GetContainersForCleaning();
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
                WriteIndented = true
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
                        var contentJson = JsonSerializer.Serialize(apiResponse.Content, _jsonOptions);
                        System.Diagnostics.Debug.WriteLine($"Content JSON: {contentJson}");

                        var container = JsonSerializer.Deserialize<ContainerWithRepairCodesModel>(contentJson, _jsonOptions);

                        System.Diagnostics.Debug.WriteLine($"Deserialized Container:");
                        System.Diagnostics.Debug.WriteLine($"  - ContNumber: {container?.ContNumber}");
                        System.Diagnostics.Debug.WriteLine($"  - IsRepairApproved: {container?.IsRepairApproved}");
                        System.Diagnostics.Debug.WriteLine($"  - ApprovalDate: {container?.ApprovalDate}");
                        System.Diagnostics.Debug.WriteLine($"  - Commodity: {container?.Commodity}");
                        System.Diagnostics.Debug.WriteLine($"  - RepairCodes Count: {container?.RepairCodes?.Count ?? 0}");

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

        // Get container cleaning details - UPDATED to include commodity
        public async Task<ContainerWithRepairCodesModel> GetContainerCleaningDetails(string contNumber)
        {
            try
            {
                string formattedContNumber = FormatContainerNumber(contNumber);
                string url = $"{_baseUrl}/api/container/GetContainerCleaningDetails/{formattedContNumber}";

                System.Diagnostics.Debug.WriteLine($"Getting cleaning details for: {contNumber} -> {formattedContNumber}");
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
                        var contentJson = JsonSerializer.Serialize(apiResponse.Content, _jsonOptions);
                        System.Diagnostics.Debug.WriteLine($"Content JSON: {contentJson}");

                        var container = JsonSerializer.Deserialize<ContainerWithRepairCodesModel>(contentJson, _jsonOptions);

                        System.Diagnostics.Debug.WriteLine($"Deserialized Cleaning Container:");
                        System.Diagnostics.Debug.WriteLine($"  - ContNumber: {container?.ContNumber}");
                        System.Diagnostics.Debug.WriteLine($"  - IsRepairApproved: {container?.IsRepairApproved}");
                        System.Diagnostics.Debug.WriteLine($"  - Commodity: {container?.Commodity}");
                        System.Diagnostics.Debug.WriteLine($"  - Cleaning Requirements Count: {container?.RepairCodes?.Count ?? 0}");

                        // Log cleaning requirements details
                        if (container?.RepairCodes != null)
                        {
                            foreach (var req in container.RepairCodes)
                            {
                                System.Diagnostics.Debug.WriteLine($"  - Cleaning Code: {req.RepairCode} - {req.RepairCodeDescription}");
                                System.Diagnostics.Debug.WriteLine($"    Component: {req.ComponentCode} - {req.ComponentCodeDescription}");
                                System.Diagnostics.Debug.WriteLine($"    Location: {req.LocationCode}");
                                System.Diagnostics.Debug.WriteLine($"    Details: {req.RepairDetailDescription}");
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
                System.Diagnostics.Debug.WriteLine($"Exception in GetContainerCleaningDetails: {ex}");
                return null;
            }
        }

        public async Task<ApiResponse> GetContainersForCleaning()
        {
            try
            {
                string url = $"{_baseUrl}/api/container/GetContainersForCleaning";

                System.Diagnostics.Debug.WriteLine($"Getting containers for cleaning from: {url}");

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
                        // Deserialize the Content property to list of containers
                        var contentJson = JsonSerializer.Serialize(apiResponse.Content, _jsonOptions);
                        System.Diagnostics.Debug.WriteLine($"Content JSON: {contentJson}");

                        var containers = JsonSerializer.Deserialize<List<ContainerWithRepairCodesModel>>(contentJson, _jsonOptions);

                        System.Diagnostics.Debug.WriteLine($"Deserialized {containers?.Count ?? 0} containers for cleaning");

                        // Log commodity information for first few containers
                        if (containers != null && containers.Any())
                        {
                            foreach (var container in containers.Take(5))
                            {
                                System.Diagnostics.Debug.WriteLine($"Container {container.ContNumber}: Commodity = '{container.Commodity ?? "NULL"}'");
                            }
                        }

                        // Return the response with properly typed content
                        return new ApiResponse
                        {
                            IsSuccess = true,
                            Message = apiResponse.Message,
                            Content = containers
                        };
                    }

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
                System.Diagnostics.Debug.WriteLine($"Exception in GetContainersForCleaning: {ex}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Error connecting to API: {ex.Message}"
                };
            }
        }

        private string FormatContainerNumber(string contNumber)
        {
            contNumber = contNumber.Replace(" ", "").ToUpper();

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