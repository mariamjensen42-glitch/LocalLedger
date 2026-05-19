using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLedger.Models;
using LocalLedger.Services;

namespace LocalLedger.ViewModels;

public partial class LedgerViewModel : ViewModelBase
{
    public ObservableCollection<Transaction> Transactions => DataService.Instance.Transactions;

    public decimal TotalIncome => Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
    public decimal TotalExpense => Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
    public decimal Balance => TotalIncome - TotalExpense;

    [ObservableProperty]
    private bool _showAddForm;

    [ObservableProperty]
    private bool _isAddingExpense = true;

    [RelayCommand]
    private void ShowAddExpense()
    {
        IsAddingExpense = true;
        ShowAddForm = true;
    }

    [RelayCommand]
    private void ShowAddIncome()
    {
        IsAddingExpense = false;
        ShowAddForm = true;
    }

    [RelayCommand]
    private void CloseAddForm()
    {
        ShowAddForm = false;
    }

    public void AddTransaction(Transaction transaction)
    {
        DataService.Instance.AddTransaction(transaction);
        ShowAddForm = false;
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(TotalExpense));
        OnPropertyChanged(nameof(Balance));
    }
}
