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

        public static string Version => "1.1.0";

        public static int MaxUpdateNotesLines => 134;

        public List<ProjectArrhythmia> Instances { get; set; } = new List<ProjectArrhythmia>();
        public FileSystemWatcher InstanceWatcher { get; set; }

        public ProjectArrhythmia Current { get; set; } = null;

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
                    var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                    for (int i = 0; i < list.Count; i++)
                    {
                        var textBlock = new TextBlock();
                        textBlock.Text = list[i];
                        textBlock.Background = new SolidColorBrush(Color.FromRgb(224, 181, 100));
                        RTFunctionsUpdates.Items.Add(textBlock);
                    }
                }

                data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/EditorManagement/master/updates.lss");

                if (!string.IsNullOrEmpty(data))
                {
                    var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                    for (int i = 0; i < list.Count; i++)
                    {
                        var textBlock = new TextBlock();
                        textBlock.Text = list[i];
                        textBlock.Background = new SolidColorBrush(Color.FromRgb(224, 181, 100));
                        EditorManagementUpdates.Items.Add(textBlock);
                    }
                }

                try
                {
                    data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/EventsCore/master/updates.lss");

                    if (!string.IsNullOrEmpty(data))
                    {
                        var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                        for (int i = 0; i < list.Count; i++)
                        {
                            var textBlock = new TextBlock();
                            textBlock.Text = list[i];
                            textBlock.Background = new SolidColorBrush(Color.FromRgb(224, 181, 100));
                            EventsCoreUpdates.Items.Add(textBlock);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"EventsCore Update Notes Exception. {ex}");
                }

                try
                {
                    data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/CreativePlayers/master/updates.lss");

                    if (!string.IsNullOrEmpty(data))
                    {
                        var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                        for (int i = 0; i < list.Count; i++)
                        {
                            var textBlock = new TextBlock();
                            textBlock.Text = list[i];
                            textBlock.Background = new SolidColorBrush(Color.FromRgb(224, 181, 100));
                            CreativePlayersUpdates.Items.Add(textBlock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CreativePlayers Update Notes Exception. {ex}");
                }

                try
                {
                    data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/ObjectModifiers/master/updates.lss");

                    if (!string.IsNullOrEmpty(data))
                    {
                        var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                        for (int i = 0; i < list.Count; i++)
                        {
                            var textBlock = new TextBlock();
                            textBlock.Text = list[i];
                            textBlock.Background = new SolidColorBrush(Color.FromRgb(224, 181, 100));
                            ObjectModifiersUpdates.Items.Add(textBlock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ObjectModifiers Update Notes Exception. {ex}");
                }

                try
                {
                    data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/ArcadiaCustoms/master/updates.lss");

                    if (!string.IsNullOrEmpty(data))
                    {
                        var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                        for (int i = 0; i < list.Count; i++)
                        {
                            var textBlock = new TextBlock();
                            textBlock.Text = list[i];
                            textBlock.Background = new SolidColorBrush(Color.FromRgb(224, 181, 100));
                            ArcadiaCustomsUpdates.Items.Add(textBlock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ArcadiaCustoms Update Notes Exception. {ex}");
                }

                try
                {
                    data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/PageCreator/master/updates.lss");

                    if (!string.IsNullOrEmpty(data))
                    {
                        var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                        for (int i = 0; i < list.Count; i++)
                        {
                            var textBlock = new TextBlock();
                            textBlock.Text = list[i];
                            textBlock.Background = new SolidColorBrush(Color.FromRgb(224, 181, 100));
                            PageCreatorUpdates.Items.Add(textBlock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"PageCreator Update Notes Exception. {ex}");
                }

                try
                {
                    data = client.DownloadString("https://raw.githubusercontent.com/RTMecha/ExampleCompanion/master/updates.lss");

                    if (!string.IsNullOrEmpty(data))
                    {
                        var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                        for (int i = 0; i < list.Count; i++)
                        {
                            var textBlock = new TextBlock();
                            textBlock.Text = list[i];
                            textBlock.Background = new SolidColorBrush(Color.FromRgb(224, 181, 100));
                            ExampleCompanionUpdates.Items.Add(textBlock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ExampleCompanion Update Notes Exception. {ex}");
                }
            }

            if (RTFile.FileExists(RTFile.ApplicationDirectory + "versions.lss"))
            {
                var localVersions = RTFile.ReadFromFile(RTFile.ApplicationDirectory + "versions.lss");

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
                        RTFunctionsEnabled.Content = $"{list[0]} - Installed: {list[1]}" + (list[1] == onlineVersions[1] ? "" : $" | Update Available: {onlineVersions[1]}");
                        EditorManagementEnabled.Content = $"{list[2]} - Installed: {list[3]}" + (list[3] == onlineVersions[3] ? "" : $" | Update Available: {onlineVersions[3]}");
                        EventsCoreEnabled.Content = $"{list[4]} - Installed: {list[5]}" + (list[5] == onlineVersions[5] ? "" : $" | Update Available: {onlineVersions[5]}");
                        CreativePlayersEnabled.Content = $"{list[6]} - Installed: {list[7]}" + (list[7] == onlineVersions[7] ? "" : $" | Update Available: {onlineVersions[7]}");
                        ObjectModifiersEnabled.Content = $"{list[8]} - Installed: {list[9]}" + (list[9] == onlineVersions[9] ? "" : $" | Update Available: {onlineVersions[9]}");
                        ArcadiaCustomsEnabled.Content = $"{list[10]} - Installed: {list[11]}" + (list[11] == onlineVersions[11] ? "" : $" | Update Available: {onlineVersions[11]}");
                        PageCreatorEnabled.Content = $"{list[12]} - Installed: {list[13]}" + (list[13] == onlineVersions[13] ? "" : $" | Update Available: {onlineVersions[13]}");
                        ExampleCompanionEnabled.Content = $"{list[14]} - Installed: {list[15]}" + (list[15] == onlineVersions[15] ? "" : $" | Update Available: {onlineVersions[15]}");
                    }
                }
            }

            if (RTFile.DirectoryExists(RTFile.ApplicationDirectory + "instances"))
            {
                InstanceWatcher = new FileSystemWatcher
                {
                    Path = RTFile.ApplicationDirectory + "instances",
                };
                InstanceWatcher.Created += OnInstancesFolderChanged;
                InstanceWatcher.Changed += OnInstancesFolderChanged;
                InstanceWatcher.Deleted += OnInstancesFolderChanged;
                InstanceWatcher.Renamed += OnInstancesFolderChanged;
            }

            LoadInstances();

            Closed += delegate
            {

            };
        }

        public void OnInstancesFolderChanged(object sender, FileSystemEventArgs e)
        {
            LoadInstances();
        }

        public void LoadInstances()
        {
            if (RTFile.DirectoryExists(RTFile.ApplicationDirectory + "instances") && InstanceWatcher == null)
            {
                InstanceWatcher = new FileSystemWatcher
                {
                    Path = RTFile.ApplicationDirectory + "instances",
                };
                InstanceWatcher.Created += OnInstancesFolderChanged;
                InstanceWatcher.Changed += OnInstancesFolderChanged;
                InstanceWatcher.Deleted += OnInstancesFolderChanged;
                InstanceWatcher.Renamed += OnInstancesFolderChanged;
            }

            if (RTFile.DirectoryExists(RTFile.ApplicationDirectory + "instances"))
            {
                Instances.Clear();
                InstancesList.Items.Clear();

                var doCreateNew = RTFile.DirectoryExists(RTFile.ApplicationDirectory + "PA Legacy - Pure");
                if (doCreateNew)
                {
                    var createNew = new Button();
                    createNew.Background = new SolidColorBrush(RTColor.FromHex("E0B564").FormsColor);
                    createNew.Content = "Create New Instance";
                    createNew.Name = "CreateNew";
                    createNew.Width = 729;
                    createNew.Click += CreateNew_Click;

                    InstancesList.Items.Add(createNew);
                }

                //if (NewNameViewbox != null)
                //{
                //    NewNameViewbox.Width = doCreateNew ? 134 : 745;
                //}

                if (NewNameLabel != null)
                    NewNameLabel.Text = doCreateNew ? "New Name:" :
                        "New Name: (requires an unmodded copy of Project Arrhythmia in the ProjectLauncher app folder called \"PA Legacy - Pure\")";

                var folders = Directory.GetDirectories(RTFile.ApplicationDirectory + "instances");
                foreach (var folder in folders)
                {
                    if (RTFile.FileExists(folder + "/Project Arrhythmia.exe"))
                    {
                        var instance = new ProjectArrhythmia(folder.Replace("\\", "/") + "/Project Arrhythmia.exe");
                        Instances.Add(instance);

                        var checkBox = new Button();
                        checkBox.Background = new SolidColorBrush(RTColor.Transparent.FormsColor);
                        checkBox.Foreground = new SolidColorBrush(RTColor.FromHex("FFCCA5").FormsColor);
                        checkBox.Content = System.IO.Path.GetFileName(folder);
                        checkBox.Click += Instance_Checked;
                        checkBox.Width = 729;

                        InstancesList.Items.Add(checkBox);
                    }
                }

                if (InstancesList.Items.Count > 0)
                {
                    InstancesList.SelectedItem = InstancesList.Items[doCreateNew ? 1 : 0];
                    SetSelected();
                }
            }
        }

        void CreateNew_Click(object sender, RoutedEventArgs e)
        {
            if (!RTFile.DirectoryExists($"{RTFile.ApplicationDirectory}/instances"))
                Directory.CreateDirectory($"{RTFile.ApplicationDirectory}/instances");

            Debug.WriteLine($"Create new clicked with New Name set as {NewName.Text}");
            if (RTFile.DirectoryExists(RTFile.ApplicationDirectory + "PA Legacy - Pure"))
            {
                Debug.WriteLine($"Getting directories...");
                var allDirectories = Directory.GetDirectories(RTFile.ApplicationDirectory + "PA Legacy - Pure", "*", SearchOption.AllDirectories);

                string s = RTFile.ApplicationDirectory + "instances/" + NewName.Text;
                int num = 0;
                while (RTFile.DirectoryExists(s))
                {
                    s = $"{RTFile.ApplicationDirectory}/instances/{NewName.Text} [{num}]";
                    num++;
                }

                var invalid = System.IO.Path.GetInvalidPathChars();
                if (s.IndexOfAny(invalid) != -1)
                {
                    for (int i = 0; i < invalid.Length; i++)
                    {
                        s = s.Replace(invalid[i].ToString(), "");
                    }
                }

                Debug.WriteLine($"Copying directories to new location {s}...");
                foreach (string dir in allDirectories)
                {
                    if (!dir.Contains("beatmaps"))
                    {
                        string dirToCreate = dir.Replace(RTFile.ApplicationDirectory + "PA Legacy - Pure", s);
                        Directory.CreateDirectory(dirToCreate);
                    }
                }

                Debug.WriteLine($"Getting files...");
                var allFiles = Directory.GetFiles(RTFile.ApplicationDirectory + "PA Legacy - Pure", "*.*", SearchOption.AllDirectories);

                Debug.WriteLine($"Copying files to new location {s}...");
                foreach (string newFile in allFiles)
                {
                    if (!newFile.Contains("beatmaps"))
                    {
                        File.Copy(newFile, newFile.Replace(RTFile.ApplicationDirectory + "PA Legacy - Pure", s));
                    }
                }

                Debug.WriteLine($"Reload instances...");
                LoadInstances();
            }
        }

        void LaunchInstance_Click(object sender, RoutedEventArgs e)
        {
            if (Current != null)
            {
                ModUpdater.Open(Current);
            }
        }

        void UpdateInstance_Click(object sender, RoutedEventArgs e)
        {
            if (Current != null)
            {
                ModUpdater.CheckForUpdates(Current);
            }
        }

        void Instance_Checked(object sender, RoutedEventArgs e)
        {
            InstancesList.SelectedItem = sender;
            SetSelected();
        }

        void InstancesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SetSelected();
        }

        void SetSelected()
        {
            int num = RTFile.DirectoryExists(RTFile.ApplicationDirectory + "PA Legacy - Pure") ? 1 : 0;
            foreach (var instance in Instances)
            {
                if (InstancesList.Items.Count > num && InstancesList.Items[num] == InstancesList.SelectedItem)
                {
                    Current = instance;
                    instance.LoadSettings();
                }

                num++;
            }
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

        string path = "";
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

        void PathField_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveSettings();
        }

        void PlayButton_Click(object sender, RoutedEventArgs e) => ModUpdater.Open();

        void UpdateButton_Click(object sender, RoutedEventArgs e) => ModUpdater.CheckForUpdates();

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

        void ResetToLocal_Click(object sender, RoutedEventArgs e)
        {
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

            if (RTFile.FileExists(RTFile.ApplicationDirectory + "versions.lss"))
            {
                var localVersions = RTFile.ReadFromFile(RTFile.ApplicationDirectory + "versions.lss");

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
                        RTFunctionsEnabled.Content = $"{list[0]} - Installed: {list[1]}" + (list[1] == onlineVersions[1] ? "" : $" | Update Available: {onlineVersions[1]}");
                        EditorManagementEnabled.Content = $"{list[2]} - Installed: {list[3]}" + (list[3] == onlineVersions[3] ? "" : $" | Update Available: {onlineVersions[3]}");
                        EventsCoreEnabled.Content = $"{list[4]} - Installed: {list[5]}" + (list[5] == onlineVersions[5] ? "" : $" | Update Available: {onlineVersions[5]}");
                        CreativePlayersEnabled.Content = $"{list[6]} - Installed: {list[7]}" + (list[7] == onlineVersions[7] ? "" : $" | Update Available: {onlineVersions[7]}");
                        ObjectModifiersEnabled.Content = $"{list[8]} - Installed: {list[9]}" + (list[9] == onlineVersions[9] ? "" : $" | Update Available: {onlineVersions[9]}");
                        ArcadiaCustomsEnabled.Content = $"{list[10]} - Installed: {list[11]}" + (list[11] == onlineVersions[11] ? "" : $" | Update Available: {onlineVersions[11]}");
                        PageCreatorEnabled.Content = $"{list[12]} - Installed: {list[13]}" + (list[13] == onlineVersions[13] ? "" : $" | Update Available: {onlineVersions[13]}");
                        ExampleCompanionEnabled.Content = $"{list[14]} - Installed: {list[15]}" + (list[15] == onlineVersions[15] ? "" : $" | Update Available: {onlineVersions[15]}");
                    }
                }
            }
        }

        void InstancesList_Selected(object sender, RoutedEventArgs e)
        {
            SetSelected();
        }
    }
}
