using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;

namespace ProjectLauncher.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public string AboutProjectLauncherInfo =>
            $"Project Launcher is an application used to handle multiple\n" +
            $"instances of Project Arrhythmia.\n\n" +
            $"In order to use this, you must first own Project Arrhythmia\n" +
            $"on Steam.";
    }
}