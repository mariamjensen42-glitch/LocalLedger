using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LocalLedger.Models;

public enum TransactionType
{
    Income,
    Expense
}

public partial class Transaction : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private TransactionType _type;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private string _note = string.Empty;

    public string DisplayAmount => Type == TransactionType.Income ? $"+{Amount:N2}" : $"-{Amount:N2}";
}
