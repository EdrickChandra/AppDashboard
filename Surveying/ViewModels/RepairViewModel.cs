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
    // ===== MIGRATED: RepairViewModel using simplified models =====
    public partial class RepairViewModel : BaseViewModel
    {
        private readonly IContainerApiService _containerApiService;

        // ===== SIMPLIFIED MODELS =====
        // OLD: SurveyModel survey, ContainerDetailModel container
        // NEW: Order order, Container container (unified models)
        public Order Order { get; }
        public Container Container { get; }

        public string ContainerNumber => Container.ContNumber;

        // Photo uploader - unchanged
        public PhotoUploadViewModel PhotoUploader { get; } = PhotoUploadViewModel.CreateForRepair();

        // ===== REPAIR PROPERTIES - UNCHANGED =====
        [ObservableProperty]
        private DateTime repairDate = DateTime.Today;

        [ObservableProperty]
        private string repairDescription = "";

        // ===== SIMPLIFIED REPAIR CODES =====
        // OLD: ObservableCollection<RepairCodeModel> repairCodes
        // NEW: Direct access to Container.RepairCodes (unified RepairCode class)
        public ObservableCollection<RepairCode> RepairCodes => Container.RepairCodes != null
            ? new ObservableCollection<RepairCode>(Container.RepairCodes)
            : new ObservableCollection<RepairCode>();

        [ObservableProperty]
        private bool isLoadingRepairCodes = true;

        [ObservableProperty]
        private string loadingMessage = "Loading repair codes...";

        // ===== SIMPLIFIED APPROVAL STATUS =====
        // OLD: Multiple properties with manual updates
        // NEW: Direct access to Container properties
        public bool IsRepairApproved => Container.IsRepairApproved;
        public DateTime? ApprovalDate => Container.ApprovalDate;
        public string ApprovedBy => Container.ApprovedBy;

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

        // ===== CONSTRUCTORS =====
        // OLD: Took SurveyModel + ContainerDetailModel
        // NEW: Takes Order + Container (unified models)
        public RepairViewModel(Order order, Container container) : this(order, container, new ContainerApiService())
        {
        }

        public RepairViewModel(Order order, Container container, IContainerApiService containerApiService)
        {
            Order = order;
            Container = container;
            _containerApiService = containerApiService;

            // Load repair codes when view model is created
            _ = LoadRepairCodesAsync();
        }

        // ===== SIMPLIFIED API LOADING =====
        private async Task LoadRepairCodesAsync()
        {
            try
            {
                IsLoadingRepairCodes = true;
                LoadingMessage = "Loading repair codes and approval status...";
                DebugInfo = $"Starting API call for container: {ContainerNumber}";

                System.Diagnostics.Debug.WriteLine($"Starting LoadRepairCodesAsync for {ContainerNumber}");

                // SIMPLIFIED: API returns Container directly (no complex mapping needed)
                var containerWithCodes = await _containerApiService.GetContainerWithRepairCodes(ContainerNumber);

                System.Diagnostics.Debug.WriteLine($"API call completed. Result: {(containerWithCodes != null ? "Success" : "Null")}");

                if (containerWithCodes != null)
                {
                    DebugInfo += $"\nAPI Success - Container found";

                    // SIMPLIFIED: Direct property updates (no complex mapping)
                    Container.IsRepairApproved = containerWithCodes.IsRepairApproved;
                    Container.ApprovalDate = containerWithCodes.ApprovalDate;
                    Container.ApprovedBy = containerWithCodes.ApprovedBy ?? string.Empty;

                    DebugInfo += $"\nApproval: {IsRepairApproved}";
                    DebugInfo += $"\nApproval Date: {ApprovalDate}";
                    DebugInfo += $"\nApproved By: {ApprovedBy}";

                    UpdateApprovalStatusDisplay();

                    // SIMPLIFIED: Direct repair codes assignment (no complex mapping)
                    if (containerWithCodes.RepairCodes != null && containerWithCodes.RepairCodes.Count > 0)
                    {
                        Container.RepairCodes.Clear();

                        foreach (var apiCode in containerWithCodes.RepairCodes)
                        {
                            // SIMPLIFIED: No more legacy property handling!
                            var repairCode = new RepairCode
                            {
                                Code = apiCode.Code,                           // Was: RepairCode
                                ComponentCode = apiCode.ComponentCode,
                                LocationCode = apiCode.LocationCode,
                                Description = apiCode.Description,             // No more RepairCodeDescription vs RepairCodeDescription_Legacy!
                                ComponentDescription = apiCode.ComponentDescription, // No more ComponentCodeDescription vs ComponentCodeDescription_Legacy!
                                DetailDescription = apiCode.DetailDescription, // No more RepairDetailDescription vs RepairDetailDescription_Legacy!

                                // Initialize status tracking
                                IsCompleted = false,
                                CompletedDate = null,
                                CompletedBy = string.Empty,
                                Notes = string.Empty
                            };

                            Container.RepairCodes.Add(repairCode);
                            System.Diagnostics.Debug.WriteLine($"Added repair code: {apiCode.Code}");
                            System.Diagnostics.Debug.WriteLine($"  - Type: {apiCode.Description}");
                            System.Diagnostics.Debug.WriteLine($"  - Component: {apiCode.ComponentDescription}");
                            System.Diagnostics.Debug.WriteLine($"  - Details: {apiCode.DetailDescription}");
                        }

                        DebugInfo += $"\nLoaded {Container.RepairCodes.Count} repair codes";
                        LoadingMessage = $"Loaded {Container.RepairCodes.Count} repair codes successfully";
                    }
                    else
                    {
                        LoadingMessage = "No repair codes found for this container.";
                        DebugInfo += "\nNo repair codes found";
                    }

                    // Trigger UI updates
                    OnPropertyChanged(nameof(RepairCodes));
                    OnPropertyChanged(nameof(IsRepairApproved));
                    OnPropertyChanged(nameof(ApprovalDate));
                    OnPropertyChanged(nameof(ApprovedBy));
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

        // ===== UI COMMANDS - SIMPLIFIED =====
        [RelayCommand]
        async void ShowDebugDetails()
        {
            await Application.Current.MainPage.DisplayAlert("Debug Info", DebugInfo, "OK");
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
        async void ViewRepairCodePhoto(RepairCode repairCode)
        {
            if (repairCode?.Photo != null)
            {
                var imageViewerPage = new ImageViewerPage(repairCode.Photo);
                await Application.Current.MainPage.Navigation.PushAsync(imageViewerPage);
            }
        }

        [RelayCommand]
        async void TakeRepairCodePhoto(RepairCode repairCode)
        {
            try
            {
                var photoResult = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = $"Photo for {repairCode.Code}"  // SIMPLIFIED: .Code instead of .RepairCode
                });

                if (photoResult != null)
                {
                    using var stream = await photoResult.OpenReadAsync();
                    repairCode.Photo = ImageSource.FromStream(() => stream);
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
        async void ToggleRepairStatus(RepairCode repairItem)
        {
            if (repairItem == null) return;

            if (repairItem.IsCompleted)
            {
                // Undo completion
                bool confirmUndo = await Application.Current.MainPage.DisplayAlert(
                    "Undo Completion",
                    $"Mark repair {repairItem.Code} as pending again?",  // SIMPLIFIED: .Code instead of .RepairCode
                    "Yes", "No");

                if (confirmUndo)
                {
                    repairItem.IsCompleted = false;
                    repairItem.CompletedDate = null;
                    repairItem.CompletedBy = string.Empty;
                    repairItem.Notes = string.Empty;  // SIMPLIFIED: .Notes instead of .RepairNotes

                    await Application.Current.MainPage.DisplayAlert("Status Updated",
                        $"Repair {repairItem.Code} marked as pending.", "OK");
                }
            }
            else
            {
                // Mark as completed - SIMPLIFIED: Clean property access
                string detailMessage = $"Mark this repair as completed?\n\n";
                detailMessage += $"Code: {repairItem.Code}\n";

                if (!string.IsNullOrEmpty(repairItem.Description))
                    detailMessage += $"Type: {repairItem.Description}\n";

                if (!string.IsNullOrEmpty(repairItem.ComponentDescription))
                    detailMessage += $"Component: {repairItem.ComponentDescription}\n";

                if (!string.IsNullOrEmpty(repairItem.DetailDescription))
                    detailMessage += $"Details: {repairItem.DetailDescription}";

                bool confirmComplete = await Application.Current.MainPage.DisplayAlert(
                    "Mark as Completed",
                    detailMessage,
                    "Yes", "No");

                if (confirmComplete)
                {
                    repairItem.IsCompleted = true;
                    repairItem.CompletedDate = DateTime.Now;
                    repairItem.CompletedBy = "Current User";

                    // Ask for optional notes
                    string notes = await Application.Current.MainPage.DisplayPromptAsync(
                        "Repair Notes (Optional)",
                        "Add any notes about this repair:",
                        placeholder: "e.g., Used chemical cleaner, replaced seal...");

                    repairItem.Notes = notes ?? string.Empty;

                    await Application.Current.MainPage.DisplayAlert("Repair Completed",
                        $"✓ Repair {repairItem.Code} marked as completed!", "OK");
                }
            }

            // Update overall completion status
            UpdateOverallRepairStatus();
        }

        private void UpdateOverallRepairStatus()
        {
            if (Container.RepairCodes.Count == 0) return;

            int completedCount = Container.RepairCodes.Count(r => r.IsCompleted);
            int totalCount = Container.RepairCodes.Count;

            // Update loading message to show progress
            LoadingMessage = $"Repair Progress: {completedCount}/{totalCount} completed";

            // SIMPLIFIED: Direct status update on Container
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

            // Update activities automatically
            Container.UpdateActivities();

            System.Diagnostics.Debug.WriteLine($"Overall repair status: {completedCount}/{totalCount} completed");
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

            // SIMPLIFIED: Direct status update
            Container.RepairStatus = StatusType.Finished;
            Container.UpdateActivities();

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
    }
}