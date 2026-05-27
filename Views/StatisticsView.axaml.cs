using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LocalLedger.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LocalLedger.Views;

public partial class StatisticsView : UserControl
{
    public StatisticsView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<StatisticsViewModel>();
    }

    private async void OnGenerateAdviceClick(object? sender, RoutedEventArgs e)
    {
        App.Log("按钮被点击");
        if (DataContext is StatisticsViewModel vm)
        {
            vm.FinancialAdvice = "正在生成建议...";
            try
            {
                vm.FinancialAdvice = await vm.AIService.GetFinancialAdvice();
            }
            catch (Exception ex)
            {
                App.Log($"错误: {ex.Message}");
                vm.FinancialAdvice = $"错误: {ex.Message}";
            }
        }
    }
}
