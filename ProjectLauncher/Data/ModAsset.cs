using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Data
{
    /// <summary>
    /// Represents a mod's file.
    /// </summary>
    public class ModAsset
    {
        /// <summary>
        /// Name of the file.
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// How the file is handled by the launcher.
        /// </summary>
        public ModPackageType PackageType { get; set; }

        /// <summary>
        /// If the file is not required for the mod to work.
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Where the file should be installed to. if <see cref="PackageType"/> is <see cref="ModPackageType.ZIP"/>, then the contents will be extracted here.
        /// </summary>
        public string InstallLocation { get; set; } = DEFAULT_INSTALL_LOCATION;

        /// <summary>
        /// Where mods are installed to.
        /// </summary>
        public const string DEFAULT_INSTALL_LOCATION = "BepInEx/plugins";
    }
}
