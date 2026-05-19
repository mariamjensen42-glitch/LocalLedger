using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using LocalLedger.ViewModels;
using LocalLedger.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LocalLedger;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public static void SetTheme(string theme)
    {
        if (Current != null)
        {
            Current.RequestedThemeVariant = theme switch
            {
                "Dark" => ThemeVariant.Dark,
                "Light" => ThemeVariant.Light,
                _ => ThemeVariant.Default
            };
        }
    }

    public static void ToggleTheme()
    {
        if (Current != null)
        {
            var current = Current.ActualThemeVariant;
            Current.RequestedThemeVariant = current == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();
    }
}
