using System;
using System.Collections.ObjectModel;
using System.Linq;
using LocalLedger.Models;

namespace LocalLedger.Services;

public class DataService
{
    private static readonly Lazy<DataService> _instance = new(() => new DataService());
    public static DataService Instance => _instance.Value;

    public ObservableCollection<Transaction> Transactions { get; } = new();

    public ObservableCollection<Category> ExpenseCategories { get; } = new()
    {
        new Category { Name = "餐饮", Icon = "食", Color = "#EF4444", Type = CategoryType.Expense },
        new Category { Name = "交通", Icon = "行", Color = "#3B82F6", Type = CategoryType.Expense },
        new Category { Name = "购物", Icon = "购", Color = "#10B981", Type = CategoryType.Expense },
        new Category { Name = "娱乐", Icon = "娱", Color = "#8B5CF6", Type = CategoryType.Expense },
        new Category { Name = "居住", Icon = "住", Color = "#F59E0B", Type = CategoryType.Expense },
        new Category { Name = "医疗", Icon = "医", Color = "#EC4899", Type = CategoryType.Expense },
        new Category { Name = "其他", Icon = "其", Color = "#6B7280", Type = CategoryType.Expense }
    };

    public ObservableCollection<Category> IncomeCategories { get; } = new()
    {
        new Category { Name = "工资", Icon = "工", Color = "#10B981", Type = CategoryType.Income },
        new Category { Name = "奖金", Icon = "奖", Color = "#3B82F6", Type = CategoryType.Income },
        new Category { Name = "投资收益", Icon = "投", Color = "#8B5CF6", Type = CategoryType.Income },
        new Category { Name = "其他", Icon = "其", Color = "#6B7280", Type = CategoryType.Income }
    };

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Insert(0, transaction);
        UpdateCategoryStats();
    }

    public void UpdateCategoryStats()
    {
        foreach (var cat in ExpenseCategories)
        {
            cat.TransactionCount = Transactions.Count(t => t.Category == cat.Name && t.Type == TransactionType.Expense);
            cat.TotalAmount = Transactions.Where(t => t.Category == cat.Name && t.Type == TransactionType.Expense).Sum(t => t.Amount);
        }

        foreach (var cat in IncomeCategories)
        {
            cat.TransactionCount = Transactions.Count(t => t.Category == cat.Name && t.Type == TransactionType.Income);
            cat.TotalAmount = Transactions.Where(t => t.Category == cat.Name && t.Type == TransactionType.Income).Sum(t => t.Amount);
        }
    }
}
