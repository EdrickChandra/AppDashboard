using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surveying.ViewModels;

public partial class BaseViewModel : ObservableObject {
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private bool hasError = false;

    public BaseViewModel() { }
}
