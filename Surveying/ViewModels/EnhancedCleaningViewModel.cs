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
    // ===== MIGRATED: EnhancedCleaningViewModel using simplified models =====
    public partial class EnhancedCleaningViewModel : BaseViewModel
    {
        private readonly IContainerApiService _containerApiService;

        // ===== SIMPLIFIED MODELS =====
        // OLD: SurveyModel survey, ContainerDetailModel container, ContainerWithRepairCodesModel containerFromApi
        // NEW: Order order, Container container (unified - no separate API model needed!)
        public Order Order { get; }
        public Container Container { get; }

        public string ContainerNumber => Container.ContNumber;
        public DateTime StartCleanDate { get; set; }
        public DateTime EndCleanDate { get; set; }

        // ===== SIMPLIFIED PHOTO UPLOADER =====
        // Photo uploader using cleaning-specific factory method
        public PhotoUploadViewModel PhotoUploader { get; }

        // ===== SIMPLIFIED CLEANING REQUIREMENTS =====
        // OLD: ObservableCollection<RepairCodeModel> cleaningRequirements (separate from container)
        // NEW: Direct access to Container.RepairCodes (filtering for cleaning-specific codes)
        public ObservableCollection<RepairCode> CleaningRequirements { get; private set; } = new ObservableCollection<RepairCode>();

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

        // ===== CONSTRUCTOR - SIMPLIFIED =====
        // OLD: Needed separate ContainerWithRepairCodesModel from API
        // NEW: Just takes Order + Container (API data gets merged into Container)
        public EnhancedCleaningViewModel(Order order, Container container)
        {
            Order = order;
            Container = container;
            _containerApiService = new ContainerApiService();

            StartCleanDate = DateTime.Today;
            EndCleanDate = DateTime.Today.AddDays(1);

            // Use factory method for cleaning-specific segments
            PhotoUploader = PhotoUploadViewModel.CreateForCleaning();

            // Load cleaning requirements when view model is created
            _ = LoadCleaningRequirementsAsync();
        }

        // ===== SIMPLIFIED CLEANING REQUIREMENTS LOADING =====
        private async Task LoadCleaningRequirementsAsync()
        {
            try
            {
                IsLoadingCleaningRequirements = true;
                LoadingMessage = "Loading cleaning requirements...";
                DebugInfo = $"Starting API call for container cleaning details: {ContainerNumber}";

                System.Diagnostics.Debug.WriteLine($"Starting LoadCleaningRequirementsAsync for {ContainerNumber}");

                // SIMPLIFIED: API returns Container directly, merge with current Container
                var cleaningContainer = await _containerApiService.GetContainerCleaningDetails(ContainerNumber);

                System.Diagnostics.Debug.WriteLine($"API call completed. Result: {(cleaningContainer != null ? "Success" : "Failed")}");

                if (cleaningContainer != null)
                {
                    DebugInfo += $"\nAPI Success - Cleaning details found";

                    // SIMPLIFIED: Update current Container with API data (no separate models!)
                    Container.CleaningRequirementsText = cleaningContainer.CleaningRequirementsText ?? "";
                    Container.Commodity = cleaningContainer.Commodity ?? "Not Specified";
                    Container.IsRepairApproved = cleaningContainer.IsRepairApproved;
                    Container.ApprovalDate = cleaningContainer.ApprovalDate;
                    Container.ApprovedBy = cleaningContainer.ApprovedBy ?? "";

                    // SIMPLIFIED: Merge repair codes (cleaning-specific)
                    if (cleaningContainer.RepairCodes != null && cleaningContainer.RepairCodes.Count > 0)
                    {
                        CleaningRequirements.Clear();

                        foreach (var apiCode in cleaningContainer.RepairCodes)
                        {
                            // SIMPLIFIED: No more complex mapping - direct RepairCode creation
                            var cleaningRequirement = new RepairCode
                            {
                                Code = apiCode.Code,                           // Clean property names
                                ComponentCode = apiCode.ComponentCode,
                                LocationCode = apiCode.LocationCode,
                                Description = apiCode.Description ?? string.Empty,             // No more legacy handling!
                                ComponentDescription = apiCode.ComponentDescription ?? string.Empty,
                                DetailDescription = apiCode.DetailDescription ?? string.Empty,

                                // Initialize cleaning status tracking
                                IsCompleted = false,
                                CompletedDate = null,
                                CompletedBy = string.Empty,
                                Notes = string.Empty
                            };

                            CleaningRequirements.Add(cleaningRequirement);
                            System.Diagnostics.Debug.WriteLine($"Added cleaning requirement: {apiCode.Code}");
                            System.Diagnostics.Debug.WriteLine($"  - Cleaning Type: {apiCode.Description}");
                            System.Diagnostics.Debug.WriteLine($"  - Component: {apiCode.ComponentDescription}");
                            System.Diagnostics.Debug.WriteLine($"  - Details: {apiCode.DetailDescription}");
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

        // ===== UI COMMANDS - SIMPLIFIED =====
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
        async void ViewFullImage(Photo photo)  // SIMPLIFIED: Single Photo type instead of PhotoItem
        {
            if (photo != null && photo.ImageSource != null)
            {
                var imageViewerPage = new ImageViewerPage(photo.ImageSource);
                await Application.Current.MainPage.Navigation.PushAsync(imageViewerPage);
            }
        }

        [RelayCommand]
        async void ToggleCleaningStatus(RepairCode cleaningItem)
        {
            if (cleaningItem == null) return;

            if (cleaningItem.IsCompleted)
            {
                // Undo completion
                bool confirmUndo = await Application.Current.MainPage.DisplayAlert(
                    "Undo Completion",
                    $"Mark cleaning task {cleaningItem.Code} as pending again?",  // SIMPLIFIED: .Code instead of .RepairCode
                    "Yes", "No");

                if (confirmUndo)
                {
                    cleaningItem.IsCompleted = false;
                    cleaningItem.CompletedDate = null;
                    cleaningItem.CompletedBy = string.Empty;
                    cleaningItem.Notes = string.Empty;  // SIMPLIFIED: .Notes instead of .RepairNotes

                    await Application.Current.MainPage.DisplayAlert("Status Updated",
                        $"Cleaning task {cleaningItem.Code} marked as pending.", "OK");
                }
            }
            else
            {
                // Mark as completed - SIMPLIFIED: Clean property access
                string detailMessage = $"Mark this cleaning task as completed?\n\n";
                detailMessage += $"Code: {cleaningItem.Code}\n";

                if (!string.IsNullOrEmpty(cleaningItem.Description))
                    detailMessage += $"Type: {cleaningItem.Description}\n";

                if (!string.IsNullOrEmpty(cleaningItem.ComponentDescription))
                    detailMessage += $"Component: {cleaningItem.ComponentDescription}\n";

                if (!string.IsNullOrEmpty(cleaningItem.DetailDescription))
                    detailMessage += $"Details: {cleaningItem.DetailDescription}";

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

                    cleaningItem.Notes = notes ?? string.Empty;

                    await Application.Current.MainPage.DisplayAlert("Cleaning Completed",
                        $"✓ Cleaning task {cleaningItem.Code} marked as completed!", "OK");
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

            // SIMPLIFIED: Direct status update on Container
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

            // Update activities automatically
            Container.UpdateActivities();

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

            // SIMPLIFIED: Direct status update
            Container.CleaningStatus = StatusType.OnReview;
            Container.UpdateActivities();

            string successMessage = HasCleaningRequirements
                ? $"Cleaning data submitted with {CleaningRequirements.Count(r => r.IsCompleted)}/{CleaningRequirements.Count} requirements completed."
                : "Cleaning data has been submitted for review.";

            await Application.Current.MainPage.DisplayAlert("Success", successMessage, "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}