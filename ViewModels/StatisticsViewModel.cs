using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLedger.Models;
using LocalLedger.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LocalLedger.ViewModels;

public enum StatsTimeFilter
{
    ThisWeek,
    ThisMonth,
    ThisYear,
    All
}

public class ChartDataPoint : ObservableObject
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class PieChartItem : ObservableObject
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Color { get; set; } = "#3B82F6";
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
    public AIService AIService { get; }

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

    public ObservableCollection<PieChartItem> ExpensePieChartData { get; } = new();
    public ObservableCollection<PieChartItem> IncomePieChartData { get; } = new();
    public ObservableCollection<ChartDataPoint> DailyExpenseTrend { get; } = new();

    [ObservableProperty]
    private string _financialAdvice = string.Empty;

    [ObservableProperty]
    private bool _isGeneratingAdvice;

    public StatisticsViewModel(AIService aiService)
    {
        AIService = aiService;
        RefreshStats();
        DataService.Instance.DataChanged += OnDataServiceDataChanged;
    }

    public StatisticsViewModel() : this(App.Services.GetRequiredService<AIService>())
    {
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

        // 更新饼图数据
        UpdatePieChartData(ExpensePieChartData, transactions, TransactionType.Expense, DataService.Instance.ExpenseCategories);
        UpdatePieChartData(IncomePieChartData, transactions, TransactionType.Income, DataService.Instance.IncomeCategories);

        // 更新趋势图数据
        UpdateDailyTrend(DailyExpenseTrend, transactions);
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

    private void UpdatePieChartData(
        ObservableCollection<PieChartItem> target,
        Transaction[] transactions,
        TransactionType type,
        ObservableCollection<Category> categories)
    {
        target.Clear();

        var categoryGroups = transactions
            .Where(t => t.Type == type)
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        foreach (var category in categories)
        {
            decimal amount = 0;
            categoryGroups.TryGetValue(category.Name, out amount);
            if (amount > 0)
            {
                target.Add(new PieChartItem
                {
                    CategoryName = category.Name,
                    Value = amount,
                    Color = category.Color
                });
            }
        }
    }

    private void UpdateDailyTrend(ObservableCollection<ChartDataPoint> target, Transaction[] transactions)
    {
        target.Clear();

        var now = DateTime.Today;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var dailyExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date >= startOfMonth)
            .GroupBy(t => t.Date)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        for (var date = startOfMonth; date <= now; date = date.AddDays(1))
        {
            decimal amount = 0;
            dailyExpenses.TryGetValue(date, out amount);
            target.Add(new ChartDataPoint
            {
                Label = date.ToString("MM/dd"),
                Value = amount
            });
        }
    }

    [RelayCommand]
    private async Task GenerateFinancialAdvice()
    {
        App.Log("GenerateFinancialAdvice 被调用");
        
        IsGeneratingAdvice = true;
        try
        {
            App.Log("开始调用 AI 服务...");
            FinancialAdvice = await AIService.GetFinancialAdvice();
            App.Log($"收到响应，长度: {FinancialAdvice?.Length ?? 0}");
        }
        catch (Exception ex)
        {
            App.Log($"异常: {ex}");
            FinancialAdvice = $"获取财务建议失败: {ex.Message}";
        }
        finally
        {
            IsGeneratingAdvice = false;
        }
    }
}
