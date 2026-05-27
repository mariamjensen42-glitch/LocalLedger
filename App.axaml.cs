using System;
using System.ClientModel;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using LocalLedger.Services;
using LocalLedger.ViewModels;
using LocalLedger.Views;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;

namespace LocalLedger;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LocalLedger", "app.log");

    public static void Log(string message)
    {
        try
        {
            var dir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
        }
        catch { }
    }

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

        // 注册 ViewModels
        services.AddSingleton<LedgerViewModel>();
        services.AddSingleton<StatisticsViewModel>();
        services.AddSingleton<CategoryViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // 注册 OpenAIChatClient (DeepSeek API 兼容 OpenAI 格式)
        services.AddSingleton<IChatClient>(provider =>
        {
            var apiKey = LoadApiKey();
            Log($"API Key 长度: {apiKey?.Length ?? 0}");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Log("警告: API Key 为空!");
            }
            
            // 配置 DeepSeek API 端点
            var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey!), new OpenAIClientOptions
            {
                Endpoint = new Uri("https://api.deepseek.com")
            });
            var client = openAiClient.GetChatClient("deepseek-chat").AsIChatClient();
            Log("IChatClient 创建成功，指向 api.deepseek.com");
            return client;
        });

        services.AddSingleton<AIService>();
    }

    private static string LoadApiKey()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var path = Path.Combine(appData, "LocalLedger", "api_key.txt");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }
}
