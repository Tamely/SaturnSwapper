using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Saturn.Backend.Data.Enums;

namespace Saturn.Backend.Data.Utils
{
    public static class Logger
    {
        private static TextWriter _writer;

        public static void Start()
        {
            if (File.Exists(Config.LogFile))
            {
                var dirInfo = new DirectoryInfo(Config.LogPath).GetFiles().OrderByDescending(x => x.LastWriteTimeUtc)
                    .ToList();

                var latestLog = dirInfo.FirstOrDefault();
                var oldestLog = dirInfo.LastOrDefault();

                if (dirInfo.Count >= 15)
                    oldestLog.Delete();

                latestLog.MoveTo(latestLog.FullName.Replace(".log",
                    $"-backup-{latestLog.LastWriteTimeUtc:yyyy.MM.dd-HH.mm.ss}.log"));
            }

            _writer = File.CreateText(Config.LogFile);
            _writer.WriteLine("# Saturn Log");
            _writer.WriteLine($"# Started on {DateTime.Now}");
            _writer.WriteLine($"# Saturn version {Constants.UserVersion}");
            _writer.WriteLine();
            _writer.Flush();
        }

        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            var method = new StackTrace().GetFrame(1).GetMethod();
            var typeName = method.ReflectedType.Name;
            var methodName = method.Name;

            if (methodName == ".ctor")
                methodName = "Constructor";

            if (typeName.Contains("Service"))
                typeName = typeName.Replace("Service", "");

            if (typeName.Contains("<"))
                typeName = typeName.Split("<")[1].Split(">")[0];


            switch (level)
            {
                case LogLevel.Debug:
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
                case LogLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }

            Console.WriteLine($"[{DateTime.Now}] [Log{typeName}::{methodName} {level.GetDescription()}] {message}");
            Console.ForegroundColor = ConsoleColor.White;


            _writer.WriteLine($"[{DateTime.Now}] [Log{typeName}::{methodName} {level.GetDescription()}] {message}");
            _writer.Flush();
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

            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                return attr.Description;

            return null;
        }
    }
}