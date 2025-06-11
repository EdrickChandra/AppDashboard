using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Surveying.Models;
using Surveying.Configuration;

namespace Surveying.Services
{
    public interface ICleaningCriteriaService
    {
        Task<CleaningCriteriaModel> GetCleaningCriteriaAsync();
        string GetFormattedCleaningCriteria();
        List<string> GetFormattedCriteriaList();
    }

    public class CleaningCriteriaService : ICleaningCriteriaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        private CleaningCriteriaModel _cachedCriteria;

        public CleaningCriteriaService()
        {
            _httpClient = new HttpClient();
            _baseUrl = AppConfig.ApiBaseUrl;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<CleaningCriteriaModel> GetCleaningCriteriaAsync()
        {
            // Return cached criteria if available
            if (_cachedCriteria != null)
            {
                return _cachedCriteria;
            }

            try
            {
                string url = $"{_baseUrl}/api/container/GetCleaningCriteria";
                System.Diagnostics.Debug.WriteLine($"Getting cleaning criteria from: {url}");

                var response = await _httpClient.GetAsync(url);
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Raw API Response: {json}");

                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, _jsonOptions);

                    if (apiResponse?.IsSuccess == true && apiResponse.Content != null)
                    {
                        var contentJson = JsonSerializer.Serialize(apiResponse.Content, _jsonOptions);
                        var criteria = JsonSerializer.Deserialize<CleaningCriteriaModel>(contentJson, _jsonOptions);

                        _cachedCriteria = criteria;
                        System.Diagnostics.Debug.WriteLine($"Loaded cleaning criteria: {GetFormattedCleaningCriteria()}");

                        return criteria;
                    }
                }

                // Fallback to default criteria if API fails
                System.Diagnostics.Debug.WriteLine("API failed, using default cleaning criteria");
                return GetDefaultCriteria();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting cleaning criteria: {ex.Message}");
                return GetDefaultCriteria();
            }
        }

        public string GetFormattedCleaningCriteria()
        {
            if (_cachedCriteria == null)
            {
                return "YXT • 1101 • APNN"; // Default fallback
            }

            // Create proper groupings instead of mixing all combinations
            var criteriaGroups = new List<string>();

            // Assume the criteria arrays are parallel (same index = same group)
            int maxCount = Math.Max(_cachedCriteria.ComponentCodes.Count,
                          Math.Max(_cachedCriteria.RepairCodes.Count, _cachedCriteria.LocationCodes.Count));

            for (int i = 0; i < maxCount; i++)
            {
                var component = i < _cachedCriteria.ComponentCodes.Count ? _cachedCriteria.ComponentCodes[i] : _cachedCriteria.ComponentCodes.LastOrDefault() ?? "";
                var repair = i < _cachedCriteria.RepairCodes.Count ? _cachedCriteria.RepairCodes[i] : _cachedCriteria.RepairCodes.LastOrDefault() ?? "";
                var location = i < _cachedCriteria.LocationCodes.Count ? _cachedCriteria.LocationCodes[i] : _cachedCriteria.LocationCodes.LastOrDefault() ?? "";

                if (!string.IsNullOrEmpty(component) && !string.IsNullOrEmpty(repair) && !string.IsNullOrEmpty(location))
                {
                    criteriaGroups.Add($"{component} • {repair} • {location}");
                }
            }

            return criteriaGroups.Any() ? string.Join("\n", criteriaGroups) : "YXT • 1101 • APNN";
        }

        public List<string> GetFormattedCriteriaList()
        {
            if (_cachedCriteria == null)
            {
                return new List<string> { "YXT • 1101 • APNN" };
            }

            var criteriaGroups = new List<string>();

            // Create proper groupings - assume parallel arrays
            int maxCount = Math.Max(_cachedCriteria.ComponentCodes.Count,
                          Math.Max(_cachedCriteria.RepairCodes.Count, _cachedCriteria.LocationCodes.Count));

            for (int i = 0; i < maxCount; i++)
            {
                var component = i < _cachedCriteria.ComponentCodes.Count ? _cachedCriteria.ComponentCodes[i] : _cachedCriteria.ComponentCodes.LastOrDefault() ?? "";
                var repair = i < _cachedCriteria.RepairCodes.Count ? _cachedCriteria.RepairCodes[i] : _cachedCriteria.RepairCodes.LastOrDefault() ?? "";
                var location = i < _cachedCriteria.LocationCodes.Count ? _cachedCriteria.LocationCodes[i] : _cachedCriteria.LocationCodes.LastOrDefault() ?? "";

                if (!string.IsNullOrEmpty(component) && !string.IsNullOrEmpty(repair) && !string.IsNullOrEmpty(location))
                {
                    criteriaGroups.Add($"{component} • {repair} • {location}");
                }
            }

            return criteriaGroups.Any() ? criteriaGroups : new List<string> { "YXT • 1101 • APNN" };
        }

        private CleaningCriteriaModel GetDefaultCriteria()
        {
            return new CleaningCriteriaModel
            {
                ComponentCodes = new List<string> { "YXT" },
                RepairCodes = new List<string> { "1101" },
                LocationCodes = new List<string> { "APNN" },
                Description = "Default cleaning criteria"
            };
        }
    }

    // Model for cleaning criteria
    public class CleaningCriteriaModel
    {
        public List<string> ComponentCodes { get; set; } = new List<string>();
        public List<string> RepairCodes { get; set; } = new List<string>();
        public List<string> LocationCodes { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
    }
}