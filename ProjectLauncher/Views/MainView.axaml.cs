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

namespace ProjectLauncher.Views
{

    public partial class MainView : UserControl
    {
        public static MainView Instance { get; set; }

        public List<PageManager> pages = new List<PageManager>();
        public bool Rounded { get; set; } = true;

        //public static string InstancesFolder => Directory.GetCurrentDirectory().Replace("\\", "/") + "/instances";
        public static string MainDirectory => Directory.GetCurrentDirectory().Replace("\\", "/") + "/";
        //public static string MainDirectory => "E:/Coding/ProjectArrhythmiaLauncher-master/ProjectLauncher.Desktop/bin/Debug/net6.0/";
        public static string InstancesFolder => $"{MainDirectory}instances";
        public static string SettingsFile => $"{MainDirectory}settings.lss";

        public static string CurrentVersion { get; set; } = "1.0.0";

        public static string Changelog =>
            $"2.0.0 > [May 22, 2024]\n" +
            $"- Completely reworked Project Launcher to use a different basis and to use the merged mods rather than the individual." +
            $"2.0.1 > [May 22, 2024]\n" +
            $"- Fixed URL for BetterLegacy versions being incorrect." +
            $"2.0.2 > [May 22, 2024]\n" +
            $"- Made launch and update buttons turn invisible when an instance is updating.";

        public MainView()
        {
            InitializeComponent();
            DataContextChanged += MainView_DataContextChanged;
            Instance = this;

            pages.Add(new PageManager(LaunchButton, LaunchWindow));
            pages.Add(new PageManager(ModsButton, ModsWindow));
            pages.Add(new PageManager(VersionsButton, VersionsWindow));
            pages.Add(new PageManager(ChangelogButton, ChangelogWindow));
            UpdateButtons(LaunchButton);

            var list = new ListBox();
            list.Background = new SolidColorBrush(Color.Parse("#FF141414"));
            list.Foreground = new SolidColorBrush(Color.Parse("#FF141414"));
            list.BorderBrush = new SolidColorBrush(Color.Parse("#FF141414"));

            list.SelectionChanged += VersionsChanged;

            Versions.Flyout = new Flyout
            {
                Content = list,
            };

            LoadVersions();
            BetterLegacyToggle.Click += BetterLegacyToggleClick;
            EditorOnStartupToggle.Click += EditorOnStartupToggleClick;
            UnityExplorerToggle.Click += UnityExplorerToggleClick;
            InstancesListBox.SelectionChanged += InstancesListBoxSelectionChanged;
            LoadInstances();
            LoadSettings();

            Launch.Click += LaunchClick;
            Update.Click += UpdateClick;
            InstancesSearchField.TextChanged += InstancesSearchChanged;
            CreateNewInstanceButton.Click += CreateNewInstanceClicked;
            AppPathBrowse.Click += AppPathBrowseClick;
            AppPathField.TextChanged += AppPathFieldChanged;
            SettingRounded.Click += SettingsRoundedClick;

            LoadUpdateNotes();
        }

        // add async when downloading versions file.
        async void LoadVersions()
        {
            if (Versions.Flyout is Flyout flyout && flyout.Content is ListBox list)
            {
                var http = new HttpClient();
                var versionString = await http.GetStringAsync("https://github.com/RTMecha/BetterLegacy/raw/master/versions.lss");

                if (string.IsNullOrEmpty(versionString))
                    return;

                var versions = versionString.Split(',');

                list.Items.Clear();
                for (int i = 0; i < versions.Length; i++)
                {
                    var version = versions[i];

                    list.Items.Add(new ListBoxItem
                    {
                        Content = version,
                        Background = new SolidColorBrush(Color.Parse("#FF424242")),
                        Foreground = new SolidColorBrush(Color.Parse("#ffba7a")),
                        BorderBrush = new SolidColorBrush(Color.Parse("#FF141414")),
                        FontWeight = FontWeight.Bold,
                        FontFamily = FontFamily.Default,
                        BorderThickness = new Thickness(0, 0, 0, 3),
                    });
                }
            }
        }

