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
    // The uninitialized path for the log file, in case the uninstaller breaks.
    private static string _logPath;

    public static void Main()
    {
        // C# doesn't like async main methods (At least not console applications) so we just do this
        Uninstall().GetAwaiter().GetResult();
    }

    // Uninstalls everything related to Saturn, including plugins, and all data.
    private static async Task Uninstall()
    {
        // Try just in case we run into an error
        try
        {
            // Gets the current directory of the running app
            var currentAssemblyPath = AppDomain.CurrentDomain.BaseDirectory;

            // Initialize log path
            _logPath = $"{currentAssemblyPath}\\uninst.log";

            // Get path for Saturn's data folder
            var dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Saturn";

            // Initialize a new DirectoryTree with 'dataPath'
            var dataTree = await DirectoryTree.CreateDirectoryTreeAsync(dataPath);

            // Delete the data folder
            await DeleteDirectoryTree(dataTree);

            // Initialize a new DirectoryTree with the assembly's current directory
            var currentTree = await DirectoryTree.CreateDirectoryTreeAsync(currentAssemblyPath);

            // Gets all the plugins in the current path
            var plugins = GetPlugins(currentTree);

            // Delete plugins
            foreach (var plugin in plugins)
            {
                plugin.Delete();
            }

            // Create an array of the partial paths of the folders associated with Saturn
            var targetFolder = new string[]
            {
                "wwwroot",
                "Saturn.exe.WebView2"
            };

            // Create an array of the partial paths of the files associated Saturn
            var targetFiles = new string[]
            {
                "oo2core_9_win64.dll",
                "Saturn"
            };

            // Delete the target folders
            foreach (var item in targetFolder)
            {
                var tree = await DirectoryTree.CreateDirectoryTreeAsync($"{currentAssemblyPath}\\{item}");
                await DeleteDirectoryTree(tree);
            }

            // This command force deletes any application
            var command = "/C choice /C Y /N /D Y /T 3 & Del ";

            // Delete all target files
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
