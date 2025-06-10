using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Services;
using Surveying.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace Surveying.ViewModels
{
    public partial class EnhancedCleaningViewModel : BaseViewModel
    {
        private readonly IContainerApiService _containerApiService;

        public SurveyModel Survey { get; }
        public ContainerDetailModel Container { get; private set; }
        public ContainerWithRepairCodesModel ContainerFromApi { get; private set; }

        public string ContainerNumber => Container?.ContNumber ?? Survey.ContNumber;
        public DateTime StartCleanDate { get; set; }
        public DateTime EndCleanDate { get; set; }

        // Photo uploader using the cleaning-specific factory method
        public PhotoUploadViewModel PhotoUploader { get; }

        [ObservableProperty]
        private ObservableCollection<RepairCodeModel> cleaningRequirements = new ObservableCollection<RepairCodeModel>();

        [ObservableProperty]
        private bool isLoadingCleaningRequirements = true;

        [ObservableProperty]
        private string loadingMessage = "Loading cleaning requirements...";

        [ObservableProperty]
        private bool hasCleaningRequirements;

        [ObservableProperty]
        private string cleaningDescription = "";

        [ObservableProperty]
        private bool showDebugInfo = true; // Set to false in production

        [ObservableProperty]
        private string debugInfo = "";

        // Constructor with container from API
        public EnhancedCleaningViewModel(SurveyModel survey, ContainerDetailModel container, ContainerWithRepairCodesModel containerFromApi)
        {
            Survey = survey;
            Container = container;
            ContainerFromApi = containerFromApi;
            _containerApiService = new ContainerApiService();

            StartCleanDate = DateTime.Today;
            EndCleanDate = DateTime.Today.AddDays(1);

            // Use factory method for cleaning-specific segments
            PhotoUploader = PhotoUploadViewModel.CreateForCleaning();

            // Load cleaning requirements when view model is created
            _ = LoadCleaningRequirementsAsync();
        }

        private async Task LoadCleaningRequirementsAsync()
        {
            try
            {
                IsLoadingCleaningRequirements = true;
                LoadingMessage = "Loading cleaning requirements...";
                DebugInfo = $"Starting API call for container cleaning details: {ContainerNumber}";

                System.Diagnostics.Debug.WriteLine($"Starting LoadCleaningRequirementsAsync for {ContainerNumber}");

                // FIXED: Use the new method that returns ContainerWithRepairCodesModel directly
                var cleaningContainer = await _containerApiService.GetContainerCleaningDetails(ContainerNumber);

                System.Diagnostics.Debug.WriteLine($"API call completed. Result: {(cleaningContainer != null ? "Success" : "Failed")}");

                if (cleaningContainer != null)
                {
                    DebugInfo += $"\nAPI Success - Cleaning details found";

                    // Update cleaning requirements
                    if (cleaningContainer.RepairCodes != null && cleaningContainer.RepairCodes.Count > 0)
                    {
                        CleaningRequirements.Clear();
                        foreach (var code in cleaningContainer.RepairCodes)
                        {
                            var cleaningRequirement = new RepairCodeModel
                            {
                                RepairCode = code.RepairCode,
                                ComponentCode = code.ComponentCode,
                                LocationCode = code.LocationCode,
                                ComponentCategory = code.ComponentCategory,
                                ComponentDescription = code.ComponentDescription,
                                RepairCodeDescription = code.RepairCodeDescription ?? string.Empty,
                                ComponentCodeDescription = code.ComponentCodeDescription ?? string.Empty,
                                RepairDetailDescription = code.RepairDetailDescription ?? string.Empty,
                                // Initialize cleaning status tracking
                                IsCompleted = false,
                                CompletedDate = null,
                                CompletedBy = string.Empty,
                                RepairNotes = string.Empty
                            };

                            CleaningRequirements.Add(cleaningRequirement);
                            System.Diagnostics.Debug.WriteLine($"Added cleaning requirement: {code.RepairCode}");
                            System.Diagnostics.Debug.WriteLine($"  - Cleaning Type: {code.RepairCodeDescription}");
                            System.Diagnostics.Debug.WriteLine($"  - Component: {code.ComponentCodeDescription}");
                            System.Diagnostics.Debug.WriteLine($"  - Details: {code.RepairDetailDescription}");
                        }

                        HasCleaningRequirements = true;
                        DebugInfo += $"\nLoaded {CleaningRequirements.Count} cleaning requirements";
                        LoadingMessage = $"Found {CleaningRequirements.Count} cleaning requirements";

                        // Set description based on loaded requirements
                        CleaningDescription = $"This container requires {CleaningRequirements.Count} specific cleaning procedures based on repair codes: YXT • 1101 • APNN";
                    }
                    else
                    {
                        HasCleaningRequirements = false;
                        LoadingMessage = "No specific cleaning requirements found for this container.";
                        CleaningDescription = "Standard cleaning procedures apply.";
                        DebugInfo += "\nNo cleaning requirements found";
                    }
                }
                else
                {
                    HasCleaningRequirements = false;
                    LoadingMessage = "Failed to load cleaning requirements. Please check your connection.";
                    CleaningDescription = "Unable to load specific cleaning requirements. Standard procedures apply.";
                    DebugInfo += "\nAPI returned null - check logs";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in LoadCleaningRequirementsAsync: {ex}");
                HasCleaningRequirements = false;
                LoadingMessage = $"Error loading cleaning requirements: {ex.Message}";
                CleaningDescription = "Error loading requirements. Standard cleaning procedures apply.";
                DebugInfo += $"\nException: {ex.Message}";
            }
            finally
            {
                IsLoadingCleaningRequirements = false;
                System.Diagnostics.Debug.WriteLine($"LoadCleaningRequirementsAsync completed for {ContainerNumber}");
            }
        }

        [RelayCommand]
        async void ShowDebugDetails()
        {
            await Application.Current.MainPage.DisplayAlert("Debug Info", DebugInfo, "OK");
        }

        [RelayCommand]
        async void RefreshCleaningRequirements()
        {
            await LoadCleaningRequirementsAsync();
        }

        [RelayCommand]
        async void ViewFullImage(PhotoItem photo)
        {
            if (photo != null && photo.ImageSource != null)
            {
                var imageViewerPage = new ImageViewerPage(photo.ImageSource);
                await Application.Current.MainPage.Navigation.PushAsync(imageViewerPage);
            }
        }

        [RelayCommand]
        async void ToggleCleaningStatus(RepairCodeModel cleaningItem)
        {
            if (cleaningItem == null) return;

            if (cleaningItem.IsCompleted)
            {
                // Undo completion
                bool confirmUndo = await Application.Current.MainPage.DisplayAlert(
                    "Undo Completion",
                    $"Mark cleaning task {cleaningItem.RepairCode} as pending again?",
                    "Yes", "No");

                if (confirmUndo)
                {
                    cleaningItem.IsCompleted = false;
                    cleaningItem.CompletedDate = null;
                    cleaningItem.CompletedBy = string.Empty;
                    cleaningItem.RepairNotes = string.Empty;

                    await Application.Current.MainPage.DisplayAlert("Status Updated",
                        $"Cleaning task {cleaningItem.RepairCode} marked as pending.", "OK");
                }
            }
            else
            {
                // Mark as completed
                string detailMessage = $"Mark this cleaning task as completed?\n\n";
                detailMessage += $"Code: {cleaningItem.RepairCode}\n";

                if (!string.IsNullOrEmpty(cleaningItem.RepairCodeDescription))
                    detailMessage += $"Type: {cleaningItem.RepairCodeDescription}\n";

                if (!string.IsNullOrEmpty(cleaningItem.ComponentCodeDescription))
                    detailMessage += $"Component: {cleaningItem.ComponentCodeDescription}\n";

                if (!string.IsNullOrEmpty(cleaningItem.RepairDetailDescription))
                    detailMessage += $"Details: {cleaningItem.RepairDetailDescription}";

                bool confirmComplete = await Application.Current.MainPage.DisplayAlert(
                    "Mark as Completed",
                    detailMessage,
                    "Yes", "No");

                if (confirmComplete)
                {
                    cleaningItem.IsCompleted = true;
                    cleaningItem.CompletedDate = DateTime.Now;
                    cleaningItem.CompletedBy = "Current User"; // You can get actual user name

                    // Ask for optional notes
                    string notes = await Application.Current.MainPage.DisplayPromptAsync(
                        "Cleaning Notes (Optional)",
                        "Add any notes about this cleaning task:",
                        placeholder: "e.g., Used detergent solution, steam cleaned...");

                    cleaningItem.RepairNotes = notes ?? string.Empty;

                    await Application.Current.MainPage.DisplayAlert("Cleaning Completed",
                        $"✓ Cleaning task {cleaningItem.RepairCode} marked as completed!", "OK");
                }
            }

            // Update overall completion status
            UpdateOverallCleaningStatus();
        }

        private void UpdateOverallCleaningStatus()
        {
            if (CleaningRequirements.Count == 0) return;

            int completedCount = CleaningRequirements.Count(r => r.IsCompleted);
            int totalCount = CleaningRequirements.Count;

            // Update loading message to show progress
            LoadingMessage = $"Cleaning Progress: {completedCount}/{totalCount} completed";

            // Update container status based on progress
            if (completedCount == 0)
            {
                Container.CleaningStatus = StatusType.NotFilled;
            }
            else if (completedCount == totalCount)
            {
                Container.CleaningStatus = StatusType.Finished;
            }
            else
            {
                Container.CleaningStatus = StatusType.OnReview; // In progress
            }

            // Update activities
            Container.UpdateActivities();
            Survey.CleaningStatus = Container.CleaningStatus;

            System.Diagnostics.Debug.WriteLine($"Overall cleaning status: {completedCount}/{totalCount} completed");
        }

        [RelayCommand]
        async void SubmitCleaning()
        {
            bool isValid = true;
            string errorMessage = "";

            // Check if all cleaning segments have photos
            var requiredSegments = new[] { "Top Outside", "Front Upper Half", "Front Lower Half", "Back Upper Half", "Back Lower Half" };
            var missingSegments = requiredSegments.Where(segment =>
                PhotoUploader.GetPhotoCountForSegment(segment) == 0).ToList();

            if (missingSegments.Any())
            {
                isValid = false;
                errorMessage += $"Please upload photos for: {string.Join(", ", missingSegments)}\n";
            }

            if (EndCleanDate < StartCleanDate)
            {
                isValid = false;
                errorMessage += "End date cannot be before start date.\n";
            }

            // Check if any cleaning requirements are pending
            if (HasCleaningRequirements)
            {
                var pendingTasks = CleaningRequirements.Where(r => !r.IsCompleted).ToList();
                if (pendingTasks.Any())
                {
                    bool continueWithPending = await Application.Current.MainPage.DisplayAlert(
                        "Pending Cleaning Tasks",
                        $"You have {pendingTasks.Count} pending cleaning tasks. Do you want to submit anyway?",
                        "Submit", "Cancel");

                    if (!continueWithPending)
                        return;
                }
            }

            if (!isValid)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", errorMessage, "OK");
                return;
            }

            // Update the container status
            if (Container != null)
            {
                Container.CleaningStatus = StatusType.OnReview;
                Container.UpdateActivities();
            }

            // Always update the survey for backward compatibility
            Survey.CleaningStatus = StatusType.OnReview;

            string successMessage = HasCleaningRequirements
                ? $"Cleaning data submitted with {CleaningRequirements.Count(r => r.IsCompleted)}/{CleaningRequirements.Count} requirements completed."
                : "Cleaning data has been submitted for review.";

            await Application.Current.MainPage.DisplayAlert("Success", successMessage, "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}