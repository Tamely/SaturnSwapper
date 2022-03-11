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
            var dataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Saturn\\";

            var currentDirectctoryTree = await DirectoryTree.CreateDirectoryTreeAsync(currentAssemblyPath);
            var plugins = currentDirectctoryTree.GetFiles(x => x.Path.Contains(".json") &&
                                                 x.FileContents.Contains("AssetPath")); // Gets all plugins. All plugin files should contain a part that says "Appdata"
            foreach (var plugin in plugins)
            {
                plugin.Delete();
            }

            var assemblyFileNames = new string[] { "uninst.exe", "uninst.dll" }; // Essentially the partial paths of the assembly.

            var targetFiles = new string[] { "Saturn.exe", "oo2core_9_win64.dll", "uninst.r", "uninst.p", "uninst.deps" };
            var targetDirectories = new string[] { "wwwroot", "Saturn.exe.WebView2" };

            await DeleteDirectoryTree(currentDirectctoryTree, targetFiles, targetDirectories);

            var adDirectoryTree = await DirectoryTree.CreateDirectoryTreeAsync(dataPath);
            var adPlugins = adDirectoryTree.GetFiles(x => x.Path.Contains(".json") &&
                                                     x.FileContents.Contains("AssetPath")); // Check if this directory contains any plugins

            foreach (var plugin in adPlugins)
            {
                plugin.Delete();
            }

            await DeleteDirectoryTree(adDirectoryTree, null, null);

            var assemblyFiles = new List<SFile>();
            foreach (var name in assemblyFileNames)
            {
                assemblyFiles.Add(currentDirectctoryTree.GetFile(name)); // Get the current assembly, and it's executing parent
            }

            // This command force deletes this assembly.
            var command = "/C choice /C Y /N /D Y /T 3 & Del ";
            foreach (var file in assemblyFiles)
            {
                Process.Start("cmd.exe", command + file.Path);
            }
        }
        catch (Exception)
        {
            return;
        }
    }

    private static async Task DeleteDirectoryTree(DirectoryTree tree, string[] partialFilePaths, string[] partialDirectoryPaths)
    {
        try
        {
            if (!Directory.Exists(tree.BaseDirectory.Path))
            {
                return;
            }

            var files = tree.GetFiles(partialFilePaths, 1).ToList();
            if (partialDirectoryPaths is not null)
            {
                foreach (var directory in partialDirectoryPaths)
                {
                    var path = tree.GetNestedDirectory(directory).Path;
                    if (!Directory.Exists(path))
                    {
                        continue;
                    }
                    else
                    {
                        var dTree = await DirectoryTree.CreateDirectoryTreeAsync(tree.GetNestedDirectory(directory).Path);
                        files.AddRange(dTree.GetFiles());
                    }
                }
            }

            foreach (var file in files)
            {
                file.Delete();
            }

            if (partialDirectoryPaths is not null)
            {
                foreach (var directory in partialDirectoryPaths)
                {
                    var path = tree.GetNestedDirectory(directory).Path;
                    if (!Directory.Exists(path))
                    {
                        continue;
                    }
                    else
                    {
                        tree.GetNestedDirectory(directory).Delete();
                    }
                }
            }
            else
            {
                foreach (var child in tree.BaseDirectory.Children)
                {
                    child.Delete();
                }
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
                fs = File.OpenRead(_logPath);
            }

            try
            {
                await fs.WriteAsync(Encoding.ASCII.GetBytes(ex.ToString()));
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
