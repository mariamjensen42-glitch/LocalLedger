using Avalonia.Controls;
using LocalLedger.ViewModels;

namespace LocalLedger.Views;

public partial class BudgetView : UserControl
{
    public BudgetView()
    {
        InitializeComponent();
        DataContext = new BudgetViewModel();
    }
}
