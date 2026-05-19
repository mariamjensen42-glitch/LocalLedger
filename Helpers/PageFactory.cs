using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LocalLedger.ViewModels;
using LocalLedger.Views;

namespace LocalLedger.Helpers;

public class PageFactory : INavigationPageFactory
{
    public Control? GetPage(Type sourcePageType)
    {
        Control? page = null;

        if (sourcePageType == typeof(LedgerView))
            page = new LedgerView();
        else if (sourcePageType == typeof(StatisticsView))
            page = new StatisticsView();
        else if (sourcePageType == typeof(CategoryView))
            page = new CategoryView();
        else if (sourcePageType == typeof(SettingsView))
            page = new SettingsView();

        if (page != null)
        {
            page.DataContext = sourcePageType switch
            {
                Type t when t == typeof(LedgerView) => new LedgerViewModel(),
                Type t when t == typeof(StatisticsView) => new StatisticsViewModel(),
                Type t when t == typeof(CategoryView) => new CategoryViewModel(),
                Type t when t == typeof(SettingsView) => new SettingsViewModel(),
                _ => null
            };
        }

        return page;
    }

    public Control? GetPageFromObject(object target)
    {
        if (target is LedgerView) return new LedgerView { DataContext = new LedgerViewModel() };
        if (target is StatisticsView) return new StatisticsView { DataContext = new StatisticsViewModel() };
        if (target is CategoryView) return new CategoryView { DataContext = new CategoryViewModel() };
        if (target is SettingsView) return new SettingsView { DataContext = new SettingsViewModel() };
        return null;
    }
}
