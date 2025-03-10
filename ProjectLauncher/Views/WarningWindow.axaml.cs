using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProjectLauncher.Views;

public partial class WarningWindow : Window
{
    public static WarningWindow Instance { get; set; }

    public WarningWindow()
    {
        InitializeComponent();
        CanResize = false;
        Instance = this;
    }
}