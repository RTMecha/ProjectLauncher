using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace ProjectLauncher.Functions
{
    public static class RTFile
    {
        public static string ApplicationDirectory => Directory.GetCurrentDirectory().Replace("\\", "/") + "/";

        public static bool FileExists(string _filePath) => !string.IsNullOrEmpty(_filePath) && File.Exists(_filePath);

        public static bool DirectoryExists(string _directoryPath) => !string.IsNullOrEmpty(_directoryPath) && Directory.Exists(_directoryPath);

        public static void WriteToFile(string path, string json)
        {
            StreamWriter streamWriter = new StreamWriter(path);
            streamWriter.Write(json);
            streamWriter.Flush();
            streamWriter.Close();
        }

        public static string ReadFromFile(string path)
        {
            if (!FileExists(path))
                return "";

            StreamReader streamReader = new StreamReader(path);
            string result = streamReader.ReadToEnd().ToString();
            streamReader.Close();
            return result;
        }

        public static byte[] ReadBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public static List<string> WordWrap(string _input, int maxCharacters)
        {
            List<string> list = _input.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> list2 = new List<string>();
            foreach (string text in list)
            {
                if (!text.Contains(' '))
                {
                    for (int i = 0; i < text.Length; i += maxCharacters)
                    {
                        list2.Add(text.Substring(i, Math.Min(maxCharacters, text.Length - i)));
                    }
                }
                else
                {
                    string[] array = text.Split(new char[] { ' ' });
                    string text2 = "";
                    foreach (string text3 in array)
                    {
                        if ((text2 + text3).Length > maxCharacters)
                        {
                            list2.Add(text2.Trim());
                            text2 = "";
                        }
                        text2 += string.Format("{0} ", text3);
                    }
                    if (text2.Length > 0)
                    {
                        list2.Add(text2.Trim());
                    }
                }
            }
            return list2;
        }

        public static void MoveFile(string file, string destination)
        {
            var fi = new FileInfo(file);
            if (fi.Exists)
            {
                fi.MoveTo(destination);
            }
        }

        /// <summary>
        /// Gets the version number of the specified assembly.
        /// </summary>
        /// <param name="path">Full path to the dll.</param>
        /// <returns>Parsed version number directly from the dll.</returns>
        public static string GetAssemblyVersion(string path)
        {
            var assembly = Assembly.LoadFrom(path);
            var ver = assembly.GetName().Version?.ToString() ?? "";

            return ver.Contains('.') ? ver.Substring(0, ver.LastIndexOf('.')) : ver;
        }

        public static class ZipUtil
        {
            public static void Zip(string path, string[] files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in files)
                        {
                            archive.CreateEntryFromFile(file, Path.GetFileName(file));
                        }
                    }

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                    }
                }
            }

            public static void UnZip(string path, string output, bool deleteZip = true)
            {
                try
                {
                    var archive = ZipFile.Open(path, ZipArchiveMode.Update);

                    for (int i = 0; i < archive.Entries.Count; i++)
                    {
                        var entry = archive.Entries[i];

                        var fullName = entry.FullName;

                        // Create folders if they don't exist already
                        var directory = Path.GetDirectoryName(output + "/" + fullName);
                        if (directory != null && !Directory.Exists(directory))
                            Directory.CreateDirectory(directory);

                        // We do not extract any folders as it would cause a DirectoryNotFound exception.
                        if (fullName.Contains("."))
                            entry.ExtractToFile(output + "/" + fullName, true);
                    }

                    archive.Dispose();
                    archive = null;

                    if (deleteZip && FileExists(path))
                        File.Delete(path);
                }
                catch
                {

                }
            }

            public static string GetZipString(string path, int file)
            {
                var archive = ZipFile.OpenRead(path);
                var stream = archive.Entries[file].Open();

                var streamReader = new StreamReader(stream);
                string result = streamReader.ReadToEnd().ToString();
                streamReader.Close();

                return result;
            }

            public static byte[] GetZipData(string path, int file)
            {
                var archive = ZipFile.OpenRead(path);
                var stream = archive.Entries[file].Open();

                var bytes = ReadBytes(stream);

                stream.Close();

                return bytes;
            }
        }
    }
}
