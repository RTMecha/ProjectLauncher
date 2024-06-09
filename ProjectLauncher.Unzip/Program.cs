using System.IO.Compression;
using System.IO;
using System.Diagnostics;

class Program
{
    public static string MainDirectory => Directory.GetCurrentDirectory().Replace("\\", "/") + "/";

    static void Main()
    {
        Console.WriteLine("UnZip installed file...");
        UnZip(MainDirectory + "ProjectLauncher.zip", Directory.GetCurrentDirectory().Replace("\\", "/"));
        Relaunch();
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

    static void Relaunch()
    {
        var startInfo = new ProcessStartInfo();
        startInfo.FileName = MainDirectory + "ProjectLauncher.Desktop.exe";
        Process.Start(startInfo);

        Environment.Exit(0);
    }
}
