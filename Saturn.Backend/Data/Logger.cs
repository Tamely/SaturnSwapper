using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data
{
    public enum LogLevel
    {
        [Description("TRC")] Trace,
        [Description("INF")] Info,
        [Description("WRN")] Warning,
        [Description("ERR")] Error
    }
    
    public static class Logger
    {
        private static readonly TextWriter _writer;
        private static List<string> _writtenLines = new();

        static Logger()
        {
            if (File.Exists(Constants.LogFile))
            {
                var directoryInfo = new DirectoryInfo(Constants.LogPath).GetFiles()
                    .OrderByDescending(x => x.LastWriteTimeUtc).ToList();

                var latestLog = directoryInfo.FirstOrDefault();
                var oldestLog = directoryInfo.LastOrDefault();

                if (directoryInfo.Count() >= 10)
                {
                    oldestLog.Delete();
                }

                latestLog.MoveTo(latestLog.FullName.Replace(".log",
                    $"-backup-{latestLog.LastWriteTimeUtc:yyyy.MM.dd-HH.mm.ss}.log"));
            }

            _writer = File.CreateText(Constants.LogFile);
            _writer.WriteLine("# Saturn Log");
            _writer.WriteLine("# Started on {0}", DateTime.Now);
            _writer.WriteLine("# Saturn Version {0}", Constants.USER_VERSION);
            _writer.WriteLine("----------------------------------------");
            _writer.WriteLine();
            _writer.Flush();
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            var method = new StackTrace().GetFrame(1).GetMethod();
            var typeName = method.ReflectedType.Name;
            var methodName = method.Name;

            if (methodName == ".ctor")
            {
                methodName = "Constructor";
            }

            if (typeName.Contains("Service"))
            {
                typeName.Replace("Service", "");
            }

            if (typeName.Contains('<'))
            {
                typeName = typeName.Split('<')[1].Split('>')[0];
            }

            if (_writtenLines.Count > 1 && _writtenLines[^1].Contains($"[Log{typeName}::{methodName}] [{level.GetDescription()}] {message}"))
            {
                _writer.Flush();
                return;
            }

            switch (level)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
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

            _writtenLines.Add($"[{DateTime.Now:HH:mm:ss}] [Log{typeName}::{methodName}] [{level.GetDescription()}] {message}");
            _writer.WriteLine($"[{DateTime.Now:HH:mm:ss}] [Log{typeName}::{methodName}] [{level.GetDescription()}] {message}");
            _writer.Flush();
        }
        
        public static List<string> GetWrittenLines()
        {
            return _writtenLines;
        }

        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null)
                return null;

            var field = type.GetField(name);
            if (field == null)
                return null;

            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute atr)
                return atr.Description;

            return null;
        }
    }
}
