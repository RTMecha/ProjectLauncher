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

        public static string CurrentVersion { get; set; } = "1.0.0"; // BetterLegacy version 1

        public static string Changelog =>
            $"2.0.0 > [May 22, 2024]\n" +
            $"- Completely reworked Project Launcher to use a different basis and to use the merged mods rather than the individual.\n" +
            $"2.0.1 > [May 22, 2024]\n" +
            $"- Fixed URL for BetterLegacy versions being incorrect.\n" +
            $"2.0.2 > [May 22, 2024]\n" +
            $"- Made launch and update buttons turn invisible when an instance is updating.\n" +
            $"2.1.0 > [May 24, 2024]\n" +
            $"- Redesigned some UI elements to be easier to look at with some new icons and better layout.\n" +
            $"- The launcher now has an auto updater. Check it out in the settings tab.\n" +
            $"2.1.1 > [May 25, 2024]\n" +
            $"- Updated some roundness and added a roundness slider.\n" +
            $"2.1.2 > [May 26, 2024]\n" +
            $"- Added interface color adjustment in the Settings.\n" +
            $"- Fixed the problem of incorrect rounded strength data load\n" +
            $"2.1.3 > [Jun 9, 2024]\n" +
            $"- The update window has been improved.\n" +
            $"2.1.4 > [Jun 9, 2024]\n" +
            $"- Fixed updater program not updating itself.\n" +
            $"- Added a light shadow for text on buttons in the left panel.\n" +
            $"2.1.5 > [Jun 26, 2024]\n" +
            $"- Fully fixed launcher not updating properly.\n" +
            $"- Tried adding a progress bar to instance updater with no success atm.\n" +
            $"- Fixed some grammar and added some tooltips.\n" +
            $"- Fixed version dropdown value not displaying the correct version on app startup some times.\n" +
            $"2.1.6 > [Jun 26, 2024]\n" +
            $"- Quick unzip hotfix.";


        public MainView()
        {
            InitializeComponent();
            DataContextChanged += MainView_DataContextChanged;
            Instance = this;

            Load();
        }

        async void Load()
        {
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

            if (!Directory.Exists(MainDirectory + "UnZIP"))
                Directory.CreateDirectory(MainDirectory + "UnZIP");

            var unzip = MainDirectory + "UnZIP/ProjectLauncher.Unzip.exe";

            File.Move(MainDirectory + "ProjectLauncher.Unzip.exe", unzip, File.Exists(unzip));

            var startInfoNnZip = new ProcessStartInfo();
            startInfoNnZip.FileName = unzip;
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

                CurrentVersion = versions[versions.Length - 1];
            }
        }

        async void LoadUpdateNotes()
        {
            try
            {
                ChangelogNotes.Text = Changelog;

                var http = new HttpClient();
                var str = await http.GetStringAsync("https://github.com/RTMecha/BetterLegacy/raw/master/updates.lss");

                if (!string.IsNullOrEmpty(str))
                    BetterLegacyNotes.Text = str;

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
                Rounded = jn["ui"]["rounded"].AsBool;

                if (!string.IsNullOrEmpty(jn["ui"]["roundness"]))
                    RoundSlider.Value = Convert.ToDouble(jn["ui"]["roundness"].Value);
                RoundValue = RoundSlider.Value;

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
            double roundessValue = Rounded ? RoundValue : 0;
            var resources = this.Resources;
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
                        ProgressBarUpdater(bepZipOutput, "BepInEx", contentLength.Value);

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
                        var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/Beatmaps.zip";

                        if (URLExists(url))
                        {
                            var bytes = await http.GetByteArrayAsync(url);

                            await File.WriteAllBytesAsync($"{projectArrhythmia.Path}/Beatmaps.zip", bytes);

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

        public static double RoundValue;
        void RoundSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                RoundValue = slider.Value;
                UpdateRoundness();

                if (!savingSettings)
                    SaveSettings();
            }
        }

        private double hsvHue;
        private double hsvSaturation;
        private double hsvValue;
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
            var color = FromHsv(hsvHue, hsvSaturation, hsvValue);

            var resources = this.Resources;
            resources["SystemColor"] = new SolidColorBrush(color);
        }
        //255 186 122
        //29 52 100
        //FFFFAD5C

        public static Color FromHsv(double h, double S, double V)
        {
            int r, g, b;
            if (S == 0)
            {
                r = g = b = (int)(V * 255.0f + 0.5f);
            }
            else
            {
                double var_H = h * 6;
                if (var_H == 6) var_H = 0;
                int var_i = (int)var_H;
                double var_1 = V * (1 - S);
                double var_2 = V * (1 - S * (var_H - var_i));
                double var_3 = V * (1 - S * (1 - (var_H - var_i)));

                double var_r, var_g, var_b;
                if (var_i == 0) { var_r = V; var_g = var_3; var_b = var_1; }
                else if (var_i == 1) { var_r = var_2; var_g = V; var_b = var_1; }
                else if (var_i == 2) { var_r = var_1; var_g = V; var_b = var_3; }
                else if (var_i == 3) { var_r = var_1; var_g = var_2; var_b = V; }
                else if (var_i == 4) { var_r = var_3; var_g = var_1; var_b = V; }
                else { var_r = V; var_g = var_1; var_b = var_2; }

                r = (int)(var_r * 255.0f + 0.5f);
                g = (int)(var_g * 255.0f + 0.5f);
                b = (int)(var_b * 255.0f + 0.5f);
            }
            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }



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
                    LabelProgressBar.Content = $"Downloading {name}" + FormatProgress(totalBytes, fileInfo.Length);
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