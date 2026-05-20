using Avalonia.Controls;
using LocalLedger.ViewModels;

namespace LocalLedger.Views;

public partial class StatisticsView : UserControl
{
    public StatisticsView()
    {
        InitializeComponent();
        DataContext = new StatisticsViewModel();
    }
}
