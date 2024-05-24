using Avalonia.Controls;

namespace ProjectLauncher.Updater.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            CanResize = false;
        }
    }
}
