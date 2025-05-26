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
        public PhotoUploadViewModel PhotoUploader { get; } = new PhotoUploadViewModel();

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
                LoadingMessage = "Loading repair codes...";

                var containerWithCodes = await _containerApiService.GetContainerWithRepairCodes(ContainerNumber);

                if (containerWithCodes != null && containerWithCodes.RepairCodes != null)
                {
                    RepairCodes.Clear();
                    foreach (var code in containerWithCodes.RepairCodes)
                    {
                        RepairCodes.Add(code);
                    }

                    if (RepairCodes.Count == 0)
                    {
                        LoadingMessage = "No repair codes found for this container.";
                    }
                }
                else
                {
                    LoadingMessage = "Failed to load repair codes. Please check your connection.";
                }
            }
            catch (Exception ex)
            {
                LoadingMessage = $"Error loading repair codes: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading repair codes: {ex}");
            }
            finally
            {
                IsLoadingRepairCodes = false;
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

            // Skip OnReview status and go directly to Finished
            if (Container != null)
            {
                Container.RepairStatus = StatusType.Finished;
                Container.UpdateActivities();
            }

            // Also update survey for backward compatibility
            Survey.RepairStatus = StatusType.Finished;

            await Application.Current.MainPage.DisplayAlert("Success",
                "Repair data has been submitted and marked as Finished.", "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}