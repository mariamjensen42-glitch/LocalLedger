using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using LocalLedger.Models;

namespace LocalLedger.Services;

public class DataService
{
    private static readonly Lazy<DataService> _instance = new(() => new DataService());
    public static DataService Instance => _instance.Value;

    public event EventHandler? DataChanged;

    private readonly string _dataPath;

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

    private DataService()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LocalLedger");
        Directory.CreateDirectory(appDataPath);
        _dataPath = Path.Combine(appDataPath, "transactions.json");
        LoadData();
    }

    private void LoadData()
    {
        if (File.Exists(_dataPath))
        {
            try
            {
                var json = File.ReadAllText(_dataPath);
                var savedTransactions = JsonSerializer.Deserialize<Transaction[]>(json);
                if (savedTransactions != null)
                {
                    foreach (var t in savedTransactions)
                    {
                        Transactions.Add(t);
                    }
                }
            }
            catch
            {
            }
        }
        UpdateCategoryStats();
    }

    private void SaveData()
    {
        try
        {
            var json = JsonSerializer.Serialize(Transactions, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_dataPath, json);
        }
        catch
        {
        }
    }

    private void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Insert(0, transaction);
        UpdateCategoryStats();
        SaveData();
        OnDataChanged();
    }

    public void UpdateTransaction(Transaction transaction)
    {
        var index = Transactions.IndexOf(Transactions.First(t => t.Id == transaction.Id));
        if (index >= 0)
        {
            Transactions[index] = transaction;
            UpdateCategoryStats();
            SaveData();
            OnDataChanged();
        }
    }

    public void DeleteTransaction(string id)
    {
        var transaction = Transactions.FirstOrDefault(t => t.Id == id);
        if (transaction != null)
        {
            Transactions.Remove(transaction);
            UpdateCategoryStats();
            SaveData();
            OnDataChanged();
        }
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
