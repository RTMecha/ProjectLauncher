using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProjectLauncher.ViewModels;

namespace ProjectLauncher.Views;

public partial class WarningView : UserControl
{
    public WarningView()
    {
        InitializeComponent();

        ConfirmButton.Click += ConfirmButtonClick;
        CancelButton.Click += CancelButtonClick;
    }

    void ConfirmButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is WarningViewModel warning)
            warning.Confirm?.Invoke();
    }

    void CancelButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e) => WarningWindow.Instance?.Close();
}