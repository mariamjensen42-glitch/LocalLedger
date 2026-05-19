using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LocalLedger.Models;
using LocalLedger.Services;

namespace LocalLedger.ViewModels;

public partial class CategoryViewModel : ViewModelBase
{
    public ObservableCollection<Category> ExpenseCategories => DataService.Instance.ExpenseCategories;
    public ObservableCollection<Category> IncomeCategories => DataService.Instance.IncomeCategories;

    public decimal TotalExpense => ExpenseCategories.Sum(c => c.TotalAmount);
    public decimal TotalIncome => IncomeCategories.Sum(c => c.TotalAmount);
}
