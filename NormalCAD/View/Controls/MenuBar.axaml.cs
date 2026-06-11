using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NormalCAD.View.Controls;

public partial class MenuBar : UserControl
{
    public MenuBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
}
