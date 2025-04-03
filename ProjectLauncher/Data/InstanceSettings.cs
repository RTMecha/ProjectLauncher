using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Data
{
    public class InstanceSettings
    {
        public InstanceSettings(string path)
        {
            Path = path + "/settings/launcher_settings.lss";
        }

        public async Task LoadSettings()
        {
            if (!File.Exists(Path))
                return;

            var json = await File.ReadAllTextAsync(Path);
            var jn = JSON.Parse(json);

            if (jn["auto_update"] != null)
                AutoUpdate = jn["auto_update"].AsBool;
            
            if (jn["better_legacy"] != null)
                BetterLegacy = jn["better_legacy"].AsBool;

            if (jn["editor_on_startup"] != null)
                EditorOnStartup = jn["editor_on_startup"].AsBool;

            if (jn["unity_explorer"] != null)
                UnityExplorer = jn["unity_explorer"].AsBool;

            if (!string.IsNullOrEmpty(jn["version"]))
                CurrentVersion = jn["version"];
        }

        public async void SaveSettings()
        {
            var jn = JSON.Parse("{}");

            if (AutoUpdate)
                jn["auto_update"] = AutoUpdate;

            if (!BetterLegacy)
                jn["better_legacy"] = BetterLegacy;
            if (EditorOnStartup)
                jn["editor_on_startup"] = EditorOnStartup;
            if (UnityExplorer)
                jn["unity_explorer"] = UnityExplorer;

            jn["version"] = CurrentVersion;

            await File.WriteAllTextAsync(Path, jn.ToString(3));
        }

        public string Path { get; set; } = string.Empty;

        public bool AutoUpdate { get; set; } = false;
        public bool BetterLegacy { get; set; } = true;
        public bool EditorOnStartup { get; set; } = false;
        public bool UnityExplorer { get; set; } = false;
        public string CurrentVersion { get; set; } = string.Empty;
    }

}
