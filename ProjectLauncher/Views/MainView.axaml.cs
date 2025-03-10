using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using ProjectLauncher.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SimpleJSON;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

using ProjectLauncher.Data;
using ProjectLauncher.Managers;
using Avalonia.Platform;

namespace ProjectLauncher.Views
{

    public partial class MainView : UserControl
    {
        public static MainView Instance { get; set; }

        public List<LauncherTab> pages = new List<LauncherTab>();

        public LauncherSettings settings = new LauncherSettings();

        double hsvHue;
        double hsvSaturation;
        double hsvValue;

        //public static string InstancesFolder => Directory.GetCurrentDirectory().Replace("\\", "/") + "/instances";
        public static string MainDirectory { get; set; }
        //public static string MainDirectory => "E:/Coding/ProjectArrhythmiaLauncher-master/ProjectLauncher.Desktop/bin/Debug/net6.0/";
        public static string InstancesFolder { get; set; }
        public static string SettingsFile { get; set; }

        public static string CurrentVersion { get; set; } = "1.0.0"; // BetterLegacy version 1

        public static string Changelog { get; set; }

        public MainView()
        {
            InitializeComponent();
            DataContextChanged += MainView_DataContextChanged;
            Instance = this;

            DefineFilesPath();
            Load();
        }

        void DefineFilesPath()
        {
            
            if (Design.IsDesignMode) MainDirectory = Directory.GetCurrentDirectory().Replace("\\", "/") + "/" + "ProjectLauncher.Desktop/bin/Debug/net6.0/";
            else MainDirectory = Directory.GetCurrentDirectory().Replace("\\", "/") + "/";
           

            InstancesFolder = $"{MainDirectory}instances";
            SettingsFile = $"{MainDirectory}settings.lss";
        }

        async void Load()
        {
            pages.Add(new LauncherTab(LaunchButton, LaunchWindow));
            pages.Add(new LauncherTab(SettingsButton, SettingsWindow));
            pages.Add(new LauncherTab(ChangelogButton, ChangelogWindow));
            pages.Add(new LauncherTab(NewsButton, NewsWindow));
            pages.Add(new LauncherTab(AboutButton, AboutWindow));
            UpdateButtons(LaunchButton);

            using var stream = AssetLoader.Open(new Uri("avares://ProjectLauncher/Assets/changelog.txt"));
            using var streamReader = new StreamReader(stream);

            Changelog = await streamReader.ReadToEndAsync();

            var list = new ListBox();
            list.Background = new SolidColorBrush(Color.Parse("#FF141414"));
            list.Foreground = new SolidColorBrush(Color.Parse("#FF141414"));
            list.BorderBrush = new SolidColorBrush(Color.Parse("#FF141414"));

            list.SelectionChanged += VersionsChanged;

            Versions.Flyout = new Flyout
            {
                Content = list,
            };

            //return; //for fixing MainView.axaml issue

            await LoadVersions();
            BetterLegacyToggle.Click += BetterLegacyToggleClick;
            EditorOnStartupToggle.Click += EditorOnStartupToggleClick;
            UnityExplorerToggle.Click += UnityExplorerToggleClick;
            InstancesListBox.SelectionChanged += InstancesListBoxSelectionChanged;
            LoadInstances();
            await LoadSettings();

            Launch.Click += LaunchClick;
            Update.Click += UpdateClick;

            InstancesSearchField.TextChanged += InstancesSearchChanged;
            CreateNewInstanceButton.Click += CreateNewInstanceClicked;
            AppPathBrowse.Click += AppPathBrowseClick;
            AppPathField.TextChanged += AppPathFieldChanged;

            OpenInstanceFolderButton.Click += OpenInstanceFolderClick;
            ZipInstanceButton.Click += ZipInstanceClick;
            PasteBeatmapsFolderButton.Click += PasteBeatmapsFolder;

            SettingRounded.Click += SettingsRoundedClick;
            SettingUpdateLauncher.Click += UpdateLauncherClick;
            RoundSlider.ValueChanged += RoundSliderValueChanged;

            HueSlider.ValueChanged += HSVdataUpdate;
            SaturationSlider.ValueChanged += HSVdataUpdate;
            ValueSlider.ValueChanged += HSVdataUpdate;
            ResetToDefaultThemeButton.Click += ResetToDefaultThemeButtonPresed;

            Loaded += OnLoaded;

            LoadUpdateNotes();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(MainDirectory + "ProjectLauncher.zip"))
                return;

