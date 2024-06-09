using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectLauncher.Updater.Views
{
    public partial class MainView : UserControl
    {
        public static string MainDirectory => Directory.GetCurrentDirectory().Replace("\\", "/") + "/";

        bool runUpdate = true; // for disabling on tests
        public MainView()
        {
            InitializeComponent();

            Update();
        }

        async void Update()
        {
            try
            {
                if (!runUpdate)
                {
                    MainWindow.Instance.Close();
                    return;
                }

                using var http = new HttpClient();

                // Getting file size
                var headRequest = new HttpRequestMessage(HttpMethod.Head, "https://github.com/RTMecha/ProjectLauncher/releases/latest/download/ProjectLauncher.zip");
                var headResponse = await http.SendAsync(headRequest);
                var contentLength = headResponse.Content.Headers.ContentLength;

                // Start ProgressBarUpdater
                ProgressBarUpdater(contentLength.Value);

                // Getting file data
                var response = await http.GetAsync("https://github.com/RTMecha/ProjectLauncher/releases/latest/download/ProjectLauncher.zip", HttpCompletionOption.ResponseHeadersRead);
                using var fileStream = File.Create(MainDirectory + "ProjectLauncher.zip");
                using var stream = await response.Content.ReadAsStreamAsync();
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                }
                fileStream.Close();

                var startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = MainDirectory + "ProjectLauncher.Unzip.exe";
                Process.Start(startInfo);

                MainWindow.Instance.Close();
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText(MainDirectory + "updater-exception.log", ex.ToString());

                    MainWindow.Instance.Close();
                }
                catch
                {

                }
            }
        }

        int CalculateProgress(long totalBytes, long bytesDownloaded)
        {
            return (int)((bytesDownloaded * 100) / totalBytes);
        }

        async void ProgressBarUpdater(long totalBytes)
        {
            while (true)
            {
                var fileInfo = new FileInfo(MainDirectory + "ProjectLauncher.zip");
                if (fileInfo.Exists)
                {
                    progressBar.Value = CalculateProgress(totalBytes, fileInfo.Length);
                    labelProgressBar.Content = FormatProgress(totalBytes, fileInfo.Length);
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


        // Probably don't need anymore
        /*
        void Relaunch()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = MainDirectory + "ProjectLauncher.Desktop.exe";
            Process.Start(startInfo);

            MainWindow.Instance.Close();
        }

        
        
        public static void UnZip(string path, string output)
        {
            using var archive = ZipFile.Open(path, ZipArchiveMode.Update);

            for (int i = 0; i < archive.Entries.Count; i++)
            {
                try
                {
                    var entry = archive.Entries[i];

                    var fullName = entry.FullName;

                    // Create folders if they don't exist already
                    var directory = Path.GetDirectoryName(output + "/" + fullName);
                    if (directory != null && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    if (fullName.Contains("."))
                        entry.ExtractToFile(output + "/" + fullName, true);
                }
                catch
                {

                }
            }
        }
        */

    }
}