        async void LoadUpdateNotes()
        {
            try
            {
                ChangelogNotes.Content = Changelog;

                var http = new HttpClient();
                var str = await http.GetStringAsync("https://github.com/RTMecha/BetterLegacy/raw/master/updates.lss");

                if (!string.IsNullOrEmpty(str))
                    BetterLegacyNotes.Content = str;
            }
            catch (Exception ex)
            {

            }
        }

        async void LoadSettings()
        {
            var settingsPath = SettingsFile;
            if (File.Exists(settingsPath))
            {
                var json = await File.ReadAllTextAsync(settingsPath);
                var jn = JSON.Parse(json);
                Rounded = jn["ui"]["rounded"].AsBool;

                if (!string.IsNullOrEmpty(jn["instances"]["app_path"]))
                {
                    AppPathField.Text = jn["instances"]["app_path"];
                }
            }

            SettingRounded.Content = $"Rounded UI   {(Rounded ? "✓" : "✕")}";

            UpdateRoundness();

            SaveSettings();
        }

        bool savingSettings;
        async void SaveSettings()
        {
            savingSettings = true;
            var settingsPath = SettingsFile;
            var jn = JSON.Parse("{}");
            jn["ui"]["rounded"] = Rounded.ToString();
            if (AppPathField != null && !string.IsNullOrEmpty(AppPathField.Text))
                jn["instances"]["app_path"] = AppPathField.Text;

            await File.WriteAllTextAsync(settingsPath, jn.ToString(3));
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

                InstancesListBox.Items.Add(new ListBoxItem
                {
                    DataContext = instance,
                    Content = Path.GetFileName(directories[i]),
                    Foreground = new SolidColorBrush(Color.Parse("#ffba7a")),
                    FontFamily = FontFamily.Default,
                    BorderThickness = new Thickness(0,0,0,3),
                    CornerRadius = new CornerRadius(7),
                    Background = new SolidColorBrush(Color.Parse("#383838")),
                    BorderBrush = new SolidColorBrush(Color.Parse("#FF141414")),
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    FontWeight = FontWeight.Bold,
                    Width = 600,
                    Margin = new Thickness(0, 0, 0, 0),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                });
            }

            SetLaunchButtonsActive(InstancesListBox.ItemCount > 0);

            if (InstancesListBox.ItemCount > 0)
                InstancesListBox.SelectedItem = InstancesListBox.Items[0];
        }

        void SetLaunchButtonsActive(bool active)
        {
            Launch.IsVisible = active;
            Update.IsVisible = active;
            Versions.IsVisible = active;
            BetterLegacyToggle.IsVisible = active;
            EditorOnStartupToggle.IsVisible = active;
            UnityExplorerToggle.IsVisible = active;
        }

        void UpdateRoundness()
        {
            var roundness = new CornerRadius(Rounded ? 7 : 0);
            LaunchButton.CornerRadius = roundness;
            ModsButton.CornerRadius = roundness;
            VersionsButton.CornerRadius = roundness;
            ChangelogButton.CornerRadius = roundness;
            AppPathField.CornerRadius = roundness;
            AppPathBrowse.CornerRadius = roundness;
            SettingRounded.CornerRadius = roundness;
            InstancesListBox.CornerRadius = roundness;
            InstancesSearchField.CornerRadius = roundness;
            NewInstanceNameField.CornerRadius = roundness;
            CreateNewInstanceButton.CornerRadius = roundness;
            Launch.CornerRadius = roundness;
            Update.CornerRadius = roundness;
            BetterLegacyToggle.CornerRadius = roundness;
            EditorOnStartupToggle.CornerRadius = roundness;
            UnityExplorerToggle.CornerRadius = roundness;
            Versions.CornerRadius = roundness;

            if (Versions.Flyout is Flyout flyout && flyout.Content is ListBox list)
            {
                list.CornerRadius = roundness;
                for (int i = 0; i < list.ItemCount; i++)
                {
                    if (list.Items[i] is ListBoxItem item)
                    {
                        item.CornerRadius = roundness;
                    }
                }
            }

            for (int i = 0; i < InstancesListBox.ItemCount; i++)
            {
                if (InstancesListBox.Items[i] is ListBoxItem item)
                {
                    item.CornerRadius = roundness;
                }
            }
        }

