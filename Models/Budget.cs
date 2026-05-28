using System;
using System.Collections.Generic;
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

    public List<CategoryBudget> CategoryBudgets { get; set; } = new();
}

public partial class CategoryBudget : ObservableObject
{
    [ObservableProperty]
    private string _categoryName = string.Empty;

    [ObservableProperty]
    private decimal _limit;

    [ObservableProperty]
    private string _color = "#3B82F6";

    [ObservableProperty]
    private string _icon = string.Empty;
}
