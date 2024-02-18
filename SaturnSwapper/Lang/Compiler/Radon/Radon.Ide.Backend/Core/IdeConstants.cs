using System.IO;
using System.Linq;
using Radon.Common;

namespace Radon.Ide.Backend.Core;

public static class IdeConstants
{
    public static IdeState State { get; set; }
    public static string? CurrentFile { get; set; }
    public static readonly string IdePath = Path.Combine(RadonConstants.RadonPath, "Ide/");
    public static readonly string LogPath = Path.Combine(IdePath, "Logs/");
    public static readonly string ProjectsPath = Path.Combine(IdePath, "Projects/");
    public static readonly string LogFile = Path.Combine(LogPath, "Ide.log");
    public const double RadonIdeVersion = 1.0;
    
    static IdeConstants()
    {
        if (!Directory.Exists(IdePath))
        {
            Directory.CreateDirectory(IdePath);
        }
        
        if (!Directory.Exists(LogPath))
        {
            Directory.CreateDirectory(LogPath);
        }
        
        if (!Directory.Exists(ProjectsPath))
        {
            Directory.CreateDirectory(ProjectsPath);
        }
        
        if (File.Exists(LogFile))
        {
            var directoryInfo = new DirectoryInfo(LogPath).GetFiles()
                .OrderByDescending(x => x.LastWriteTimeUtc).ToList();

            var latestLog = directoryInfo.FirstOrDefault();
            var oldestLog = directoryInfo.LastOrDefault();

            if (directoryInfo.Count >= 10 &&
                oldestLog != null)
            {
                oldestLog.Delete();
            }

            latestLog?.MoveTo(latestLog.FullName.Replace(".log",
                $"-backup-{latestLog.LastWriteTimeUtc:yyyy.MM.dd-HH.mm.ss}.log"));
        }
    }
}