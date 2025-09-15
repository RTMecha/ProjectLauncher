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
using Avalonia.OpenGL;

#pragma warning disable CS0618 // Type or member is obsolete
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

        public static string LatestBetterLegacyVersion { get; set; } = "1.0.0";

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

            Changelog = await LoadAssetStream("changelog.txt");
            AboutPageInfo.Markdown = await LoadAssetStream("about.md");

            var list = new ListBox();
            list.Background = new SolidColorBrush(Color.Parse("#FF141414"));
            list.Foreground = new SolidColorBrush(Color.Parse("#FF141414"));
            list.BorderBrush = new SolidColorBrush(Color.Parse("#FF141414"));

            list.SelectionChanged += VersionsChanged;

            Versions.Flyout = new Flyout
            {
                Content = list,
            };

            await LoadVersions();
            RefreshVersions.Click += RefreshVersionsClick;
            AutoUpdateToggle.Click += AutoUpdateToggleClick;
            BetterLegacyToggle.Click += BetterLegacyToggleClick;
            EditorOnStartupToggle.Click += EditorOnStartupToggleClick;
            UnityExplorerToggle.Click += UnityExplorerToggleClick;
            InstancesListBox.SelectionChanged += InstancesListBoxSelectionChanged;
            InstanceManager.LoadInstances();
            await LoadSettings();

            Launch.Click += LaunchClick;
            Update.Click += UpdateClick;

            InstancesSearchField.TextChanged += InstancesSearchChanged;
            CreateNewInstanceButton.Click += CreateNewInstanceClicked;
            AppPathBrowse.Click += AppPathBrowseClick;
            AppPathField.TextChanged += AppPathFieldChanged;

            OpenInstanceFolderButton.Click += OpenInstanceFolderClick;
            ZipInstanceButton.Click += ZipInstanceClick;
            PasteBeatmapsFolderButton.Click += PasteBeatmapsFolderClick;
            DownloadDemoBeatmapsButton.Click += DownloadDemoBeatmapsClick;

            SettingRounded.Click += SettingsRoundedClick;
            SettingUpdateLauncher.Click += UpdateLauncherClick;
            RoundSlider.ValueChanged += RoundSliderValueChanged;

            HueSlider.ValueChanged += HSVdataUpdate;
            SaturationSlider.ValueChanged += HSVdataUpdate;
            ValueSlider.ValueChanged += HSVdataUpdate;
            ResetToDefaultThemeButton.Click += ResetToDefaultThemeButtonPresed;

            SettingShowSnapshots.Click += SettingShowSnapshotsChanged;

            LoadNews();

            NewsListBox.SelectionChanged += ArticleSelected;

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

        async Task<string> LoadAssetStream(string asset)
        {
            using var stream = AssetLoader.Open(new Uri("avares://ProjectLauncher/Assets/" + asset));
            using var streamReader = new StreamReader(stream);

            return await streamReader.ReadToEndAsync();
        }

        async Task LoadVersions()
        {
            try
            {
                Versions.IsVisible = false;
                if (Versions.Flyout is not Flyout flyout || flyout.Content is not ListBox list)
                    return;

                var http = new HttpClient();
                http.Timeout = new TimeSpan(0, 0, 20);
                var versionString = await http.GetStringAsync("https://github.com/RTMecha/BetterLegacy/raw/master/versions.lss");

                if (string.IsNullOrEmpty(versionString))
                    return;

                var versions = versionString.Split(',');

                list.Items.Clear();

                bool setLatest = false;
                for (int i = versions.Length - 1; i >= 0; i--)
                {
                    var version = versions[i];

                    if (!settings.ShowSnapshots && (version.StartsWith("snapshot") || version.Contains("pre")))
                        continue;

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

                    if (setLatest)
                        continue;

                    setLatest = true;
                    LatestBetterLegacyVersion = version;
                }
                Debug.WriteLine($"Latest BetterLegacy version: {LatestBetterLegacyVersion}");
                Versions.IsVisible = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load versions due to the exception: {ex}");
            }
        }

        async void LoadUpdateNotes()
        {
            try
            {
                ChangelogNotes.Markdown = Changelog;

                var http = new HttpClient();
                var str = await http.GetStringAsync("https://github.com/RTMecha/BetterLegacy/raw/master/updates.md");

                if (!string.IsNullOrEmpty(str)) BetterLegacyNotes.Markdown = str;


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
                
                if (jn["instances"]["show_snapshots"] != null)
                    settings.ShowSnapshots = jn["instances"]["show_snapshots"].AsBool;

                if (jn["hsv"]["hue"] != null)
                    HueSlider.Value = Convert.ToDouble(jn["hsv"]["hue"].Value);

                if (jn["hsv"]["saturation"] != null)
                    SaturationSlider.Value = Convert.ToDouble(jn["hsv"]["saturation"].Value);

                if (jn["hsv"]["value"] != null)
                    ValueSlider.Value = Convert.ToDouble(jn["hsv"]["value"].Value);

                if (jn["hsv"]["hue"] == null || jn["hsv"]["saturation"] == null || jn["hsv"]["value"] == null)
                    ResetToDefaultTheme();

                ColorsUpdate();
            }

            SettingRounded.Content = $"Rounded UI   {(settings.Rounded ? "✓" : "✕")}";
            SettingShowSnapshots.Content = $"Show Snapshots / Prereleases   {(settings.ShowSnapshots ? "✓" : "✕")}";

            UpdateRoundness();
            SaveSettings();
        }

        bool savingSettings;
        async void SaveSettings()
        {
            savingSettings = true;
            var settingsPath = SettingsFile;
            var jn = JSON.Parse("{}");
            jn["ui"]["rounded"] = settings.Rounded;
            jn["ui"]["roundness"] = RoundSlider.Value;
            jn["hsv"]["hue"] = HueSlider.Value;
            jn["hsv"]["saturation"] = SaturationSlider.Value;
            jn["hsv"]["value"] = ValueSlider.Value;
            if (AppPathField != null && !string.IsNullOrEmpty(AppPathField.Text))
                jn["instances"]["app_path"] = AppPathField.Text;

            if (settings.ShowSnapshots)
                jn["instances"]["show_snapshots"] = settings.ShowSnapshots;

            await File.WriteAllTextAsync(settingsPath, jn.ToString(6));
            savingSettings = false;
        }

        void UpdateRoundness()
        {
            double roundessValue = settings.Rounded ? settings.Roundness : 0;
            var resources = Resources;
            resources["CornerRadius"] = new CornerRadius(roundessValue);
            resources["TextBoxCornerRadius"] = new CornerRadius(roundessValue, roundessValue, 0,0);
        }

        async void LoadNews()
        {
            try
            {
                var url = LauncherHelper.NewsURL;

                using var http = new HttpClient();
                var text = await http.GetStringAsync(url + "news.json");
                if (string.IsNullOrEmpty(text))
                    return;

                var jn = JSON.Parse(text);


                if (jn["articles"].IsArray)
                {
                    NewsListBox.Items.Clear();
                    for (int i = 0; i < jn["articles"].Count; i++)
                    {
                        var article = jn["articles"][i].Value;
                        var articleURL = url + article + "/article.md";

                        var item = new ListBoxItem
                        {
                            DataContext = articleURL,
                            Content = article
                        };
                        item.Classes.Add("lbs1");
                        NewsListBox.Items.Add(item);
                    }
                }

                var latestURL = url + jn["latest"] + "/article.md";
                var latest = await http.GetStringAsync(latestURL);
                if (string.IsNullOrEmpty(latest))
                    return;

                News.Markdown = latest;
            }
            catch
            {
                
            }
        }

        #region Senders

        async void SettingShowSnapshotsChanged(object sender, RoutedEventArgs e)
        {
            if (settings == null)
                return;

            settings.ShowSnapshots = !settings.ShowSnapshots;
            SettingShowSnapshots.Content = $"Show Snapshots / Prereleases   {(settings.ShowSnapshots ? "✓" : "✕")}";
            SaveSettings();
            await LoadVersions();
        }

        async void ArticleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox list && list.SelectedItem is ListBoxItem item && item.DataContext is string articleURL)
            {
                using var http = new HttpClient();

                var article = await http.GetStringAsync(articleURL);

                News.Markdown = article;
            }
        }

        void DownloadDemoBeatmapsClick(object sender, RoutedEventArgs e) => InstanceManager.DownloadDemoBeatmaps();

        void PasteBeatmapsFolderClick(object sender, RoutedEventArgs e) => InstanceManager.PasteBeatmapsFolder();

        void OpenModSettingsWindow(object sender, RoutedEventArgs e)
        {
            var modSettingsWindow = new ModSettingsWindow()
            {
                DataContext = new ModSettingsViewModel(),
            };
            modSettingsWindow.Show(MainWindow.Instance);
        }

        void ZipInstanceClick(object sender, RoutedEventArgs e) => InstanceManager.ZipInstance();

        void OpenInstanceFolderClick(object sender, RoutedEventArgs e) => InstanceManager.OpenInstanceFolder();

        void UpdateLauncherClick(object sender, RoutedEventArgs e) => LauncherHelper.UpdateLauncher();

        void AppPathFieldChanged(object sender, TextChangedEventArgs e)
        {
            if (!savingSettings)
                SaveSettings();
        }

        async void AppPathBrowseClick(object sender, RoutedEventArgs e)
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

            if (File.Exists(path + "/Game Assembly.dll")) // launcher doesn't support alpha for now
                return;

            AppPathField.Text = path;
        }

        void CreateNewInstanceClicked(object sender, RoutedEventArgs e) => InstanceManager.CreateNewInstance();

        void InstancesSearchChanged(object sender, TextChangedEventArgs e) => InstanceManager.LoadInstances();

        void LaunchClick(object sender, RoutedEventArgs e) => InstanceManager.LaunchInstance();

        void UpdateClick(object sender, RoutedEventArgs e) => InstanceManager.UpdateInstance();

        bool shouldSaveVersions = true;
        void VersionsChanged(object sender, SelectionChangedEventArgs e)
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

        void UnityExplorerToggleClick(object sender, RoutedEventArgs e)
        {
            var projectArrhythmia = InstanceManager.Current;
            if (projectArrhythmia == null || projectArrhythmia.Settings == null)
                return;

            projectArrhythmia.Settings.UnityExplorer = !projectArrhythmia.Settings.UnityExplorer;
            UnityExplorerToggle.Content = $"Unity Explorer {(projectArrhythmia.Settings.UnityExplorer ? "✓" : "✕")}";
            projectArrhythmia.Settings.SaveSettings();
        }
        
        void EditorOnStartupToggleClick(object sender, RoutedEventArgs e)
        {
            var projectArrhythmia = InstanceManager.Current;
            if (projectArrhythmia == null || projectArrhythmia.Settings == null)
                return;

            projectArrhythmia.Settings.EditorOnStartup = !projectArrhythmia.Settings.EditorOnStartup;
            EditorOnStartupToggle.Content = $"Editor on Startup {(projectArrhythmia.Settings.EditorOnStartup ? "✓" : "✕")}";
            projectArrhythmia.Settings.SaveSettings();
        }
        
        void BetterLegacyToggleClick(object sender, RoutedEventArgs e)
        {
            var projectArrhythmia = InstanceManager.Current;
            if (projectArrhythmia == null || projectArrhythmia.Settings == null)
                return;

            projectArrhythmia.Settings.BetterLegacy = !projectArrhythmia.Settings.BetterLegacy;
            BetterLegacyToggle.Content = $"BetterLegacy {(projectArrhythmia.Settings.BetterLegacy ? "✓" : "✕")}";
            projectArrhythmia.Settings.SaveSettings();
        }

        void AutoUpdateToggleClick(object sender, RoutedEventArgs e)
        {
            var projectArrhythmia = InstanceManager.Current;
            if (projectArrhythmia == null || projectArrhythmia.Settings == null)
                return;

            projectArrhythmia.Settings.AutoUpdate = !projectArrhythmia.Settings.AutoUpdate;
            AutoUpdateToggle.Content = $"Auto Update {(projectArrhythmia.Settings.AutoUpdate ? "✓" : "✕")}";
            projectArrhythmia.Settings.SaveSettings();
        }

        async void RefreshVersionsClick(object sender, RoutedEventArgs e)
        {
            await LoadVersions();
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

        void InstancesListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox list && list.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia)
                UpdateInstanceView(projectArrhythmia);
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

        #region View Functions

        public void UpdateInstanceView(ProjectArrhythmia projectArrhythmia)
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

            AutoUpdateToggle.Content = $"Auto Update {(projectArrhythmia.Settings.AutoUpdate ? "✓" : "✕")}";
            BetterLegacyToggle.Content = $"BetterLegacy {(projectArrhythmia.Settings.BetterLegacy ? "✓" : "✕")}";
            EditorOnStartupToggle.Content = $"Editor on Startup {(projectArrhythmia.Settings.EditorOnStartup ? "✓" : "✕")}";
            UnityExplorerToggle.Content = $"Unity Explorer {(projectArrhythmia.Settings.UnityExplorer ? "✓" : "✕")}";
        }

        public void SetLaunchButtonsActive(bool active)
        {
            FunctionButtons.IsVisible = active;
            BetterLegacyToggle.IsVisible = active;
            EditorOnStartupToggle.IsVisible = active;
            UnityExplorerToggle.IsVisible = active;

            ProgressBar.IsVisible = !active;
            LabelProgressBar.IsVisible = !active;
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

        int CalculateProgress(long totalBytes, long bytesDownloaded) => (int)((bytesDownloaded * 100) / totalBytes);

        public async void ProgressBarUpdater(string path, string name, long totalBytes)
        {
            while (true)
            {
                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    ProgressBar.Value = CalculateProgress(totalBytes, fileInfo.Length);
                    LabelProgressBar.Content = $"{name} " + FormatProgress(totalBytes, fileInfo.Length);
                }
                else
                {
                    ProgressBar.Value = 0;
                    LabelProgressBar.Content = $"{name} " + FormatProgress(totalBytes, 0);
                }
                await Task.Delay(100);

                if (!InstanceManager.Downloading)
                    break;
            }
        }

        string FormatBytes(long bytes) => $"{(double)bytes / (1024 * 1024):F1} MB";

        string FormatProgress(long totalBytes, long bytesDownloaded) => $"{FormatBytes(bytesDownloaded)} / {FormatBytes(totalBytes)}";

        #endregion
    }
}
#pragma warning restore CS0618 // Type or member is obsolete