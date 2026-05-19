using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using LocalLedger.ViewModels;

namespace LocalLedger.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        TitleBar.ExtendsContentIntoTitleBar = true;
        NavigationView.ItemInvoked += OnNavigationViewItemInvoked;
        LedgerNavItem.IsSelected = true;
        ContentFrame.Navigate(typeof(LedgerView));
    }

    private void OnNavigationViewItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.IsSettingsInvoked)
        {
            ContentFrame.Navigate(typeof(SettingsView));
            return;
        }

        if (e.InvokedItemContainer is NavigationViewItem item && item.Tag is string tagStr)
        {
            var pageType = tagStr switch
            {
                "LedgerView" => typeof(LedgerView),
                "StatisticsView" => typeof(StatisticsView),
                "CategoryView" => typeof(CategoryView),
                _ => null
            };

            if (pageType != null)
            {
                ContentFrame.Navigate(pageType);
            }
        }
    }
}
