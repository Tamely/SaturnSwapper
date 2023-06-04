using System.Diagnostics;
using Newtonsoft.Json;

namespace Saturn.Launcher;

public class Program
{
    private static Dictionary<string, List<(long, byte[])>> currentSwapBackups = new();
    public static void Main(string[] args)
    {
        foreach (var process in Process.GetProcesses())
            if (process.ProcessName == "Saturn")
                process.Kill();
        
        Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine("████████████████████████████████████████");
        Console.WriteLine("█─▄▄▄▄██▀▄─██─▄─▄─█▄─██─▄█▄─▄▄▀█▄─▀█▄─▄█");
        Console.WriteLine("█▄▄▄▄─██─▀─████─████─██─███─▄─▄██─█▄▀─██");
        Console.WriteLine("▀▄▄▄▄▄▀▄▄▀▄▄▀▀▄▄▄▀▀▀▄▄▄▄▀▀▄▄▀▄▄▀▄▄▄▀▀▄▄▀");
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(">> ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Welcome to Saturn Launcher");
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(">> ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("DO NOT CLOSE THIS WINDOW! IT WILL AUTOMATICALLY CLOSE WHEN YOU CLOSE FORTNITE!");
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(">> ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Please wait while we apply the game patches...");
        
        foreach (var directory in Directory.EnumerateDirectories(GetGamePath()))
            Directory.Delete(directory, true);
        
        foreach (var file in Directory.EnumerateFiles(GetGamePath()))
            if (!file.Contains("Windows") && !file.Contains("global"))
                File.Delete(file);

        string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Saturn/";
        Directory.CreateDirectory(basePath);
        string swapPath = basePath + "SwapData/";
        Directory.CreateDirectory(swapPath);

        Dictionary<string, long> FileSizes = Directory.EnumerateFiles(GetGamePath()).ToDictionary(file => file, file => new FileInfo(file).Length);
        foreach (var file in Directory.EnumerateFiles(GetGamePath()))
        {
            if (file.Contains(".pak") || file.Contains(".utoc"))
            {
                File.Copy(file, swapPath + Path.GetFileName(file), true);
            }

            FileSizes[Path.GetFileName(file)] = new FileInfo(file).Length;
        }

        foreach (var file in new DirectoryInfo(swapPath).GetFileSystemInfos().OrderBy(x => x.CreationTime))
        {
            if (file.Extension != ".json") continue;
            ItemModel item = JsonConvert.DeserializeObject<ItemModel>(File.ReadAllText(file.FullName));
            
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(">> ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Converting " + item!.Name);
            
            foreach (var swap in item.Swaps)
            {
                if (swap == null) continue;
                
                if (swap.Offset != 0)
                {
                    using (var stream = new FileStream(swap.File, FileMode.Open, FileAccess.ReadWrite))
                    {
                        stream.Seek(swap.Offset, SeekOrigin.Begin);
                        byte[] readData = new byte[swap.Data.Length];
                        stream.Read(readData, 0, readData.Length);

                        if (currentSwapBackups.ContainsKey(Path.GetFileName(swap.File.ToLower())))
                        {
                            currentSwapBackups[Path.GetFileName(swap.File.ToLower())].Add((swap.Offset, readData));
                        }
                        else
                        {
                            currentSwapBackups.Add(Path.GetFileName(swap.File.ToLower()), new List<(long, byte[])>()
                            {
                                (swap.Offset, readData)
                            });
                        }

                        stream.Seek(swap.Offset, SeekOrigin.Begin);
                        stream.Write(swap.Data);
                    }
                }
                else
                {
                    using (var stream = new FileStream(swap.File, FileMode.Append, FileAccess.Write))
                    {
                        stream.Write(swap.Data);
                    }
                }
            }
        }
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(">> ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Launching Fortnite!");

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c start com.epicgames.launcher://apps/Fortnite?action=launch&silent=true",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
        });
        
        while (Process.GetProcessesByName("FortniteClient-Win64-Shipping").Length == 0)
            Thread.Sleep(500);
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(">> ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Waiting for Fortnite to close...");

        Thread.Sleep(5000);

        while (Process.GetProcessesByName("FortniteClient-Win64-Shipping").Length != 0)
            Thread.Sleep(2000);
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(">> ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Closing Epic Games Launcher...");
        
        foreach (var process in Process.GetProcesses())
            if (process.ProcessName == "EpicGamesLauncher")
                process.Kill();
        
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(">> ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Reverting game patches...");

        foreach (var file in Directory.EnumerateFiles(GetGamePath()))
        {
            if (FileSizes[file] != new FileInfo(file).Length)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.SetLength(FileSizes[file]);
                }
            }

            if (currentSwapBackups.ContainsKey(Path.GetFileName(file.ToLower())))
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite))
                {
                    foreach (var swap in currentSwapBackups[Path.GetFileName(file.ToLower())])
                    {
                        stream.Position = swap.Item1;
                        stream.Write(swap.Item2);
                    }
                }
            }
        }
        
        foreach (var file in Directory.EnumerateFiles(swapPath))
        {
            if (file.Contains(".pak") || file.Contains(".utoc"))
            {
                File.Move(file, GetGamePath() + Path.GetFileName(file), true);
            }
            
            File.Delete(file);
        }

        int i = 100;
        while (File.Exists(GetGamePath() + $"\\pakchunk{i++}-WindowsClient.ucas"))
        {
            File.Delete(GetGamePath() + $"\\pakchunk{i - 1}-WindowsClient.ucas");
            File.Delete(GetGamePath() + $"\\pakchunk{i - 1}-WindowsClient.utoc");
            File.Delete(GetGamePath() + $"\\pakchunk{i - 1}-WindowsClient.pak");
            File.Delete(GetGamePath() + $"\\pakchunk{i - 1}-WindowsClient.sig");
        }

        Process.GetCurrentProcess().Kill();
    }
    
    private static string GetGamePath()
    {
        if (File.Exists("C:\\ProgramData\\Epic\\UnrealEngineLauncher\\LauncherInstalled.dat"))
        {
            FortniteInstallationModel? model = JsonConvert.DeserializeObject<FortniteInstallationModel>(File.ReadAllText("C:\\ProgramData\\Epic\\UnrealEngineLauncher\\LauncherInstalled.dat"));
            if (model == null)
                return string.Empty;
            if (model.InstallationList.Any(x => x.AppName == "Fortnite"))
                return model.InstallationList.First(x => x.AppName == "Fortnite").InstallLocation + "\\FortniteGame\\Content\\Paks\\";
        }

        return string.Empty;
    }
}