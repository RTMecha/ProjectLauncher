using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

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
                    Relaunch();
                    return;
                }

                using var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync("https://github.com/RTMecha/ProjectLauncher/releases/latest/download/ProjectLauncher.zip");
                await File.WriteAllBytesAsync(MainDirectory + "ProjectLauncher.zip", bytes);

                UnZip(MainDirectory + "ProjectLauncher.zip", Directory.GetCurrentDirectory().Replace("\\", "/"));

                Relaunch();
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

    }
}
