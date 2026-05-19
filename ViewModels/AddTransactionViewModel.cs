using System;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLedger.Models;

namespace LocalLedger.ViewModels;

public partial class AddTransactionViewModel : ViewModelBase
{
    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private bool _isExpense = true;

    [ObservableProperty]
    private int _selectedCategoryIndex;

    [ObservableProperty]
    private DateTime? _selectedDate = DateTime.Today;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private string _amountText = string.Empty;

    public ObservableCollection<string> ExpenseCategories { get; } = new()
    {
        "餐饮", "交通", "购物", "娱乐", "居住", "医疗", "其他"
    };

    public ObservableCollection<string> IncomeCategories { get; } = new()
    {
        "工资", "奖金", "投资收益", "其他"
    };

    public ObservableCollection<string> CurrentCategories => IsExpense ? ExpenseCategories : IncomeCategories;

    public string Title => IsExpense ? "记支出" : "记收入";

    public Action<Transaction?>? OnClose { get; set; }

    partial void OnIsExpenseChanged(bool value)
    {
        OnPropertyChanged(nameof(CurrentCategories));
        OnPropertyChanged(nameof(Title));
        SelectedCategoryIndex = 0;
    }

    partial void OnAmountTextChanged(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
        {
            Amount = amount;
        }
    }

    [RelayCommand]
    private void SetExpense()
    {
        IsExpense = true;
    }

    [RelayCommand]
    private void SetIncome()
    {
        IsExpense = false;
    }

    [RelayCommand]
    private void Save()
    {
        if (Amount <= 0 || !SelectedDate.HasValue) return;

        var transaction = new Transaction
        {
            Amount = Amount,
            Type = IsExpense ? TransactionType.Expense : TransactionType.Income,
            Category = CurrentCategories.Count > SelectedCategoryIndex
                ? CurrentCategories[SelectedCategoryIndex]
                : string.Empty,
            Date = SelectedDate.Value,
            Note = Note
        };

        OnClose?.Invoke(transaction);
    }

    [RelayCommand]
    private void Cancel()
    {
        OnClose?.Invoke(null);
    }
}
