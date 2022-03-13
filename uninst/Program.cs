using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uninstaller;

public static class Program
{
    private static string _logPath;

    public static void Main()
    {
        Uninstall().GetAwaiter().GetResult();
    }

    private static async Task Uninstall()
    {
        try
        {
            var currentAssemblyPath = AppDomain.CurrentDomain.BaseDirectory;
            _logPath = $"{currentAssemblyPath}\\uninst.log";
            var dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Saturn";

            var dataTree = await DirectoryTree.CreateDirectoryTreeAsync(dataPath);
            await DeleteDirectoryTree(dataTree);

            var currentTree = await DirectoryTree.CreateDirectoryTreeAsync(currentAssemblyPath);
            var plugins = GetPlugins(currentTree);
            foreach (var plugin in plugins)
            {
                plugin.Delete();
            }

            var targetFolder = new string[]
            {
                "wwwroot",
                "Saturn.exe.WebView2"
            };

            var targetFiles = new string[]
            {
                "oo2core_9_win64.dll",
                "Saturn"
            };

            foreach (var item in targetFolder)
            {
                var tree = await DirectoryTree.CreateDirectoryTreeAsync($"{currentAssemblyPath}\\{item}");
                await DeleteDirectoryTree(tree);
            }

            // This command force deletes this assembly.
            var command = "/C choice /C Y /N /D Y /T 3 & Del ";
            foreach (var file in targetFiles)
            {
                var path = currentTree.GetFile(file).Path;
                Process.Start("cmd.exe", $"{command}{path}");
            }
        }
        catch (Exception)
        {
            return;
        }
    }

    private static IEnumerable<SFile> GetPlugins(DirectoryTree directoryTree)
    {
        return directoryTree.GetFiles(x => (x.Path.ToLower().Contains(".json") ||
                                            x.Path.ToLower().Contains(".saturn")) &&
                                            x.FileContents.Contains("AssetPath"));
    }

    private static async Task DeleteDirectoryTree(DirectoryTree tree)
    {
        try
        {
            var directories = tree.GetNestedDirectories().Reverse();
            foreach (var d in directories)
            {
                foreach (var file in d.Files)
                {
                    file.Delete();
                }

                d.Delete();
            }
        }
        catch (Exception ex)
        {
            FileStream fs;
            if (!File.Exists(_logPath))
            {
                fs = File.Create(_logPath);
            }
            else
            {
                fs = File.Open(_logPath, FileMode.Open);
            }

            try
            {
                await fs.WriteAsync(Encoding.UTF8.GetBytes(ex.ToString()));
            }
            finally
            {
                if (fs != null)
                {
                    ((IDisposable)fs).Dispose();
                }
            }
        }
    }
}