            var startInfoNnZip = new ProcessStartInfo();
            startInfoNnZip.FileName = MainDirectory + "ProjectLauncher.Unzip.exe";
            Process.Start(startInfoNnZip);

            MainWindow.Instance.Close();
        }

        // add async when downloading versions file.
        async Task LoadVersions()
        {
            if (Versions.Flyout is Flyout flyout && flyout.Content is ListBox list)
            {
                var http = new HttpClient();
                var versionString = await http.GetStringAsync("https://github.com/RTMecha/BetterLegacy/raw/master/versions.lss");

                if (string.IsNullOrEmpty(versionString))
                    return;

                var versions = versionString.Split(',').ToList();
                versions.Reverse();

                list.Items.Clear();
                for (int i = 0; i < versions.Count; i++)
                {
                    var version = versions[i];

                    var element = new ListBoxItem
                    {
                        Content = version,
                        MaxWidth = 500,
                        FontWeight = FontWeight.Bold,
                        FontFamily = FontFamily.Default,
                        //BorderThickness = new Thickness(0, 0, 0, 3),
                    };
                    element.Classes.Set("droppedList", true);
                    list.Items.Add(element);
                }

                CurrentVersion = versions[versions.Count - 1];
            }
        }

        async void LoadUpdateNotes()
        {
            try
            {
                ChangelogNotes.Text = Changelog;

                var http = new HttpClient();
                var str = await http.GetStringAsync("https://github.com/RTMecha/BetterLegacy/raw/master/updates.lss");

                if (!string.IsNullOrEmpty(str)) BetterLegacyNotes.Text = str;


            }
            catch (Exception ex)
            {

            }
        }

        async Task LoadSettings()
        {
            var settingsPath = SettingsFile;
            if (File.Exists(settingsPath))
            {
                var json = await File.ReadAllTextAsync(settingsPath);
                var jn = JSON.Parse(json);
                settings.Rounded = jn["ui"]["rounded"].AsBool;

                if (!string.IsNullOrEmpty(jn["ui"]["roundness"]))
                    RoundSlider.Value = Convert.ToDouble(jn["ui"]["roundness"].Value);
                settings.Roundness = RoundSlider.Value;

                if (!string.IsNullOrEmpty(jn["instances"]["app_path"]))
                    AppPathField.Text = jn["instances"]["app_path"];

                if (!string.IsNullOrEmpty(jn["hsv"]["hue"]))
                    HueSlider.Value = Convert.ToDouble(jn["hsv"]["hue"].Value);

                if (!string.IsNullOrEmpty(jn["hsv"]["saturation"]))
                    SaturationSlider.Value = Convert.ToDouble(jn["hsv"]["saturation"].Value);

                if (!string.IsNullOrEmpty(jn["hsv"]["value"]))
                    ValueSlider.Value = Convert.ToDouble(jn["hsv"]["value"].Value);

                if (!string.IsNullOrEmpty(jn["hsv"]["hue"]) && !string.IsNullOrEmpty(jn["hsv"]["saturation"]) && !string.IsNullOrEmpty(jn["hsv"]["value"]))
                {
                    if(jn["hsv"]["hue"].Value == "0" && jn["hsv"]["saturation"].Value == "0" && jn["hsv"]["value"].Value == "0")
                    {
                        ResetToDefaultTheme();
                    }
                }

                ColorsUpdate();
            }

            SettingRounded.Content = $"Rounded UI   {(settings.Rounded ? "✓" : "✕")}";

            UpdateRoundness();
            SaveSettings();
        }

        bool savingSettings;
        async void SaveSettings()
        {
            savingSettings = true;
            var settingsPath = SettingsFile;
            var jn = JSON.Parse("{}");
            jn["ui"]["rounded"] = settings.Rounded.ToString();
            jn["ui"]["roundness"] = RoundSlider.Value.ToString();
            jn["hsv"]["hue"] = HueSlider.Value.ToString();
            jn["hsv"]["saturation"] = SaturationSlider.Value.ToString();
            jn["hsv"]["value"] = ValueSlider.Value.ToString();
            if (AppPathField != null && !string.IsNullOrEmpty(AppPathField.Text))
                jn["instances"]["app_path"] = AppPathField.Text;

            await File.WriteAllTextAsync(settingsPath, jn.ToString(6));
            savingSettings = false;
        }

