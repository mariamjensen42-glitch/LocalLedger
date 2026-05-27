using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLedger.Models;
using LocalLedger.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LocalLedger.ViewModels;

public partial class AddTransactionViewModel : ViewModelBase
{
    private readonly AIService _aiService;

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

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isAnalyzing;

    public ObservableCollection<string> ExpenseCategories { get; } = new()
    {
        "餐饮", "交通", "购物", "娱乐", "居住", "医疗", "其他"
    };

    public ObservableCollection<string> IncomeCategories { get; } = new()
    {
        "工资", "奖金", "投资收益", "其他"
    };

    public ObservableCollection<string> CurrentCategories => IsExpense ? ExpenseCategories : IncomeCategories;

    public string Title => IsEditing ? (IsExpense ? "编辑支出" : "编辑收入") : (IsExpense ? "记支出" : "记收入");

    public Action<Transaction?>? OnClose { get; set; }

    public AddTransactionViewModel(AIService aiService)
    {
        _aiService = aiService;
    }

    public AddTransactionViewModel() : this(App.Services.GetRequiredService<AIService>())
    {
    }

    public void LoadTransaction(Transaction? transaction)
    {
        if (transaction == null)
        {
            ResetForm();
            return;
        }

        IsEditing = true;
        Amount = transaction.Amount;
        AmountText = transaction.Amount.ToString(CultureInfo.InvariantCulture);
        IsExpense = transaction.Type == TransactionType.Expense;
        SelectedDate = transaction.Date;
        Note = transaction.Note;

        var categories = CurrentCategories;
        var index = categories.IndexOf(transaction.Category);
        SelectedCategoryIndex = index >= 0 ? index : 0;
    }

    public void ResetForm()
    {
        IsEditing = false;
        Amount = 0;
        AmountText = string.Empty;
        SelectedCategoryIndex = 0;
        SelectedDate = DateTime.Today;
        Note = string.Empty;
    }

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

    [RelayCommand]
    private async Task AnalyzeNote()
    {
        if (string.IsNullOrWhiteSpace(Note)) return;

        IsAnalyzing = true;
        try
        {
            var suggestedCategory = await _aiService.AnalyzeTransactionNote(Note);
            
            var categories = CurrentCategories;
            var index = categories.IndexOf(suggestedCategory);
            if (index >= 0)
            {
                SelectedCategoryIndex = index;
            }
        }
        catch
        {
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task GenerateNote()
    {
        if (Amount <= 0) return;

        IsAnalyzing = true;
        try
        {
            var category = CurrentCategories.Count > SelectedCategoryIndex
                ? CurrentCategories[SelectedCategoryIndex]
                : "其他";
            Note = await _aiService.GenerateNoteSuggestion(Amount, category);
        }
        catch
        {
        }
        finally
        {
            IsAnalyzing = false;
        }
    }
}
