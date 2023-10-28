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
                        { "CustomShapes", "1.0.0" },
                        { "ExampleCompanion", "1.0.0" },
                    };
                return onlineVersions;
            }
            set
            {
                onlineVersions = value;
            }
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
                        { "CustomShapes", "1.0.0" },
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
                                    OnlineVersions[list[i]] = list[i + 1];
                                }
                            }
                        }

                        client.Dispose();
                    }
                }

                // Verify (Makes sure RTFunctions is on if any dependant mod is)
                {
                    if (MainWindow.Instance.EditorManagementEnabled.IsChecked == true ||
                        MainWindow.Instance.EventsCoreEnabled.IsChecked == true ||
                        MainWindow.Instance.CreativePlayersEnabled.IsChecked == true || 
                        MainWindow.Instance.ObjectModifiersEnabled.IsChecked == true ||
                        MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == true ||
                        MainWindow.Instance.PageCreatorEnabled.IsChecked == true ||
                        MainWindow.Instance.CustomShapesEnabled.IsChecked == true ||
                        MainWindow.Instance.ExampleCompanionEnabled.IsChecked == true)
                    {
                        MainWindow.Instance.RTFunctionsEnabled.IsChecked = true;
                    }
                }

                var b = a + BepInExPlugins;
                // In case the BepInEx/plugins folder doesn't exist
                if (!Directory.Exists(b))
                    Directory.CreateDirectory(b);

                if (MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/RTFunctions.dll") || OnlineVersions["RTFunctions"] != LocalVersions["RTFunctions"]))
                    DownloadFile("https://github.com/RTMecha/RTFunctions/releases/download/Current/RTFunctions.zip", b, "RTFunctions.zip");
                else if (RTFile.FileExists(b + "/RTFunctions.dll") && MainWindow.Instance.RTFunctionsEnabled != null && MainWindow.Instance.RTFunctionsEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/RTFunctions.dll");
                    }
                    catch
                    {

                    }
                }
                
                if (MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/EditorManagement.dll") || OnlineVersions["EditorManagement"] != LocalVersions["EditorManagement"]))
                    DownloadFile("https://github.com/RTMecha/EditorManagement/releases/download/Current/EditorManagement.zip", b, "EditorManagement.zip");
                else if (RTFile.FileExists(b + "/EditorManagement.dll") && MainWindow.Instance.EditorManagementEnabled != null && MainWindow.Instance.EditorManagementEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/EditorManagement.dll");
                    }
                    catch
                    {

                    }
                }

                if (MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/EventsCore.dll") || OnlineVersions["EventsCore"] != LocalVersions["EventsCore"]))
                    DownloadFile("https://github.com/RTMecha/EventsCore/releases/download/Current/EventsCore.zip", b, "EventsCore.zip");
                else if (RTFile.FileExists(b + "/EventsCore.dll") && MainWindow.Instance.EventsCoreEnabled != null && MainWindow.Instance.EventsCoreEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/EventsCore.dll");
                    }
                    catch
                    {

                    }
                }

                if (MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/CreativePlayers.dll") || OnlineVersions["CreativePlayers"] != LocalVersions["CreativePlayers"]))
                    DownloadFile("https://github.com/RTMecha/CreativePlayers/releases/download/Current/CreativePlayers.zip", b, "CreativePlayers.zip");
                else if (RTFile.FileExists(b + "/CreativePlayers.dll") && MainWindow.Instance.CreativePlayersEnabled != null && MainWindow.Instance.CreativePlayersEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/CreativePlayers.dll");
                    }
                    catch
                    {

                    }
                }

                if (MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/ObjectModifiers.dll") || OnlineVersions["ObjectModifiers"] != LocalVersions["ObjectModifiers"]))
                    DownloadFile("https://github.com/RTMecha/ObjectModifiers/releases/download/Current/ObjectModifiers.zip", b, "ObjectModifiers.zip");
                else if (RTFile.FileExists(b + "/ObjectModifiers.dll") && MainWindow.Instance.ObjectModifiersEnabled != null && MainWindow.Instance.ObjectModifiersEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/ObjectModifiers.dll");
                    }
                    catch
                    {

                    }
                }

                if (MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/ArcadiaCustoms.dll") || OnlineVersions["ArcadiaCustoms"] != LocalVersions["ArcadiaCustoms"]))
                    DownloadFile("https://github.com/RTMecha/ArcadiaCustoms/releases/download/Current/ArcadiaCustoms.zip", b, "ArcadiaCustoms.zip");
                else if (RTFile.FileExists(b + "/ArcadiaCustoms.dll") && MainWindow.Instance.ArcadiaCustomsEnabled != null && MainWindow.Instance.ArcadiaCustomsEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/ArcadiaCustoms.dll");
                    }
                    catch
                    {

                    }
                }

                if (MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/PageCreator.dll") || OnlineVersions["PageCreator"] != LocalVersions["PageCreator"]))
                    DownloadFile("https://github.com/RTMecha/PageCreator/releases/download/Current/PageCreator.zip", b, "PageCreator.zip");
                else if (RTFile.FileExists(b + "/PageCreator.dll") && MainWindow.Instance.PageCreatorEnabled != null && MainWindow.Instance.PageCreatorEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/PageCreator.dll");
                    }
                    catch
                    {

                    }
                }

                if (MainWindow.Instance.CustomShapesEnabled != null && MainWindow.Instance.CustomShapesEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/CustomShapes.dll") || OnlineVersions["CustomShapes"] != LocalVersions["CustomShapes"]))
                    DownloadFile("https://github.com/RTMecha/CustomShapes/releases/download/Current/CustomShapes.zip", b, "CustomShapes.zip");
                else if (RTFile.FileExists(b + "/CustomShapes.dll") && MainWindow.Instance.CustomShapesEnabled != null && MainWindow.Instance.CustomShapesEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/CustomShapes.dll");
                    }
                    catch
                    {

                    }
                }

                if (MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == true
                    && (!RTFile.FileExists(b + "/ExampleCompanion.dll") || OnlineVersions["ExampleCompanion"] != LocalVersions["ExampleCompanion"]))
                    DownloadFile("https://github.com/RTMecha/ExampleCompanion/releases/download/Current/ExampleCompanion.zip", b, "ExampleCompanion.zip");
                else if (RTFile.FileExists(b + "/ExampleCompanion.dll") && MainWindow.Instance.ExampleCompanionEnabled != null && MainWindow.Instance.ExampleCompanionEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/ExampleCompanion.dll");
                    }
                    catch
                    {

                    }
                }

                if (MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == true
                    && !RTFile.FileExists(b + "/ConfigurationManager.dll"))
                {
                    // Since the dll's are in sub-folders within the zip file, we have to handle them differently compared to the other mods.
                    var rep = b.Replace("/BepInEx/plugins", "");
                    Directory.Exists(rep);

                    var rt = b + "/ConfigurationManager.zip";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/download/v18.0.1/BepInEx.ConfigurationManager_v18.0.1.zip", rt);
                        RTFile.ZipUtil.UnZip(rt, rep);
                    }
                }
                else if (RTFile.FileExists(b + "/ConfigurationManager.dll") && MainWindow.Instance.ConfigurationManagerEnabled != null && MainWindow.Instance.ConfigurationManagerEnabled.IsChecked == false)
                {
                    try
                    {
                        File.Delete(b + "/ConfigurationManager.dll");
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

                // Save Versions (For later use when the game is relaunched)
                {
                    var str = "";

                    for (int i = 0; i < OnlineVersions.Count; i++)
                    {
                        str += OnlineVersions.ElementAt(i).Key + Environment.NewLine + OnlineVersions.ElementAt(i).Value + Environment.NewLine;
                    }

                    RTFile.WriteToFile(RTFile.ApplicationDirectory + "versions.lss", str);
                }

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = MainWindow.Instance.Path;
                Process.Start(startInfo);

                return true;
            }

            return false;
        }

        public static void DownloadFile(string url, string output, string file)
        {
            var rt = output + "/" + file;
            using (var client = new WebClient())
            {
                client.DownloadFile(url, rt);
                RTFile.ZipUtil.UnZip(rt, output);
            }
        }
    }
}
