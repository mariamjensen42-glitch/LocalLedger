using System.Collections.ObjectModel;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LocalLedger.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _selectedThemeIndex;

    public ObservableCollection<string> Themes { get; } = new()
    {
        "Dark",
        "Light",
        "System"
    };

    public SettingsViewModel()
    {
        SelectedThemeIndex = GetCurrentThemeIndex();
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
}
