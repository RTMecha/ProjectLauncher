using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Data
{
    /// <summary>
    /// Represents individual mod settings.
    /// </summary>
    public class ModSettings
    {
        /// <summary>
        /// The currently selected version of the mod for the instance.
        /// </summary>
        public string CurrentVersion { get; set; }

        /// <summary>
        /// If the mod is enabled for the instance.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// If the mod will be automatically updated whenever there is an update.
        /// </summary>
        public bool AutoUpdate { get; set; }
    }
}
