using ProjectLauncher.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Managers
{
    public class ModManager
    {
        public static string BepInExURL => "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip";

        public Mod betterLegacy = new Mod()
        {
            RepositoryURL = "https://github.com/RTMecha/BetterLegacy",
            ChangelogURL = "https://github.com/RTMecha/BetterLegacy/raw/refs/heads/master/updates.lss",
            VersionsURL = "https://github.com/RTMecha/BetterLegacy/raw/refs/heads/master/versions.lss",
            assets = new List<ModAsset>()
            {
                new ModAsset()
                {
                    File = "Beatmaps.zip",
                    Optional = true, // optional because demo files are planned to be included.
                    PackageType = ModPackageType.ZIP,
                }, // Beatmaps
                new ModAsset()
                {
                    File = "BetterLegacy.zip",
                    Optional = false,
                    PackageType = ModPackageType.ZIP,
                }, // BetterLegacy
                new ModAsset()
                {
                    File = "steam_api64.dll",
                    Optional = false,
                    PackageType = ModPackageType.DLL,
                    InstallLocation = "Project Arrhythmia_Data",
                } // Updated Steam API
            }
        };

        public Mod editorOnStartup = new Mod()
        {
            RepositoryURL = "https://github.com/enchart/EditorOnStartup",
            assets = new List<ModAsset>()
            {
                new ModAsset()
                {
                    File = "EditorOnStartup_1.0.0.dll",
                    Optional = false,
                    PackageType = ModPackageType.DLL,
                } // EditorOnStartup.dll
            }
        };

        public Mod unityExplorer = new Mod()
        {
            RepositoryURL = "https://github.com/sinai-dev/UnityExplorer",
            assets = new List<ModAsset>()
            {
                new ModAsset()
                {
                    File = "UnityExplorer.BepInEx5.Mono.zip",
                    Optional = false,
                    PackageType = ModPackageType.ZIP,
                    InstallLocation = "BepInEx",
                } // UnityExplorer.BepInEx5.Mono.zip
            },
        };

        // testing alpha stuff
        public Mod paMultiplayer = new Mod()
        {
            RepositoryURL = "https://github.com/Aiden-ytarame/PAMultiplayer",
            RequiredVersion = ArrhythmiaType.Modern,
            assets = new List<ModAsset>()
            {
                new ModAsset()
                {
                    File = "PAM.zip",
                    Optional = false,
                    PackageType = ModPackageType.ZIP,
                } // PAM.zip
            }
        };
    }
}
