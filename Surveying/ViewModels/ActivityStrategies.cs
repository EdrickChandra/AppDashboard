using Surveying.Models;
using Surveying.Services;

namespace Surveying.ViewModels
{
    // ===== STRATEGY PATTERN FOR ACTIVITY-SPECIFIC BEHAVIOR =====

    /// <summary>
    /// Interface for activity-specific behavior strategies
    /// Replaces the need for separate ViewModels for each activity type
    /// </summary>
    public interface IActivityStrategy
    {
        Task<ActivityData> LoadActivityDataAsync(Container container);
        Task ApplyActivitySpecificDataAsync(ContainerActivityViewModel viewModel, ActivityData data);
        Task<ValidationResult> ValidateSubmissionAsync(ContainerActivityViewModel viewModel);
        Task<bool> SubmitActivityAsync(ContainerActivityViewModel viewModel);
    }

    /// <summary>
    /// Data transfer object for activity-specific data
    /// </summary>
    public class ActivityData
    {
        public bool IsApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; } = "";
        public List<RepairCode> RepairCodes { get; set; } = new();
        public string Description { get; set; } = "";
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Validation result for activity submissions
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = "";

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Error(string message) => new() { IsValid = false, ErrorMessage = message };
    }

    // ===== CLEANING STRATEGY =====
    /// <summary>
    /// Strategy for Cleaning activity - replaces EnhancedCleaningViewModel logic
    /// </summary>
    public class CleaningStrategy : IActivityStrategy
    {
        private readonly IContainerApiService _containerApiService;
        private readonly ICleaningCriteriaService _cleaningCriteriaService;

        public CleaningStrategy(IContainerApiService containerApiService, ICleaningCriteriaService cleaningCriteriaService)
        {
            _containerApiService = containerApiService;
            _cleaningCriteriaService = cleaningCriteriaService;
        }

        public async Task<ActivityData> LoadActivityDataAsync(Container container)
        {
            try
            {
                var cleaningContainer = await _containerApiService.GetContainerCleaningDetails(container.ContNumber);

                if (cleaningContainer != null)
                {
                    var activityData = new ActivityData
                    {
                        IsApproved = cleaningContainer.IsRepairApproved,
                        ApprovalDate = cleaningContainer.ApprovalDate,
                        ApprovedBy = cleaningContainer.ApprovedBy ?? "",
                        RepairCodes = cleaningContainer.RepairCodes ?? new List<RepairCode>(),
                        Description = $"This container requires {cleaningContainer.RepairCodes?.Count ?? 0} specific cleaning procedures"
                    };

                    // Add cleaning-specific data
                    activityData.AdditionalData["Commodity"] = cleaningContainer.Commodity ?? "Not Specified";
                    activityData.AdditionalData["CleaningRequirementsText"] = cleaningContainer.CleaningRequirementsText ?? "";

                    return activityData;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cleaning data: {ex.Message}");
                return null;
            }
        }

        public async Task ApplyActivitySpecificDataAsync(ContainerActivityViewModel viewModel, ActivityData data)
        {
            // Update container with cleaning data
            viewModel.Container.Commodity = data.AdditionalData.GetValueOrDefault("Commodity", "Not Specified").ToString();
            viewModel.Container.CleaningRequirementsText = data.AdditionalData.GetValueOrDefault("CleaningRequirementsText", "").ToString();

            // Add cleaning requirements to ViewModel collection
            viewModel.CleaningRequirements.Clear();
            foreach (var repairCode in data.RepairCodes)
            {
                var cleaningRequirement = new RepairCode
                {
                    Code = repairCode.Code,
                    ComponentCode = repairCode.ComponentCode,
                    LocationCode = repairCode.LocationCode,
                    Description = repairCode.Description,
                    ComponentDescription = repairCode.ComponentDescription,
                    DetailDescription = repairCode.DetailDescription,
                    IsCompleted = false
                };

                viewModel.CleaningRequirements.Add(cleaningRequirement);
            }

            viewModel.ActivityDescription = data.Description;
            await Task.CompletedTask;
        }

        public async Task<ValidationResult> ValidateSubmissionAsync(ContainerActivityViewModel viewModel)
        {
            // Check if all cleaning segments have photos
            var requiredSegments = new[] { "Top Outside", "Front Upper Half", "Front Lower Half", "Back Upper Half", "Back Lower Half" };
            var missingSegments = requiredSegments.Where(segment =>
                viewModel.GetPhotoCountForSegment(segment) == 0).ToList();

            if (missingSegments.Any())
            {
                return ValidationResult.Error($"Please upload photos for: {string.Join(", ", missingSegments)}");
            }

            if (viewModel.EndDate < viewModel.ActivityDate)
            {
                return ValidationResult.Error("End date cannot be before start date.");
            }

            // Check if any cleaning requirements are pending
            if (viewModel.CleaningRequirements.Any())
            {
                var pendingTasks = viewModel.CleaningRequirements.Where(r => !r.IsCompleted).ToList();
                if (pendingTasks.Any())
                {
                    bool continueWithPending = await Application.Current.MainPage.DisplayAlert(
                        "Pending Cleaning Tasks",
                        $"You have {pendingTasks.Count} pending cleaning tasks. Do you want to submit anyway?",
                        "Submit", "Cancel");

                    if (!continueWithPending)
                        return ValidationResult.Error("Submission cancelled due to pending tasks.");
                }
            }

            return ValidationResult.Success();
        }

        public async Task<bool> SubmitActivityAsync(ContainerActivityViewModel viewModel)
        {
            try
            {
                // Update container status
                viewModel.Container.CleaningStatus = StatusType.OnReview;
                viewModel.Container.CleaningStartDate = viewModel.ActivityDate;
                viewModel.Container.CleaningCompleteDate = viewModel.EndDate;
                viewModel.Container.UpdateActivities();

                // Here you would normally send data to API
                // await _containerApiService.SubmitCleaningData(viewModel.Container, viewModel.Photos);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error submitting cleaning data: {ex.Message}");
                return false;
            }
        }
    }

    // ===== REPAIR STRATEGY =====
    /// <summary>
    /// Strategy for Repair activity - replaces RepairViewModel logic
    /// </summary>
    public class RepairStrategy : IActivityStrategy
    {
        private readonly IContainerApiService _containerApiService;

        public RepairStrategy(IContainerApiService containerApiService)
        {
            _containerApiService = containerApiService;
        }

        public async Task<ActivityData> LoadActivityDataAsync(Container container)
        {
            try
            {
                var containerWithCodes = await _containerApiService.GetContainerWithRepairCodes(container.ContNumber);

                if (containerWithCodes != null)
                {
                    var activityData = new ActivityData
                    {
                        IsApproved = containerWithCodes.IsRepairApproved,
                        ApprovalDate = containerWithCodes.ApprovalDate,
                        ApprovedBy = containerWithCodes.ApprovedBy ?? "",
                        RepairCodes = containerWithCodes.RepairCodes ?? new List<RepairCode>(),
                        Description = $"Container requires {containerWithCodes.RepairCodes?.Count ?? 0} repair procedures"
                    };

                    return activityData;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading repair data: {ex.Message}");
                return null;
            }
        }

        public async Task ApplyActivitySpecificDataAsync(ContainerActivityViewModel viewModel, ActivityData data)
        {
            // Add repair codes to ViewModel collection
            viewModel.RepairCodes.Clear();
            foreach (var apiCode in data.RepairCodes)
            {
                var repairCode = new RepairCode
                {
                    Code = apiCode.Code,
                    ComponentCode = apiCode.ComponentCode,
                    LocationCode = apiCode.LocationCode,
                    Description = apiCode.Description,
                    ComponentDescription = apiCode.ComponentDescription,
                    DetailDescription = apiCode.DetailDescription,
                    IsCompleted = false
                };

                viewModel.RepairCodes.Add(repairCode);
            }

            viewModel.ActivityDescription = data.Description;
            await Task.CompletedTask;
        }

        public async Task<ValidationResult> ValidateSubmissionAsync(ContainerActivityViewModel viewModel)
        {
            if (viewModel.Photos.Count == 0)
            {
                return ValidationResult.Error("Please upload at least one photo.");
            }

            // Check approval status before submitting
            if (!viewModel.IsActivityApproved)
            {
                bool continueSubmit = await Application.Current.MainPage.DisplayAlert(
                    "Repair Not Approved",
                    "This repair has not been approved yet. Do you want to continue submitting?",
                    "Yes", "No");

                if (!continueSubmit)
                    return ValidationResult.Error("Submission cancelled - repair not approved.");
            }

            return ValidationResult.Success();
        }

        public async Task<bool> SubmitActivityAsync(ContainerActivityViewModel viewModel)
        {
            try
            {
                // Update container status
                viewModel.Container.RepairStatus = StatusType.Finished;
                viewModel.Container.UpdateActivities();

                // Here you would normally send data to API
                // await _containerApiService.SubmitRepairData(viewModel.Container, viewModel.Photos);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error submitting repair data: {ex.Message}");
                return false;
            }
        }
    }

    // ===== PERIODIC STRATEGY =====
    /// <summary>
    /// Strategy for Periodic activity - replaces PeriodicViewModel logic
    /// </summary>
    public class PeriodicStrategy : IActivityStrategy
    {
        public async Task<ActivityData> LoadActivityDataAsync(Container container)
        {
            // Periodic maintenance typically doesn't require API data loading
            var activityData = new ActivityData
            {
                IsApproved = true, // Periodic maintenance is typically always approved
                Description = "Periodic Test: 2.5 Year Inspection"
            };

            return activityData;
        }

        public async Task ApplyActivitySpecificDataAsync(ContainerActivityViewModel viewModel, ActivityData data)
        {
            viewModel.ActivityDescription = data.Description;

            // Set periodic-specific dates
            viewModel.ActivityDate = DateTime.Today; // Inspection date
            viewModel.EndDate = DateTime.Today.AddYears(2).AddMonths(6); // Next due date

            await Task.CompletedTask;
        }

        public async Task<ValidationResult> ValidateSubmissionAsync(ContainerActivityViewModel viewModel)
        {
            // Check if CSC Plate photo is uploaded
            if (viewModel.Photos.Count == 0)
            {
                return ValidationResult.Error("Please upload at least one photo of the CSC Plate.");
            }

            return ValidationResult.Success();
        }

        public async Task<bool> SubmitActivityAsync(ContainerActivityViewModel viewModel)
        {
            try
            {
                // Update container status
                viewModel.Container.PeriodicStatus = StatusType.Finished;
                viewModel.Container.CleaningStartDate = viewModel.ActivityDate; // Reuse for inspection date
                viewModel.Container.CleaningCompleteDate = viewModel.EndDate; // Reuse for next due date
                viewModel.Container.UpdateActivities();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error submitting periodic data: {ex.Message}");
                return false;
            }
        }
    }

    // ===== SURVEY STRATEGY =====
    /// <summary>
    /// Strategy for Survey activity - replaces SurveyorViewModel logic
    /// </summary>
    public class SurveyStrategy : IActivityStrategy
    {
        public async Task<ActivityData> LoadActivityDataAsync(Container container)
        {
            // Survey activity uses existing container data
            var activityData = new ActivityData
            {
                IsApproved = true, // Survey review is typically always available
                Description = "Review and approve/reject completed activities"
            };

            // Add survey-specific data
            activityData.AdditionalData["CanReview"] = container.CleaningStatus == StatusType.OnReview;
            activityData.AdditionalData["ReviewStatus"] = container.CleaningStatus.ToString();

            return activityData;
        }

        public async Task ApplyActivitySpecificDataAsync(ContainerActivityViewModel viewModel, ActivityData data)
        {
            viewModel.ActivityDescription = data.Description;

            // Set review capabilities
            viewModel.CanReviewActivity = (bool)data.AdditionalData.GetValueOrDefault("CanReview", false);

            // Initialize review state from container
            viewModel.ActivityAccept = viewModel.Container.CleaningStatus == StatusType.Finished;
            viewModel.ActivityReject = viewModel.Container.CleaningStatus == StatusType.Rejected;

            await Task.CompletedTask;
        }

        public async Task<ValidationResult> ValidateSubmissionAsync(ContainerActivityViewModel viewModel)
        {
            if (!viewModel.CanReviewActivity)
            {
                return ValidationResult.Error("This item is not ready for review. Make sure activities have been submitted first.");
            }

            if (viewModel.Photos.Count == 0)
            {
                return ValidationResult.Error("Please upload at least one photo before submitting.");
            }

            if (!viewModel.ActivityAccept && !viewModel.ActivityReject)
            {
                return ValidationResult.Error("Please select either Finish or Reject before submitting.");
            }

            return ValidationResult.Success();
        }

        public async Task<bool> SubmitActivityAsync(ContainerActivityViewModel viewModel)
        {
            try
            {
                // Update container status based on review decision
                if (viewModel.ActivityAccept)
                {
                    viewModel.Container.CleaningStatus = StatusType.Finished;
                }
                else if (viewModel.ActivityReject)
                {
                    viewModel.Container.CleaningStatus = StatusType.Rejected;
                }

                viewModel.Container.UpdateActivities();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error submitting survey data: {ex.Message}");
                return false;
            }
        }
    }
}