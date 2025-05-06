using Avalonia.Controls;
using ProjectLauncher.Data;
using ProjectLauncher.ViewModels;
using ProjectLauncher.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLauncher.Managers
{
    public class InstanceManager
    {
        public static ProjectArrhythmia Current
            => MainView.Instance.InstancesListBox.SelectedItem is ListBoxItem item && item.DataContext is ProjectArrhythmia projectArrhythmia ? projectArrhythmia : null;

        static bool zipping;

        public static void LaunchInstance()
        {
            var projectArrhythmia = Current;
            if (projectArrhythmia == null || !File.Exists(Path.Combine(projectArrhythmia.Path, "Project Arrhythmia.exe")))
                return;

            if (projectArrhythmia.Settings != null && projectArrhythmia.Settings.AutoUpdate && projectArrhythmia.Settings.CurrentVersion != MainView.LatestBetterLegacyVersion)
            {
                Debug.WriteLine($"DEBUG -> Update version");
                projectArrhythmia.Settings.CurrentVersion = MainView.LatestBetterLegacyVersion;
                projectArrhythmia.Settings.SaveSettings();
                MainView.Instance.UpdateInstanceView(projectArrhythmia);

                UpdateInstance(() => LaunchInstanceInternal(projectArrhythmia));
                return;
            }

            LaunchInstanceInternal(projectArrhythmia);
        }

        static void LaunchInstanceInternal(ProjectArrhythmia projectArrhythmia)
        {
            Debug.WriteLine($"Launch: {projectArrhythmia}");
            Process.Start(new ProcessStartInfo(Path.Combine(projectArrhythmia.Path, "Project Arrhythmia.exe")));
        }

        public static async void UpdateInstance(Action onUpdateFinish = null)
        {
            var projectArrhythmia = Current;
            if (projectArrhythmia == null || projectArrhythmia.Settings == null)
            {
                onUpdateFinish?.Invoke();
                return;
            }

            try
            {
                MainView.Instance.SetLaunchButtonsActive(false);
                using var http = new HttpClient();

                // Download BepInEx (Obviously will need BepInEx itself to run any mods)
                if (!Directory.Exists(projectArrhythmia.Path + "/BepInEx"))
                {
                    var bepZipOutput = projectArrhythmia.Path + "/BepInEx-5.4.21.zip";

                    var headRequest = new HttpRequestMessage(HttpMethod.Head, ModManager.BepInExURL);
                    var headResponse = await http.SendAsync(headRequest);
                    var contentLength = headResponse.Content.Headers.ContentLength;
                    MainView.Instance.ProgressBarUpdater(bepZipOutput, "Downloading BepInEx", contentLength.Value);

                    // Getting file data
                    var response = await http.GetAsync(ModManager.BepInExURL, HttpCompletionOption.ResponseHeadersRead);
                    using var fileStream = File.Create(bepZipOutput);
                    using var stream = await response.Content.ReadAsStreamAsync();
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                    fileStream.Close();

                    LauncherHelper.UnZip(bepZipOutput, projectArrhythmia.Path + "/");

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
                    File.Delete(editorOnStartupPath);

                if (projectArrhythmia.Settings.BetterLegacy)
                {
                    var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/BetterLegacy.zip";

                    if (http.URLExists(url))
                    {
                        var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                        var headResponse = await http.SendAsync(headRequest);
                        var contentLength = headResponse.Content.Headers.ContentLength;
                        MainView.Instance.ProgressBarUpdater($"{pluginsPath}/BetterLegacy.zip", "Downloading BetterLegacy", contentLength.Value);

                        // Getting file data
                        var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                        using var fileStream = File.Create($"{pluginsPath}/BetterLegacy.zip");
                        using var stream = await response.Content.ReadAsStreamAsync();
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            await fileStream.WriteAsync(buffer, 0, bytesRead);

                        fileStream.Close();

                        LauncherHelper.UnZip($"{pluginsPath}/BetterLegacy.zip", pluginsPath);

                        File.Delete($"{pluginsPath}/BetterLegacy.zip");
                    }
                }
                else if (File.Exists(betterLegacyPath) && !projectArrhythmia.Settings.BetterLegacy)
                    File.Delete(betterLegacyPath);

                if (!Directory.Exists($"{pluginsPath}/sinai-dev-UnityExplorer") && projectArrhythmia.Settings.UnityExplorer)
                {
                    var url = "https://github.com/sinai-dev/UnityExplorer/releases/download/4.9.0/UnityExplorer.BepInEx5.Mono.zip";
                    var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                    var headResponse = await http.SendAsync(headRequest);
                    var contentLength = headResponse.Content.Headers.ContentLength;
                    MainView.Instance.ProgressBarUpdater($"{pluginsPath}/UnityExplorer.zip", "Downloading UnityExplorer", contentLength.Value);

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

                    LauncherHelper.UnZip($"{pluginsPath}/UnityExplorer.zip", pluginsPath.Replace("/plugins", ""));

                    File.Delete($"{pluginsPath}/UnityExplorer.zip");
                }
                else if (Directory.Exists($"{pluginsPath}/sinai-dev-UnityExplorer") && !projectArrhythmia.Settings.UnityExplorer)
                    Directory.Delete($"{pluginsPath}/sinai-dev-UnityExplorer", true);

                // Download steam_api.dll
                if (!File.Exists($"{projectArrhythmia.Path}/Project Arrhythmia_Data/Plugins/steam_api_updated.txt"))
                {
                    var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/steam_api64.dll";

                    if (http.URLExists(url))
                    {
                        var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                        var headResponse = await http.SendAsync(headRequest);
                        var contentLength = headResponse.Content.Headers.ContentLength;
                        MainView.Instance.ProgressBarUpdater($"{projectArrhythmia.Path}/Project Arrhythmia_Data/Plugins/steam_api64.dll", "Updating steam_api64.dll", contentLength.Value);

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
            }
            catch { }
            MainView.Instance.SetLaunchButtonsActive(true);
            onUpdateFinish?.Invoke();
        }

        public static async void CreateNewInstance()
        {
            if (string.IsNullOrEmpty(MainView.Instance.AppPathField.Text) || !Directory.Exists(MainView.Instance.AppPathField.Text.Replace("\\", "/").Replace("/Project Arrhythmia.exe", "")) || string.IsNullOrEmpty(MainView.Instance.NewInstanceNameField.Text))
                return;

            var path = MainView.Instance.AppPathField.Text.Replace("\\", "/").Replace("//", "").Replace("/Project Arrhythmia.exe", "");
            var newPath = $"{MainView.InstancesFolder}/{MainView.Instance.NewInstanceNameField.Text}";

            if (!Directory.Exists(MainView.InstancesFolder))
                Directory.CreateDirectory(MainView.InstancesFolder);

            int num = 0;
            while (Directory.Exists(newPath))
            {
                newPath = $"{MainView.InstancesFolder}/{MainView.Instance.NewInstanceNameField.Text} [{num}]";
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

        public static async void LoadInstances()
        {
            MainView.Instance.InstancesListBox.Items.Clear();

            if (!Directory.Exists(MainView.InstancesFolder))
            {
                MainView.Instance.SetLaunchButtonsActive(false);
                return;
            }

            var directories = Directory.GetDirectories(MainView.InstancesFolder);

            if (directories.Length == 0)
            {
                MainView.Instance.SetLaunchButtonsActive(false);
                return;
            }

            MainView.Instance.SetLaunchButtonsActive(true);
            for (int i = 0; i < directories.Length; i++)
            {
                var folder = Path.GetFileName(directories[i]);
                if (!string.IsNullOrEmpty(MainView.Instance.InstancesSearchField.Text) && !folder.Contains(MainView.Instance.InstancesSearchField.Text, StringComparison.OrdinalIgnoreCase) || !File.Exists($"{directories[i]}/Project Arrhythmia.exe"))
                    continue;

                var instance = new ProjectArrhythmia
                {
                    Path = directories[i],
                    Settings = new InstanceSettings(directories[i]),
                };

                await instance.Settings.LoadSettings();

                if (string.IsNullOrEmpty(instance.Settings.CurrentVersion))
                    instance.Settings.CurrentVersion = MainView.LatestBetterLegacyVersion;
                var item = new ListBoxItem
                {
                    DataContext = instance,
                    Content = Path.GetFileName(directories[i])
                };
                item.Classes.Add("lbs1");
                MainView.Instance.InstancesListBox.Items.Add(item);
            }

            MainView.Instance.SetLaunchButtonsActive(MainView.Instance.InstancesListBox.ItemCount > 0);

            if (MainView.Instance.InstancesListBox.ItemCount > 0)
                MainView.Instance.InstancesListBox.SelectedItem = MainView.Instance.InstancesListBox.Items[0];
        }

        public static void OpenInstanceFolder()
        {
            var projectArrhythmia = Current;
            if (projectArrhythmia == null)
                return;

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

        public static void ZipInstance()
        {
            var projectArrhythmia = Current;
            if (zipping || projectArrhythmia == null)
                return;

            string path = projectArrhythmia.Path.Replace("/", "\\");

            Task.Run(() =>
            {
                if (zipping)
                    return;

                zipping = true;

                var zipPath = $"{path}-{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}.zip";
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                ZipFile.CreateFromDirectory(path, zipPath);

                zipping = false;
            });
        }

        public static void PasteBeatmapsFolder()
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

        public static async void DownloadDemoBeatmaps()
        {
            var projectArrhythmia = Current;
            if (projectArrhythmia == null || projectArrhythmia.Settings == null)
                return;

            try
            {
                var url = $"https://github.com/RTMecha/BetterLegacy/releases/download/{projectArrhythmia.Settings.CurrentVersion}/Beatmaps.zip";

                MainView.Instance.SetLaunchButtonsActive(false);

                using var http = new HttpClient();

                if (LauncherHelper.URLExists(url))
                {
                    var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                    var headResponse = await http.SendAsync(headRequest);
                    var contentLength = headResponse.Content.Headers.ContentLength;
                    MainView.Instance.ProgressBarUpdater($"{projectArrhythmia.Path}/Beatmaps.zip", "Downloading Beatmaps.zip", contentLength.Value);

                    // Getting file data
                    var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    using var fileStream = File.Create($"{projectArrhythmia.Path}/Beatmaps.zip");
                    using var stream = await response.Content.ReadAsStreamAsync();
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                    fileStream.Close();

                    LauncherHelper.UnZip($"{projectArrhythmia.Path}/Beatmaps.zip", $"{projectArrhythmia.Path}");

                    File.Delete($"{projectArrhythmia.Path}/Beatmaps.zip");
                }

            }
            catch { }

            MainView.Instance.SetLaunchButtonsActive(true);
        }

        // todo: look into how to recycle bin
        public static void DeleteInstance()
        {
            var projectArrhythmia = Current;
            if (projectArrhythmia == null || projectArrhythmia.Settings == null)
                return;

            var warningWindow = new WarningWindow()
            {
                DataContext = new WarningViewModel()
                {
                    WarningMessage = "Are you sure you want to delete this\ninstance? THIS IS NOT UNDO-ABLE,\nYOU WILL LOSE EVERYTHING.",
                    Confirm = () =>
                    {
                        Directory.Delete(projectArrhythmia.Path, true);
                        WarningWindow.Instance?.Close();
                        LoadInstances();
                    },
                },
            };
            warningWindow.Show(MainWindow.Instance);
        }
    }
}
