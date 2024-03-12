using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;

using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media.Animation;
using System.Security.Policy;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Windows.Controls;

namespace ProjectLauncher.Functions
{
    public static class ModUpdater
    {
        public static string BepInExPlugins => "BepInEx/plugins";
        public static string BepInExURL => "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip";
        public static string CurrentVersionsList => "https://raw.githubusercontent.com/RTMecha/RTFunctions/master/mod_info.lss";

        static Dictionary<string, string> onlineVersions;
        public static Dictionary<string, string> OnlineVersions
        {
            get
            {
                if (onlineVersions == null || onlineVersions.Count == 0)
                    onlineVersions = new Dictionary<string, string>
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
                return onlineVersions;
            }
            set => onlineVersions = value;
        }

        static Dictionary<string, string> localVersions;
        public static Dictionary<string, string> LocalVersions
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

        public async static void Update()
        {
            if (MainWindow.Instance == null || !RTFile.DirectoryExists(MainWindow.Instance.Path.Replace("/Project Arrhythmia.exe", "")))
            {
                if (MainWindow.Instance != null)
                    MainWindow.Instance.DebugLogger.Text = $"Path does not exist.";
                 
                return;
            }

            MainWindow.Instance.DebugLogger.Text = $"Updating mods, please wait...";

            await MainWindow.Instance.SaveSettings();

            var a = MainWindow.Instance.Path.Replace("Project Arrhythmia.exe", "");

            // Download BepInEx (Obviously will need BepInEx itself to run any mods)
            if (!Directory.Exists(a + "BepInEx"))
            {
                var bep = a + "BepInEx-5.4.21.zip";

                using var http = new HttpClient();

                var bytes = await http.GetByteArrayAsync(BepInExURL);

                await File.WriteAllBytesAsync(bep, bytes);

                RTFile.ZipUtil.UnZip(bep, a);
            }

            // Load Versions (For version comparison, so we're not re-downloading the mods every time we launch the game)
            {
                if (RTFile.FileExists(RTFile.ApplicationDirectory + "versions.lss"))
                {
                    var localVersions = await File.ReadAllTextAsync(RTFile.ApplicationDirectory + "versions.lss");

                    if (!string.IsNullOrEmpty(localVersions))
                    {
                        var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < list.Length; i++)
                        {
                            if (LocalVersions.ContainsKey(list[i]) && list.Length > i + 1)
                            {
                                LocalVersions[list[i]] = list[i + 1];
                            }
                        }
                    }
                }

                using var http = new HttpClient();
                var data = await http.GetStringAsync(CurrentVersionsList);

                if (!string.IsNullOrEmpty(data))
                {
                    var list = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < list.Length; i++)
                    {
                        if (OnlineVersions.ContainsKey(list[i]) && list.Length > i + 1)
                        {
                            MainWindow.Instance.DebugLogger.Text = $"Setting online version {list[i + 1]}";
                            OnlineVersions[list[i]] = list[i + 1];
                        }
                    }
                }
            }

            MainWindow.Instance.DebugLogger.Text = $"Checking dependants...";

            // Verify (Makes sure RTFunctions is on if any dependant mod is)
            {
                if (MainWindow.Instance.EditorManagementEnabled.IsChecked == true ||
                    MainWindow.Instance.EventsCoreEnabled.IsChecked == true ||
                    MainWindow.Instance.CreativePlayersEnabled.IsChecked == true ||
                    MainWindow.Instance.ObjectModifiersEnabled.IsChecked == true ||
                    MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == true ||
                    MainWindow.Instance.PageCreatorEnabled.IsChecked == true ||
                    MainWindow.Instance.ExampleCompanionEnabled.IsChecked == true)
                {
                    MainWindow.Instance.RTFunctionsEnabled.IsChecked = true;
                }
            }

            MainWindow.Instance.DebugLogger.Text = $"Verifying directory...";

            var b = a + BepInExPlugins;
            // In case the BepInEx/plugins folder doesn't exist
            if (!Directory.Exists(b))
                Directory.CreateDirectory(b);

