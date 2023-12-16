using ProjectLauncher.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }

        public static string Version => "1.0.0";

        public MainWindow()
        {
            InitializeComponent();

            Instance = this;

            if (RTFile.FileExists(RTFile.ApplicationDirectory + "settings.lss"))
            {
                var settings = RTFile.ReadFromFile(RTFile.ApplicationDirectory + "settings.lss");

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
                                        CheckedAll.IsChecked = value;
                                    break;
                                }
                            case "RTFunctions":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        RTFunctionsEnabled.IsChecked = value;
                                    break;
                                }
                            case "EditorManagement":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        EditorManagementEnabled.IsChecked = value;
                                    break;
                                }
                            case "EventsCore":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        EventsCoreEnabled.IsChecked = value;
                                    break;
                                }
                            case "CreativePlayers":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        CreativePlayersEnabled.IsChecked = value;
                                    break;
                                }
                            case "ObjectModifiers":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        ObjectModifiersEnabled.IsChecked = value;
                                    break;
                                }
                            case "ArcadiaCustoms":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        ArcadiaCustomsEnabled.IsChecked = value;
                                    break;
                                }
                            case "PageCreator":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        PageCreatorEnabled.IsChecked = value;
                                    break;
                                }
                            case "ExampleCompanion":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        ExampleCompanionEnabled.IsChecked = value;
                                    break;
                                }
                            case "ConfigurationManager":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        ConfigurationManagerEnabled.IsChecked = value;
                                    break;
                                }
                            case "UnityExplorer":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        UnityExplorerEnabled.IsChecked = value;
                                    break;
                                }
                            case "EditorOnStartup":
                                {
                                    if (list.Count > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        EditorOnStartupEnabled.IsChecked = value;
                                    break;
                                }
                            case "Path":
                                {
                                    if (list.Count > i + 1)
                                        PathField.Text = list[i + 1];
                                    break;
                                }
                        }
                    }
                }
            }

            Title = $"Project Launcher {Version}";

            using (var client = new WebClient())
            {
                var data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/updates.lss");

                if (!string.IsNullOrEmpty(data))
                {
                    var list = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var textBlock = new TextBlock();
                        textBlock.Text = list[i];
                        RTFunctionsUpdates.Items.Add(textBlock);
                    }
                }

                data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/EditorManagement/master/updates.lss");

                if (!string.IsNullOrEmpty(data))
                {
                    var list = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    for (int i = 0; i < list.Count; i++)
                    {
                        var textBlock = new TextBlock();
                        textBlock.Text = list[i];
                        EditorManagementUpdates.Items.Add(textBlock);
                    }
                }
            }

            Closed += delegate
            {

            };
        }

        public void SaveSettings()
        {
            string str = "";

            if (CheckedAll == null)
                return;

            str += "All" + Environment.NewLine + CheckedAll.IsChecked.ToString() + Environment.NewLine;
            str += "RTFunctions" + Environment.NewLine + RTFunctionsEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "EditorManagement" + Environment.NewLine + EditorManagementEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "EventsCore" + Environment.NewLine + EventsCoreEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "CreativePlayers" + Environment.NewLine + CreativePlayersEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ObjectModifiers" + Environment.NewLine + ObjectModifiersEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ArcadiaCustoms" + Environment.NewLine + ArcadiaCustomsEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "PageCreator" + Environment.NewLine + PageCreatorEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ExampleCompanion" + Environment.NewLine + ExampleCompanionEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "ConfigurationManager" + Environment.NewLine + ConfigurationManagerEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "UnityExplorer" + Environment.NewLine + UnityExplorerEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "EditorOnStartup" + Environment.NewLine + EditorOnStartupEnabled.IsChecked.ToString() + Environment.NewLine;
            str += "Path" + Environment.NewLine + PathField.Text + Environment.NewLine;

            RTFile.WriteToFile(RTFile.ApplicationDirectory + "settings.lss", str);
        }

        string path;
        public string Path
        {
            get
            {
                path = PathField.Text.Replace("\\", "/").Replace("//", "");
                if (!path.Contains("/Project Arrhythmia.exe"))
                    path += "/Project Arrhythmia.exe";
                if (!path.Contains("Project Arrhythmia.exe"))
                    path += "Project Arrhythmia.exe";

                return path;
            }
        }

        public void Log(string message) => Console.Write(message);

        void PathField_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveSettings();
        }

        void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ModUpdater.CheckForUpdates();

                //string p = Path;
                //if (!p.Contains("/Project Arrhythmia.exe"))
                //    p += "/Project Arrhythmia.exe";
                //if (!p.Contains("Project Arrhythmia.exe"))
                //    p += "Project Arrhythmia.exe";

                //ProcessStartInfo startInfo = new ProcessStartInfo();
                //startInfo.FileName = p;
                //Process.Start(startInfo);
            }
            catch (Exception ex)
            {

            }
        }

        void CheckedAll_Checked(object sender, RoutedEventArgs e)
        {
            RTFunctionsEnabled.IsChecked = true;
            EditorManagementEnabled.IsChecked = true;
            EventsCoreEnabled.IsChecked = true;
            CreativePlayersEnabled.IsChecked = true;
            ObjectModifiersEnabled.IsChecked = true;
            ArcadiaCustomsEnabled.IsChecked = true;
            PageCreatorEnabled.IsChecked = true;
            ExampleCompanionEnabled.IsChecked = true;
            ConfigurationManagerEnabled.IsChecked = true;
            UnityExplorerEnabled.IsChecked = true;
            EditorOnStartupEnabled.IsChecked = true;
            isChanging = false;
        }

        bool isChanging;

        void CheckedAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isChanging)
            {
                RTFunctionsEnabled.IsChecked = false;
                EditorManagementEnabled.IsChecked = false;
                EventsCoreEnabled.IsChecked = false;
                CreativePlayersEnabled.IsChecked = false;
                ObjectModifiersEnabled.IsChecked = false;
                ArcadiaCustomsEnabled.IsChecked = false;
                PageCreatorEnabled.IsChecked = false;
                ExampleCompanionEnabled.IsChecked = false;
                ConfigurationManagerEnabled.IsChecked = false;
                UnityExplorerEnabled.IsChecked = false;
                EditorOnStartupEnabled.IsChecked = false;
            }
            isChanging = false;
        }

        void RTFunctionsEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void RTFunctionsEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = false;
            EditorManagementEnabled.IsChecked = false;
            EventsCoreEnabled.IsChecked = false;
            CreativePlayersEnabled.IsChecked = false;
            ObjectModifiersEnabled.IsChecked = false;
            ArcadiaCustomsEnabled.IsChecked = false;
            PageCreatorEnabled.IsChecked = false;
            ExampleCompanionEnabled.IsChecked = false;
            CheckedAll.IsChecked = false;
        }

        void EditorManagementEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (EditorManagementEnabled.IsChecked == true && RTFunctionsEnabled.IsChecked != true)
                RTFunctionsEnabled.IsChecked = true;

            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void EditorManagementEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void EventsCoreEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (EventsCoreEnabled.IsChecked == true && RTFunctionsEnabled.IsChecked != true)
                RTFunctionsEnabled.IsChecked = true;

            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void EventsCoreEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void CreativePlayersEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (CreativePlayersEnabled.IsChecked == true && RTFunctionsEnabled.IsChecked != true)
                RTFunctionsEnabled.IsChecked = true;

            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void CreativePlayersEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void ObjectModifiersEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (ObjectModifiersEnabled.IsChecked == true && RTFunctionsEnabled.IsChecked != true)
                RTFunctionsEnabled.IsChecked = true;

            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void ObjectModifiersEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void ArcadiaCustomsEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (ArcadiaCustomsEnabled.IsChecked == true && RTFunctionsEnabled.IsChecked != true)
                RTFunctionsEnabled.IsChecked = true;

            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void ArcadiaCustomsEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void PageCreatorEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (PageCreatorEnabled.IsChecked == true && RTFunctionsEnabled.IsChecked != true)
                RTFunctionsEnabled.IsChecked = true;

            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void PageCreatorEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void ExampleCompanionEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (ExampleCompanionEnabled.IsChecked == true && RTFunctionsEnabled.IsChecked != true)
                RTFunctionsEnabled.IsChecked = true;

            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void ExampleCompanionEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void ConfigurationManagerEnabled_Checked(object sender, RoutedEventArgs e)
        {

            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void ConfigurationManagerEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void UnityExplorerEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void UnityExplorerEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void EditorOnStartupEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsAllChecked())
                CheckedAll.IsChecked = true;
            else
            {
                isChanging = true;
                CheckedAll.IsChecked = false;
            }
        }

        void EditorOnStartupEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            isChanging = true;
            CheckedAll.IsChecked = false;
        }

        void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public bool IsAllChecked() => RTFunctionsEnabled.IsChecked == true &&
            EditorManagementEnabled.IsChecked == true &&
            EventsCoreEnabled.IsChecked == true &&
            CreativePlayersEnabled.IsChecked == true &&
            ObjectModifiersEnabled.IsChecked == true &&
            ArcadiaCustomsEnabled.IsChecked == true &&
            ExampleCompanionEnabled.IsChecked == true &&
            ConfigurationManagerEnabled.IsChecked == true &&
            UnityExplorerEnabled.IsChecked == true &&
            EditorOnStartupEnabled.IsChecked == true;
    }
}
