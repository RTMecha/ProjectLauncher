using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Data
{
    /// <summary>
    /// Represents how a mod package is handled.
    /// </summary>
    public enum ModPackageType
    {
        /// <summary>
        /// Only needs to be written.
        /// </summary>
        DLL,
        /// <summary>
        /// Needs to be both written and extracted.
        /// </summary>
        ZIP,
    }
}