            Action<string, CheckBox> downloadhandler = delegate (string mod, CheckBox checkBox)
            {
                if (checkBox != null && checkBox.IsChecked == true
                    && (!RTFile.FileExists($"{b}/{mod}.dll") && !RTFile.FileExists($"{b}/{mod}.disabled") ||
                    OnlineVersions[$"{mod}"] != LocalVersions[$"{mod}"]))
                {
                    if (RTFile.FileExists($"{b}/{mod}.disabled"))
                    {
                        File.Delete($"{b}/{mod}.disabled");
                    }

                    if (RTFile.FileExists($"{b}/{mod}.dll"))
                    {
                        File.Delete($"{b}/{mod}.dll");
                    }

                    DownloadFile($"https://github.com/RTMecha/{mod}/releases/latest/download/{mod}.zip", b, $"{mod}.zip");
                }

                if (!RTFile.FileExists($"{b}/{mod}.dll") || !RTFile.FileExists($"{b}/{mod}.disabled"))
                {
                    bool enabled = checkBox != null && checkBox.IsChecked == false;

                    RTFile.MoveFile(enabled ? $"{b}/{mod}.dll" : $"{b}/{mod}.disabled", !enabled ? $"{b}/{mod}.dll" : $"{b}/{mod}.disabled");
                }
            };

            downloadhandler?.Invoke("RTFunctions", MainWindow.Instance.RTFunctionsEnabled);
            downloadhandler?.Invoke("EditorManagement", MainWindow.Instance.EditorManagementEnabled);
            downloadhandler?.Invoke("EventsCore", MainWindow.Instance.EventsCoreEnabled);
            downloadhandler?.Invoke("CreativePlayers", MainWindow.Instance.CreativePlayersEnabled);
            downloadhandler?.Invoke("ObjectModifiers", MainWindow.Instance.ObjectModifiersEnabled);
            downloadhandler?.Invoke("ArcadiaCustoms", MainWindow.Instance.ArcadiaCustomsEnabled);
            downloadhandler?.Invoke("PageCreator", MainWindow.Instance.PageCreatorEnabled);
            downloadhandler?.Invoke("ExampleCompanion", MainWindow.Instance.ExampleCompanionEnabled);

            if (MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == true
                && !RTFile.DirectoryExists(b + "/ConfigurationManager"))
            {
                // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                var rep = b.Replace("/BepInEx/plugins", "");

                var rt = b + "/ConfigurationManager.zip";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/download/v18.2/BepInEx.ConfigurationManager.BepInEx5_v18.2.zip");

                await File.WriteAllBytesAsync(rt, bytes);

                RTFile.ZipUtil.UnZip(rt, rep);
            }
            else if (RTFile.DirectoryExists(b + "/ConfigurationManager") && MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == false)
            {
                try
                {
                    var files = Directory.GetFiles(b + "/ConfigurationManager");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    Directory.Delete(b + "/ConfigurationManager");
                }
                catch
                {

                }
            }

