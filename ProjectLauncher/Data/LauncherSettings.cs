using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Data
{
    public class LauncherSettings
    {
        public double Hue { get; set; }
        public double Saturation { get; set; }
        public double Value { get; set; }
        public bool Rounded { get; set; } = true;
        public double Roundness { get; set; }

        public bool ShowSnapshots { get; set; } = true; // true for now since snapshots aren't done yet
    }
}
