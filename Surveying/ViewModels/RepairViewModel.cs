using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Services;
using Surveying.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Surveying.ViewModels
{
    public partial class RepairViewModel : BaseViewModel
    {
        private readonly IContainerApiService _containerApiService;

        public SurveyModel Survey { get; }
        public ContainerDetailModel Container { get; }

        public string ContainerNumber => Container.ContNumber;

        public PhotoUploadViewModel PhotoUploader { get; } = PhotoUploadViewModel.CreateForRepair();

        [ObservableProperty]
        private DateTime repairDate = DateTime.Today;

        [ObservableProperty]
        private string repairDescription = "";

        [ObservableProperty]
        private ObservableCollection<RepairCodeModel> repairCodes = new ObservableCollection<RepairCodeModel>();

        [ObservableProperty]
        private bool isLoadingRepairCodes = true;

        [ObservableProperty]
        private string loadingMessage = "Loading repair codes...";

        // New approval status properties
        [ObservableProperty]
        private bool isRepairApproved;

        [ObservableProperty]
        private DateTime? approvalDate;

        [ObservableProperty]
        private string approvedBy = string.Empty;

        [ObservableProperty]
        private string approvalStatus = "Checking...";

        [ObservableProperty]
        private string approvalStatusColor = "#FFA500"; // Orange for pending

        [ObservableProperty]
        private bool showApprovalInfo;

        [ObservableProperty]
        private bool showDebugInfo = true; // Set to false in production

        [ObservableProperty]
        private string debugInfo = "";



        [RelayCommand]
        async void ShowDebugDetails()
        {
            await Application.Current.MainPage.DisplayAlert("Debug Info", DebugInfo, "OK");
        }

        public RepairViewModel(SurveyModel survey, ContainerDetailModel container) : this(survey, container, new ContainerApiService())
        {
        }

        public RepairViewModel(SurveyModel survey, ContainerDetailModel container, IContainerApiService containerApiService)
        {
            Survey = survey;
            Container = container;
            _containerApiService = containerApiService;

            // Load repair codes when view model is created
            _ = LoadRepairCodesAsync();
        }

        private async Task LoadRepairCodesAsync()
        {
            try
            {
                IsLoadingRepairCodes = true;
                LoadingMessage = "Loading repair codes and approval status...";
                DebugInfo = $"Starting API call for container: {ContainerNumber}";

                System.Diagnostics.Debug.WriteLine($"Starting LoadRepairCodesAsync for {ContainerNumber}");

                var containerWithCodes = await _containerApiService.GetContainerWithRepairCodes(ContainerNumber);

                System.Diagnostics.Debug.WriteLine($"API call completed. Result: {(containerWithCodes != null ? "Success" : "Null")}");

                if (containerWithCodes != null)
                {
                    DebugInfo += $"\nAPI Success - Container found";

                    // Update approval status
                    IsRepairApproved = containerWithCodes.IsRepairApproved;
                    ApprovalDate = containerWithCodes.ApprovalDate;
                    ApprovedBy = containerWithCodes.ApprovedBy ?? string.Empty;

                    DebugInfo += $"\nApproval: {IsRepairApproved}";
                    DebugInfo += $"\nApproval Date: {ApprovalDate}";
                    DebugInfo += $"\nApproved By: {ApprovedBy}";

                    UpdateApprovalStatusDisplay();

                    // Update repair codes
                    if (containerWithCodes.RepairCodes != null && containerWithCodes.RepairCodes.Count > 0)
                    {
                        RepairCodes.Clear();
                        foreach (var code in containerWithCodes.RepairCodes)
                        {
                            // Map the API model to the view model with all new properties
                            var repairCodeModel = new RepairCodeModel
                            {
                                RepairCode = code.RepairCode,
                                ComponentCode = code.ComponentCode,
                                LocationCode = code.LocationCode,
                                ComponentCategory = code.ComponentCategory,
                                ComponentDescription = code.ComponentDescription,
                                // NEW: Map the additional description properties
                                RepairCodeDescription = code.RepairCodeDescription ?? string.Empty,
                                ComponentCodeDescription = code.ComponentCodeDescription ?? string.Empty,
                                RepairDetailDescription = code.RepairDetailDescription ?? string.Empty,
                                // Initialize status tracking
                                IsCompleted = false,
                                CompletedDate = null,
                                CompletedBy = string.Empty,
                                RepairNotes = string.Empty
                            };

                            RepairCodes.Add(repairCodeModel);
                            System.Diagnostics.Debug.WriteLine($"Added repair code: {code.RepairCode}");
                            System.Diagnostics.Debug.WriteLine($"  - Repair Type: {code.RepairCodeDescription}");
                            System.Diagnostics.Debug.WriteLine($"  - Component Desc: {code.ComponentCodeDescription}");
                            System.Diagnostics.Debug.WriteLine($"  - Detail Desc: {code.RepairDetailDescription}");
                        }

                        DebugInfo += $"\nLoaded {RepairCodes.Count} repair codes";
                        LoadingMessage = $"Loaded {RepairCodes.Count} repair codes successfully";
                    }
                    else
                    {
                        LoadingMessage = "No repair codes found for this container.";
                        DebugInfo += "\nNo repair codes found";
                    }
                }
                else
                {
                    LoadingMessage = "Failed to load repair codes. Please check your connection.";
                    ApprovalStatus = "Unable to check status";
                    ApprovalStatusColor = "#FF0000";
                    DebugInfo += "\nAPI returned null - check logs";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in LoadRepairCodesAsync: {ex}");
                LoadingMessage = $"Error loading repair codes: {ex.Message}";
                ApprovalStatus = "Error checking status";
                ApprovalStatusColor = "#FF0000";
                DebugInfo += $"\nException: {ex.Message}";
            }
            finally
            {
                IsLoadingRepairCodes = false;
                System.Diagnostics.Debug.WriteLine($"LoadRepairCodesAsync completed for {ContainerNumber}");
            }
        }

        private void UpdateApprovalStatusDisplay()
        {
            if (IsRepairApproved)
            {
                ApprovalStatus = "Approved";
                ApprovalStatusColor = "#28A745"; // Green
                ShowApprovalInfo = true;
            }
            else
            {
                ApprovalStatus = "Pending Approval";
                ApprovalStatusColor = "#FFC107"; // Yellow/Orange
                ShowApprovalInfo = false;
            }
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
        async void ViewRepairCodePhoto(RepairCodeModel repairCode)
        {
            if (repairCode?.RepairPhoto != null)
            {
                var imageViewerPage = new ImageViewerPage(repairCode.RepairPhoto);
                await Application.Current.MainPage.Navigation.PushAsync(imageViewerPage);
            }
        }

        [RelayCommand]
        async void TakeRepairCodePhoto(RepairCodeModel repairCode)
        {
            try
            {
                var photoResult = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = $"Photo for {repairCode.RepairCode}"
                });

                if (photoResult != null)
                {
                    using var stream = await photoResult.OpenReadAsync();
                    repairCode.RepairPhoto = ImageSource.FromStream(() => stream);
                    repairCode.HasPhoto = true;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Failed to take photo: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async void SubmitRepair()
        {
            bool isValid = true;
            string errorMessage = "";

            if (PhotoUploader.Photos == null || PhotoUploader.Photos.Count == 0)
            {
                isValid = false;
                errorMessage += "Please upload at least one photo.\n";
            }

            if (!isValid)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", errorMessage, "OK");
                return;
            }

            // Check approval status before submitting
            if (!IsRepairApproved)
            {
                bool continueSubmit = await Application.Current.MainPage.DisplayAlert(
                    "Repair Not Approved",
                    "This repair has not been approved yet. Do you want to continue submitting?",
                    "Yes", "No");

                if (!continueSubmit)
                    return;
            }

            // Skip OnReview status and go directly to Finished
            if (Container != null)
            {
                Container.RepairStatus = StatusType.Finished;
                Container.UpdateActivities();
            }

            // Also update survey for backward compatibility
            Survey.RepairStatus = StatusType.Finished;

            string successMessage = IsRepairApproved
                ? "Repair data has been submitted and marked as Finished."
                : "Repair data has been submitted (Pending Approval) and marked as Finished.";

            await Application.Current.MainPage.DisplayAlert("Success", successMessage, "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }

        [RelayCommand]
        async void RefreshApprovalStatus()
        {
            await LoadRepairCodesAsync();
        }

        [RelayCommand]
        async void ToggleRepairStatus(RepairCodeModel repairItem)
        {
            if (repairItem == null) return;

            if (repairItem.IsCompleted)
            {
                // Undo completion
                bool confirmUndo = await Application.Current.MainPage.DisplayAlert(
                    "Undo Completion",
                    $"Mark repair {repairItem.RepairCode} as pending again?",
                    "Yes", "No");

                if (confirmUndo)
                {
                    repairItem.IsCompleted = false;
                    repairItem.CompletedDate = null;
                    repairItem.CompletedBy = string.Empty;
                    repairItem.RepairNotes = string.Empty;

                    await Application.Current.MainPage.DisplayAlert("Status Updated",
                        $"Repair {repairItem.RepairCode} marked as pending.", "OK");
                }
            }
            else
            {
                // Mark as completed - Show detailed info about what's being completed
                string detailMessage = $"Mark this repair as completed?\n\n";
                detailMessage += $"Code: {repairItem.RepairCode}\n";

                if (!string.IsNullOrEmpty(repairItem.RepairCodeDescription))
                    detailMessage += $"Type: {repairItem.RepairCodeDescription}\n";

                if (!string.IsNullOrEmpty(repairItem.ComponentCodeDescription))
                    detailMessage += $"Component: {repairItem.ComponentCodeDescription}\n";

                if (!string.IsNullOrEmpty(repairItem.RepairDetailDescription))
                    detailMessage += $"Details: {repairItem.RepairDetailDescription}";

                bool confirmComplete = await Application.Current.MainPage.DisplayAlert(
                    "Mark as Completed",
                    detailMessage,
                    "Yes", "No");

                if (confirmComplete)
                {
                    repairItem.IsCompleted = true;
                    repairItem.CompletedDate = DateTime.Now;
                    repairItem.CompletedBy = "Current User"; // You can get actual user name

                    // Ask for optional notes
                    string notes = await Application.Current.MainPage.DisplayPromptAsync(
                        "Repair Notes (Optional)",
                        "Add any notes about this repair:",
                        placeholder: "e.g., Used chemical cleaner, replaced seal...");

                    repairItem.RepairNotes = notes ?? string.Empty;

                    await Application.Current.MainPage.DisplayAlert("Repair Completed",
                        $"✓ Repair {repairItem.RepairCode} marked as completed!", "OK");
                }
            }

            // Update overall completion status
            UpdateOverallRepairStatus();
        }

        private void UpdateOverallRepairStatus()
        {
            if (RepairCodes.Count == 0) return;

            int completedCount = RepairCodes.Count(r => r.IsCompleted);
            int totalCount = RepairCodes.Count;

            // Update loading message to show progress
            LoadingMessage = $"Repair Progress: {completedCount}/{totalCount} completed";

            // Update container status based on progress
            if (completedCount == 0)
            {
                Container.RepairStatus = StatusType.NotFilled;
            }
            else if (completedCount == totalCount)
            {
                Container.RepairStatus = StatusType.Finished;
            }
            else
            {
                Container.RepairStatus = StatusType.OnReview; // In progress
            }

            // Update activities
            Container.UpdateActivities();
            Survey.RepairStatus = Container.RepairStatus;

            System.Diagnostics.Debug.WriteLine($"Overall repair status: {completedCount}/{totalCount} completed");
        }
    }
}