        public void UpdateButtons(Button button)
        {
            foreach (var page in pages)
            {
                var selected = page.PageButton.Name == button.Name;
                page.Page.IsVisible = selected;

                page.PageButton.Background = new SolidColorBrush(Color.Parse(selected ? "#FFFF550D" : "#FF383838"));
                page.PageButton.Foreground = new SolidColorBrush(Color.Parse(selected ? "#FFFFFF" : "#FFFFBA7A"));
            }
        }

        #region Senders

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
                    Launch.IsVisible = false;
                    Update.IsVisible = false;
                    // Download BepInEx (Obviously will need BepInEx itself to run any mods)
                    if (!Directory.Exists(projectArrhythmia.Path + "/BepInEx"))
                    {
                        var bep = projectArrhythmia.Path + "/BepInEx-5.4.21.zip";

                        using var http = new HttpClient();

                        var bytes = await http.GetByteArrayAsync(BepInExURL);

                        await File.WriteAllBytesAsync(bep, bytes);

                        UnZip(bep, projectArrhythmia.Path + "/");

                        File.Delete(bep);
                    }

                    var pluginsPath = $"{projectArrhythmia.Path}/BepInEx/plugins";
                    var editorOnStartupPath = $"{pluginsPath}/EditorOnStartup.dll";
                    var betterLegacyPath = $"{pluginsPath}/BetterLegacy.dll";

                    if (!Directory.Exists(pluginsPath))
                        Directory.CreateDirectory(pluginsPath);

                    if (!File.Exists(editorOnStartupPath) && projectArrhythmia.Settings.EditorOnStartup)
                    {
                        using var http = new HttpClient();
                        var bytes = await http.GetByteArrayAsync("https://github.com/enchart/EditorOnStartup/releases/download/1.0.0/EditorOnStartup_1.0.0.dll");
                        await File.WriteAllBytesAsync(editorOnStartupPath, bytes);
                    }
                    else if (File.Exists(editorOnStartupPath) && !projectArrhythmia.Settings.EditorOnStartup)
                    {
                        File.Delete(editorOnStartupPath);
                    }

