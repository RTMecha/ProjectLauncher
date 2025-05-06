using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using ProjectLauncher.Views;

namespace ProjectLauncher
{
    public static class LauncherHelper
    {
        public static string NewsURL => "https://raw.githubusercontent.com/RTMecha/ProjectLauncher/refs/heads/master/PANews/";

        public static void UpdateLauncher()
        {
            Process.Start(new ProcessStartInfo(MainView.MainDirectory + "ProjectLauncher.Updater.exe"));

            MainWindow.Instance.Close();
        }

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

        public static bool URLExists(string url)
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

        public static bool URLExists(this HttpClient http, string url)
        {
            try
            {
                using var response = http.GetAsync(url);
                return response.Result.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

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
    }
}