            if (MainWindow.Instance.UnityExplorerEnabled != null && MainWindow.Instance.UnityExplorerEnabled.IsChecked == true
                && !RTFile.FileExists(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll"))
            {
                // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                var rep = b.Replace("/plugins", "");

                var rt = b + "/UnityExplorer.zip";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip");

                await File.WriteAllBytesAsync(rt, bytes);

                RTFile.ZipUtil.UnZip(rt, rep);
            }
            else if (RTFile.FileExists(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll") && MainWindow.Instance.UnityExplorerEnabled != null && MainWindow.Instance.UnityExplorerEnabled.IsChecked == false)
            {
                try
                {
                    File.Delete(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll");
                }
                catch
                {

                }
            }

            if (MainWindow.Instance.EditorOnStartupEnabled != null && MainWindow.Instance.EditorOnStartupEnabled.IsChecked == true
                && !RTFile.FileExists(b + "/EditorOnStartup.dll"))
            {
                var rt = b + "/EditorOnStartup.dll";
                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/enchart/EditorOnStartup/releases/download/1.0.0/EditorOnStartup_1.0.0.dll");
                await File.WriteAllBytesAsync(rt, bytes);
            }
            else if (RTFile.FileExists(b + "/EditorOnStartup.dll") && MainWindow.Instance.EditorOnStartupEnabled != null && MainWindow.Instance.EditorOnStartupEnabled.IsChecked == false)
            {
                try
                {
                    File.Delete(b + "/EditorOnStartup.dll");
                }
                catch
                {

                }
            }

            if (!RTFile.DirectoryExists(a + "beatmaps/prefabtypes") || !RTFile.DirectoryExists(a + "beatmaps/shapes") || !RTFile.DirectoryExists(a + "beatmaps/menus"))
                DownloadFile("https://github.com/RTMecha/RTFunctions/releases/latest/download/Beatmaps.zip", a, "Beatmaps.zip");

            // Save Versions (For later use when the game is relaunched)
            {
                var str = "";

                for (int i = 0; i < OnlineVersions.Count; i++)
                {
                    str += OnlineVersions.ElementAt(i).Key + Environment.NewLine + OnlineVersions.ElementAt(i).Value + Environment.NewLine;
                }

                await File.WriteAllTextAsync(RTFile.ApplicationDirectory + "versions.lss", str);
            }

            MainWindow.Instance.DebugLogger.Text = $"Finished updating.";
        }

        public async static void Launch()
        {
            if (MainWindow.Instance == null || !RTFile.DirectoryExists(MainWindow.Instance.Path.Replace("/Project Arrhythmia.exe", "")))
            {
                if (MainWindow.Instance != null)
                    MainWindow.Instance.DebugLogger.Text = $"Path does not exist.";

                return;
            }

            MainWindow.Instance.DebugLogger.Text = $"Start opening the game, please wait...";

            await MainWindow.Instance.SaveSettings();

            var a = MainWindow.Instance.Path.Replace("Project Arrhythmia.exe", "");
            var b = a + BepInExPlugins;

            // Download steam_api.dll
            if (!RTFile.FileExists(a + "Project Arrhythmia_Data/Plugins/steam_api_updated.txt"))
            {
                using var http = new HttpClient();

                var bytes = await http.GetByteArrayAsync("https://github.com/RTMecha/RTFunctions/releases/latest/download/steam_api64.dll");

                await File.WriteAllBytesAsync(a + "Project Arrhythmia_Data/Plugins/steam_api64.dll", bytes);

                await File.WriteAllTextAsync(a + "Project Arrhythmia_Data/Plugins/steam_api_updated.txt", "Yes");
            }

            // Download BepInEx (Obviously will need BepInEx itself to run any mods)
            if (!Directory.Exists(a + "BepInEx"))
            {
                var bep = a + "BepInEx-5.4.21.zip";

                using var http = new HttpClient();

                var bytes = await http.GetByteArrayAsync(BepInExURL);

                await File.WriteAllBytesAsync(bep, bytes);

                RTFile.ZipUtil.UnZip(bep, a);
            }

            // Load Versions (For version comparison, so we're not re-downloading the mods every time we launch the game)
            {
                if (RTFile.FileExists(RTFile.ApplicationDirectory + "versions.lss"))
                {
                    var localVersions = await File.ReadAllTextAsync(RTFile.ApplicationDirectory + "versions.lss");

                    if (!string.IsNullOrEmpty(localVersions))
                    {
                        var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < list.Length; i++)
                        {
                            if (LocalVersions.ContainsKey(list[i]) && list.Length > i + 1)
                            {
                                LocalVersions[list[i]] = list[i + 1];
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < LocalVersions.Count; i++)
                    {
                        var key = LocalVersions.ElementAt(i).Key;
                        if (RTFile.FileExists($"{b}/{key}.dll"))
                        {
                            var version = RTFile.GetAssemblyVersion($"{b}/{key}.dll");

                            LocalVersions[key] = version;
                        }
                    }
                }
            }

            MainWindow.Instance.DebugLogger.Text = $"Checking dependants...";

            // Verify (Makes sure RTFunctions is on if any dependant mod is)
            {
                if (MainWindow.Instance.EditorManagementEnabled.IsChecked == true ||
                    MainWindow.Instance.EventsCoreEnabled.IsChecked == true ||
                    MainWindow.Instance.CreativePlayersEnabled.IsChecked == true ||
                    MainWindow.Instance.ObjectModifiersEnabled.IsChecked == true ||
                    MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == true ||
                    MainWindow.Instance.PageCreatorEnabled.IsChecked == true ||
                    MainWindow.Instance.ExampleCompanionEnabled.IsChecked == true)
                {
                    MainWindow.Instance.RTFunctionsEnabled.IsChecked = true;
                }
            }

            MainWindow.Instance.DebugLogger.Text = $"Verifying directory...";

            // In case the BepInEx/plugins folder doesn't exist
            if (!Directory.Exists(b))
                Directory.CreateDirectory(b);

            Action<string, CheckBox> downloadhandler = delegate (string mod, CheckBox checkBox)
            {
                MainWindow.Instance.DebugLogger.Text = $"Checking {mod}...";
                bool enabled = checkBox != null && checkBox.IsChecked == true;
                bool disabled = checkBox == null || checkBox.IsChecked == false;

                if (checkBox != null && checkBox.IsChecked == true && !RTFile.FileExists($"{b}/{mod}.dll") && !RTFile.FileExists($"{b}/{mod}.disabled"))
                {
                    DownloadFile($"https://github.com/RTMecha/{mod}/releases/latest/download/{mod}.zip", b, $"{mod}.zip", true);
                }
                else if (enabled && !RTFile.FileExists($"{b}/{mod}.dll") && RTFile.FileExists($"{b}/{mod}.disabled") || disabled && RTFile.FileExists($"{b}/{mod}.dll") && !RTFile.FileExists($"{b}/{mod}.disabled"))
                {

                    RTFile.MoveFile(disabled ? $"{b}/{mod}.dll" : $"{b}/{mod}.disabled", !disabled ? $"{b}/{mod}.dll" : $"{b}/{mod}.disabled");
                }
            };

            downloadhandler?.Invoke("RTFunctions", MainWindow.Instance.RTFunctionsEnabled);
            downloadhandler?.Invoke("EditorManagement", MainWindow.Instance.EditorManagementEnabled);
            downloadhandler?.Invoke("EventsCore", MainWindow.Instance.EventsCoreEnabled);
            downloadhandler?.Invoke("CreativePlayers", MainWindow.Instance.CreativePlayersEnabled);
            downloadhandler?.Invoke("ObjectModifiers", MainWindow.Instance.ObjectModifiersEnabled);
            downloadhandler?.Invoke("ArcadiaCustoms", MainWindow.Instance.ArcadiaCustomsEnabled);
            downloadhandler?.Invoke("PageCreator", MainWindow.Instance.PageCreatorEnabled);
            downloadhandler?.Invoke("ExampleCompanion", MainWindow.Instance.ExampleCompanionEnabled);

            MainWindow.Instance.DebugLogger.Text = $"Checking ConfigurationManager...";

            if (MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == true
                && !RTFile.DirectoryExists(b + "/ConfigurationManager"))
            {
                // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                var rep = b.Replace("/BepInEx/plugins", "");

                var rt = b + "/ConfigurationManager.zip";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/download/v18.2/BepInEx.ConfigurationManager.BepInEx5_v18.2.zip");

                await File.WriteAllBytesAsync(rt, bytes);
                RTFile.ZipUtil.UnZip(rt, rep);
            }
            else if (RTFile.DirectoryExists(b + "/ConfigurationManager") && MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == false)
            {
                try
                {
                    var files = Directory.GetFiles(b + "/ConfigurationManager");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    Directory.Delete(b + "/ConfigurationManager");
                }
                catch
                {

                }
            }

            MainWindow.Instance.DebugLogger.Text = $"Checking UnityExplorer...";

            if (MainWindow.Instance.UnityExplorerEnabled != null && MainWindow.Instance.UnityExplorerEnabled.IsChecked == true
                && !RTFile.FileExists(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll"))
            {
                // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                var rep = b.Replace("/plugins", "");

                var rt = b + "/UnityExplorer.zip";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip");

                await File.WriteAllBytesAsync(rt, bytes);
                RTFile.ZipUtil.UnZip(rt, rep);
            }
            else if (RTFile.FileExists(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll") && MainWindow.Instance.UnityExplorerEnabled != null && MainWindow.Instance.UnityExplorerEnabled.IsChecked == false)
            {
                try
                {
                    File.Delete(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll");
                }
                catch
                {

                }
            }

            MainWindow.Instance.DebugLogger.Text = $"Checking EditorOnStartup...";

            if (MainWindow.Instance.EditorOnStartupEnabled != null && MainWindow.Instance.EditorOnStartupEnabled.IsChecked == true
                && !RTFile.FileExists(b + "/EditorOnStartup.dll"))
            {
                var rt = b + "/EditorOnStartup.dll";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/enchart/EditorOnStartup/releases/download/1.0.0/EditorOnStartup_1.0.0.dll");

                await File.WriteAllBytesAsync(rt, bytes);
            }
            else if (RTFile.FileExists(b + "/EditorOnStartup.dll") && MainWindow.Instance.EditorOnStartupEnabled != null && MainWindow.Instance.EditorOnStartupEnabled.IsChecked == false)
            {
                try
                {
                    File.Delete(b + "/EditorOnStartup.dll");
                }
                catch
                {

                }
            }

            MainWindow.Instance.DebugLogger.Text = $"Checking default beatmaps folder...";

            if (!RTFile.DirectoryExists(a + "beatmaps/prefabtypes") || !RTFile.DirectoryExists(a + "beatmaps/shapes") || !RTFile.DirectoryExists(a + "beatmaps/menus"))
                DownloadFile("https://github.com/RTMecha/RTFunctions/releases/latest/download/Beatmaps.zip", a, "Beatmaps.zip");

            // Save Versions (For later use when the game is relaunched)
            {
                var str = "";

                for (int i = 0; i < LocalVersions.Count; i++)
                {
                    str += LocalVersions.ElementAt(i).Key + Environment.NewLine + LocalVersions.ElementAt(i).Value + Environment.NewLine;
                }

                await File.WriteAllTextAsync(RTFile.ApplicationDirectory + "versions.lss", str);
            }

            MainWindow.Instance.DebugLogger.Text = $"Opening...";

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = MainWindow.Instance.Path;
            Process.Start(startInfo);

            MainWindow.Instance.DebugLogger.Text = $"Done!";
        }

        public async static void Update(ProjectArrhythmia projectArrhythmia)
        {
            if (MainWindow.Instance == null)
                return;

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Updating mods, please wait...";

            Debug.WriteLine(projectArrhythmia.FolderPath);

            projectArrhythmia.SaveSettings();

            var a = projectArrhythmia.FolderPath;
            var b = a + BepInExPlugins;

            // Download BepInEx (Obviously will need BepInEx itself to run any mods)
            if (!Directory.Exists(a + "BepInEx"))
            {
                var bep = a + "BepInEx-5.4.21.zip";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync(BepInExURL);

                await File.WriteAllBytesAsync(bep, bytes);

                RTFile.ZipUtil.UnZip(bep, a);
            }

            // Load Versions (For version comparison, so we're not re-downloading the mods every time we launch the game)
            {
                if (RTFile.FileExists(projectArrhythmia.FolderPath + "settings/versions.lss"))
                {
                    var localVersions = await File.ReadAllTextAsync(projectArrhythmia.FolderPath + "settings/versions.lss");

                    if (!string.IsNullOrEmpty(localVersions))
                    {
                        var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < list.Length; i++)
                        {
                            if (projectArrhythmia.LocalVersions.ContainsKey(list[i]) && list.Length > i + 1)
                            {
                                projectArrhythmia.LocalVersions[list[i]] = list[i + 1];
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < projectArrhythmia.LocalVersions.Count; i++)
                    {
                        var key = projectArrhythmia.LocalVersions.ElementAt(i).Key;
                        if (RTFile.FileExists($"{b}/{key}.dll"))
                        {
                            var version = projectArrhythmia.GetModVersion($"{b}/{key}.dll");

                            projectArrhythmia.LocalVersions[key] = version;
                        }
                    }
                }

                using var http = new HttpClient();

                var data = await http.GetStringAsync(CurrentVersionsList);

                if (!string.IsNullOrEmpty(data))
                {
                    var list = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < list.Length; i++)
                    {
                        if (OnlineVersions.ContainsKey(list[i]) && list.Length > i + 1)
                        {
                            MainWindow.Instance.CurrentInstanceProgress.Text = $"Setting online version {list[i + 1]}";
                            OnlineVersions[list[i]] = list[i + 1];
                        }
                    }
                }
            }

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Checking dependents...";

            // Verify (Makes sure RTFunctions is on if any dependant mod is)
            {
                if (MainWindow.Instance.InstanceEditorManagementEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceEventsCoreEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceCreativePlayersEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceObjectModifiersEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceArcadiaCustomsEnabled.IsChecked == true ||
                    MainWindow.Instance.InstancePageCreatorEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceExampleCompanionEnabled.IsChecked == true)
                {
                    MainWindow.Instance.InstanceRTFunctionsEnabled.IsChecked = true;
                }
            }

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Verifying directory...";

            // In case the BepInEx/plugins folder doesn't exist
            if (!Directory.Exists(b))
                Directory.CreateDirectory(b);

            Action<string, CheckBox> downloadhandler = delegate (string mod, CheckBox checkBox)
            {
                if (checkBox != null && checkBox.IsChecked == true
                    && ((!RTFile.FileExists($"{b}/{mod}.dll") && !RTFile.FileExists($"{b}/{mod}.disabled")) ||
                    OnlineVersions[$"{mod}"] != projectArrhythmia.LocalVersions[$"{mod}"]))
                {
                    if (RTFile.FileExists($"{b}/{mod}.disabled"))
                    {
                        File.Delete($"{b}/{mod}.disabled");
                    }

                    if (RTFile.FileExists($"{b}/{mod}.dll"))
                    {
                        File.Delete($"{b}/{mod}.dll");
                    }

                    DownloadFile($"https://github.com/RTMecha/{mod}/releases/latest/download/{mod}.zip", b, $"{mod}.zip", true);
                }

                if (!RTFile.FileExists($"{b}/{mod}.dll") && RTFile.FileExists($"{b}/{mod}.disabled")
                    || RTFile.FileExists($"{b}/{mod}.dll") && !RTFile.FileExists($"{b}/{mod}.disabled"))
                {
                    bool enabled = checkBox != null && checkBox.IsChecked == false;

                    RTFile.MoveFile(enabled ? $"{b}/{mod}.dll" : $"{b}/{mod}.disabled", !enabled ? $"{b}/{mod}.dll" : $"{b}/{mod}.disabled");
                }
            };

            downloadhandler?.Invoke("RTFunctions", MainWindow.Instance.InstanceRTFunctionsEnabled);
            downloadhandler?.Invoke("EditorManagement", MainWindow.Instance.InstanceEditorManagementEnabled);
            downloadhandler?.Invoke("EventsCore", MainWindow.Instance.InstanceEventsCoreEnabled);
            downloadhandler?.Invoke("CreativePlayers", MainWindow.Instance.InstanceCreativePlayersEnabled);
            downloadhandler?.Invoke("ObjectModifiers", MainWindow.Instance.InstanceObjectModifiersEnabled);
            downloadhandler?.Invoke("ArcadiaCustoms", MainWindow.Instance.InstanceArcadiaCustomsEnabled);
            downloadhandler?.Invoke("PageCreator", MainWindow.Instance.InstancePageCreatorEnabled);
            downloadhandler?.Invoke("ExampleCompanion", MainWindow.Instance.InstanceExampleCompanionEnabled);

            if (MainWindow.Instance.InstanceConfigurationManagerEnabled != null && MainWindow.Instance.InstanceConfigurationManagerEnabled.IsChecked == true
                && !RTFile.DirectoryExists(b + "/ConfigurationManager"))
            {
                // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                var rep = b.Replace("/BepInEx/plugins", "");

                var rt = b + "/ConfigurationManager.zip";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/download/v18.2/BepInEx.ConfigurationManager.BepInEx5_v18.2.zip");
                await File.WriteAllBytesAsync(rt, bytes);
                RTFile.ZipUtil.UnZip(rt, rep);
            }
            else if (RTFile.DirectoryExists(b + "/ConfigurationManager") && MainWindow.Instance.InstanceConfigurationManagerEnabled != null && MainWindow.Instance.InstanceConfigurationManagerEnabled.IsChecked == false)
            {
                try
                {
                    var files = Directory.GetFiles(b + "/ConfigurationManager");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    Directory.Delete(b + "/ConfigurationManager");
                }
                catch
                {

                }
            }

            if (MainWindow.Instance.InstanceUnityExplorerEnabled != null && MainWindow.Instance.InstanceUnityExplorerEnabled.IsChecked == true
                && !RTFile.FileExists(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll"))
            {
                // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                var rep = b.Replace("/plugins", "");

                var rt = b + "/UnityExplorer.zip";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip");
                await File.WriteAllBytesAsync(rt, bytes);
                RTFile.ZipUtil.UnZip(rt, rep);
            }
            else if (RTFile.FileExists(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll") && MainWindow.Instance.InstanceUnityExplorerEnabled != null && MainWindow.Instance.InstanceUnityExplorerEnabled.IsChecked == false)
            {
                try
                {
                    File.Delete(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll");
                }
                catch
                {

                }
            }

            if (MainWindow.Instance.InstanceEditorOnStartupEnabled != null && MainWindow.Instance.InstanceEditorOnStartupEnabled.IsChecked == true
                && !RTFile.FileExists(b + "/EditorOnStartup.dll"))
            {
                var rt = b + "/EditorOnStartup.dll";
                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/enchart/EditorOnStartup/releases/download/1.0.0/EditorOnStartup_1.0.0.dll");
                await File.WriteAllBytesAsync(rt, bytes);
            }
            else if (RTFile.FileExists(b + "/EditorOnStartup.dll") && MainWindow.Instance.InstanceEditorOnStartupEnabled != null && MainWindow.Instance.InstanceEditorOnStartupEnabled.IsChecked == false)
            {
                try
                {
                    File.Delete(b + "/EditorOnStartup.dll");
                }
                catch
                {

                }
            }

            if (!RTFile.DirectoryExists(a + "beatmaps/prefabtypes") || !RTFile.DirectoryExists(a + "beatmaps/shapes") || !RTFile.DirectoryExists(a + "beatmaps/menus"))
                DownloadFile("https://github.com/RTMecha/RTFunctions/releases/latest/download/Beatmaps.zip", a, "Beatmaps.zip", true);

            // Save Versions (For later use when the game is relaunched)
            {
                var str = "";

                for (int i = 0; i < OnlineVersions.Count; i++)
                {
                    var key = OnlineVersions.ElementAt(i).Key;
                    var version = OnlineVersions.ElementAt(i).Value;
                    str += key + Environment.NewLine + version + Environment.NewLine;

                    MainWindow.Instance.GetModInstanceToggle(i).Content = $"{key} - Installed: {version}";
                }

                await File.WriteAllTextAsync(projectArrhythmia.FolderPath + "settings/versions.lss", str);
            }

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Finished updating.";
        }

        public async static void Launch(ProjectArrhythmia projectArrhythmia)
        {
            if (MainWindow.Instance == null)
                return;

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Start opening the game, please wait...";

            projectArrhythmia.SaveSettings();

            var a = projectArrhythmia.FolderPath;
            var b = a + BepInExPlugins;

            // Download steam_api.dll
            if (!RTFile.FileExists(a + "Project Arrhythmia_Data/Plugins/steam_api_updated.txt"))
            {
                using var http = new HttpClient();

                var bytes = await http.GetByteArrayAsync("https://github.com/RTMecha/RTFunctions/releases/latest/download/steam_api64.dll");

                await File.WriteAllBytesAsync(a + "Project Arrhythmia_Data/Plugins/steam_api64.dll", bytes);

                await File.WriteAllTextAsync(a + "Project Arrhythmia_Data/Plugins/steam_api_updated.txt", "Yes");
            }

            // Download BepInEx (Obviously will need BepInEx itself to run any mods)
            if (!Directory.Exists(a + "BepInEx"))
            {
                var bep = a + "BepInEx-5.4.21.zip";

                using var http = new HttpClient();

                var bytes = await http.GetByteArrayAsync(BepInExURL);

                await File.WriteAllBytesAsync(bep, bytes);

                RTFile.ZipUtil.UnZip(bep, a);
            }

            // Load Versions (For version comparison, so we're not re-downloading the mods every time we launch the game)
            await projectArrhythmia.LoadVersions();

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Checking dependants...";

            // Verify (Makes sure RTFunctions is on if any dependant mod is)
            {
                if (MainWindow.Instance.InstanceEditorManagementEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceEventsCoreEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceCreativePlayersEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceObjectModifiersEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceArcadiaCustomsEnabled.IsChecked == true ||
                    MainWindow.Instance.InstancePageCreatorEnabled.IsChecked == true ||
                    MainWindow.Instance.InstanceExampleCompanionEnabled.IsChecked == true)
                {
                    MainWindow.Instance.InstanceRTFunctionsEnabled.IsChecked = true;
                }
            }

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Verifying directory...";

            // In case the BepInEx/plugins folder doesn't exist
            if (!Directory.Exists(b))
                Directory.CreateDirectory(b);

            Action<string, CheckBox> downloadhandler = delegate (string mod, CheckBox checkBox)
            {
                MainWindow.Instance.CurrentInstanceProgress.Text = $"Checking {mod}...";
                bool enabled = checkBox != null && checkBox.IsChecked == true;
                bool disabled = checkBox == null || checkBox.IsChecked == false;

                if (checkBox != null && checkBox.IsChecked == true && !RTFile.FileExists($"{b}/{mod}.dll") && !RTFile.FileExists($"{b}/{mod}.disabled"))
                {
                    DownloadFile($"https://github.com/RTMecha/{mod}/releases/latest/download/{mod}.zip", b, $"{mod}.zip", true);
                }
                else if (enabled && !RTFile.FileExists($"{b}/{mod}.dll") && RTFile.FileExists($"{b}/{mod}.disabled") || disabled && RTFile.FileExists($"{b}/{mod}.dll") && !RTFile.FileExists($"{b}/{mod}.disabled"))
                {

                    RTFile.MoveFile(disabled ? $"{b}/{mod}.dll" : $"{b}/{mod}.disabled", !disabled ? $"{b}/{mod}.dll" : $"{b}/{mod}.disabled");
                }
            };

            downloadhandler?.Invoke("RTFunctions", MainWindow.Instance.InstanceRTFunctionsEnabled);
            downloadhandler?.Invoke("EditorManagement", MainWindow.Instance.InstanceEditorManagementEnabled);
            downloadhandler?.Invoke("EventsCore", MainWindow.Instance.InstanceEventsCoreEnabled);
            downloadhandler?.Invoke("CreativePlayers", MainWindow.Instance.InstanceCreativePlayersEnabled);
            downloadhandler?.Invoke("ObjectModifiers", MainWindow.Instance.InstanceObjectModifiersEnabled);
            downloadhandler?.Invoke("ArcadiaCustoms", MainWindow.Instance.InstanceArcadiaCustomsEnabled);
            downloadhandler?.Invoke("PageCreator", MainWindow.Instance.InstancePageCreatorEnabled);
            downloadhandler?.Invoke("ExampleCompanion", MainWindow.Instance.InstanceExampleCompanionEnabled);

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Checking ConfigurationManager...";

            if (MainWindow.Instance.InstanceConfigurationManagerEnabled != null && MainWindow.Instance.InstanceConfigurationManagerEnabled.IsChecked == true
                && !RTFile.DirectoryExists(b + "/ConfigurationManager"))
            {
                // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                var rep = b.Replace("/BepInEx/plugins", "");

                var rt = b + "/ConfigurationManager.zip";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/download/v18.2/BepInEx.ConfigurationManager.BepInEx5_v18.2.zip");
                await File.WriteAllBytesAsync(rt, bytes);
                RTFile.ZipUtil.UnZip(rt, rep);
            }
            else if (RTFile.DirectoryExists(b + "/ConfigurationManager") && MainWindow.Instance.InstanceConfigurationManagerEnabled != null && MainWindow.Instance.InstanceConfigurationManagerEnabled.IsChecked == false)
            {
                try
                {
                    var files = Directory.GetFiles(b + "/ConfigurationManager");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    Directory.Delete(b + "/ConfigurationManager");
                }
                catch
                {

                }
            }

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Checking UnityExplorer...";

            if (MainWindow.Instance.InstanceUnityExplorerEnabled != null && MainWindow.Instance.InstanceUnityExplorerEnabled.IsChecked == true
                && !RTFile.FileExists(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll"))
            {
                // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                var rep = b.Replace("/plugins", "");

                var rt = b + "/UnityExplorer.zip";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip");
                await File.WriteAllBytesAsync(rt, bytes);
                RTFile.ZipUtil.UnZip(rt, rep);
            }
            else if (RTFile.FileExists(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll") && MainWindow.Instance.InstanceUnityExplorerEnabled != null && MainWindow.Instance.InstanceUnityExplorerEnabled.IsChecked == false)
            {
                try
                {
                    File.Delete(b + "/sinai-dev-UnityExplorer/UnityExplorer.BIE5.Mono.dll");
                }
                catch
                {

                }
            }

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Checking EditorOnStartup...";

            if (MainWindow.Instance.InstanceEditorOnStartupEnabled != null && MainWindow.Instance.InstanceEditorOnStartupEnabled.IsChecked == true
                && !RTFile.FileExists(b + "/EditorOnStartup.dll"))
            {
                var rt = b + "/EditorOnStartup.dll";

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/enchart/EditorOnStartup/releases/download/1.0.0/EditorOnStartup_1.0.0.dll");
                await File.WriteAllBytesAsync(rt, bytes);
            }
            else if (RTFile.FileExists(b + "/EditorOnStartup.dll") && MainWindow.Instance.InstanceEditorOnStartupEnabled != null && MainWindow.Instance.InstanceEditorOnStartupEnabled.IsChecked == false)
            {
                try
                {
                    File.Delete(b + "/EditorOnStartup.dll");
                }
                catch
                {

                }
            }

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Checking default beatmaps folder...";

            if (!RTFile.DirectoryExists(a + "beatmaps/prefabtypes") || !RTFile.DirectoryExists(a + "beatmaps/shapes") || !RTFile.DirectoryExists(a + "beatmaps/menus"))
                DownloadFile("https://github.com/RTMecha/RTFunctions/releases/latest/download/Beatmaps.zip", a, "Beatmaps.zip", true);

            // Save Versions (For later use when the game is relaunched)
            {
                var str = "";

                for (int i = 0; i < projectArrhythmia.LocalVersions.Count; i++)
                {
                    str += projectArrhythmia.LocalVersions.ElementAt(i).Key + Environment.NewLine + projectArrhythmia.LocalVersions.ElementAt(i).Value + Environment.NewLine;
                }

                await File.WriteAllTextAsync(projectArrhythmia.FolderPath + "settings/versions.lss", str);
            }

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Opening...";

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = projectArrhythmia.Path;
            Process.Start(startInfo);

            MainWindow.Instance.CurrentInstanceProgress.Text = $"Done!";
        }

        public async static void DownloadFile(string url, string output, string file, bool instance = false)
        {
            if (MainWindow.Instance != null)
                (instance ? MainWindow.Instance.CurrentInstanceProgress : MainWindow.Instance.DebugLogger).Text = $"Downloading {file}...";

            var rt = output + "/" + file;

            using var http = new HttpClient();
            var bytes = await http.GetByteArrayAsync(url);
            
            await File.WriteAllBytesAsync(rt, bytes);

            RTFile.ZipUtil.UnZip(rt, output);
        }
    }
}