        async void CreateNewInstance()
        {
            if (string.IsNullOrEmpty(AppPathField.Text) || !Directory.Exists(AppPathField.Text.Replace("\\", "/").Replace("/Project Arrhythmia.exe", "")) || string.IsNullOrEmpty(NewInstanceNameField.Text))
                return;

            var path = AppPathField.Text.Replace("\\", "/").Replace("//", "").Replace("/Project Arrhythmia.exe", "");
            var newPath = $"{InstancesFolder}/{NewInstanceNameField.Text}";

            if (!Directory.Exists(InstancesFolder))
                Directory.CreateDirectory(InstancesFolder);

            int num = 0;
            while (Directory.Exists(newPath))
            {
                newPath = $"{InstancesFolder}/{NewInstanceNameField.Text} [{num}]";
                num++;
            }

            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i].Replace("\\", "/");

                if (file == null || file.Contains("beatmaps") ||
                    file.Contains("BepInEx") ||
                    file.Contains("doorstop_config.ini") ||
                    file.Contains("winhttp.dll") ||
                    file.Contains("screenshots"))
                    continue;

                var directory = Path.GetDirectoryName(file)?.Replace("\\", "/");

                if (directory == null)
                    continue;

                var newDirectory = directory.Replace(path, newPath);

                if (!Directory.Exists(newDirectory))
                    Directory.CreateDirectory(newDirectory);

                var newFile = file.Replace(path, newPath);

                var fileBytes = await File.ReadAllBytesAsync(file);

