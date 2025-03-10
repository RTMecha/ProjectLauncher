using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Data
{
    /// <summary>
    /// Represents the era an instance of Project Arrhythmia comes from.
    /// </summary>
    public enum ArrhythmiaType
    {
        /// <summary>
        /// Legacy branch (20.4.4 / 4.1.16)
        /// </summary>
        Legacy,
        /// <summary>
        /// Default / alpha branch
        /// </summary>
        Modern,
    }
}
