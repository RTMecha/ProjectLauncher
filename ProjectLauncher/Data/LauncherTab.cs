using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Data
{
    public class LauncherTab
    {
        public LauncherTab(Button button, StackPanel page)
        {
            PageButton = button;
            Page = page;
        }
        public Button PageButton { get; private set; }
        public StackPanel Page { get; private set; }
    }

}
