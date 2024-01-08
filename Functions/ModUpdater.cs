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

namespace ProjectLauncher.Functions
{
    public static class ModUpdater
    {
        public static string BepInExPlugins => "BepInEx/plugins";
        public static string BepInExURL => "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip";

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

        public static bool CheckForUpdates()
        {
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.DebugLogger.Text = $"Updating mods, please wait...";

                MainWindow.Instance.SaveSettings();

                var a = MainWindow.Instance.Path.Replace("Project Arrhythmia.exe", "");

                // Download BepInEx (Obviously will need BepInEx itself to run any mods)
                if (!Directory.Exists(a + "BepInEx"))
                {
                    var bep = a + "BepInEx-5.4.21.zip";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(BepInExURL, bep);

                        RTFile.ZipUtil.UnZip(bep, a);
                        client.Dispose();
                    }
                }

                // Load Versions (For version comparison, so we're not re-downloading the mods every time we launch the game)
                {
                    if (RTFile.FileExists(RTFile.ApplicationDirectory + "versions.lss"))
                    {
                        var localVersions = RTFile.ReadFromFile(RTFile.ApplicationDirectory + "versions.lss");

                        if (!string.IsNullOrEmpty(localVersions))
                        {
                            var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0; i < list.Count; i++)
                            {
                                if (LocalVersions.ContainsKey(list[i]) && list.Count > i + 1)
                                {
                                    LocalVersions[list[i]] = list[i + 1];
                                }
                            }
                        }
                    }

                    using (var client = new WebClient())
                    {
                        var data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/mod_info.lss");

                        if (!string.IsNullOrEmpty(data))
                        {
                            var list = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0 ; i < list.Count; i++)
                            {
                                if (OnlineVersions.ContainsKey(list[i]) && list.Count > i + 1)
                                {
                                    MainWindow.Instance.DebugLogger.Text = $"Updating {list[i + 1]}";
                                    OnlineVersions[list[i]] = list[i + 1];
                                }
                            }
                        }

                        client.Dispose();
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

                #region RTFunctions

