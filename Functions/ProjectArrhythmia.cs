using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public bool ApplicationExists => Path != null && Path.Contains("/Project Arrhythmia.exe") && RTFile.FileExists(Path);

        public List<string> Mods { get; set; } = new List<string>();

        public void SaveSettings()
        {
            string str = "";

            if (MainWindow.Instance == null || MainWindow.Instance.CheckedAll == null)
                return;

            str += "All" + Environment.NewLine + MainWindow.Instance.CheckedAll.IsChecked.ToString() + Environment.NewLine;
            str += "RTFunctions" + Environment.NewLine + MainWindow.Instance.RTFunctionsEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "EditorManagement" + Environment.NewLine + MainWindow.Instance.EditorManagementEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "EventsCore" + Environment.NewLine + MainWindow.Instance.EventsCoreEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "CreativePlayers" + Environment.NewLine + MainWindow.Instance.CreativePlayersEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ObjectModifiers" + Environment.NewLine + MainWindow.Instance.ObjectModifiersEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ArcadiaCustoms" + Environment.NewLine + MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "PageCreator" + Environment.NewLine + MainWindow.Instance.PageCreatorEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ExampleCompanion" + Environment.NewLine + MainWindow.Instance.ExampleCompanionEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ConfigurationManager" + Environment.NewLine + MainWindow.Instance.ConfigurationManagerEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "UnityExplorer" + Environment.NewLine + MainWindow.Instance.UnityExplorerEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "EditorOnStartup" + Environment.NewLine + MainWindow.Instance.EditorOnStartupEnabled.IsChecked.ToString() + Environment.NewLine;

            RTFile.WriteToFile(FolderPath + "settings/mod_settings.lss", str);
        }

        public void LoadSettings()
        {
            if (MainWindow.Instance == null)
                return;

            if (RTFile.FileExists(FolderPath + "settings/mod_settings.lss"))
            {
                var settings = RTFile.ReadFromFile(FolderPath + "settings/mod_settings.lss");

                if (!string.IsNullOrEmpty(settings))
                {
                    var list = settings.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    for (int i = 0; i < list.Count; i++)
                    {
                        switch (list[i])
                        {
                            case "All":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.CheckedAll.IsChecked = value;
                                    break;
                                }
                            case "RTFunctions":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.RTFunctionsEnabled.IsChecked = value;
                                    break;
                                }
                            case "EditorManagement":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.EditorManagementEnabled.IsChecked = value;
                                    break;
                                }
                            case "EventsCore":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.EventsCoreEnabled.IsChecked = value;
                                    break;
                                }
                            case "CreativePlayers":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.CreativePlayersEnabled.IsChecked = value;
                                    break;
                                }
                            case "ObjectModifiers":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.ObjectModifiersEnabled.IsChecked = value;
                                    break;
                                }
                            case "ArcadiaCustoms":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked = value;
                                    break;
                                }
                            case "PageCreator":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.PageCreatorEnabled.IsChecked = value;
                                    break;
                                }
                            case "ExampleCompanion":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.ExampleCompanionEnabled.IsChecked = value;
                                    break;
                                }
                            case "ConfigurationManager":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.ConfigurationManagerEnabled.IsChecked = value;
                                    break;
                                }
                            case "UnityExplorer":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.UnityExplorerEnabled.IsChecked = value;
                                    break;
                                }
                            case "EditorOnStartup":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        MainWindow.Instance.EditorOnStartupEnabled.IsChecked = value;
                                    break;
                                }
                        }
                    }
                }
            }

            if (RTFile.FileExists(FolderPath + "settings/versions.lss"))
            {
                var localVersions = RTFile.ReadFromFile(FolderPath + "settings/versions.lss");

                List<string> onlineVersions = new List<string>();

                using (var client = new WebClient())
                {
                    var data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/mod_info.lss");

                    if (!string.IsNullOrEmpty(data))
                    {
                        onlineVersions = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(localVersions))
                {
                    var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    if (list.Count > 0 && onlineVersions.Count > 0)
                    {
                        MainWindow.Instance.RTFunctionsEnabled.Content = $"{list[0]} - Installed: {list[1]}" + (list[1] == onlineVersions[1] ? "" : $" | Update Available: {onlineVersions[1]}");
                        MainWindow.Instance.EditorManagementEnabled.Content = $"{list[2]} - Installed: {list[3]}" + (list[3] == onlineVersions[3] ? "" : $" | Update Available: {onlineVersions[3]}");
                        MainWindow.Instance.EventsCoreEnabled.Content = $"{list[4]} - Installed: {list[5]}" + (list[5] == onlineVersions[5] ? "" : $" | Update Available: {onlineVersions[5]}");
                        MainWindow.Instance.CreativePlayersEnabled.Content = $"{list[6]} - Installed: {list[7]}" + (list[7] == onlineVersions[7] ? "" : $" | Update Available: {onlineVersions[7]}");
                        MainWindow.Instance.ObjectModifiersEnabled.Content = $"{list[8]} - Installed: {list[9]}" + (list[9] == onlineVersions[9] ? "" : $" | Update Available: {onlineVersions[9]}");
                        MainWindow.Instance.ArcadiaCustomsEnabled.Content = $"{list[10]} - Installed: {list[11]}" + (list[11] == onlineVersions[11] ? "" : $" | Update Available: {onlineVersions[11]}");
                        MainWindow.Instance.PageCreatorEnabled.Content = $"{list[12]} - Installed: {list[13]}" + (list[13] == onlineVersions[13] ? "" : $" | Update Available: {onlineVersions[13]}");
                        MainWindow.Instance.ExampleCompanionEnabled.Content = $"{list[14]} - Installed: {list[15]}" + (list[15] == onlineVersions[15] ? "" : $" | Update Available: {onlineVersions[15]}");
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
