using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;

namespace ProjectLauncher.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        private float RightPanelWidth => 100;


        public string AboutProjectLauncherInfo =>
            $"Project Launcher is an application used to handle multiple\n" +
            $"instances of Project Arrhythmia.";

        public int RelativeSource => 200;


        public static void PercentageConverter()
        {

        }


    }
}