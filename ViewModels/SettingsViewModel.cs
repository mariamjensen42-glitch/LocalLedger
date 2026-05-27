using System;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalLedger.Models;
using LocalLedger.Services;

namespace LocalLedger.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _selectedThemeIndex;

    [ObservableProperty]
    private string _openAiApiKey = string.Empty;

    [ObservableProperty]
    private decimal _monthlyBudgetLimit;

    [ObservableProperty]
    private bool _budgetEnabled;

    [ObservableProperty]
    private int _budgetWarningThreshold;

    public ObservableCollection<string> Themes { get; } = new()
    {
        "Dark",
        "Light",
        "System"
    };

    public SettingsViewModel()
    {
        SelectedThemeIndex = GetCurrentThemeIndex();
        LoadApiKey();
        LoadBudgetSettings();
    }

    private void LoadBudgetSettings()
    {
        var budget = BudgetService.Instance.CurrentBudget;
        MonthlyBudgetLimit = budget.MonthlyLimit;
        BudgetEnabled = budget.IsEnabled;
        BudgetWarningThreshold = budget.WarningThreshold;
    }

    private void LoadApiKey()
    {
        var path = GetApiKeyPath();
        if (File.Exists(path))
        {
            OpenAiApiKey = File.ReadAllText(path);
        }
    }

    private string GetApiKeyPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "LocalLedger", "api_key.txt");
    }

    private int GetCurrentThemeIndex()
    {
        if (App.Current == null) return 0;
        var variant = App.Current.ActualThemeVariant;
        if (variant == ThemeVariant.Dark) return 0;
        if (variant == ThemeVariant.Light) return 1;
        return 2;
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        var theme = value switch
        {
            0 => "Dark",
            1 => "Light",
            _ => "System"
        };
        App.SetTheme(theme);
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        App.ToggleTheme();
    }

    [RelayCommand]
    private void SaveApiKey()
    {
        var path = GetApiKeyPath();
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(path, OpenAiApiKey);
    }

    [RelayCommand]
    private void SaveBudgetSettings()
    {
        BudgetService.Instance.UpdateBudget(MonthlyBudgetLimit, BudgetEnabled, BudgetWarningThreshold);
    }
}
