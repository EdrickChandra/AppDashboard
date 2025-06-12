using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Windows.Devices.Sensors;

namespace Surveying.ViewModels
{
    /// <summary>
    /// BASE ACTIVITY VIEWMODEL
    /// Provides shared functionality for all activity-related ViewModels
    /// Extracted common patterns from the old activity ViewModels
    /// </summary>
    public abstract partial class BaseActivityViewModel : BaseViewModel
    {
        // ===== CORE ACTIVITY PROPERTIES =====
        public Order Order { get; protected set; }
        public Container Container { get; protected set; }

        [ObservableProperty]
        protected string containerNumber;

        [ObservableProperty]
        protected DateTime activityDate = DateTime.Today;

        [ObservableProperty]
        protected string activityDescription = "";

        // ===== COMMON STATUS PROPERTIES =====
        [ObservableProperty]
        protected bool isActivityApproved;

        [ObservableProperty]
        protected DateTime? approvalDate;

        [ObservableProperty]
        protected string approvedBy = "";

        [ObservableProperty]
        protected string approvalStatus = "";

        [ObservableProperty]
        protected string approvalStatusColor = "#FFC107";

        [ObservableProperty]
        protected bool showApprovalInfo;

        // ===== COMMON LOADING STATES =====
        [ObservableProperty]
        protected bool isLoadingData = true;

        [ObservableProperty]
        protected string loadingMessage = "Loading...";

        [ObservableProperty]
        protected bool hasData;

        [ObservableProperty]
        protected string errorMessage = "";

        [ObservableProperty]
        protected bool hasError;

        // ===== DEBUGGING =====
        [ObservableProperty]
        protected string debugInfo = "";

        [ObservableProperty]
        protected bool showDebugInfo = false; // Set to true in development

        // ===== CONSTRUCTOR =====
        protected BaseActivityViewModel(Order order, Container container)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
            Container = container ?? throw new ArgumentNullException(nameof(container));
            ContainerNumber = container.ContNumber;
        }

        // ===== COMMON APPROVAL LOGIC =====
        protected virtual void UpdateApprovalDisplay()
        {
            if (IsActivityApproved)
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

        // ===== COMMON ERROR HANDLING =====
        protected virtual void HandleError(Exception ex, string context = "")
        {
            HasError = true;
            ErrorMessage = string.IsNullOrEmpty(context)
                ? ex.Message
                : $"Error in {context}: {ex.Message}";

            System.Diagnostics.Debug.WriteLine($"BaseActivityViewModel Error: {ErrorMessage}");

            if (ShowDebugInfo)
            {
                DebugInfo += $"\n{DateTime.Now:HH:mm:ss} - {ErrorMessage}";
            }
        }

        protected virtual void ClearError()
        {
            HasError = false;
            ErrorMessage = "";
        }

        // ===== COMMON LOADING MANAGEMENT =====
        protected virtual void SetLoadingState(bool isLoading, string message = "")
        {
            IsLoadingData = isLoading;
            LoadingMessage = string.IsNullOrEmpty(message)
                ? (isLoading ? "Loading..." : "Ready")
                : message;
        }

        // ===== COMMON VALIDATION =====
        protected virtual ValidationResult ValidateBasicRequirements()
        {
            if (string.IsNullOrWhiteSpace(ContainerNumber))
            {
                return ValidationResult.Error("Container number is required.");
            }

            if (ActivityDate > DateTime.Today.AddDays(1))
            {
                return ValidationResult.Error("Activity date cannot be in the future.");
            }

            return ValidationResult.Success();
        }

        // ===== ABSTRACT METHODS FOR DERIVED CLASSES =====
        protected abstract Task LoadActivityDataAsync();
        protected abstract Task<ValidationResult> ValidateActivityAsync();
        protected abstract Task<bool> SubmitActivityAsync();

        // ===== COMMON COMMANDS =====
        [RelayCommand]
        protected virtual async Task RefreshData()
        {
            ClearError();
            await LoadActivityDataAsync();
        }

        [RelayCommand]
        protected virtual async Task ShowDebugDetails()
        {
            if (!string.IsNullOrEmpty(DebugInfo))
            {
                await Application.Current.MainPage.DisplayAlert("Debug Information", DebugInfo, "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Debug Information", "No debug information available.", "OK");
            }
        }

        [RelayCommand]
        protected virtual async Task Submit()
        {
            try
            {
                ClearError();

                // Basic validation
                var basicValidation = ValidateBasicRequirements();
                if (!basicValidation.IsValid)
                {
                    await Application.Current.MainPage.DisplayAlert("Validation Error", basicValidation.ErrorMessage, "OK");
                    return;
                }

                // Activity-specific validation
                var activityValidation = await ValidateActivityAsync();
                if (!activityValidation.IsValid)
                {
                    await Application.Current.MainPage.DisplayAlert("Validation Error", activityValidation.ErrorMessage, "OK");
                    return;
                }

                // Submit
                SetLoadingState(true, "Submitting...");
                bool success = await SubmitActivityAsync();

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Success",
                        "Data has been submitted successfully.", "OK");

                    await Application.Current.MainPage.Navigation.PopToRootAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "Failed to submit data. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "Submit");
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        // ===== NAVIGATION HELPERS =====
        protected virtual async Task NavigateBackAsync()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        protected virtual async Task NavigateToRootAsync()
        {
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }

        // ===== CONTAINER STATUS HELPERS =====
        protected virtual void UpdateContainerStatus(StatusType status, ActivityType activityType)
        {
            switch (activityType)
            {
                case ActivityType.Cleaning:
                    Container.CleaningStatus = status;
                    break;
                case ActivityType.Repair:
                    Container.RepairStatus = status;
                    break;
                case ActivityType.Periodic:
                    Container.PeriodicStatus = status;
                    break;
                case ActivityType.Survey:
                    Container.SurveyStatus = status;
                    break;
            }

            Container.UpdateActivities();
        }

        // ===== CLEANUP =====
        protected virtual void OnDisposing()
        {
            // Override in derived classes for cleanup
        }

        ~BaseActivityViewModel()
        {
            OnDisposing();
        }
    }

    // ===== VALIDATION RESULT (if not already defined) =====
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = "";

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Error(string message) => new() { IsValid = false, ErrorMessage = message };
    }
}   