using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProjectLauncher.Views;

public partial class ModSettingsWindow : Window
{
    public ModSettingsWindow()
    {
        InitializeComponent();
        CanResize = false;
    }
}