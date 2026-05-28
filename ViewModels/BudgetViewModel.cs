using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLedger.Models;
using LocalLedger.Services;

namespace LocalLedger.ViewModels;

public partial class CategoryBudgetDisplay : ObservableObject
{
    [ObservableProperty]
    private string _categoryName = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private string _color = "#3B82F6";

    [ObservableProperty]
    private decimal _limit;

    [ObservableProperty]
    private decimal _spent;

    [ObservableProperty]
    private double _percentage;

    [ObservableProperty]
    private bool _isOverBudget;

    [ObservableProperty]
    private bool _isNearBudget;

    public decimal Remaining => Limit - Spent;
}

public partial class BudgetViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _budgetEnabled;

    [ObservableProperty]
    private decimal _monthlyLimit;

    [ObservableProperty]
    private int _warningThreshold = 80;

    [ObservableProperty]
    private decimal _currentMonthExpense;

    [ObservableProperty]
    private double _budgetPercentage;

    [ObservableProperty]
    private bool _isOverBudget;

    [ObservableProperty]
    private bool _isNearBudget;

    [ObservableProperty]
    private decimal _remainingBudget;

    [ObservableProperty]
    private int _daysLeftInMonth;

    [ObservableProperty]
    private decimal _suggestedDailyBudget;

    [ObservableProperty]
    private string _selectedCategoryName = string.Empty;

    [ObservableProperty]
    private decimal _newCategoryLimit;

    public ObservableCollection<CategoryBudgetDisplay> CategoryBudgets { get; } = new();

    public ObservableCollection<string> AvailableCategories { get; } = new();

    public BudgetViewModel()
    {
        LoadFromService();
        BudgetService.Instance.BudgetChanged += OnBudgetChanged;
        DataService.Instance.DataChanged += OnDataChanged;
    }

    private void OnBudgetChanged(object? sender, EventArgs e)
    {
        LoadFromService();
    }

    private void OnDataChanged(object? sender, EventArgs e)
    {
        UpdateSpending();
    }

    private void LoadFromService()
    {
        var budget = BudgetService.Instance.CurrentBudget;
        BudgetEnabled = budget.IsEnabled;
        MonthlyLimit = budget.MonthlyLimit;
        WarningThreshold = budget.WarningThreshold;

        LoadAvailableCategories();
        RefreshCategoryBudgets();
        UpdateSpending();
    }

    private void LoadAvailableCategories()
    {
        AvailableCategories.Clear();
        foreach (var cat in DataService.Instance.ExpenseCategories)
        {
            AvailableCategories.Add(cat.Name);
        }
    }

    private void RefreshCategoryBudgets()
    {
        CategoryBudgets.Clear();
        var budget = BudgetService.Instance.CurrentBudget;

        foreach (var cb in budget.CategoryBudgets)
        {
            var spent = BudgetService.Instance.GetCurrentMonthCategoryExpense(cb.CategoryName);
            var display = new CategoryBudgetDisplay
            {
                CategoryName = cb.CategoryName,
                Icon = cb.Icon,
                Color = cb.Color,
                Limit = cb.Limit,
                Spent = spent,
                Percentage = BudgetService.Instance.GetCategoryBudgetPercentage(cb),
                IsOverBudget = BudgetService.Instance.IsCategoryOverBudget(cb),
                IsNearBudget = BudgetService.Instance.IsCategoryNearBudget(cb)
            };
            CategoryBudgets.Add(display);
        }
    }

    private void UpdateSpending()
    {
        CurrentMonthExpense = BudgetService.Instance.GetCurrentMonthExpense();
        BudgetPercentage = BudgetService.Instance.GetBudgetPercentage();
        IsOverBudget = BudgetService.Instance.IsOverBudget();
        IsNearBudget = BudgetService.Instance.IsNearBudget();
        RemainingBudget = BudgetService.Instance.GetRemainingBudget();
        DaysLeftInMonth = BudgetService.Instance.GetDaysLeftInMonth();
        SuggestedDailyBudget = BudgetService.Instance.GetSuggestedDailyBudget();

        RefreshCategoryBudgets();
    }

    [RelayCommand]
    private void SaveGeneralBudget()
    {
        BudgetService.Instance.UpdateBudget(MonthlyLimit, BudgetEnabled, WarningThreshold);
    }

    [RelayCommand]
    private void AddCategoryBudget()
    {
        if (string.IsNullOrEmpty(SelectedCategoryName) || NewCategoryLimit <= 0)
            return;

        var budget = BudgetService.Instance.CurrentBudget;
        if (budget.CategoryBudgets.Any(cb => cb.CategoryName == SelectedCategoryName))
            return;

        var cat = DataService.Instance.ExpenseCategories.FirstOrDefault(c => c.Name == SelectedCategoryName);
        var newCb = new CategoryBudget
        {
            CategoryName = SelectedCategoryName,
            Limit = NewCategoryLimit,
            Color = cat?.Color ?? "#3B82F6",
            Icon = cat?.Icon ?? ""
        };

        budget.CategoryBudgets.Add(newCb);
        BudgetService.Instance.UpdateCategoryBudgets(budget.CategoryBudgets);

        NewCategoryLimit = 0;
        SelectedCategoryName = string.Empty;
    }

    [RelayCommand]
    private void RemoveCategoryBudget(string categoryName)
    {
        var budget = BudgetService.Instance.CurrentBudget;
        var cb = budget.CategoryBudgets.FirstOrDefault(c => c.CategoryName == categoryName);
        if (cb != null)
        {
            budget.CategoryBudgets.Remove(cb);
            BudgetService.Instance.UpdateCategoryBudgets(budget.CategoryBudgets);
        }
    }
}
