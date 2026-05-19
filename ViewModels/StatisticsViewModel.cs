using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LocalLedger.Models;
using LocalLedger.Services;

namespace LocalLedger.ViewModels;

public enum StatsTimeFilter
{
    ThisWeek,
    ThisMonth,
    ThisYear,
    All
}

public partial class CategoryStats : ObservableObject
{
    [ObservableProperty]
    private string _categoryName = string.Empty;
    [ObservableProperty]
    private string _icon = string.Empty;
    [ObservableProperty]
    private string _color = "#3B82F6";
    [ObservableProperty]
    private decimal _totalAmount;
    [ObservableProperty]
    private double _percentage;
}

public partial class StatisticsViewModel : ViewModelBase
{
    [ObservableProperty]
    private StatsTimeFilter _currentTimeFilter = StatsTimeFilter.ThisMonth;

    [ObservableProperty]
    private decimal _totalIncome;

    [ObservableProperty]
    private decimal _totalExpense;

    [ObservableProperty]
    private decimal _balance;

    public ObservableCollection<CategoryStats> ExpenseCategoryStats { get; } = new();
    public ObservableCollection<CategoryStats> IncomeCategoryStats { get; } = new();

    public StatisticsViewModel()
    {
        RefreshStats();
        DataService.Instance.DataChanged += OnDataServiceDataChanged;
    }

    private void OnDataServiceDataChanged(object? sender, EventArgs e)
    {
        RefreshStats();
    }

    partial void OnCurrentTimeFilterChanged(StatsTimeFilter value)
    {
        RefreshStats();
    }

    public void RefreshStats()
    {
        var transactions = GetFilteredTransactions();

        // 计算总收入和总支出
        TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        TotalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        Balance = TotalIncome - TotalExpense;

        // 更新支出分类统计
        UpdateCategoryStats(ExpenseCategoryStats, transactions, TransactionType.Expense, DataService.Instance.ExpenseCategories);

        // 更新收入分类统计
        UpdateCategoryStats(IncomeCategoryStats, transactions, TransactionType.Income, DataService.Instance.IncomeCategories);
    }

    private Transaction[] GetFilteredTransactions()
    {
        var now = DateTime.Today;
        var allTransactions = DataService.Instance.Transactions.AsEnumerable();

        switch (CurrentTimeFilter)
        {
            case StatsTimeFilter.ThisWeek:
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                return allTransactions.Where(t => t.Date.Date >= startOfWeek && t.Date.Date <= now).ToArray();

            case StatsTimeFilter.ThisMonth:
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                return allTransactions.Where(t => t.Date.Date >= startOfMonth && t.Date.Date <= now).ToArray();

            case StatsTimeFilter.ThisYear:
                var startOfYear = new DateTime(now.Year, 1, 1);
                return allTransactions.Where(t => t.Date.Date >= startOfYear && t.Date.Date <= now).ToArray();

            case StatsTimeFilter.All:
            default:
                return allTransactions.ToArray();
        }
    }

    private void UpdateCategoryStats(
        ObservableCollection<CategoryStats> target,
        Transaction[] transactions,
        TransactionType type,
        ObservableCollection<Category> categories)
    {
        target.Clear();

        var categoryGroups = transactions
            .Where(t => t.Type == type)
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        var total = categoryGroups.Values.Sum();

        foreach (var category in categories)
        {
            decimal amount = 0;
            categoryGroups.TryGetValue(category.Name, out amount);
            var stats = new CategoryStats
            {
                CategoryName = category.Name,
                Icon = category.Icon,
                Color = category.Color,
                TotalAmount = amount,
                Percentage = total > 0 ? (double)(amount / total) * 100 : 0
            };
            target.Add(stats);
        }
    }
}
