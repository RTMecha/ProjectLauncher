using ProjectLauncher.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Data
{
    /// <summary>
    /// Represents an instance of Project Arrhythmia.
    /// </summary>
    public class ProjectArrhythmia
    {
        /// <summary>
        /// Path to the folder.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Version of the instance.
        /// </summary>
        public Version Version { get; set; } = new Version("20.4.4");

        /// <summary>
        /// Instance settings.
        /// </summary>
        public InstanceSettings Settings { get; set; }

        public override string ToString() => System.IO.Path.GetFileName(Path);
    }
}
