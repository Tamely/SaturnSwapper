using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CUE4Parse;
using CUE4Parse.FileProvider;

namespace Saturn.Backend.Core.Utils.Textures
{

    internal class AbstractImporter
    {
        public DefaultFileProvider Provider { get; }
        public BinaryWriter Stream { get; protected set; }
        public BinaryWriter TocStream { get; protected set; }

        internal enum FileT
        {
            Cas,
            Toc
        }

        public string Path => SaturnData.Parition == 0
                ? SaturnData.Path.Replace("WindowsClient", "SaturnClient")
                : SaturnData.Path.Replace("WindowsClient", "SaturnClient" + "_s" + SaturnData.Parition);

        public AbstractImporter(DefaultFileProvider provider)
        {
            Provider = provider;
        }

        public bool Open(string file)
        {
            try
            {
                if (file.EndsWith(".ucas")) Stream = new BinaryWriter(File.OpenWrite(file.Replace("WindowsClient", "SaturnClient")));
                else TocStream = new BinaryWriter(File.OpenWrite(file));
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        public void Close(FileT type)
        {
            switch (type)
            {
                case FileT.Cas:
                    Stream.Close();
                    break;
                case FileT.Toc:
                    TocStream.Close();
                    break;
            }
        }

        internal static List<byte[]> ChunkData(byte[] ubulkBytes)
        {
            List<byte[]> result = new();
            var remainingLength = ubulkBytes.Length;
            for (var i = 0; remainingLength > 0; i++)
            {
                var chunkData = new byte[remainingLength >= 65536 ? 65536 : remainingLength];
                Array.Copy(ubulkBytes, i * 65536, chunkData, 0, remainingLength >= 65536 ? 65536 : remainingLength);
                result.Add(chunkData);
                remainingLength -= remainingLength >= 65536 ? 65536 : remainingLength;
            }

            return result;
        }
    }
}