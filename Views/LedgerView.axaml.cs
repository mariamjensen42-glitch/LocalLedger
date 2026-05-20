using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LocalLedger.Models;
using LocalLedger.ViewModels;
using LocalLedger.Views;

namespace LocalLedger.Views;

public partial class LedgerView : UserControl
{
    private LedgerViewModel? _viewModel;
    private AddTransactionViewModel? _addViewModel;

    public LedgerView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
        DataContext = new LedgerViewModel();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is LedgerViewModel vm)
        {
            _viewModel = vm;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            SetupAddForm();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LedgerViewModel.ShowAddForm))
        {
            SetupAddForm();
        }
    }

    private void SetupAddForm()
    {
        if (_viewModel == null) return;
        if (!_viewModel.ShowAddForm) return;

        _addViewModel = new AddTransactionViewModel
        {
            IsExpense = _viewModel.IsAddingExpense
        };

        // 如果是编辑模式，加载现有交易
        if (_viewModel.EditingTransaction != null)
        {
            _addViewModel.LoadTransaction(_viewModel.EditingTransaction);
        }

        _addViewModel.OnClose = transaction =>
        {
            if (transaction != null)
            {
                _viewModel.AddOrUpdateTransaction(transaction);
            }
            else
            {
                _viewModel.ShowAddForm = false;
            }
        };

        AddForm.DataContext = _addViewModel;
    }
}
