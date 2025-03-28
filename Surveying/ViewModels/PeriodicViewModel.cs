using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using System;

namespace Surveying.ViewModels
{
    public partial class PeriodicViewModel : BaseViewModel
    {
        public SurveyModel Survey { get; }

        public string ContainerNumber => Survey.ContNumber;
        public PhotoUploadViewModel PhotoUploader { get; } = new PhotoUploadViewModel();
        public PeriodicViewModel(SurveyModel survey)
        {
            Survey = survey;
        }

        [RelayCommand]
        void SubmitPeriodic()
        {
            Survey.PeriodicStatus = StatusType.OnReview;
        }
    }
}
