using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System;

namespace Surveying.ViewModels
{
    public partial class RepairViewModel : BaseViewModel
    {
        public SurveyModel Survey { get; }

        public PhotoUploadViewModel PhotoUploader { get; } = new PhotoUploadViewModel();
        public string ContainerNumber => Survey.ContNumber;


        public RepairViewModel(SurveyModel survey)
        {
            Survey = survey;
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

            Survey.CleaningStatus = StatusType.OnReview;

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}