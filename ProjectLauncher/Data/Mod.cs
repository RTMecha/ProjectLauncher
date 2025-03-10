using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Data
{
    /// <summary>
    /// Represents a mod that can be installed to PA.
    /// </summary>
    public class Mod
    {
        /// <summary>
        /// Name of the mod.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the mod.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the mod that is saved to the instance settings.
        /// </summary>
        public string RegistryName { get; set; }

        /// <summary>
        /// Link to the mod's repository.
        /// </summary>
        public string RepositoryURL { get; set; }

        /// <summary>
        /// Link to the mod's versions file.
        /// </summary>
        public string VersionsURL { get; set; }

        /// <summary>
        /// Link to the mod's changelogs file.
        /// </summary>
        public string ChangelogURL { get; set; }

        /// <summary>
        /// Version of Project Arrhythmia the mod has to be for.
        /// </summary>
        public ArrhythmiaType RequiredVersion { get; set; } = ArrhythmiaType.Legacy;

        /// <summary>
        /// Assets that are downloaded from the mods' releases.
        /// </summary>
        public List<ModAsset> assets = new List<ModAsset>();
    }
}