                await File.WriteAllBytesAsync(newFile, fileBytes);
            }

            LoadInstances();
        }

        async void LoadInstances()
        {
            InstancesListBox.Items.Clear();

            if (!Directory.Exists(InstancesFolder))
            {
                SetLaunchButtonsActive(false);
                return;
            }

            var directories = Directory.GetDirectories(InstancesFolder);

            if (directories.Length == 0)
            {
                SetLaunchButtonsActive(false);
                return;
            }

            SetLaunchButtonsActive(true);
            for (int i = 0; i < directories.Length; i++)
            {
                var folder = Path.GetFileName(directories[i]);
                if (!string.IsNullOrEmpty(InstancesSearchField.Text) && !folder.Contains(InstancesSearchField.Text, StringComparison.OrdinalIgnoreCase) || !File.Exists($"{directories[i]}/Project Arrhythmia.exe"))
                    continue;

                var instance = new ProjectArrhythmia
                {
                    Path = directories[i],
                    Settings = new InstanceSettings(directories[i]),
                };

                await instance.Settings.LoadSettings();

                if (string.IsNullOrEmpty(instance.Settings.CurrentVersion))
                    instance.Settings.CurrentVersion = CurrentVersion;
                var item = new ListBoxItem
                {
                    DataContext = instance,
                    Content = Path.GetFileName(directories[i])
                };
                item.Classes.Add("lbs1");
                InstancesListBox.Items.Add(item);
            }

            SetLaunchButtonsActive(InstancesListBox.ItemCount > 0);

            if (InstancesListBox.ItemCount > 0)
                InstancesListBox.SelectedItem = InstancesListBox.Items[0];
        }

        void SetLaunchButtonsActive(bool active)
        {
            FunctionButtons.IsVisible = active;
            BetterLegacyToggle.IsVisible = active;
            EditorOnStartupToggle.IsVisible = active;
            UnityExplorerToggle.IsVisible = active;

            ProgressBar.IsVisible = !active;
            LabelProgressBar.IsVisible = !active;
        }

        void UpdateRoundness()
        {
            double roundessValue = settings.Rounded ? settings.Roundness : 0;
            var resources = Resources;
            resources["CornerRadius"] = new CornerRadius(roundessValue);
            resources["TextBoxCornerRadius"] = new CornerRadius(roundessValue, roundessValue, 0,0);
        }

        public void UpdateButtons(Button button)
        {
            foreach (var page in pages)
            {
                var selected = page.PageButton.Name == button.Name;
                page.Page.IsVisible = selected;

                if (selected) page.PageButton.Classes.Set("main", true);
                else page.PageButton.Classes.Set("main", false);
            }
        }

        #region Senders

        void PasteBeatmapsFolder(object sender, RoutedEventArgs e)
        {
            var warningWindow = new WarningWindow()
            {
                DataContext = new WarningViewModel()
                {
                    WarningMessage = "Are you sure you want to paste?",
                    Confirm = () =>
                    {
                        Debug.WriteLine("Test");
                        WarningWindow.Instance?.Close();
                    },
                }
            };
            warningWindow.Show(MainWindow.Instance);
        }

        void OpenModSettingsWindow(object sender, RoutedEventArgs e)
        {
            var modSettingsWindow = new ModSettingsWindow()
            {
                DataContext = new ModSettingsViewModel(),
            };
            modSettingsWindow.Show(MainWindow.Instance);
        }

        public bool zipping;

        void ZipInstanceClick(object? sender, RoutedEventArgs e)
        {
            if (zipping)
                return;

            if (InstancesListBox.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia)
            {
                string path = projectArrhythmia.Path.Replace("/", "\\");

                Task.Run(() =>
                {
                    if (zipping)
                        return;

                    zipping = true;

                    if (File.Exists(path + ".zip"))
                        File.Delete(path + ".zip");

                    ZipFile.CreateFromDirectory(path, path + ".zip");

                    zipping = false;
                });
            }
        }

        void OpenInstanceFolderClick(object? sender, RoutedEventArgs e)
        {
            if (InstancesListBox.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia)
            {
                string path = projectArrhythmia.Path.Replace("/", "\\");

                try
                {
                    Process.Start("explorer.exe", (Directory.Exists(path) ? "/root," : "/select,") + path);
                }
                catch (Win32Exception ex)
                {
                    ex.HelpLink = "";
                }
            }
        }

        void UpdateLauncherClick(object? sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = MainDirectory + "ProjectLauncher.Updater.exe";
            Process.Start(startInfo);

            MainWindow.Instance.Close();
        }

        void AppPathFieldChanged(object? sender, TextChangedEventArgs e)
        {
            if (!savingSettings)
                SaveSettings();
        }

        async void AppPathBrowseClick(object? sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter
                    {
                        Name = "PA Legacy",
                        Extensions = new List<string> { "exe" },
                    },
                    new FileDialogFilter
                    {
                        Name = "All files",
                        Extensions = new List<string> { "*" },
                    },
                },
            };

            var show = await openFileDialog.ShowAsync(MainWindow.Instance);

            if (show == null)
                return;

            Debug.WriteLine($"Length: {show.Length}");
            if (show.Length < 1)
                return;

            Debug.WriteLine($"Directory: {show[0]}");

            var path = show[0].Replace("\\", "/").Replace("/Project Arrhythmia.exe", "");

            if (File.Exists(path + "/Game Assembly.dll"))
                return;

            AppPathField.Text = path;
        }

        void CreateNewInstanceClicked(object? sender, RoutedEventArgs e)
        {
            CreateNewInstance();
        }

        void InstancesSearchChanged(object? sender, TextChangedEventArgs e)
        {
            LoadInstances();
        }

        void LaunchClick(object? sender, RoutedEventArgs e)
        {
            if (InstancesListBox.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia)
            {
                if (File.Exists(projectArrhythmia.Path + "/Project Arrhythmia.exe"))
                {
                    Debug.WriteLine($"Launch: {projectArrhythmia}");
                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = projectArrhythmia.Path + "/Project Arrhythmia.exe";
                    Process.Start(startInfo);
                }
            }
        }

        async void UpdateClick(object? sender, RoutedEventArgs e)
        {
            if (InstancesListBox.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia && projectArrhythmia.Settings != null)
            {
                try
                {
                    SetLaunchButtonsActive(false);
                    using var http = new HttpClient();

                    // Download BepInEx (Obviously will need BepInEx itself to run any mods)
                    if (!Directory.Exists(projectArrhythmia.Path + "/BepInEx"))
                    {
                        var bepZipOutput = projectArrhythmia.Path + "/BepInEx-5.4.21.zip";

                        var headRequest = new HttpRequestMessage(HttpMethod.Head, BepInExURL);
                        var headResponse = await http.SendAsync(headRequest);
                        var contentLength = headResponse.Content.Headers.ContentLength;
                        ProgressBarUpdater(bepZipOutput, "Downloading BepInEx", contentLength.Value);

                        // Getting file data
                        var response = await http.GetAsync(BepInExURL, HttpCompletionOption.ResponseHeadersRead);
                        using var fileStream = File.Create(bepZipOutput);
                        using var stream = await response.Content.ReadAsStreamAsync();
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                        }
                        fileStream.Close();

                        UnZip(bepZipOutput, projectArrhythmia.Path + "/");

                        File.Delete(bepZipOutput);
                    }

                    var pluginsPath = $"{projectArrhythmia.Path}/BepInEx/plugins";
                    var editorOnStartupPath = $"{pluginsPath}/EditorOnStartup.dll";
                    var betterLegacyPath = $"{pluginsPath}/BetterLegacy.dll";

                    if (!Directory.Exists(pluginsPath))
                        Directory.CreateDirectory(pluginsPath);

                    if (!File.Exists(editorOnStartupPath) && projectArrhythmia.Settings.EditorOnStartup)
                    {
                        var bytes = await http.GetByteArrayAsync("https://github.com/enchart/EditorOnStartup/releases/download/1.0.0/EditorOnStartup_1.0.0.dll");
                        await File.WriteAllBytesAsync(editorOnStartupPath, bytes);
                    }
                    else if (File.Exists(editorOnStartupPath) && !projectArrhythmia.Settings.EditorOnStartup)
                    {
                        File.Delete(editorOnStartupPath);
                    }

                    if (projectArrhythmia.Settings.BetterLegacy)
                    {
                        var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/BetterLegacy.zip";

                        if (URLExists(url))
                        {
                            var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                            var headResponse = await http.SendAsync(headRequest);
                            var contentLength = headResponse.Content.Headers.ContentLength;
                            ProgressBarUpdater($"{pluginsPath}/BetterLegacy.zip", "Downloading BetterLegacy", contentLength.Value);

                            // Getting file data
                            var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                            using var fileStream = File.Create($"{pluginsPath}/BetterLegacy.zip");
                            using var stream = await response.Content.ReadAsStreamAsync();
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                            }
                            fileStream.Close();

                            UnZip($"{pluginsPath}/BetterLegacy.zip", pluginsPath);

                            File.Delete($"{pluginsPath}/BetterLegacy.zip");
                        }
                    }
                    else if (File.Exists(betterLegacyPath) && !projectArrhythmia.Settings.BetterLegacy)
                    {
                        File.Delete(betterLegacyPath);
                    }

                    if (!Directory.Exists($"{pluginsPath}/sinai-dev-UnityExplorer") && projectArrhythmia.Settings.UnityExplorer)
                    {
                        var url = "https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip";
                        var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                        var headResponse = await http.SendAsync(headRequest);
                        var contentLength = headResponse.Content.Headers.ContentLength;
                        ProgressBarUpdater($"{pluginsPath}/UnityExplorer.zip", "Downloading UnityExplorer", contentLength.Value);

                        // Getting file data
                        var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                        using var fileStream = File.Create($"{pluginsPath}/UnityExplorer.zip");
                        using var stream = await response.Content.ReadAsStreamAsync();
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                        }
                        fileStream.Close();

                        UnZip($"{pluginsPath}/UnityExplorer.zip", pluginsPath.Replace("/plugins", ""));

                        File.Delete($"{pluginsPath}/UnityExplorer.zip");
                    }
                    else if (Directory.Exists($"{pluginsPath}/sinai-dev-UnityExplorer") && !projectArrhythmia.Settings.UnityExplorer)
                    {
                        Directory.Delete($"{pluginsPath}/sinai-dev-UnityExplorer", true);
                    }

                    // Download steam_api.dll
                    if (!File.Exists($"{projectArrhythmia.Path}/Project Arrhythmia_Data/Plugins/steam_api_updated.txt"))
                    {
                        var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/steam_api64.dll";

                        if (URLExists(url))
                        {
                            var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                            var headResponse = await http.SendAsync(headRequest);
                            var contentLength = headResponse.Content.Headers.ContentLength;
                            ProgressBarUpdater($"{projectArrhythmia.Path}/Project Arrhythmia_Data/Plugins/steam_api64.dll", "Updating steam_api64.dll", contentLength.Value);

                            // Getting file data
                            var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                            using var fileStream = File.Create($"{projectArrhythmia.Path}/Project Arrhythmia_Data/Plugins/steam_api64.dll");
                            using var stream = await response.Content.ReadAsStreamAsync();
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                            }
                            fileStream.Close();

                            await File.WriteAllTextAsync($"{projectArrhythmia.Path}/Project Arrhythmia_Data/Plugins/steam_api_updated.txt", "Yes");
                        }
                    }

                    if (!Directory.Exists($"{projectArrhythmia.Path}/beatmaps/menus"))
                    {
                        var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/Beatmaps.zip";

                        if (URLExists(url))
                        {
                            var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                            var headResponse = await http.SendAsync(headRequest);
                            var contentLength = headResponse.Content.Headers.ContentLength;
                            ProgressBarUpdater($"{projectArrhythmia.Path}/Beatmaps.zip", "Downloading Beatmaps.zip", contentLength.Value);

                            // Getting file data
                            var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                            using var fileStream = File.Create($"{projectArrhythmia.Path}/Beatmaps.zip");
                            using var stream = await response.Content.ReadAsStreamAsync();
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                            }
                            fileStream.Close();

                            UnZip($"{projectArrhythmia.Path}/Beatmaps.zip", $"{projectArrhythmia.Path}");

                            File.Delete($"{projectArrhythmia.Path}/Beatmaps.zip");
                        }
                    }

                    SetLaunchButtonsActive(true);
                }
                catch
                {
                    SetLaunchButtonsActive(true);
                }
            }

            bool URLExists(string url)
            {
                try
                {
                    using var http = new HttpClient();
                    using var res = http.GetAsync(url);

                    return res.Result.StatusCode == HttpStatusCode.OK;
                }
                catch
                {
                    //Any exception will returns false.
                    return false;
                }
            }
        }

        bool shouldSaveVersions = true;
        void VersionsChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox list && list.SelectedItem is ListBoxItem item && item.Content is string version)
            {
                Versions.Content = $"Version: {version}";
                Versions.Flyout?.Hide();

                if (!shouldSaveVersions)
                {
                    shouldSaveVersions = true;
                    return;
                }

                if (InstancesListBox.SelectedItem is ListBoxItem instance && instance.DataContext is ProjectArrhythmia projectArrhythmia && projectArrhythmia.Settings != null)
                {
                    Debug.WriteLine($"Changed version to {version}");
                    projectArrhythmia.Settings.CurrentVersion = version;
                    projectArrhythmia.Settings.SaveSettings();
                }
            }
        }

        void UnityExplorerToggleClick(object? sender, RoutedEventArgs e)
        {
            if (InstancesListBox.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia && projectArrhythmia.Settings != null)
            {
                projectArrhythmia.Settings.UnityExplorer = !projectArrhythmia.Settings.UnityExplorer;
                UnityExplorerToggle.Content = $"Unity Explorer {(projectArrhythmia.Settings.UnityExplorer ? "✓" : "✕")}";
                projectArrhythmia.Settings.SaveSettings();
            }
        }
        
        void EditorOnStartupToggleClick(object? sender, RoutedEventArgs e)
        {
            if (InstancesListBox.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia && projectArrhythmia.Settings != null)
            {
                projectArrhythmia.Settings.EditorOnStartup = !projectArrhythmia.Settings.EditorOnStartup;
                EditorOnStartupToggle.Content = $"Editor on Startup {(projectArrhythmia.Settings.EditorOnStartup ? "✓" : "✕")}";
                projectArrhythmia.Settings.SaveSettings();
            }
        }
        
        void BetterLegacyToggleClick(object? sender, RoutedEventArgs e)
        {
            if (InstancesListBox.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia && projectArrhythmia.Settings != null)
            {
                projectArrhythmia.Settings.BetterLegacy = !projectArrhythmia.Settings.BetterLegacy;
                BetterLegacyToggle.Content = $"BetterLegacy {(projectArrhythmia.Settings.BetterLegacy ? "✓" : "✕")}";
                projectArrhythmia.Settings.SaveSettings();
            }
        }

        void RoundSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                settings.Roundness = slider.Value;
                UpdateRoundness();

                if (!savingSettings)
                    SaveSettings();
            }
        }

        void HSVdataUpdate(object sender, RangeBaseValueChangedEventArgs e)
        {
            var slider = sender as Slider;
            if (slider != null)
            {
                if (slider.Name == "HueSlider") hsvHue = slider.Value;
                if (slider.Name == "SaturationSlider") hsvSaturation = slider.Value;
                if (slider.Name == "ValueSlider") hsvValue = slider.Value;
            }
            ColorsUpdate();
            if(!savingSettings) SaveSettings();


        }

        void ResetToDefaultThemeButtonPresed(object sender, EventArgs e)
        {
            ResetToDefaultTheme();
        }

        void ResetToDefaultTheme()
        {
            var color = Color.FromRgb(255, 140, 0);
            var resources = this.Resources;
            resources["SystemColor"] = new SolidColorBrush(color);
            HueSlider.Value = 33 / 400.0;
            SaturationSlider.Value = 64 / 100.0;
            ValueSlider.Value = 1;
        }

        void ColorsUpdate()
        {
            hsvHue = HueSlider.Value;
            hsvSaturation = SaturationSlider.Value;
            hsvValue = ValueSlider.Value;
            var color = LauncherHelper.FromHsv(hsvHue, hsvSaturation, hsvValue);

            var resources = Resources;
            resources["SystemColor"] = new SolidColorBrush(color);
        }
        //255 186 122
        //29 52 100
        //FFFFAD5C

        void InstancesListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox list && list.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia)
            {
                Debug.WriteLine($"Selected: {projectArrhythmia}");
                Debug.WriteLine($"Settings is null: {projectArrhythmia.Settings == null}");

                if (projectArrhythmia.Settings == null)
                    return;

                Debug.WriteLine($"BetterLegacy Version: {projectArrhythmia.Settings.CurrentVersion}");
                if (Versions.Flyout is Flyout flyout && flyout.Content is ListBox versionList && versionList.Items.Any(x => x is ListBoxItem version && version.Content is string str && str == projectArrhythmia.Settings.CurrentVersion))
                {
                    shouldSaveVersions = false;
                    versionList.SelectedItem = versionList.Items.First(x => x is ListBoxItem version && version.Content is string str && str == projectArrhythmia.Settings.CurrentVersion);
                    shouldSaveVersions = true;
                }

                BetterLegacyToggle.Content = $"BetterLegacy {(projectArrhythmia.Settings.BetterLegacy ? "✓" : "✕")}";
                EditorOnStartupToggle.Content = $"Editor on Startup {(projectArrhythmia.Settings.EditorOnStartup ? "✓" : "✕")}";
                UnityExplorerToggle.Content = $"Unity Explorer {(projectArrhythmia.Settings.UnityExplorer ? "✓" : "✕")}";
            }
        }

        public void MenuButtonPressed(object? sender, RoutedEventArgs args)
        {
            UpdateButtons(sender as Button);
        }

        void MainView_DataContextChanged(object? sender, EventArgs e)
        {

        }

        public void SettingsRoundedClick(object? sender, EventArgs e)
        {
            settings.Rounded = !settings.Rounded;
            SettingRounded.Content = $"Rounded UI   {(settings.Rounded ? "✓" : "✕")}";
            UpdateRoundness();
            SaveSettings();
        }

        #endregion

        #region Misc

        public static string BepInExURL => "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip";

        public static void UnZip(string path, string output)
        {
            using var archive = ZipFile.Open(path, ZipArchiveMode.Update);

            for (int i = 0; i < archive.Entries.Count; i++)
            {
                var entry = archive.Entries[i];

                var fullName = entry.FullName;

                // Create folders if they don't exist already
                var directory = Path.GetDirectoryName(output + "/" + fullName);
                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (fullName.Contains('.'))
                    entry.ExtractToFile(output + "/" + fullName, true);
            }
        }

        int CalculateProgress(long totalBytes, long bytesDownloaded)
        {
            return (int)((bytesDownloaded * 100) / totalBytes);
        }

        async void ProgressBarUpdater(string path, string name, long totalBytes)
        {
            while (true)
            {
                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    ProgressBar.Value = CalculateProgress(totalBytes, fileInfo.Length);
                    LabelProgressBar.Content = $"{name} " + FormatProgress(totalBytes, fileInfo.Length);
                }
                await Task.Delay(2000);
            }
        }

        string FormatBytes(long bytes)
        {
            double megabytes = (double)bytes / (1024 * 1024);
            return $"{megabytes:F1} MB";
        }

        string FormatProgress(long totalBytes, long bytesDownloaded)
        {
            return $"{FormatBytes(bytesDownloaded)} / {FormatBytes(totalBytes)}";
        }

        #endregion
    }
}