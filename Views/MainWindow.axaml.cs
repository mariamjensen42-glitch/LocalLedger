using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using LocalLedger.ViewModels;

namespace LocalLedger.Views;

public partial class MainWindow : FAAppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        NavigationView.SelectionChanged += OnNavigationViewSelectionChanged;
        LedgerNavItem.IsSelected = true;
        ContentFrame.Navigate(typeof(LedgerView));
    }

    private void OnNavigationViewSelectionChanged(object? sender, FANavigationViewSelectionChangedEventArgs e)
    {
        if (sender is not FANavigationView nv || nv.SelectedItem is not FANavigationViewItem item)
            return;

        if (item == nv.SettingsItem)
        {
            ContentFrame.Navigate(typeof(SettingsView));
            return;
        }

        if (item.Tag is string tagStr)
        {
            var pageType = tagStr switch
            {
                "LedgerView" => typeof(LedgerView),
                "StatisticsView" => typeof(StatisticsView),
                "CategoryView" => typeof(CategoryView),
                "BudgetView" => typeof(BudgetView),
                _ => null
            };

            if (pageType != null)
            {
                ContentFrame.Navigate(pageType);
            }
        }
    }
}