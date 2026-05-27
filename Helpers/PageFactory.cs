using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LocalLedger.ViewModels;
using LocalLedger.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LocalLedger.Helpers;

public class PageFactory
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
                Type t when t == typeof(LedgerView) => App.Services.GetRequiredService<LedgerViewModel>(),
                Type t when t == typeof(StatisticsView) => App.Services.GetRequiredService<StatisticsViewModel>(),
                Type t when t == typeof(CategoryView) => App.Services.GetRequiredService<CategoryViewModel>(),
                Type t when t == typeof(SettingsView) => App.Services.GetRequiredService<SettingsViewModel>(),
                _ => null
            };
        }

        return page;
    }

    public Control? GetPageFromObject(object target)
    {
        if (target is LedgerView) return new LedgerView { DataContext = App.Services.GetRequiredService<LedgerViewModel>() };
        if (target is StatisticsView) return new StatisticsView { DataContext = App.Services.GetRequiredService<StatisticsViewModel>() };
        if (target is CategoryView) return new CategoryView { DataContext = App.Services.GetRequiredService<CategoryViewModel>() };
        if (target is SettingsView) return new SettingsView { DataContext = App.Services.GetRequiredService<SettingsViewModel>() };
        return null;
    }
}
