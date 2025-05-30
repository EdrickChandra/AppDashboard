﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Surveying.Models;
using Surveying.Views;
using System;

namespace Surveying.ViewModels
{
    public partial class CleaningViewModel : BaseViewModel
    {
        public SurveyModel Survey { get; }
        public ContainerDetailModel Container { get; private set; }
        public string ContainerNumber => Container?.ContNumber ?? Survey.ContNumber;
        public DateTime StartCleanDate { get; set; }
        public DateTime EndCleanDate { get; set; }

        public PhotoUploadViewModel PhotoUploader { get; } = new PhotoUploadViewModel(4);

        // Constructor with just Survey (for backward compatibility)
        public CleaningViewModel(SurveyModel survey)
        {
            Survey = survey;
            StartCleanDate = DateTime.Today;
            EndCleanDate = DateTime.Today.AddDays(1);
        }

        // New constructor with Survey and Container
        public CleaningViewModel(SurveyModel survey, ContainerDetailModel container) : this(survey)
        {
            Container = container;
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
        async void SubmitCleaning()
        {
            bool isValid = true;
            string errorMessage = "";

            if (PhotoUploader.Photos == null || PhotoUploader.Photos.Count == 0)
            {
                isValid = false;
                errorMessage += "Please upload at least one photo.\n";
            }

            if (EndCleanDate < StartCleanDate)
            {
                isValid = false;
                errorMessage += "End date cannot be before start date.\n";
            }

            if (!isValid)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", errorMessage, "OK");
                return;
            }

            // Update the container if available, otherwise update the survey
            if (Container != null)
            {
                Container.CleaningStatus = StatusType.OnReview;
            }

            // Always update the survey for backward compatibility
            Survey.CleaningStatus = StatusType.OnReview;

            await Application.Current.MainPage.DisplayAlert("Success",
                "Cleaning data has been submitted for review.", "OK");

            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }
    }
}