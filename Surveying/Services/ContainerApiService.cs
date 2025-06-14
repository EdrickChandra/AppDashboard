using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Surveying.Models;
using Surveying.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Surveying.Services
{
    // ===== API RESPONSE MODELS (match exactly what API returns) =====
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Content { get; set; }
    }

    public class ApiContainerModel
    {
        public long Id { get; set; }
        public string ContNumber { get; set; } = string.Empty;
        public DateTime? DtmIn { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public bool IsRepairApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public string Commodity { get; set; } = string.Empty;
        public string CleaningRequirementsText { get; set; } = string.Empty;
        public int RequirementCount { get; set; } = 0;
        public DateTime? CleaningStartDate { get; set; }
        public DateTime? CleaningCompleteDate { get; set; }
        public List<ApiRepairCodeModel> RepairCodes { get; set; } = new List<ApiRepairCodeModel>();
    }

    public class ApiRepairCodeModel
    {
        public string RepairCode { get; set; } = string.Empty;
        public string ComponentCode { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string RepairCodeDescription { get; set; } = string.Empty;
        public string ComponentCodeDescription { get; set; } = string.Empty;
        public string RepairDetailDescription { get; set; } = string.Empty;
    }

    // ===== INTERFACE =====
    public interface IContainerApiService
    {
        Task<ApiResponse> CheckContainerExists(string contNumber);
        Task<Container> GetContainerWithRepairCodes(string contNumber);
        Task<Container> GetContainerCleaningDetails(string contNumber);
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

                    return apiResponse ?? new ApiResponse { IsSuccess = false, Message = "Failed to deserialize response" };
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

        public async Task<Container> GetContainerWithRepairCodes(string contNumber)
        {
            try
            {
                string formattedContNumber = FormatContainerNumber(contNumber);
                string url = $"{_baseUrl}/api/container/GetContainerWithRepairCodes/{formattedContNumber}";

                System.Diagnostics.Debug.WriteLine($"Getting repair codes for: {contNumber} -> {formattedContNumber}");

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Raw API Response: {json}");

                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, _jsonOptions);
                    if (apiResponse?.IsSuccess == true && apiResponse.Content != null)
                    {
                        // Convert JsonElement to ApiContainerModel
                        var contentJson = JsonSerializer.Serialize(apiResponse.Content, _jsonOptions);
                        var apiContainer = JsonSerializer.Deserialize<ApiContainerModel>(contentJson, _jsonOptions);

                        if (apiContainer != null)
                        {
                            return ConvertApiContainerToAppContainer(apiContainer);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetContainerWithRepairCodes: {ex}");
                return null;
            }
        }

        public async Task<Container> GetContainerCleaningDetails(string contNumber)
        {
            try
            {
                string formattedContNumber = FormatContainerNumber(contNumber);
                string url = $"{_baseUrl}/api/container/GetContainerCleaningDetails/{formattedContNumber}";

                System.Diagnostics.Debug.WriteLine($"Getting cleaning details for: {contNumber} -> {formattedContNumber}");

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Raw API Response: {json}");

                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, _jsonOptions);
                    if (apiResponse?.IsSuccess == true && apiResponse.Content != null)
                    {
                        // Convert JsonElement to ApiContainerModel
                        var contentJson = JsonSerializer.Serialize(apiResponse.Content, _jsonOptions);
                        var apiContainer = JsonSerializer.Deserialize<ApiContainerModel>(contentJson, _jsonOptions);

                        if (apiContainer != null)
                        {
                            return ConvertApiContainerToAppContainer(apiContainer);
                        }
                    }
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

                    if (apiResponse?.IsSuccess == true && apiResponse.Content != null)
                    {
                        // Convert JsonElement array to ApiContainerModel array
                        var contentJson = JsonSerializer.Serialize(apiResponse.Content, _jsonOptions);
                        System.Diagnostics.Debug.WriteLine($"Content JSON: {contentJson}");

                        var apiContainers = JsonSerializer.Deserialize<List<ApiContainerModel>>(contentJson, _jsonOptions);

                        if (apiContainers != null)
                        {
                            // Convert to app Container models
                            var appContainers = new List<Container>();
                            for (int i = 0; i < apiContainers.Count; i++)
                            {
                                var appContainer = ConvertApiContainerToAppContainer(apiContainers[i]);
                                if (appContainer != null)
                                {
                                    appContainer.RowNumber = i + 1; // Set row number
                                    appContainers.Add(appContainer);
                                }
                            }

                            System.Diagnostics.Debug.WriteLine($"Converted {appContainers.Count} API containers to app containers");

                            return new ApiResponse
                            {
                                IsSuccess = true,
                                Message = apiResponse.Message,
                                Content = appContainers
                            };
                        }
                    }

                    return apiResponse ?? new ApiResponse { IsSuccess = false, Message = "No response data" };
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

        // ===== CONVERSION METHOD =====
        private Container ConvertApiContainerToAppContainer(ApiContainerModel apiContainer)
        {
            try
            {
                var appContainer = new Container
                {
                    // Basic properties
                    Id = apiContainer.Id,
                    ContNumber = apiContainer.ContNumber,
                    DtmIn = apiContainer.DtmIn,
                    CustomerCode = apiContainer.CustomerCode,

                    // Approval properties
                    IsRepairApproved = apiContainer.IsRepairApproved,
                    ApprovalDate = apiContainer.ApprovalDate,
                    ApprovedBy = apiContainer.ApprovedBy,

                    // Cleaning properties
                    Commodity = apiContainer.Commodity,
                    CleaningRequirementsText = apiContainer.CleaningRequirementsText,
                    CleaningStartDate = apiContainer.CleaningStartDate,
                    CleaningCompleteDate = apiContainer.CleaningCompleteDate,

                    // Default container properties
                    ContSize = "20",
                    ContType = "Tank",

                    // Initialize status
                    CleaningStatus = StatusType.NotFilled,
                    RepairStatus = StatusType.NotFilled,
                    PeriodicStatus = StatusType.NotFilled,
                    SurveyStatus = StatusType.NotFilled
                };

                // Convert repair codes
                if (apiContainer.RepairCodes != null)
                {
                    foreach (var apiRepairCode in apiContainer.RepairCodes)
                    {
                        var appRepairCode = new RepairCode
                        {
                            Code = apiRepairCode.RepairCode,
                            ComponentCode = apiRepairCode.ComponentCode,
                            LocationCode = apiRepairCode.LocationCode,
                            Description = apiRepairCode.RepairCodeDescription,
                            ComponentDescription = apiRepairCode.ComponentCodeDescription,
                            DetailDescription = apiRepairCode.RepairDetailDescription,
                            IsCompleted = false
                        };
                        appContainer.RepairCodes.Add(appRepairCode);
                    }
                }

                // Update activities
                appContainer.UpdateActivities();

                System.Diagnostics.Debug.WriteLine($"Converted container {appContainer.ContNumber}: {appContainer.RepairCodes.Count} repair codes");

                return appContainer;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting API container: {ex.Message}");
                return null;
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