                if (MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/RTFunctions.dll") ||
                    OnlineVersions["RTFunctions"] != LocalVersions["RTFunctions"]))
                {
                    if (RTFile.FileExists(b + "/RTFunctions.disabled"))
                    {
                        File.Delete(b + "/RTFunctions.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/RTFunctions/releases/latest/download/RTFunctions.zip", b, "RTFunctions.zip");
                }

                if (!RTFile.FileExists(b + "/RTFunctions.dll") && RTFile.FileExists(b + "RTFunctions.disabled") || RTFile.FileExists(b + "/RTFunctions.dll") && !RTFile.FileExists(b + "RTFunctions.disabled"))
                {
                    bool enabled = MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/RTFunctions.dll" : b + "/RTFunctions.disabled", !enabled ? b + "/RTFunctions.dll" : b + "/RTFunctions.disabled");
                }

                #endregion

                #region EditorManagement

                if (MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/EditorManagement.dll") ||
                    OnlineVersions["EditorManagement"] != LocalVersions["EditorManagement"]))
                {
                    if (RTFile.FileExists(b + "/EditorManagement.disabled"))
                    {
                        File.Delete(b + "/EditorManagement.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/EditorManagement/releases/latest/download/EditorManagement.zip", b, "EditorManagement.zip");
                }

                if (!RTFile.FileExists(b + "/EditorManagement.dll") && RTFile.FileExists(b + "EditorManagement.disabled") || RTFile.FileExists(b + "/EditorManagement.dll") && !RTFile.FileExists(b + "EditorManagement.disabled"))
                {
                    bool enabled = MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/EditorManagement.dll" : b + "/EditorManagement.disabled", !enabled ? b + "/EditorManagement.dll" : b + "/EditorManagement.disabled");
                }

                #endregion

                #region EventsCore

                if (MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/EventsCore.dll") ||
                    OnlineVersions["EventsCore"] != LocalVersions["EventsCore"]))
                {
                    if (RTFile.FileExists(b + "/EventsCore.disabled"))
                    {
                        File.Delete(b + "/EventsCore.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/EventsCore/releases/latest/download/EventsCore.zip", b, "EventsCore.zip");
                }

                if (!RTFile.FileExists(b + "/EventsCore.dll") && RTFile.FileExists(b + "EventsCore.disabled") || RTFile.FileExists(b + "/EventsCore.dll") && !RTFile.FileExists(b + "EventsCore.disabled"))
                {
                    bool enabled = MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/EventsCore.dll" : b + "/EventsCore.disabled", !enabled ? b + "/EventsCore.dll" : b + "/EventsCore.disabled");
                }

                #endregion

                #region CreativePlayers

                if (MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/CreativePlayers.dll") ||
                    OnlineVersions["CreativePlayers"] != LocalVersions["CreativePlayers"]))
                {
                    if (RTFile.FileExists(b + "/CreativePlayers.disabled"))
                    {
                        File.Delete(b + "/CreativePlayers.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/CreativePlayers/releases/latest/download/CreativePlayers.zip", b, "CreativePlayers.zip");
                }

                if (!RTFile.FileExists(b + "/CreativePlayers.dll") && RTFile.FileExists(b + "CreativePlayers.disabled") || RTFile.FileExists(b + "/CreativePlayers.dll") && !RTFile.FileExists(b + "CreativePlayers.disabled"))
                {
                    bool enabled = MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/CreativePlayers.dll" : b + "/CreativePlayers.disabled", !enabled ? b + "/CreativePlayers.dll" : b + "/CreativePlayers.disabled");
                }

                #endregion

                #region ObjectModifiers

                if (MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/ObjectModifiers.dll") ||
                    OnlineVersions["ObjectModifiers"] != LocalVersions["ObjectModifiers"]))
                {
                    if (RTFile.FileExists(b + "/ObjectModifiers.disabled"))
                    {
                        File.Delete(b + "/ObjectModifiers.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/ObjectModifiers/releases/latest/download/ObjectModifiers.zip", b, "ObjectModifiers.zip");
                }

                if (!RTFile.FileExists(b + "/ObjectModifiers.dll") && RTFile.FileExists(b + "ObjectModifiers.disabled") || RTFile.FileExists(b + "/ObjectModifiers.dll") && !RTFile.FileExists(b + "ObjectModifiers.disabled"))
                {
                    bool enabled = MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ObjectModifiers.dll" : b + "/ObjectModifiers.disabled", !enabled ? b + "/ObjectModifiers.dll" : b + "/ObjectModifiers.disabled");
                }

                #endregion

                #region ArcadiaCustoms

                if (MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/ArcadiaCustoms.dll") ||
                    OnlineVersions["ArcadiaCustoms"] != LocalVersions["ArcadiaCustoms"]))
                {
                    if (RTFile.FileExists(b + "/ArcadiaCustoms.disabled"))
                    {
                        File.Delete(b + "/ArcadiaCustoms.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/ArcadiaCustoms/releases/latest/download/ArcadiaCustoms.zip", b, "ArcadiaCustoms.zip");
                }

                if (!RTFile.FileExists(b + "/ArcadiaCustoms.dll") && RTFile.FileExists(b + "ArcadiaCustoms.disabled") || RTFile.FileExists(b + "/ArcadiaCustoms.dll") && !RTFile.FileExists(b + "ArcadiaCustoms.disabled"))
                {
                    bool enabled = MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ArcadiaCustoms.dll" : b + "/ArcadiaCustoms.disabled", !enabled ? b + "/ArcadiaCustoms.dll" : b + "/ArcadiaCustoms.disabled");
                }

                #endregion

                #region PageCreator

                if (MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/PageCreator.dll") ||
                    OnlineVersions["PageCreator"] != LocalVersions["PageCreator"]))
                {
                    if (RTFile.FileExists(b + "/PageCreator.disabled"))
                    {
                        File.Delete(b + "/PageCreator.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/PageCreator/releases/latest/download/PageCreator.zip", b, "PageCreator.zip");
                }

                if (!RTFile.FileExists(b + "/PageCreator.dll") && RTFile.FileExists(b + "PageCreator.disabled") || RTFile.FileExists(b + "/PageCreator.dll") && !RTFile.FileExists(b + "PageCreator.disabled"))
                {
                    bool enabled = MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/PageCreator.dll" : b + "/PageCreator.disabled", !enabled ? b + "/PageCreator.dll" : b + "/PageCreator.disabled");
                }

                #endregion

                #region ExampleCompanion

                if (MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/ExampleCompanion.dll") ||
                    OnlineVersions["ExampleCompanion"] != LocalVersions["ExampleCompanion"]))
                {
                    if (RTFile.FileExists(b + "/ExampleCompanion.disabled"))
                    {
                        File.Delete(b + "/ExampleCompanion.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/ExampleCompanion/releases/latest/download/ExampleCompanion.zip", b, "ExampleCompanion.zip");
                }

                if (!RTFile.FileExists(b + "/ExampleCompanion.dll") && RTFile.FileExists(b + "ExampleCompanion.disabled") || RTFile.FileExists(b + "/ExampleCompanion.dll") && !RTFile.FileExists(b + "ExampleCompanion.disabled"))
                {
                    bool enabled = MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ExampleCompanion.dll" : b + "/ExampleCompanion.disabled", !enabled ? b + "/ExampleCompanion.dll" : b + "/ExampleCompanion.disabled");
                }

                #endregion

                if (MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == true
                    && !RTFile.DirectoryExists(b + "/ConfigurationManager"))
                {
                    // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                    var rep = b.Replace("/BepInEx/plugins", "");

                    var rt = b + "/ConfigurationManager.zip";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/download/v18.2/BepInEx.ConfigurationManager.BepInEx5_v18.2.zip", rt);
                        RTFile.ZipUtil.UnZip(rt, rep);
                    }
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
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip", rt);
                        RTFile.ZipUtil.UnZip(rt, rep);
                    }
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
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://cdn.discordapp.com/attachments/1092449110805725256/1092449111141257296/EditorOnStartup.dll", rt);
                    }
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

                    RTFile.WriteToFile(RTFile.ApplicationDirectory + "versions.lss", str);
                }

                MainWindow.Instance.DebugLogger.Text = $"Finished updating!";

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = MainWindow.Instance.Path;
                Process.Start(startInfo);

                return true;
            }

            return false;
        }

        public static bool Open()
        {
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.DebugLogger.Text = $"Start opening the game, please wait...";

                MainWindow.Instance.SaveSettings();

                var a = MainWindow.Instance.Path.Replace("Project Arrhythmia.exe", "");
                var b = a + BepInExPlugins;

                // Download BepInEx (Obviously will need BepInEx itself to run any mods)
                if (!Directory.Exists(a + "BepInEx"))
                {
                    var bep = a + "BepInEx-5.4.21.zip";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(BepInExURL, bep);

                        RTFile.ZipUtil.UnZip(bep, a);
                        client.Dispose();
                    }
                }

                // Load Versions (For version comparison, so we're not re-downloading the mods every time we launch the game)
                {
                    if (RTFile.FileExists(RTFile.ApplicationDirectory + "versions.lss"))
                    {
                        var localVersions = RTFile.ReadFromFile(RTFile.ApplicationDirectory + "versions.lss");

                        if (!string.IsNullOrEmpty(localVersions))
                        {
                            var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0; i < list.Count; i++)
                            {
                                if (LocalVersions.ContainsKey(list[i]) && list.Count > i + 1)
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

                #region RTFunctions

                if (MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == true && !RTFile.FileExists(b + "/RTFunctions.dll") && !RTFile.FileExists(b + "/RTFunctions.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/RTFunctions/releases/latest/download/RTFunctions.zip", b, "RTFunctions.zip");
                }
                else if (!RTFile.FileExists(b + "/RTFunctions.dll") && RTFile.FileExists(b + "/RTFunctions.disabled") || RTFile.FileExists(b + "/RTFunctions.dll") && !RTFile.FileExists(b + "/RTFunctions.disabled"))
                {
                    bool enabled = MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/RTFunctions.dll" : b + "/RTFunctions.disabled", !enabled ? b + "/RTFunctions.dll" : b + "/RTFunctions.disabled");
                }

                #endregion

                #region EditorManagement

                if (MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == true && !RTFile.FileExists(b + "/EditorManagement.dll") && !RTFile.FileExists(b + "/EditorManagement.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/EditorManagement/releases/latest/download/EditorManagement.zip", b, "EditorManagement.zip");
                }
                else if (!RTFile.FileExists(b + "/EditorManagement.dll") && RTFile.FileExists(b + "/EditorManagement.disabled") || RTFile.FileExists(b + "/EditorManagement.dll") && !RTFile.FileExists(b + "/EditorManagement.disabled"))
                {
                    bool enabled = MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/EditorManagement.dll" : b + "/EditorManagement.disabled", !enabled ? b + "/EditorManagement.dll" : b + "/EditorManagement.disabled");
                }

                #endregion

                #region EventsCore

                if (MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == true && !RTFile.FileExists(b + "/EventsCore.dll") && !RTFile.FileExists(b + "/EventsCore.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/EventsCore/releases/latest/download/EventsCore.zip", b, "EventsCore.zip");
                }
                else if (!RTFile.FileExists(b + "/EventsCore.dll") && RTFile.FileExists(b + "/EventsCore.disabled") || RTFile.FileExists(b + "/EventsCore.dll") && !RTFile.FileExists(b + "/EventsCore.disabled"))
                {
                    bool enabled = MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/EventsCore.dll" : b + "/EventsCore.disabled", !enabled ? b + "/EventsCore.dll" : b + "/EventsCore.disabled");
                }

                #endregion

                #region CreativePlayers

                if (MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == true && !RTFile.FileExists(b + "/CreativePlayers.dll") && !RTFile.FileExists(b + "/CreativePlayers.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/CreativePlayers/releases/latest/download/CreativePlayers.zip", b, "CreativePlayers.zip");
                }
                else if (!RTFile.FileExists(b + "/CreativePlayers.dll") && RTFile.FileExists(b + "CreativePlayers.disabled") || RTFile.FileExists(b + "/CreativePlayers.dll") && !RTFile.FileExists(b + "/CreativePlayers.disabled"))
                {
                    bool enabled = MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/CreativePlayers.dll" : b + "/CreativePlayers.disabled", !enabled ? b + "/CreativePlayers.dll" : b + "/CreativePlayers.disabled");
                }

                #endregion

                #region ObjectModifiers

                if (MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == true && !RTFile.FileExists(b + "/ObjectModifiers.dll") && !RTFile.FileExists(b + "/ObjectModifiers.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/ObjectModifiers/releases/latest/download/ObjectModifiers.zip", b, "ObjectModifiers.zip");
                }
                else if (!RTFile.FileExists(b + "/ObjectModifiers.dll") && RTFile.FileExists(b + "/ObjectModifiers.disabled") || RTFile.FileExists(b + "/ObjectModifiers.dll") && !RTFile.FileExists(b + "/ObjectModifiers.disabled"))
                {
                    bool enabled = MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ObjectModifiers.dll" : b + "/ObjectModifiers.disabled", !enabled ? b + "/ObjectModifiers.dll" : b + "/ObjectModifiers.disabled");
                }

                #endregion

                #region ArcadiaCustoms

                if (MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == true && !RTFile.FileExists(b + "/ArcadiaCustoms.dll") && !RTFile.FileExists(b + "/ArcadiaCustoms.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/ArcadiaCustoms/releases/latest/download/ArcadiaCustoms.zip", b, "ArcadiaCustoms.zip");
                }
                else if (!RTFile.FileExists(b + "/ArcadiaCustoms.dll") && RTFile.FileExists(b + "/ArcadiaCustoms.disabled") || RTFile.FileExists(b + "/ArcadiaCustoms.dll") && !RTFile.FileExists(b + "/ArcadiaCustoms.disabled"))
                {
                    bool enabled = MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ArcadiaCustoms.dll" : b + "/ArcadiaCustoms.disabled", !enabled ? b + "/ArcadiaCustoms.dll" : b + "/ArcadiaCustoms.disabled");
                }

                #endregion

                #region PageCreator

                if (MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == true && !RTFile.FileExists(b + "/PageCreator.dll") && !RTFile.FileExists(b + "/PageCreator.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/PageCreator/releases/latest/download/PageCreator.zip", b, "PageCreator.zip");
                }
                else if (!RTFile.FileExists(b + "/PageCreator.dll") && RTFile.FileExists(b + "/PageCreator.disabled") || RTFile.FileExists(b + "/PageCreator.dll") && !RTFile.FileExists(b + "/PageCreator.disabled"))
                {
                    bool enabled = MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/PageCreator.dll" : b + "/PageCreator.disabled", !enabled ? b + "/PageCreator.dll" : b + "/PageCreator.disabled");
                }

                #endregion

                #region ExampleCompanion

                if (MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == true && !RTFile.FileExists(b + "/ExampleCompanion.dll") && !RTFile.FileExists(b + "/ExampleCompanion.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/ExampleCompanion/releases/latest/download/ExampleCompanion.zip", b, "ExampleCompanion.zip");
                }
                else if (!RTFile.FileExists(b + "/ExampleCompanion.dll") && RTFile.FileExists(b + "/ExampleCompanion.disabled") || RTFile.FileExists(b + "/ExampleCompanion.dll") && !RTFile.FileExists(b + "/ExampleCompanion.disabled"))
                {
                    bool enabled = MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ExampleCompanion.dll" : b + "/ExampleCompanion.disabled", !enabled ? b + "/ExampleCompanion.dll" : b + "/ExampleCompanion.disabled");
                }

                #endregion

                if (MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == true
                    && !RTFile.DirectoryExists(b + "/ConfigurationManager"))
                {
                    // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                    var rep = b.Replace("/BepInEx/plugins", "");

                    var rt = b + "/ConfigurationManager.zip";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/download/v18.2/BepInEx.ConfigurationManager.BepInEx5_v18.2.zip", rt);
                        RTFile.ZipUtil.UnZip(rt, rep);
                    }
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
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip", rt);
                        RTFile.ZipUtil.UnZip(rt, rep);
                    }
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
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://cdn.discordapp.com/attachments/1092449110805725256/1092449111141257296/EditorOnStartup.dll", rt);
                    }
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
                    DownloadFile("https://github.com/RTMecha/RTFunctions/releases/download/Current/Beatmaps.zip", a, "Beatmaps.zip");

                // Save Versions (For later use when the game is relaunched)
                {
                    var str = "";

                    for (int i = 0; i < LocalVersions.Count; i++)
                    {
                        str += LocalVersions.ElementAt(i).Key + Environment.NewLine + LocalVersions.ElementAt(i).Value + Environment.NewLine;
                    }

                    RTFile.WriteToFile(RTFile.ApplicationDirectory + "versions.lss", str);
                }

                MainWindow.Instance.DebugLogger.Text = $"Opening...";

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = MainWindow.Instance.Path;
                Process.Start(startInfo);

                return true;
            }

            return false;
        }

        public static bool CheckForUpdates(ProjectArrhythmia projectArrhythmia)
        {
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.DebugLogger.Text = $"Updating mods, please wait...";

                projectArrhythmia.SaveSettings();

                var a = projectArrhythmia.FolderPath;
                var b = a + BepInExPlugins;

                // Download BepInEx (Obviously will need BepInEx itself to run any mods)
                if (!Directory.Exists(a + "BepInEx"))
                {
                    var bep = a + "BepInEx-5.4.21.zip";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(BepInExURL, bep);

                        RTFile.ZipUtil.UnZip(bep, a);
                        client.Dispose();
                    }
                }

                // Load Versions (For version comparison, so we're not re-downloading the mods every time we launch the game)
                {
                    if (RTFile.FileExists(projectArrhythmia.FolderPath + "settings/versions.lss"))
                    {
                        var localVersions = RTFile.ReadFromFile(projectArrhythmia.FolderPath + "settings/versions.lss");

                        if (!string.IsNullOrEmpty(localVersions))
                        {
                            var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0; i < list.Count; i++)
                            {
                                if (LocalVersions.ContainsKey(list[i]) && list.Count > i + 1)
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
                                var version = projectArrhythmia.GetModVersion($"{b}/{key}.dll");

                                LocalVersions[key] = version;
                            }
                        }
                    }

                    using (var client = new WebClient())
                    {
                        var data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/mod_info.lss");

                        if (!string.IsNullOrEmpty(data))
                        {
                            var list = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0; i < list.Count; i++)
                            {
                                if (OnlineVersions.ContainsKey(list[i]) && list.Count > i + 1)
                                {
                                    MainWindow.Instance.DebugLogger.Text = $"Updating {list[i + 1]}";
                                    OnlineVersions[list[i]] = list[i + 1];
                                }
                            }
                        }

                        client.Dispose();
                    }
                }

                MainWindow.Instance.CurrentInstanceProgress.Text = $"Checking dependents...";

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

                MainWindow.Instance.CurrentInstanceProgress.Text = $"Verifying directory...";

                // In case the BepInEx/plugins folder doesn't exist
                if (!Directory.Exists(b))
                    Directory.CreateDirectory(b);

                #region RTFunctions

                if (MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/RTFunctions.dll") ||
                    OnlineVersions["RTFunctions"] != LocalVersions["RTFunctions"]))
                {
                    if (RTFile.FileExists(b + "/RTFunctions.disabled"))
                    {
                        File.Delete(b + "/RTFunctions.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/RTFunctions/releases/latest/download/RTFunctions.zip", b, "RTFunctions.zip");
                }

                if (!RTFile.FileExists(b + "/RTFunctions.dll") && RTFile.FileExists(b + "RTFunctions.disabled") || RTFile.FileExists(b + "/RTFunctions.dll") && !RTFile.FileExists(b + "RTFunctions.disabled"))
                {
                    bool enabled = MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/RTFunctions.dll" : b + "/RTFunctions.disabled", !enabled ? b + "/RTFunctions.dll" : b + "/RTFunctions.disabled");
                }

                #endregion
                
                #region EditorManagement

                if (MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/EditorManagement.dll") ||
                    OnlineVersions["EditorManagement"] != LocalVersions["EditorManagement"]))
                {
                    if (RTFile.FileExists(b + "/EditorManagement.disabled"))
                    {
                        File.Delete(b + "/EditorManagement.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/EditorManagement/releases/latest/download/EditorManagement.zip", b, "EditorManagement.zip");
                }

                if (!RTFile.FileExists(b + "/EditorManagement.dll") && RTFile.FileExists(b + "EditorManagement.disabled") || RTFile.FileExists(b + "/EditorManagement.dll") && !RTFile.FileExists(b + "EditorManagement.disabled"))
                {
                    bool enabled = MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/EditorManagement.dll" : b + "/EditorManagement.disabled", !enabled ? b + "/EditorManagement.dll" : b + "/EditorManagement.disabled");
                }

                #endregion

                #region EventsCore

                if (MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/EventsCore.dll") ||
                    OnlineVersions["EventsCore"] != LocalVersions["EventsCore"]))
                {
                    if (RTFile.FileExists(b + "/EventsCore.disabled"))
                    {
                        File.Delete(b + "/EventsCore.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/EventsCore/releases/latest/download/EventsCore.zip", b, "EventsCore.zip");
                }
                
                if (!RTFile.FileExists(b + "/EventsCore.dll") && RTFile.FileExists(b + "EventsCore.disabled") || RTFile.FileExists(b + "/EventsCore.dll") && !RTFile.FileExists(b + "EventsCore.disabled"))
                {
                    bool enabled = MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/EventsCore.dll" : b + "/EventsCore.disabled", !enabled ? b + "/EventsCore.dll" : b + "/EventsCore.disabled");
                }

                #endregion

                #region CreativePlayers

                if (MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/CreativePlayers.dll") ||
                    OnlineVersions["CreativePlayers"] != LocalVersions["CreativePlayers"]))
                {
                    if (RTFile.FileExists(b + "/CreativePlayers.disabled"))
                    {
                        File.Delete(b + "/CreativePlayers.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/CreativePlayers/releases/latest/download/CreativePlayers.zip", b, "CreativePlayers.zip");
                }
                
                if (!RTFile.FileExists(b + "/CreativePlayers.dll") && RTFile.FileExists(b + "CreativePlayers.disabled") || RTFile.FileExists(b + "/CreativePlayers.dll") && !RTFile.FileExists(b + "CreativePlayers.disabled"))
                {
                    bool enabled = MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/CreativePlayers.dll" : b + "/CreativePlayers.disabled", !enabled ? b + "/CreativePlayers.dll" : b + "/CreativePlayers.disabled");
                }

                #endregion

                #region ObjectModifiers

                if (MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/ObjectModifiers.dll") ||
                    OnlineVersions["ObjectModifiers"] != LocalVersions["ObjectModifiers"]))
                {
                    if (RTFile.FileExists(b + "/ObjectModifiers.disabled"))
                    {
                        File.Delete(b + "/ObjectModifiers.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/ObjectModifiers/releases/latest/download/ObjectModifiers.zip", b, "ObjectModifiers.zip");
                }
                
                if (!RTFile.FileExists(b + "/ObjectModifiers.dll") && RTFile.FileExists(b + "ObjectModifiers.disabled") || RTFile.FileExists(b + "/ObjectModifiers.dll") && !RTFile.FileExists(b + "ObjectModifiers.disabled"))
                {
                    bool enabled = MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ObjectModifiers.dll" : b + "/ObjectModifiers.disabled", !enabled ? b + "/ObjectModifiers.dll" : b + "/ObjectModifiers.disabled");
                }

                #endregion

                #region ArcadiaCustoms

                if (MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/ArcadiaCustoms.dll") ||
                    OnlineVersions["ArcadiaCustoms"] != LocalVersions["ArcadiaCustoms"]))
                {
                    if (RTFile.FileExists(b + "/ArcadiaCustoms.disabled"))
                    {
                        File.Delete(b + "/ArcadiaCustoms.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/ArcadiaCustoms/releases/latest/download/ArcadiaCustoms.zip", b, "ArcadiaCustoms.zip");
                }
                
                if (!RTFile.FileExists(b + "/ArcadiaCustoms.dll") && RTFile.FileExists(b + "ArcadiaCustoms.disabled") || RTFile.FileExists(b + "/ArcadiaCustoms.dll") && !RTFile.FileExists(b + "ArcadiaCustoms.disabled"))
                {
                    bool enabled = MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ArcadiaCustoms.dll" : b + "/ArcadiaCustoms.disabled", !enabled ? b + "/ArcadiaCustoms.dll" : b + "/ArcadiaCustoms.disabled");
                }

                #endregion

                #region PageCreator

                if (MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/PageCreator.dll") ||
                    OnlineVersions["PageCreator"] != LocalVersions["PageCreator"]))
                {
                    if (RTFile.FileExists(b + "/PageCreator.disabled"))
                    {
                        File.Delete(b + "/PageCreator.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/PageCreator/releases/latest/download/PageCreator.zip", b, "PageCreator.zip");
                }
                
                if (!RTFile.FileExists(b + "/PageCreator.dll") && RTFile.FileExists(b + "PageCreator.disabled") || RTFile.FileExists(b + "/PageCreator.dll") && !RTFile.FileExists(b + "PageCreator.disabled"))
                {
                    bool enabled = MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/PageCreator.dll" : b + "/PageCreator.disabled", !enabled ? b + "/PageCreator.dll" : b + "/PageCreator.disabled");
                }

                #endregion

                #region ExampleCompanion

                if (MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/ExampleCompanion.dll") ||
                    OnlineVersions["ExampleCompanion"] != LocalVersions["ExampleCompanion"]))
                {
                    if (RTFile.FileExists(b + "/ExampleCompanion.disabled"))
                    {
                        File.Delete(b + "/ExampleCompanion.disabled");
                    }

                    DownloadFile("https://github.com/RTMecha/ExampleCompanion/releases/latest/download/ExampleCompanion.zip", b, "ExampleCompanion.zip");
                }
                
                if (!RTFile.FileExists(b + "/ExampleCompanion.dll") && RTFile.FileExists(b + "ExampleCompanion.disabled") || RTFile.FileExists(b + "/ExampleCompanion.dll") && !RTFile.FileExists(b + "ExampleCompanion.disabled"))
                {
                    bool enabled = MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ExampleCompanion.dll" : b + "/ExampleCompanion.disabled", !enabled ? b + "/ExampleCompanion.dll" : b + "/ExampleCompanion.disabled");
                }

                #endregion

                if (MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == true
                    && !RTFile.DirectoryExists(b + "/ConfigurationManager"))
                {
                    // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                    var rep = b.Replace("/BepInEx/plugins", "");

                    var rt = b + "/ConfigurationManager.zip";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/download/v18.2/BepInEx.ConfigurationManager.BepInEx5_v18.2.zip", rt);
                        RTFile.ZipUtil.UnZip(rt, rep);
                    }
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
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip", rt);
                        RTFile.ZipUtil.UnZip(rt, rep);
                    }
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
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://cdn.discordapp.com/attachments/1092449110805725256/1092449111141257296/EditorOnStartup.dll", rt);
                    }
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

                    RTFile.WriteToFile(projectArrhythmia.FolderPath + "settings/versions.lss", str);
                }

                MainWindow.Instance.CurrentInstanceProgress.Text = $"Finished updating!";

                //ProcessStartInfo startInfo = new ProcessStartInfo();
                //startInfo.FileName = MainWindow.Instance.Path;
                //Process.Start(startInfo);

                return true;
            }

            return false;
        }

        public static bool Open(ProjectArrhythmia projectArrhythmia)
        {
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.DebugLogger.Text = $"Start opening the game, please wait...";

                projectArrhythmia.SaveSettings();

                var a = projectArrhythmia.FolderPath;
                var b = a + BepInExPlugins;

                // Download BepInEx (Obviously will need BepInEx itself to run any mods)
                if (!Directory.Exists(a + "BepInEx"))
                {
                    var bep = a + "BepInEx-5.4.21.zip";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(BepInExURL, bep);

                        RTFile.ZipUtil.UnZip(bep, a);
                        client.Dispose();
                    }
                }

                // Load Versions (For version comparison, so we're not re-downloading the mods every time we launch the game)
                {
                    if (RTFile.FileExists(projectArrhythmia.FolderPath + "settings/versions.lss"))
                    {
                        var localVersions = RTFile.ReadFromFile(projectArrhythmia.FolderPath + "settings/versions.lss");

                        if (!string.IsNullOrEmpty(localVersions))
                        {
                            var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            for (int i = 0; i < list.Count; i++)
                            {
                                if (LocalVersions.ContainsKey(list[i]) && list.Count > i + 1)
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
                                var version = projectArrhythmia.GetModVersion($"{b}/{key}.dll");

                                LocalVersions[key] = version;
                            }
                        }
                    }
                }

                MainWindow.Instance.CurrentInstanceProgress.Text = $"Checking dependants...";

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

                MainWindow.Instance.CurrentInstanceProgress.Text = $"Verifying directory...";

                // In case the BepInEx/plugins folder doesn't exist
                if (!Directory.Exists(b))
                    Directory.CreateDirectory(b);

                #region RTFunctions

                if (MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == true && !RTFile.FileExists(b + "/RTFunctions.dll") && !RTFile.FileExists(b + "/RTFunctions.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/RTFunctions/releases/latest/download/RTFunctions.zip", b, "RTFunctions.zip", true);
                }
                else if (!RTFile.FileExists(b + "/RTFunctions.dll") && RTFile.FileExists(b + "/RTFunctions.disabled") || RTFile.FileExists(b + "/RTFunctions.dll") && !RTFile.FileExists(b + "/RTFunctions.disabled"))
                {
                    bool enabled = MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/RTFunctions.dll" : b + "/RTFunctions.disabled", !enabled ? b + "/RTFunctions.dll" : b + "/RTFunctions.disabled");
                }

                #endregion

                #region EditorManagement

                if (MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == true && !RTFile.FileExists(b + "/EditorManagement.dll") && !RTFile.FileExists(b + "/EditorManagement.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/EditorManagement/releases/latest/download/EditorManagement.zip", b, "EditorManagement.zip", true);
                }
                else if (!RTFile.FileExists(b + "/EditorManagement.dll") && RTFile.FileExists(b + "/EditorManagement.disabled") || RTFile.FileExists(b + "/EditorManagement.dll") && !RTFile.FileExists(b + "/EditorManagement.disabled"))
                {
                    bool enabled = MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/EditorManagement.dll" : b + "/EditorManagement.disabled", !enabled ? b + "/EditorManagement.dll" : b + "/EditorManagement.disabled");
                }

                #endregion

                #region EventsCore

                if (MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == true && !RTFile.FileExists(b + "/EventsCore.dll") && !RTFile.FileExists(b + "/EventsCore.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/EventsCore/releases/latest/download/EventsCore.zip", b, "EventsCore.zip", true);
                }
                else if (!RTFile.FileExists(b + "/EventsCore.dll") && RTFile.FileExists(b + "/EventsCore.disabled") || RTFile.FileExists(b + "/EventsCore.dll") && !RTFile.FileExists(b + "/EventsCore.disabled"))
                {
                    bool enabled = MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/EventsCore.dll" : b + "/EventsCore.disabled", !enabled ? b + "/EventsCore.dll" : b + "/EventsCore.disabled");
                }

                #endregion
                
                #region CreativePlayers

                if (MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == true && !RTFile.FileExists(b + "/CreativePlayers.dll") && !RTFile.FileExists(b + "/CreativePlayers.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/CreativePlayers/releases/latest/download/CreativePlayers.zip", b, "CreativePlayers.zip", true);
                }
                else if (!RTFile.FileExists(b + "/CreativePlayers.dll") && RTFile.FileExists(b + "CreativePlayers.disabled") || RTFile.FileExists(b + "/CreativePlayers.dll") && !RTFile.FileExists(b + "/CreativePlayers.disabled"))
                {
                    bool enabled = MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/CreativePlayers.dll" : b + "/CreativePlayers.disabled", !enabled ? b + "/CreativePlayers.dll" : b + "/CreativePlayers.disabled");
                }

                #endregion

                #region ObjectModifiers

                if (MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == true && !RTFile.FileExists(b + "/ObjectModifiers.dll") && !RTFile.FileExists(b + "/ObjectModifiers.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/ObjectModifiers/releases/latest/download/ObjectModifiers.zip", b, "ObjectModifiers.zip", true);
                }
                else if (!RTFile.FileExists(b + "/ObjectModifiers.dll") && RTFile.FileExists(b + "/ObjectModifiers.disabled") || RTFile.FileExists(b + "/ObjectModifiers.dll") && !RTFile.FileExists(b + "/ObjectModifiers.disabled"))
                {
                    bool enabled = MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ObjectModifiers.dll" : b + "/ObjectModifiers.disabled", !enabled ? b + "/ObjectModifiers.dll" : b + "/ObjectModifiers.disabled");
                }

                #endregion

                #region ArcadiaCustoms

                if (MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == true && !RTFile.FileExists(b + "/ArcadiaCustoms.dll") && !RTFile.FileExists(b + "/ArcadiaCustoms.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/ArcadiaCustoms/releases/latest/download/ArcadiaCustoms.zip", b, "ArcadiaCustoms.zip", true);
                }
                else if (!RTFile.FileExists(b + "/ArcadiaCustoms.dll") && RTFile.FileExists(b + "/ArcadiaCustoms.disabled") || RTFile.FileExists(b + "/ArcadiaCustoms.dll") && !RTFile.FileExists(b + "/ArcadiaCustoms.disabled"))
                {
                    bool enabled = MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ArcadiaCustoms.dll" : b + "/ArcadiaCustoms.disabled", !enabled ? b + "/ArcadiaCustoms.dll" : b + "/ArcadiaCustoms.disabled");
                }

                #endregion

                #region PageCreator

                if (MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == true && !RTFile.FileExists(b + "/PageCreator.dll") && !RTFile.FileExists(b + "/PageCreator.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/PageCreator/releases/latest/download/PageCreator.zip", b, "PageCreator.zip", true);
                }
                else if (!RTFile.FileExists(b + "/PageCreator.dll") && RTFile.FileExists(b + "/PageCreator.disabled") || RTFile.FileExists(b + "/PageCreator.dll") && !RTFile.FileExists(b + "/PageCreator.disabled"))
                {
                    bool enabled = MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/PageCreator.dll" : b + "/PageCreator.disabled", !enabled ? b + "/PageCreator.dll" : b + "/PageCreator.disabled");
                }

                #endregion

                #region ExampleCompanion

                if (MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == true && !RTFile.FileExists(b + "/ExampleCompanion.dll") && !RTFile.FileExists(b + "/ExampleCompanion.disabled"))
                {
                    DownloadFile("https://github.com/RTMecha/ExampleCompanion/releases/latest/download/ExampleCompanion.zip", b, "ExampleCompanion.zip", true);
                }
                else if (!RTFile.FileExists(b + "/ExampleCompanion.dll") && RTFile.FileExists(b + "/ExampleCompanion.disabled") || RTFile.FileExists(b + "/ExampleCompanion.dll") && !RTFile.FileExists(b + "/ExampleCompanion.disabled"))
                {
                    bool enabled = MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == false;

                    RTFile.MoveFile(enabled ? b + "/ExampleCompanion.dll" : b + "/ExampleCompanion.disabled", !enabled ? b + "/ExampleCompanion.dll" : b + "/ExampleCompanion.disabled");
                }

                #endregion

                if (MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == true
                    && !RTFile.DirectoryExists(b + "/ConfigurationManager"))
                {
                    // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                    var rep = b.Replace("/BepInEx/plugins", "");

                    var rt = b + "/ConfigurationManager.zip";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/download/v18.2/BepInEx.ConfigurationManager.BepInEx5_v18.2.zip", rt);
                        RTFile.ZipUtil.UnZip(rt, rep);
                    }
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
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip", rt);
                        RTFile.ZipUtil.UnZip(rt, rep);
                    }
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
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://cdn.discordapp.com/attachments/1092449110805725256/1092449111141257296/EditorOnStartup.dll", rt);
                    }
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
                    DownloadFile("https://github.com/RTMecha/RTFunctions/releases/download/Current/Beatmaps.zip", a, "Beatmaps.zip", true);

                // Save Versions (For later use when the game is relaunched)
                {
                    var str = "";

                    for (int i = 0; i < LocalVersions.Count; i++)
                    {
                        str += LocalVersions.ElementAt(i).Key + Environment.NewLine + LocalVersions.ElementAt(i).Value + Environment.NewLine;
                    }

                    RTFile.WriteToFile(projectArrhythmia.FolderPath + "settings/versions.lss", str);
                }

                MainWindow.Instance.CurrentInstanceProgress.Text = $"Opening...";

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = projectArrhythmia.Path;
                Process.Start(startInfo);

                return true;
            }

            return false;
        }

        public static void DownloadFile(string url, string output, string file, bool instance = false)
        {
            if (MainWindow.Instance != null)
                (instance ? MainWindow.Instance.CurrentInstanceProgress : MainWindow.Instance.DebugLogger).Text = $"Downloading {file}...";

            var rt = output + "/" + file;
            using (var client = new WebClient())
            {
                client.DownloadFile(url, rt);
                RTFile.ZipUtil.UnZip(rt, output);
            }
        }
    }
}
