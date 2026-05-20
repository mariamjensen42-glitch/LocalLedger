using Avalonia.Controls;
using LocalLedger.ViewModels;

namespace LocalLedger.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }
}
