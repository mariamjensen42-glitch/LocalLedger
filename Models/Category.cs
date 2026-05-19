using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalLedger.Models;

public enum CategoryType
{
    Expense,
    Income
}

public partial class Category : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private CategoryType _type;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private string _color = "#3B82F6";

    [ObservableProperty]
    private int _transactionCount;

    [ObservableProperty]
    private decimal _totalAmount;
}
