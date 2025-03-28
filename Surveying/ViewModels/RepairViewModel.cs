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
        void SubmitRepair()
        {
            Survey.RepairStatus = StatusType.OnReview;
        }
    }
}