using System.Diagnostics;
using System.Reflection;
namespace Uninstaller;

public static class Program
{
    public static void Main(string[] args)
    {
        Uninstall();
    }

    internal static void Uninstall()
    {
        try
        {
            string[] currentAssembly =
            {
                Assembly.GetExecutingAssembly().Location,
                AppDomain.CurrentDomain.BaseDirectory + $"{typeof(Program).Assembly.GetName().Name}.exe"
            };

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Saturn\\";
            var cd_AllFiles = GetAllFilesInDirectory(baseDirectory, currentAssembly);
            foreach (var file in cd_AllFiles)
            {
                DeletePath(file, 1);
            }
            var cd_AllDirectories = GetAllSubDirectories(baseDirectory);
            cd_AllDirectories.Reverse();
            foreach (var directory in cd_AllDirectories)
            {
                DeletePath(directory, 0);
            }
            var data_AllFiles = GetAllFilesInDirectory(dataPath);
            foreach (var file in data_AllFiles)
            {
                DeletePath(file, 1);
            }
            var data_AllDirectories = GetAllSubDirectories(dataPath);
            data_AllDirectories.Reverse();
            foreach (var directory in data_AllDirectories)
            {
                DeletePath(directory, 0);
            }
            foreach (var file in currentAssembly)
            {
                Process.Start("cmd.exe", $"/C choice /C Y /N /D Y /T 3 & Del {file}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.ReadKey();
        }
    }

    internal static List<string> GetAllSubDirectories(string directory)
    {
        var subDirectories = new List<string>();
        var level = Directory.GetDirectories(directory).ToList();
        subDirectories.AddRange(level);
        foreach (var dir in level)
        {
            subDirectories.AddRange(GetAllSubDirectories(dir));
        }

        return subDirectories;
    }

    internal static List<string> GetAllFilesInDirectory(string directory, string[] exclude = null)
    {
        var files = new List<string>();
        files.AddRange(Directory.GetFiles(directory));
        if (exclude != null)
        {
            foreach (var file in exclude)
            {
                files.Remove(file);
            }
        }

        var subDirs = GetAllSubDirectories(directory);
        foreach (var dir in subDirs)
        {
            files.AddRange(GetAllFilesInDirectory(dir, null));
        }

        return files;
    }

    internal static void DeletePath(string path, int type)
    {
        if (type == 0 &&
            Directory.Exists(path))
        {
            Directory.Delete(path);
        }
        else if (type == 1 &&
                 File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
