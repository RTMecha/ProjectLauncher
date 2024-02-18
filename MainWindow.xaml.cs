using Microsoft.Win32;
using ProjectLauncher.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        public bool init = false;
        public static string Version => "1.2.2";

        public static int MaxUpdateNotesLines => 120;

        public List<ProjectArrhythmia> Instances { get; set; } = new List<ProjectArrhythmia>();

        public ProjectArrhythmia Current { get; set; } = null;

        public async Task SetupUpdateNotes()
        {
            var http = new HttpClient();
            var data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/updates.lss");

            if (!string.IsNullOrEmpty(data))
            {
                var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                for (int i = 0; i < list.Count; i++)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = list[i];
                    textBlock.Background = new SolidColorBrush(RTColor.FromHex("211F1D").SystemColor);
                    textBlock.Foreground = new SolidColorBrush(RTColor.FromHex("E0B564").SystemColor);
                    RTFunctionsUpdates.Items.Add(textBlock);
                }
            }

            data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/EditorManagement/master/updates.lss");

            if (!string.IsNullOrEmpty(data))
            {
                var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                for (int i = 0; i < list.Count; i++)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = list[i];
                    textBlock.Background = new SolidColorBrush(RTColor.FromHex("211F1D").SystemColor);
                    textBlock.Foreground = new SolidColorBrush(RTColor.FromHex("E0B564").SystemColor);
                    EditorManagementUpdates.Items.Add(textBlock);
                }
            }

            data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/EventsCore/master/updates.lss");

            if (!string.IsNullOrEmpty(data))
            {
                var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                for (int i = 0; i < list.Count; i++)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = list[i];
                    textBlock.Background = new SolidColorBrush(RTColor.FromHex("211F1D").SystemColor);
                    textBlock.Foreground = new SolidColorBrush(RTColor.FromHex("E0B564").SystemColor);
                    EventsCoreUpdates.Items.Add(textBlock);
                }
            }

            data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/CreativePlayers/master/updates.lss");

            if (!string.IsNullOrEmpty(data))
            {
                var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                for (int i = 0; i < list.Count; i++)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = list[i];
                    textBlock.Background = new SolidColorBrush(RTColor.FromHex("211F1D").SystemColor);
                    textBlock.Foreground = new SolidColorBrush(RTColor.FromHex("E0B564").SystemColor);
                    CreativePlayersUpdates.Items.Add(textBlock);
                }
            }

            data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/ObjectModifiers/master/updates.lss");

            if (!string.IsNullOrEmpty(data))
            {
                var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                for (int i = 0; i < list.Count; i++)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = list[i];
                    textBlock.Background = new SolidColorBrush(RTColor.FromHex("211F1D").SystemColor);
                    textBlock.Foreground = new SolidColorBrush(RTColor.FromHex("E0B564").SystemColor);
                    ObjectModifiersUpdates.Items.Add(textBlock);
                }
            }

            data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/ArcadiaCustoms/master/updates.lss");

            if (!string.IsNullOrEmpty(data))
            {
                var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                for (int i = 0; i < list.Count; i++)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = list[i];
                    textBlock.Background = new SolidColorBrush(RTColor.FromHex("211F1D").SystemColor);
                    textBlock.Foreground = new SolidColorBrush(RTColor.FromHex("E0B564").SystemColor);
                    ArcadiaCustomsUpdates.Items.Add(textBlock);
                }
            }

            data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/PageCreator/master/updates.lss");

            if (!string.IsNullOrEmpty(data))
            {
                var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                for (int i = 0; i < list.Count; i++)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = list[i];
                    textBlock.Background = new SolidColorBrush(RTColor.FromHex("211F1D").SystemColor);
                    textBlock.Foreground = new SolidColorBrush(RTColor.FromHex("E0B564").SystemColor);
                    PageCreatorUpdates.Items.Add(textBlock);
                }
            }

            data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/ExampleCompanion/master/updates.lss");

            if (!string.IsNullOrEmpty(data))
            {
                var list = RTFile.WordWrap(data, MaxUpdateNotesLines);

                for (int i = 0; i < list.Count; i++)
                {
                    var textBlock = new TextBlock();
                    textBlock.Text = list[i];
                    textBlock.Background = new SolidColorBrush(RTColor.FromHex("211F1D").SystemColor);
                    textBlock.Foreground = new SolidColorBrush(RTColor.FromHex("E0B564").SystemColor);
                    ExampleCompanionUpdates.Items.Add(textBlock);
                }
            }

            http.Dispose();
        }

        public async Task SetupVersions()
        {
            if (RTFile.FileExists(RTFile.ApplicationDirectory + "versions.lss"))
            {
                var localVersions = await File.ReadAllTextAsync(RTFile.ApplicationDirectory + "versions.lss");

                string[]? onlineVersions = null;

                var http = new HttpClient();
                var data = await http.GetStringAsync("https://raw.githubusercontent.com/RTMecha/RTFunctions/master/mod_info.lss");

                if (!string.IsNullOrEmpty(data))
                {
                    onlineVersions = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                }

                if (!string.IsNullOrEmpty(localVersions))
                {
                    var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                    if (onlineVersions != null && list.Length > 0 && onlineVersions.Length > 0)
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

        public async Task SetupSettings()
        {
            loadingSettings = true;
            if (RTFile.FileExists(RTFile.ApplicationDirectory + "settings.lss"))
            {
                var settings = await File.ReadAllTextAsync(RTFile.ApplicationDirectory + "settings.lss");

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
                                        CheckedAll.IsChecked = value;
                                    break;
                                }
                            case "RTFunctions":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        RTFunctionsEnabled.IsChecked = value;
                                    break;
                                }
                            case "EditorManagement":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        EditorManagementEnabled.IsChecked = value;
                                    break;
                                }
                            case "EventsCore":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        EventsCoreEnabled.IsChecked = value;
                                    break;
                                }
                            case "CreativePlayers":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        CreativePlayersEnabled.IsChecked = value;
                                    break;
                                }
                            case "ObjectModifiers":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        ObjectModifiersEnabled.IsChecked = value;
                                    break;
                                }
                            case "ArcadiaCustoms":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        ArcadiaCustomsEnabled.IsChecked = value;
                                    break;
                                }
                            case "PageCreator":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        PageCreatorEnabled.IsChecked = value;
                                    break;
                                }
                            case "ExampleCompanion":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        ExampleCompanionEnabled.IsChecked = value;
                                    break;
                                }
                            case "ConfigurationManager":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        ConfigurationManagerEnabled.IsChecked = value;
                                    break;
                                }
                            case "UnityExplorer":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        UnityExplorerEnabled.IsChecked = value;
                                    break;
                                }
                            case "EditorOnStartup":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        EditorOnStartupEnabled.IsChecked = value;
                                    break;
                                }
                            case "Path":
                                {
                                    if (list.Length > i + 1)
                                        PathField.Text = list[i + 1];
                                    break;
                                }
                            case "CreateNewPath":
                                {
                                    if (list.Length > i + 1)
                                        PurePath.Text = list[i + 1];
                                    break;
                                }
                        }
                    }
                }
            }
            loadingSettings = false;
        }

        public async void Start()
        {
            await SetupUpdateNotes();
            await SetupVersions();
            await SetupSettings();

            LoadInstances();

            init = true;
        }

        public MainWindow()
        {
            InitializeComponent();

            Instance = this;

            Title = $"Project Launcher {Version}";

            Start();
        }

        #region Instances

        public bool creating = false;

        void SearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadUI(true);
            UpdatePlaceholderVisibility();
        }

        void ReloadInstances_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadInstances();
            }
            catch
            {

            }
        }

        public void LoadInstances()
        {
            Instances.Clear();
            if (RTFile.DirectoryExists(RTFile.ApplicationDirectory + "instances"))
            {
                var folders = Directory.GetDirectories(RTFile.ApplicationDirectory + "instances");
                if (folders.Length > 0)
                {
                    foreach (var folder in folders)
                    {
                        if (RTFile.FileExists(folder + "/Project Arrhythmia.exe"))
                        {
                            var instance = new ProjectArrhythmia(folder.Replace("\\", "/") + "/Project Arrhythmia.exe");
                            Instances.Add(instance);
                        }
                    }
                }
            }
            LoadUI();
        }

        public static double InstancesButtonWidth => 710;

        public void LoadUI(bool search = false)
        {
            if (InstancesList == null || InstancesList.Items == null)
                return;

            var doCreateNew = RTFile.DirectoryExists(PurePath.Text);

            if (NewNameLabel != null)
                NewNameLabel.Text = doCreateNew ? "New Name:" :
                    "New Name: (requires app path to be set to an existing copy of Project Arrhythmia Legacy)";

            InstancesList.Items.Clear();

            if (doCreateNew)
            {
                var createNew = new Button();
                createNew.Background = new SolidColorBrush(RTColor.FromHex("E0B564").SystemColor);
                createNew.Content = "Create New Instance";
                createNew.Name = "CreateNew";
                createNew.Width = InstancesButtonWidth;
                createNew.Click += CreateNew_Click;

                InstancesList.Items.Add(createNew);
            }

            foreach (var instance in Instances)
            {
                if (SearchField == null || string.IsNullOrEmpty(SearchField.Text) || instance.Name.ToLower().Contains(SearchField.Text.ToLower()))
                {
                    var button = new Button();
                    button.Background = new SolidColorBrush(RTColor.Transparent.SystemColor);
                    button.Foreground = new SolidColorBrush(RTColor.FromHex("FFCCA5").SystemColor);
                    button.Content = instance.Name;
                    button.Click += Instance_Checked;
                    button.Width = InstancesButtonWidth;

                    InstancesList.Items.Add(button);
                }
            }

            if (!search && InstancesList.Items.Count > (doCreateNew ? 1 : 0))
            {
                InstancesList.SelectedItem = InstancesList.Items[doCreateNew ? 1 : 0];
                SetSelected();
            }
        }

        async void CreateNew_Click(object sender, RoutedEventArgs e)
        {
            if (!RTFile.DirectoryExists($"{RTFile.ApplicationDirectory}/instances"))
                Directory.CreateDirectory($"{RTFile.ApplicationDirectory}/instances");

            Debug.WriteLine($"Create new clicked with New Name set as {NewName.Text}");
            if (RTFile.DirectoryExists(PurePath.Text))
            {
                creating = true;

                Debug.WriteLine($"Getting directories...");
                var allDirectories = Directory.GetDirectories(PurePath.Text, "*", SearchOption.AllDirectories);

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
                        string dirToCreate = dir.Replace(PurePath.Text, s);
                        Directory.CreateDirectory(dirToCreate);
                    }
                }

                Debug.WriteLine($"Getting files...");
                var allFiles = Directory.GetFiles(PurePath.Text, "*.*", SearchOption.AllDirectories);

                Debug.WriteLine($"Copying files to new location {s}...");
                foreach (string newFile in allFiles)
                {
                    if (!newFile.Contains("beatmaps") &&
                        !newFile.Contains("BepInEx") &&
                        !newFile.Contains("doorstop_config.ini") &&
                        !newFile.Contains("winhttp.dll") &&
                        !newFile.Contains("screenshots"))
                    {
                        var bytes = await File.ReadAllBytesAsync(newFile);

                        await File.WriteAllBytesAsync(newFile.Replace(PurePath.Text, s), bytes);

                        //File.Copy(newFile, newFile.Replace(PurePath.Text, s));
                    }
                }

                creating = false;
                LoadInstances();
            }
        }

        void LaunchInstance_Click(object sender, RoutedEventArgs e)
        {
            if (Current != null)
            {
                ModUpdater.Launch(Current);
            }
        }

        void UpdateInstance_Click(object sender, RoutedEventArgs e)
        {
            if (Current != null)
            {
                ModUpdater.Update(Current);
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
            int num = RTFile.DirectoryExists(PurePath.Text) ? 1 : 0;

            foreach (var item in InstancesList.Items)
            {
                var button = item as Button;

                if (Instances.TryFind(x => x.Name == (string)button.Content, out ProjectArrhythmia instance) && item == InstancesList.SelectedItem)
                {
                    Current = instance;
                    try
                    {
                        instanceLoading = true;
                        instance.LoadSettings();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        public bool instanceLoading = false;

        void InstancesList_Selected(object sender, RoutedEventArgs e)
        {
            SetSelected();
        }

        void T_MouseEnter(object sender, MouseEventArgs e)
        {
            searchFieldHovered = false;
            UpdatePlaceholderVisibility();
        }

        void SearchField_MouseLeave(object sender, MouseEventArgs e)
        {
            searchFieldHovered = true;
            UpdatePlaceholderVisibility();
        }

        void SearchField_MouseEnter(object sender, MouseEventArgs e)
        {
            searchFieldHovered = false;
            UpdatePlaceholderVisibility();
        }

        public bool searchFieldHovered;

        public void UpdatePlaceholderVisibility()
        {
            T.IsEnabled = searchFieldHovered;
            T.Visibility = searchFieldHovered && (SearchField == null || string.IsNullOrEmpty(SearchField.Text)) ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Instance Settings

        bool instanceAllBeingChecked = false;
        void InstanceCheckedAll_Checked(object sender, RoutedEventArgs e)
        {
            instanceAllBeingChecked = true;

            InstanceRTFunctionsEnabled.IsChecked = true;
            InstanceEditorManagementEnabled.IsChecked = true;
            InstanceEventsCoreEnabled.IsChecked = true;
            InstanceCreativePlayersEnabled.IsChecked = true;
            InstanceObjectModifiersEnabled.IsChecked = true;
            InstanceArcadiaCustomsEnabled.IsChecked = true;
            InstancePageCreatorEnabled.IsChecked = true;
            InstanceExampleCompanionEnabled.IsChecked = true;
            InstanceConfigurationManagerEnabled.IsChecked = true;
            InstanceUnityExplorerEnabled.IsChecked = true;
            InstanceEditorOnStartupEnabled.IsChecked = true;
            isInstanceChanging = false;

            Current?.SaveSettings();

            instanceAllBeingChecked = false;
        }

        bool isInstanceChanging;

        bool instanceAllBeingUnchecked = false;
        void InstanceCheckedAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
            {
                instanceAllBeingUnchecked = true;

                InstanceRTFunctionsEnabled.IsChecked = false;
                InstanceEditorManagementEnabled.IsChecked = false;
                InstanceEventsCoreEnabled.IsChecked = false;
                InstanceCreativePlayersEnabled.IsChecked = false;
                InstanceObjectModifiersEnabled.IsChecked = false;
                InstanceArcadiaCustomsEnabled.IsChecked = false;
                InstancePageCreatorEnabled.IsChecked = false;
                InstanceExampleCompanionEnabled.IsChecked = false;
                InstanceConfigurationManagerEnabled.IsChecked = false;
                InstanceUnityExplorerEnabled.IsChecked = false;
                InstanceEditorOnStartupEnabled.IsChecked = false;

                instanceAllBeingUnchecked = false;
            }
            isInstanceChanging = false;
        }

        void InstanceRTFunctionsEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        bool instanceRTFunctionsBeingUnchecked = false;
        void InstanceRTFunctionsEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            Current?.SaveSettings();
            isInstanceChanging = false;

            instanceRTFunctionsBeingUnchecked = true;

            InstanceEditorManagementEnabled.IsChecked = false;
            InstanceEventsCoreEnabled.IsChecked = false;
            InstanceCreativePlayersEnabled.IsChecked = false;
            InstanceObjectModifiersEnabled.IsChecked = false;
            InstanceArcadiaCustomsEnabled.IsChecked = false;
            InstancePageCreatorEnabled.IsChecked = false;
            InstanceExampleCompanionEnabled.IsChecked = false;
            InstanceCheckedAll.IsChecked = false;

            instanceRTFunctionsBeingUnchecked = false;
        }

        void InstanceEditorManagementEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (InstanceEditorManagementEnabled.IsChecked == true && InstanceRTFunctionsEnabled.IsChecked != true)
                InstanceRTFunctionsEnabled.IsChecked = true;

            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstanceEditorManagementEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstanceEventsCoreEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (InstanceEventsCoreEnabled.IsChecked == true && InstanceRTFunctionsEnabled.IsChecked != true)
                InstanceRTFunctionsEnabled.IsChecked = true;

            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstanceEventsCoreEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstanceCreativePlayersEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (InstanceCreativePlayersEnabled.IsChecked == true && InstanceRTFunctionsEnabled.IsChecked != true)
                InstanceRTFunctionsEnabled.IsChecked = true;

            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstanceCreativePlayersEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstanceObjectModifiersEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (InstanceObjectModifiersEnabled.IsChecked == true && InstanceRTFunctionsEnabled.IsChecked != true)
                InstanceRTFunctionsEnabled.IsChecked = true;

            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstanceObjectModifiersEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstanceArcadiaCustomsEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (InstanceArcadiaCustomsEnabled.IsChecked == true && InstanceRTFunctionsEnabled.IsChecked != true)
                InstanceRTFunctionsEnabled.IsChecked = true;

            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstanceArcadiaCustomsEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstancePageCreatorEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (InstancePageCreatorEnabled.IsChecked == true && InstanceRTFunctionsEnabled.IsChecked != true)
                InstanceRTFunctionsEnabled.IsChecked = true;

            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstancePageCreatorEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstanceExampleCompanionEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (InstanceExampleCompanionEnabled.IsChecked == true && InstanceRTFunctionsEnabled.IsChecked != true)
                InstanceRTFunctionsEnabled.IsChecked = true;

            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstanceExampleCompanionEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstanceConfigurationManagerEnabled_Checked(object sender, RoutedEventArgs e)
        {

            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstanceConfigurationManagerEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstanceUnityExplorerEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstanceUnityExplorerEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstanceEditorOnStartupEnabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsAllInstanceChecked())
                InstanceCheckedAll.IsChecked = true;
            else
            {
                if (!isInstanceChanging)
                    Current?.SaveSettings();
                isInstanceChanging = true;
                InstanceCheckedAll.IsChecked = false;
            }
        }

        void InstanceEditorOnStartupEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInstanceChanging)
                Current?.SaveSettings();
            isInstanceChanging = true;
            InstanceCheckedAll.IsChecked = false;
        }

        void InstanceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public bool IsAllInstanceChecked() => InstanceRTFunctionsEnabled.IsChecked == true &&
            InstanceEditorManagementEnabled.IsChecked == true &&
            InstanceEventsCoreEnabled.IsChecked == true &&
            InstanceCreativePlayersEnabled.IsChecked == true &&
            InstanceObjectModifiersEnabled.IsChecked == true &&
            InstanceArcadiaCustomsEnabled.IsChecked == true &&
            InstanceExampleCompanionEnabled.IsChecked == true &&
            InstanceConfigurationManagerEnabled.IsChecked == true &&
            InstanceUnityExplorerEnabled.IsChecked == true &&
            InstanceEditorOnStartupEnabled.IsChecked == true;

        // Set this to instance path
        async void InstanceResetToLocal_Click(object sender, RoutedEventArgs e)
        {
            if (Current == null)
                return;

            if (RTFile.FileExists(Current.FolderPath + "settings/mod_settings.lss"))
            {
                var settings = await File.ReadAllTextAsync(Current.FolderPath + "settings/mod_settings.lss");

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
                                        InstanceCheckedAll.IsChecked = value;
                                    break;
                                }
                            case "RTFunctions":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceRTFunctionsEnabled.IsChecked = value;
                                    break;
                                }
                            case "EditorManagement":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceEditorManagementEnabled.IsChecked = value;
                                    break;
                                }
                            case "EventsCore":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceEventsCoreEnabled.IsChecked = value;
                                    break;
                                }
                            case "CreativePlayers":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceCreativePlayersEnabled.IsChecked = value;
                                    break;
                                }
                            case "ObjectModifiers":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceObjectModifiersEnabled.IsChecked = value;
                                    break;
                                }
                            case "ArcadiaCustoms":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceArcadiaCustomsEnabled.IsChecked = value;
                                    break;
                                }
                            case "PageCreator":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstancePageCreatorEnabled.IsChecked = value;
                                    break;
                                }
                            case "ExampleCompanion":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceExampleCompanionEnabled.IsChecked = value;
                                    break;
                                }
                            case "ConfigurationManager":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceConfigurationManagerEnabled.IsChecked = value;
                                    break;
                                }
                            case "UnityExplorer":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceUnityExplorerEnabled.IsChecked = value;
                                    break;
                                }
                            case "EditorOnStartup":
                                {
                                    if (list.Length > i + 1 && bool.TryParse(list[i + 1], out var value))
                                        InstanceEditorOnStartupEnabled.IsChecked = value;
                                    break;
                                }
                        }
                    }
                }
            }

            if (RTFile.FileExists(Current.FolderPath + "settings/versions.lss"))
            {
                var localVersions = await File.ReadAllTextAsync(Current.FolderPath + "settings/versions.lss");

                string[]? onlineVersions = null;

                var http = new HttpClient();
                var data = await http.GetStringAsync(ModUpdater.CurrentVersionsList);
                if (!string.IsNullOrEmpty(data))
                {
                    onlineVersions = data.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                }

                if (!string.IsNullOrEmpty(localVersions) && onlineVersions != null)
                {
                    var list = localVersions.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                    if (list.Length > 0 && onlineVersions.Length > 0)
                    {
                        InstanceRTFunctionsEnabled.Content = $"{list[0]} - Installed: {list[1]}" + (list[1] == onlineVersions[1] ? "" : $" | Update Available: {onlineVersions[1]}");
                        InstanceEditorManagementEnabled.Content = $"{list[2]} - Installed: {list[3]}" + (list[3] == onlineVersions[3] ? "" : $" | Update Available: {onlineVersions[3]}");
                        InstanceEventsCoreEnabled.Content = $"{list[4]} - Installed: {list[5]}" + (list[5] == onlineVersions[5] ? "" : $" | Update Available: {onlineVersions[5]}");
                        InstanceCreativePlayersEnabled.Content = $"{list[6]} - Installed: {list[7]}" + (list[7] == onlineVersions[7] ? "" : $" | Update Available: {onlineVersions[7]}");
                        InstanceObjectModifiersEnabled.Content = $"{list[8]} - Installed: {list[9]}" + (list[9] == onlineVersions[9] ? "" : $" | Update Available: {onlineVersions[9]}");
                        InstanceArcadiaCustomsEnabled.Content = $"{list[10]} - Installed: {list[11]}" + (list[11] == onlineVersions[11] ? "" : $" | Update Available: {onlineVersions[11]}");
                        InstancePageCreatorEnabled.Content = $"{list[12]} - Installed: {list[13]}" + (list[13] == onlineVersions[13] ? "" : $" | Update Available: {onlineVersions[13]}");
                        InstanceExampleCompanionEnabled.Content = $"{list[14]} - Installed: {list[15]}" + (list[15] == onlineVersions[15] ? "" : $" | Update Available: {onlineVersions[15]}");
                    }
                }
            }
        }

        public CheckBox GetModInstanceToggle(int i)
        {
            switch (i)
            {
                case 0: return InstanceRTFunctionsEnabled;
                case 1: return InstanceEditorManagementEnabled;
                case 2: return InstanceEventsCoreEnabled;
                case 3: return InstanceCreativePlayersEnabled;
                case 4: return InstanceObjectModifiersEnabled;
                case 5: return InstanceArcadiaCustomsEnabled;
                case 6: return InstancePageCreatorEnabled;
                case 7: return InstanceExampleCompanionEnabled;
                default: throw new ArgumentOutOfRangeException("Toggle does not exist.");
            }
        }

        #endregion

        public bool loadingSettings = false;

        public async Task SaveSettings()
        {
            string str = "";

            if (CheckedAll == null || loadingSettings)
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
            str += "CreateNewPath" + Environment.NewLine + PurePath.Text + Environment.NewLine;

            await File.WriteAllTextAsync(RTFile.ApplicationDirectory + "settings.lss", str);
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

        async void PathField_TextChanged(object sender, TextChangedEventArgs e) => await SaveSettings();

        void PlayButton_Click(object sender, RoutedEventArgs e) => ModUpdater.Launch();

        void UpdateButton_Click(object sender, RoutedEventArgs e) => ModUpdater.Update();

        #region Settings

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

        public CheckBox GetModToggle(int i)
        {
            switch (i)
            {
                case 0: return RTFunctionsEnabled;
                case 1: return EditorManagementEnabled;
                case 2: return EventsCoreEnabled;
                case 3: return CreativePlayersEnabled;
                case 4: return ObjectModifiersEnabled;
                case 5: return ArcadiaCustomsEnabled;
                case 6: return PageCreatorEnabled;
                case 7: return ExampleCompanionEnabled;
                default: throw new ArgumentOutOfRangeException("Toggle does not exist.");
            }
        }

        #endregion

        async void PurePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (init)
            {
                LoadUI();
                await SaveSettings();
            }
        }

        void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "PA Legacy (*.exe)|Project Arrhythmia.exe|All files (*.*)|*.*",
            };
            if (openFileDialog.ShowDialog() == true && openFileDialog.FileName.Replace("\\", "/").Contains("/Project Arrhythmia.exe"))
            {
                if (!RTFile.FileExists(openFileDialog.FileName.Replace("\\", "/").Replace("/Project Arrhythmia.exe", "") + "/GameAssembly.dll"))
                    PathField.Text = openFileDialog.FileName.Replace("\\", "/").Replace("/Project Arrhythmia.exe", "");
                else
                {
                    MessageBox.Show("Cannot use non-PA Legacy versions.");
                }
            }
        }

        void InstanceBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "PA Legacy (*.exe)|Project Arrhythmia.exe|All files (*.*)|*.*",
            };
            if (openFileDialog.ShowDialog() == true && openFileDialog.FileName.Replace("\\", "/").Contains("/Project Arrhythmia.exe"))
            {
                if (!RTFile.FileExists(openFileDialog.FileName.Replace("\\", "/").Replace("/Project Arrhythmia.exe", "") + "/GameAssembly.dll"))
                    PurePath.Text = openFileDialog.FileName.Replace("\\", "/").Replace("/Project Arrhythmia.exe", "");
                else
                {
                    MessageBox.Show("Cannot use non-PA Legacy versions.");
                }
            }
        }
    }
}
