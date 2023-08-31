using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Radon.Common;

public static class Logger
{
    private static readonly TextWriter Writer;
    private static readonly List<string> WrittenLines;

    static Logger()
    {
        if (File.Exists(Constants.LogFile))
        {
            var directoryInfo = new DirectoryInfo(Constants.LogPath).GetFiles()
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

        WrittenLines = new List<string>();
        Writer = File.CreateText(Constants.LogFile);
        Writer.WriteLine("# Runtime Log");
        Writer.WriteLine("# Started on {0:yy-mm-dd-hh-mm-ss}", DateTime.Now);
        Writer.WriteLine("# Runtime Version {0}", Constants.RadonVersion);
        Writer.WriteLine(new string('-', 40));
        Writer.WriteLine();
        Writer.Flush();
    }
    
    public static void Log(string message, LogLevel level)
    {
        var method = new StackTrace().GetFrame(1)?.GetMethod();
        if (method == null)
        {
            return;
        }
        
        var typeName = method.ReflectedType.Name;
        var methodName = method.Name;

        if (methodName == ".ctor")
        {
            methodName = "Constructor";
        }

        if (typeName.Contains("Service"))
        {
            typeName = typeName.Replace("Service", "");
        }

        if (typeName.Contains('<'))
        {
            typeName = typeName.Split('<')[1].Split('>')[0];
        }

        if (WrittenLines.Count > 1 && WrittenLines[^1].Contains($"[Log{typeName}::{methodName}] [{level.GetDescription()}] {message}"))
        {
            Writer.Flush();
            return;
        }

        switch (level)
        {
            case LogLevel.Trace:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case LogLevel.Info:
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
        }

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Log{typeName}::{methodName}] [{level.GetDescription()}] {message}");
        Console.ForegroundColor = ConsoleColor.White;

        WrittenLines.Add($"[{DateTime.Now:HH:mm:ss}] [Log{typeName}::{methodName}] [{level.GetDescription()}] {message}");
        Writer.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Log{typeName}::{methodName}] [{level.GetDescription()}] {message}");
        Writer.Flush();
    }
    
    private static string? GetDescription(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        if (name == null)
        {
            return null;
        }

        var field = type.GetField(name);
        if (field == null)
        {
            return null;
        }

        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute atr)
        {
            return atr.Description;
        }

        return null;
    }
}