                    if (!File.Exists(betterLegacyPath) && projectArrhythmia.Settings.BetterLegacy)
                    {
                        using var http = new HttpClient();

                        var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/BetterLegacy.zip";

                        if (URLExists(url))
                        {
                            var bytes = await http.GetByteArrayAsync(url);
                            await File.WriteAllBytesAsync($"{pluginsPath}/BetterLegacy.zip", bytes);

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
                        using var http = new HttpClient();
                        var bytes = await http.GetByteArrayAsync("https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip");
                        await File.WriteAllBytesAsync($"{pluginsPath}/UnityExplorer.zip", bytes);

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
                        using var http = new HttpClient();

                        var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/steam_api64.dll";

                        if (URLExists(url))
                        {
                            var bytes = await http.GetByteArrayAsync(url);

                            await File.WriteAllBytesAsync($"{projectArrhythmia.Path}/Project Arrhythmia_Data/Plugins/steam_api64.dll", bytes);

                            await File.WriteAllTextAsync($"{projectArrhythmia.Path}/Project Arrhythmia_Data/Plugins/steam_api_updated.txt", "Yes");
                        }
                    }

                    if (!Directory.Exists($"{projectArrhythmia.Path}/beatmaps/prefabtypes") || !Directory.Exists($"{projectArrhythmia.Path}/beatmaps/shapes") || !Directory.Exists($"{projectArrhythmia.Path}/beatmaps/menus"))
                    {
                        using var http = new HttpClient();

                        var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/Beatmaps.zip";

                        if (URLExists(url))
                        {
                            var bytes = await http.GetByteArrayAsync(url);

                            await File.WriteAllBytesAsync($"{projectArrhythmia.Path}/Beatmaps.zip", bytes);

                            UnZip($"{projectArrhythmia.Path}/Beatmaps.zip", $"{projectArrhythmia.Path}");

                            File.Delete($"{projectArrhythmia.Path}/Beatmaps.zip");
                        }
                    }

                    Launch.IsVisible = true;
                    Update.IsVisible = true;
                }
                catch
                {
                    Launch.IsVisible = true;
                    Update.IsVisible = true;
                }
            }

            bool URLExists(string url)
            {
                try
                {
                    var http = new HttpClient();

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
                Debug.WriteLine($"Changed version to {version}");
                Versions.Content = $"Version: {version}";
                Versions.Flyout?.Hide();

                if (!shouldSaveVersions)
                {
                    shouldSaveVersions = true;
                    return;
                }

                if (InstancesListBox.SelectedItem is ListBoxItem instance && instance.DataContext is ProjectArrhythmia projectArrhythmia && projectArrhythmia.Settings != null)
                {
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

        void InstancesListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox list && list.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia)
            {
                Debug.WriteLine($"Selected: {projectArrhythmia}");
                Debug.WriteLine($"Settings is null: {projectArrhythmia.Settings == null}");

                if (projectArrhythmia.Settings == null)
                    return;

                if (Versions.Flyout is Flyout flyout && flyout.Content is ListBox versionList && versionList.Items.Any(x => x is ListBoxItem version && version.Content is string str && str == projectArrhythmia.Settings.CurrentVersion))
                {
                    Debug.WriteLine($"Update settings");
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
            Rounded = !Rounded;
            SettingRounded.Content = $"Rounded UI   {(Rounded ? "✓" : "✕")}";
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

        #endregion
    }


    public class PageManager
    {
        public PageManager(Button button, StackPanel page)
        {
            PageButton = button;
            Page = page;
        }
        public Button PageButton { get; private set; }
        public StackPanel Page { get; private set; }
    }

    public class ProjectArrhythmia
    {
        public string Path { get; set; } = string.Empty;

        public InstanceSettings? Settings { get; set; }

        public override string ToString() => System.IO.Path.GetFileName(Path);
    }

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

            if (!string.IsNullOrEmpty(jn["better_legacy"]))
                BetterLegacy = jn["better_legacy"].AsBool;

            if (!string.IsNullOrEmpty(jn["editor_on_startup"]))
                EditorOnStartup = jn["editor_on_startup"].AsBool;

            if (!string.IsNullOrEmpty(jn["unity_explorer"]))
                UnityExplorer = jn["unity_explorer"].AsBool;

            if (!string.IsNullOrEmpty(jn["version"]))
                CurrentVersion = jn["version"];
        }

        public async void SaveSettings()
        {
            var jn = JSON.Parse("{}");

            if (!BetterLegacy)
                jn["better_legacy"] = BetterLegacy.ToString();
            if (EditorOnStartup)
                jn["editor_on_startup"] = EditorOnStartup.ToString();
            if (UnityExplorer)
                jn["unity_explorer"] = UnityExplorer.ToString();

            jn["version"] = CurrentVersion;

            await File.WriteAllTextAsync(Path, jn.ToString(3));
        }

        public string Path { get; set; } = string.Empty;
        public bool BetterLegacy { get; set; } = true;
        public bool EditorOnStartup { get; set; } = false;
        public bool UnityExplorer { get; set; } = false;
        public string CurrentVersion { get; set; } = string.Empty;
    }
}