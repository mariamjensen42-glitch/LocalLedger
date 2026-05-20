using Avalonia.Controls;
using LocalLedger.ViewModels;

namespace LocalLedger.Views;

public partial class CategoryView : UserControl
{
    public CategoryView()
    {
        InitializeComponent();
        DataContext = new CategoryViewModel();
    }
}
