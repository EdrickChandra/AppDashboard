using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Surveying.Models;

namespace Surveying.ViewModels
{
    public class CleaningViewModel : BaseViewModel
    {
        
        public string ContainerNumber { get; set; }
        public DateTime StartCleanDate { get; set; }
        public DateTime EndCleanDate { get; set; }

        public ObservableCollection<PhotoItem> Photos { get; set; }
            = new ObservableCollection<PhotoItem>();

        public CleaningViewModel(string containerNumber)
        {
            ContainerNumber = containerNumber;
            StartCleanDate = DateTime.Today;
            EndCleanDate = DateTime.Today.AddDays(1);
        }
    }

}
