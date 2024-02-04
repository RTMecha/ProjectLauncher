using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ProjectLauncher.Functions
{
    public class ProjectArrhythmia
    {
        /// <summary>
        /// Use for verifying specific mod versions you have in your plugins folder.
        /// </summary>
        /// <param name="path">Full path to the dll.</param>
        /// <returns>Parsed version number directly from the dll.</returns>
        public string GetModVersion(string path)
        {
            var assembly = Assembly.LoadFrom(path);
            var ver = assembly.GetName().Version?.ToString() ?? "";

            return ver.Contains('.') ? ver.Substring(0, ver.LastIndexOf('.')) : ver;
        }

        public string Path { get; set; }

        public string FolderPath => Path.Replace("/Project Arrhythmia.exe", "/");

        public string Name => System.IO.Path.GetFileName(Path.Replace("/Project Arrhythmia.exe", ""));

        public bool ApplicationExists => Path != null && Path.Contains("/Project Arrhythmia.exe") && RTFile.FileExists(Path);

        public List<string> Mods { get; set; } = new List<string>();

        Dictionary<string, string> localVersions;
        public Dictionary<string, string> LocalVersions
        {
            get
            {
                if (localVersions == null || localVersions.Count == 0)
                    localVersions = new Dictionary<string, string>
                    {
                        { "RTFunctions", "1.0.0" },
                        { "EditorManagement", "1.0.0" },
                        { "EventsCore", "1.0.0" },
                        { "CreativePlayers", "1.0.0" },
                        { "ObjectModifiers", "1.0.0" },
                        { "ArcadiaCustoms", "1.0.0" },
                        { "PageCreator", "1.0.0" },
                        { "ExampleCompanion", "1.0.0" },
                    };
                return localVersions;
            }
            set
            {
                localVersions = value;
            }
        }

        public async void SaveSettings()
        {
            string str = "";

            if (MainWindow.Instance == null || MainWindow.Instance.InstanceCheckedAll == null)
                return;

            str += "All" + Environment.NewLine + MainWindow.Instance.InstanceCheckedAll.IsChecked.ToString() + Environment.NewLine;
            str += "RTFunctions" + Environment.NewLine + MainWindow.Instance.InstanceRTFunctionsEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "EditorManagement" + Environment.NewLine + MainWindow.Instance.InstanceEditorManagementEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "EventsCore" + Environment.NewLine + MainWindow.Instance.InstanceEventsCoreEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "CreativePlayers" + Environment.NewLine + MainWindow.Instance.InstanceCreativePlayersEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ObjectModifiers" + Environment.NewLine + MainWindow.Instance.InstanceObjectModifiersEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ArcadiaCustoms" + Environment.NewLine + MainWindow.Instance.InstanceArcadiaCustomsEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "PageCreator" + Environment.NewLine + MainWindow.Instance.InstancePageCreatorEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ExampleCompanion" + Environment.NewLine + MainWindow.Instance.InstanceExampleCompanionEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ConfigurationManager" + Environment.NewLine + MainWindow.Instance.InstanceConfigurationManagerEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "UnityExplorer" + Environment.NewLine + MainWindow.Instance.InstanceUnityExplorerEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "EditorOnStartup" + Environment.NewLine + MainWindow.Instance.InstanceEditorOnStartupEnabled.IsChecked.ToString() + Environment.NewLine;

            if (RTFile.DirectoryExists(FolderPath + "settings"))
                Directory.CreateDirectory(FolderPath + "settings");

            await File.WriteAllTextAsync(FolderPath + "settings/mod_settings.lss", str);
        }

        public async void LoadSettings()
        {
            if (MainWindow.Instance != null)
            {
                if (RTFile.FileExists(FolderPath + "settings/mod_settings.lss"))
                {
                    var settings = await File.ReadAllTextAsync(FolderPath + "settings/mod_settings.lss");

                    if (!string.IsNullOrEmpty(settings))
                    {
                        var list = settings.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < list.Length; i++)
                        {
                            switch (list[i])
                            {
                                case "All":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceCheckedAll.IsChecked = value;
                                        break;
                                    }
                                case "RTFunctions":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceRTFunctionsEnabled.IsChecked = value;
                                        break;
                                    }
                                case "EditorManagement":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceEditorManagementEnabled.IsChecked = value;
                                        break;
                                    }
                                case "EventsCore":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceEventsCoreEnabled.IsChecked = value;
                                        break;
                                    }
                                case "CreativePlayers":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceCreativePlayersEnabled.IsChecked = value;
                                        break;
                                    }
                                case "ObjectModifiers":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceObjectModifiersEnabled.IsChecked = value;
                                        break;
                                    }
                                case "ArcadiaCustoms":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceArcadiaCustomsEnabled.IsChecked = value;
                                        break;
                                    }
                                case "PageCreator":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstancePageCreatorEnabled.IsChecked = value;
                                        break;
                                    }
                                case "ExampleCompanion":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceExampleCompanionEnabled.IsChecked = value;
                                        break;
                                    }
                                case "ConfigurationManager":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceConfigurationManagerEnabled.IsChecked = value;
                                        break;
                                    }
                                case "UnityExplorer":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceUnityExplorerEnabled.IsChecked = value;
                                        break;
                                    }
                                case "EditorOnStartup":
                                    {
                                        if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                            MainWindow.Instance.InstanceEditorOnStartupEnabled.IsChecked = value;
                                        break;
                                    }
                            }
                        }
                    }
                }
                else
                {
                    MainWindow.Instance.InstanceCheckedAll.IsChecked = false;
                }

                await LoadVersions();
            }

        }

        public async Task LoadVersions()
        {
            if (MainWindow.Instance == null)
                return;

            if (RTFile.FileExists(FolderPath + "settings/versions.lss"))
            {
                var localVersions = await File.ReadAllTextAsync(FolderPath + "settings/versions.lss");

                string[]? onlineVersions = null;

                //using (var client = new WebClient())
                //{
                //    var data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/mod_info.lss");

                //    if (!string.IsNullOrEmpty(data))
                //    {
                //        onlineVersions = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                //    }
                //}

                var http = new HttpClient();
                var data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/mod_info.lss");

                if (!string.IsNullOrEmpty(data))
                {
                    onlineVersions = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                }

                http.Dispose();

                if (!string.IsNullOrEmpty(localVersions))
                {
                    var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                    if (list.Length > 0 && onlineVersions != null && onlineVersions.Length > 0)
                    {
                        for (int i = 0; i < list.Length; i++)
                        {
                            if (LocalVersions.ContainsKey(list[i]) && list.Length > i + 1)
                            {
                                LocalVersions[list[i]] = list[i + 1];
                            }
                        }

                        MainWindow.Instance.InstanceRTFunctionsEnabled.Content = $"{list[0]} - Installed: {list[1]}" + (list[1] == onlineVersions[1] ? "" : $" | Update Available: {onlineVersions[1]}");
                        MainWindow.Instance.InstanceEditorManagementEnabled.Content = $"{list[2]} - Installed: {list[3]}" + (list[3] == onlineVersions[3] ? "" : $" | Update Available: {onlineVersions[3]}");
                        MainWindow.Instance.InstanceEventsCoreEnabled.Content = $"{list[4]} - Installed: {list[5]}" + (list[5] == onlineVersions[5] ? "" : $" | Update Available: {onlineVersions[5]}");
                        MainWindow.Instance.InstanceCreativePlayersEnabled.Content = $"{list[6]} - Installed: {list[7]}" + (list[7] == onlineVersions[7] ? "" : $" | Update Available: {onlineVersions[7]}");
                        MainWindow.Instance.InstanceObjectModifiersEnabled.Content = $"{list[8]} - Installed: {list[9]}" + (list[9] == onlineVersions[9] ? "" : $" | Update Available: {onlineVersions[9]}");
                        MainWindow.Instance.InstanceArcadiaCustomsEnabled.Content = $"{list[10]} - Installed: {list[11]}" + (list[11] == onlineVersions[11] ? "" : $" | Update Available: {onlineVersions[11]}");
                        MainWindow.Instance.InstancePageCreatorEnabled.Content = $"{list[12]} - Installed: {list[13]}" + (list[13] == onlineVersions[13] ? "" : $" | Update Available: {onlineVersions[13]}");
                        MainWindow.Instance.InstanceExampleCompanionEnabled.Content = $"{list[14]} - Installed: {list[15]}" + (list[15] == onlineVersions[15] ? "" : $" | Update Available: {onlineVersions[15]}");
                    }
                }
            }
            else
            {
                List<string>? onlineVersions = null;

                //using (var client = new WebClient())
                //{
                //    var data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/mod_info.lss");

                //    if (!string.IsNullOrEmpty(data))
                //    {
                //        onlineVersions = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                //    }
                //}

                var http = new HttpClient();
                var data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/mod_info.lss");

                if (!string.IsNullOrEmpty(data))
                {
                    onlineVersions = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                http.Dispose();

                for (int i = 0; i < LocalVersions.Count; i++)
                {
                    var key = LocalVersions.ElementAt(i).Key;
                    if (onlineVersions != null && RTFile.FileExists($"{FolderPath}{ModUpdater.BepInExPlugins}/{key}.dll"))
                    {
                        var version = GetModVersion($"{FolderPath}{ModUpdater.BepInExPlugins}/{key}.dll");

                        int num = onlineVersions.FindIndex(x => x == key) + 1;

                        LocalVersions[key] = version;
                        MainWindow.Instance.GetModInstanceToggle(i).Content = $"{key} - Installed: {version}" + (onlineVersions.Count > num && version != onlineVersions[num] ? $" | Update Available: {onlineVersions[num]}" : "");
                    }
                }
            }
        }

        /// <summary>
        /// An instance of Project Arrhythmia. Path MUST contain the exe.
        /// </summary>
        /// <param name="path"></param>
        public ProjectArrhythmia(string path)
        {
            Path = path;

            if (RTFile.DirectoryExists(FolderPath + "BepInEx/plugins"))
            {
                var files = Directory.GetFiles(FolderPath + "BepInEx/plugins", "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    Mods.Add(file);
                }
            }
        }
    }
}
