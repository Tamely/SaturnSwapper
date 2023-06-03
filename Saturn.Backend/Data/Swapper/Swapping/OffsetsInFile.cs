using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Saturn.Backend.Data.Fortnite;

namespace Saturn.Backend.Data.Swapper.Swapping
{
    public class OffsetsInFile
    {
        public static Dictionary<string, long> BytesWritten = new Dictionary<string, long>();
        public static (string, long) Allocate(string filePath, long length)
        {
            Utilities.CorrectFiles();
            
            List<string> potentialParentFiles = new();

            string filePattern = Path.GetFileNameWithoutExtension(filePath).Split('-')[0] + "-WindowsClient";
            Logger.Log($"Attempting to get potential files to append to with search pattern: {filePattern}");

            foreach (var file in Directory.EnumerateFiles(DataCollection.GetGamePath()))
            {
                if (file.Contains(filePattern) && file.ToLower().Contains(".ucas") && !file.ToLower().Contains(".o."))
                {
                    Logger.Log($"Added {file} to potential files");
                    potentialParentFiles.Add(file);
                }
            }

            Logger.Log("Found: " + potentialParentFiles.Count + " that match the pattern");

            string parentFile = string.Empty;
            foreach (var file in potentialParentFiles.Where(file => !string.IsNullOrWhiteSpace(file) && file != string.Empty).Where(_ => parentFile == string.Empty))
            {
                Logger.Log("Set parent file to " + file);
                parentFile = file;
            }

            if (parentFile == string.Empty)
                throw new Exception("Parent file is empty! Do backups exist?");
            
            foreach (var file in potentialParentFiles.Where(file => new FileInfo(file).Length < new FileInfo(parentFile).Length))
            {
                Logger.Log($"Setting the parent file to {file}");
                parentFile = file;
            }

            long offset = new FileInfo(parentFile).Length;
            
            if (BytesWritten.ContainsKey(parentFile))
            {
                offset += BytesWritten[parentFile];
                BytesWritten[parentFile] += length;
            }
            else
            {
                BytesWritten.Add(parentFile, length);
            }

            return (parentFile, offset);
        }
    }
}
