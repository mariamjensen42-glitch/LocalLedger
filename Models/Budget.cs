using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalLedger.Models;

public partial class Budget : ObservableObject
{
    [ObservableProperty]
    private decimal _monthlyLimit;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private int _warningThreshold = 80;
}
