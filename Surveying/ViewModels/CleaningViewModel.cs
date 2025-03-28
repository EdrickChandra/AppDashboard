using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System;

namespace Surveying.ViewModels
{
    public partial class CleaningViewModel : BaseViewModel
    {
        public SurveyModel Survey { get; }
        public string ContainerNumber => Survey.ContNumber;
        public DateTime StartCleanDate { get; set; }
        public DateTime EndCleanDate { get; set; }

        // Expose the separate PhotoUploadViewModel for photo functionality.
        public PhotoUploadViewModel PhotoUploader { get; } = new PhotoUploadViewModel();

        public CleaningViewModel(SurveyModel survey)
        {
            Survey = survey;
            StartCleanDate = DateTime.Today;
            EndCleanDate = DateTime.Today.AddDays(1);
        }

        [RelayCommand]
        void SubmitCleaning()
        {
            // For example, set the cleaning status to OnReview.
            Survey.CleaningStatus = StatusType.OnReview;
        }
    }
}
