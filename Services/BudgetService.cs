using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using LocalLedger.Models;

namespace LocalLedger.Services;

public class BudgetService
{
    private static readonly Lazy<BudgetService> _instance = new(() => new BudgetService());
    public static BudgetService Instance => _instance.Value;

    public Budget CurrentBudget { get; private set; } = new();

    private readonly string _budgetPath;

    public event EventHandler? BudgetChanged;

    private BudgetService()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LocalLedger");
        Directory.CreateDirectory(appDataPath);
        _budgetPath = Path.Combine(appDataPath, "budget.json");
        LoadBudget();
    }

    private void LoadBudget()
    {
        if (File.Exists(_budgetPath))
        {
            try
            {
                var json = File.ReadAllText(_budgetPath);
                CurrentBudget = JsonSerializer.Deserialize<Budget>(json) ?? new Budget();
            }
            catch
            {
                CurrentBudget = new Budget();
            }
        }
        else
        {
            CurrentBudget = new Budget();
        }
    }

    public void SaveBudget()
    {
        try
        {
            var json = JsonSerializer.Serialize(CurrentBudget, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_budgetPath, json);
            BudgetChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
        }
    }

    public void UpdateBudget(decimal monthlyLimit, bool isEnabled, int warningThreshold)
    {
        CurrentBudget.MonthlyLimit = monthlyLimit;
        CurrentBudget.IsEnabled = isEnabled;
        CurrentBudget.WarningThreshold = warningThreshold;
        SaveBudget();
    }

    public decimal GetCurrentMonthExpense()
    {
        var now = DateTime.Today;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        return DataService.Instance.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date >= startOfMonth)
            .Sum(t => t.Amount);
    }

    public bool IsOverBudget()
    {
        if (!CurrentBudget.IsEnabled || CurrentBudget.MonthlyLimit <= 0)
            return false;

        return GetCurrentMonthExpense() > CurrentBudget.MonthlyLimit;
    }

    public bool IsNearBudget()
    {
        if (!CurrentBudget.IsEnabled || CurrentBudget.MonthlyLimit <= 0)
            return false;

        var expense = GetCurrentMonthExpense();
        var percentage = expense / CurrentBudget.MonthlyLimit * 100;
        return percentage >= CurrentBudget.WarningThreshold && !IsOverBudget();
    }

    public double GetBudgetPercentage()
    {
        if (!CurrentBudget.IsEnabled || CurrentBudget.MonthlyLimit <= 0)
            return 0;

        var expense = GetCurrentMonthExpense();
        return (double)(expense / CurrentBudget.MonthlyLimit * 100);
    }
}
