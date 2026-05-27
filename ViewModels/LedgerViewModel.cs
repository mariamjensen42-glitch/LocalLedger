using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLedger.Models;
using LocalLedger.Services;

namespace LocalLedger.ViewModels;

public enum DateFilter
{
    All,
    Today,
    ThisWeek,
    ThisMonth
}

public partial class LedgerViewModel : ViewModelBase
{
    [ObservableProperty]
    private DateFilter _currentFilter = DateFilter.All;

    [ObservableProperty]
    private Transaction? _editingTransaction;

    public ObservableCollection<Transaction> AllTransactions => DataService.Instance.Transactions;

    public ObservableCollection<Transaction> FilteredTransactions { get; } = new();

    public decimal TotalIncome => FilteredTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
    public decimal TotalExpense => FilteredTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
    public decimal Balance => TotalIncome - TotalExpense;

    [ObservableProperty]
    private bool _showAddForm;

    [ObservableProperty]
    private bool _isAddingExpense = true;

    [ObservableProperty]
    private bool _isBudgetOver;

    [ObservableProperty]
    private bool _isBudgetNear;

    [ObservableProperty]
    private decimal _budgetLimit;

    [ObservableProperty]
    private decimal _currentMonthExpense;

    [ObservableProperty]
    private double _budgetPercentage;

    public LedgerViewModel()
    {
        RefreshFilteredTransactions();
        BudgetService.Instance.BudgetChanged += OnBudgetChanged;
        DataService.Instance.DataChanged += OnDataChanged;
        UpdateBudgetStatus();
    }

    private void OnBudgetChanged(object? sender, EventArgs e)
    {
        UpdateBudgetStatus();
    }

    private void OnDataChanged(object? sender, EventArgs e)
    {
        UpdateBudgetStatus();
    }

    private void UpdateBudgetStatus()
    {
        BudgetLimit = BudgetService.Instance.CurrentBudget.MonthlyLimit;
        CurrentMonthExpense = BudgetService.Instance.GetCurrentMonthExpense();
        BudgetPercentage = BudgetService.Instance.GetBudgetPercentage();
        IsBudgetOver = BudgetService.Instance.IsOverBudget();
        IsBudgetNear = BudgetService.Instance.IsNearBudget();
    }

    partial void OnCurrentFilterChanged(DateFilter value)
    {
        RefreshFilteredTransactions();
    }

    private void RefreshFilteredTransactions()
    {
        FilteredTransactions.Clear();
        var filtered = GetFilteredTransactions();
        foreach (var t in filtered)
        {
            FilteredTransactions.Add(t);
        }
        UpdateTotals();
    }

    private IOrderedEnumerable<Transaction> GetFilteredTransactions()
    {
        var now = DateTime.Today;
        var query = AllTransactions.AsEnumerable();

        switch (CurrentFilter)
        {
            case DateFilter.Today:
                query = query.Where(t => t.Date.Date == now);
                break;
            case DateFilter.ThisWeek:
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                query = query.Where(t => t.Date.Date >= startOfWeek && t.Date.Date <= now);
                break;
            case DateFilter.ThisMonth:
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                query = query.Where(t => t.Date.Date >= startOfMonth && t.Date.Date <= now);
                break;
        }

        return query.OrderByDescending(t => t.Date);
    }

    private void UpdateTotals()
    {
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(TotalExpense));
        OnPropertyChanged(nameof(Balance));
    }

    [RelayCommand]
    private void ShowAddExpense()
    {
        EditingTransaction = null;
        IsAddingExpense = true;
        ShowAddForm = true;
    }

    [RelayCommand]
    private void ShowAddIncome()
    {
        EditingTransaction = null;
        IsAddingExpense = false;
        ShowAddForm = true;
    }

    [RelayCommand]
    private void EditTransaction(Transaction transaction)
    {
        EditingTransaction = transaction;
        IsAddingExpense = transaction.Type == TransactionType.Expense;
        ShowAddForm = true;
    }

    [RelayCommand]
    private void DeleteTransaction(Transaction transaction)
    {
        DataService.Instance.DeleteTransaction(transaction.Id);
        RefreshFilteredTransactions();
        UpdateBudgetStatus();
    }

    [RelayCommand]
    private void CloseAddForm()
    {
        ShowAddForm = false;
        EditingTransaction = null;
    }

    public void AddOrUpdateTransaction(Transaction transaction)
    {
        if (EditingTransaction != null)
        {
            transaction.Id = EditingTransaction.Id;
            DataService.Instance.UpdateTransaction(transaction);
        }
        else
        {
            DataService.Instance.AddTransaction(transaction);
        }
        ShowAddForm = false;
        EditingTransaction = null;
        RefreshFilteredTransactions();
        UpdateBudgetStatus();
    }
}
