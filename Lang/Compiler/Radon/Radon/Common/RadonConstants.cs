using System;
using System.IO;

namespace Radon.Common;

public static class RadonConstants
{
    static RadonConstants()
    {
        RadonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Radon/");
        LogPath = Path.Combine(RadonPath, "Logs/");
        if (!Directory.Exists(RadonPath))
        {
            Directory.CreateDirectory(RadonPath);
        }
        
        if (!Directory.Exists(LogPath))
        {
            Directory.CreateDirectory(LogPath);
        }
        
        LogFile = Path.Combine(LogPath, "Runtime.log");
    }

    public static readonly string RadonPath;
    public static readonly string LogPath;
    public static readonly string LogFile;
    public const string RadonVersion = "1.1.0";
    public const double RadonVersionNumber = 1.10;
}