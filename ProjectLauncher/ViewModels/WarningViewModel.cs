using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.ViewModels
{
    public class WarningViewModel : ViewModelBase
    {
        public string WarningMessage { get; set; } = "Test";

        public Action Confirm { get; set; }
    }
}
