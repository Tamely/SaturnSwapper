﻿using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Backend.Data.Utils
{
    public class FileUtil
    {
        private static MemoryStream? _stream;

        public static async Task OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // different way because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else
                {
                    throw; // you never know what bugs might happen
                }
            }
        }

        public static async Task<List<byte[]>> GetColorsFromSeries(string seriesPath, DefaultFileProvider _provider)
        {
            List<byte[]> colors = new();
            Dictionary<string, List<float>> colorValues = new();
            await Task.Run(() =>
            {
                if (_provider.TryLoadObject(seriesPath, out var series))
                {
                    if (series.TryGetValue(out FStructFallback Colors, "Colors"))
                    {
                        foreach (var colors in Colors.Properties)
                        {
                            var floatValues = new List<float>
                            {
                                Colors.Get<FLinearColor>(colors.Name.Text).R,
                                Colors.Get<FLinearColor>(colors.Name.Text).G,
                                Colors.Get<FLinearColor>(colors.Name.Text).B,
                                Colors.Get<FLinearColor>(colors.Name.Text).A
                            };
                            colorValues.Add(colors.Name.Text, floatValues);
                        }
                    }
                    else
                    {
                        Logger.Log("No Colors found in " + seriesPath);
                    }
                }
                else
                {
                    Logger.Log("Could not find series " + seriesPath);
                }
            });

            foreach (var color in colorValues)
            {
                Logger.Log(color.Key);
                // Use 'Array.Empty<byte>()' to avoid unnecessary zero-length array allocations 
                var colorBytes = Array.Empty<byte>();
                foreach (var colorValue in color.Value)
                {
                    // Inline method 'FloatToBytes' as it is only used here
                    colorBytes = Combine(colorBytes, BitConverter.GetBytes(colorValue));
                    Logger.Log(" - " + colorValue);

                }
                colors.Add(colorBytes);
            }

            while (colors.Count < 5)
                colors.Add(new byte[] { 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255 });

            return colors;

        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            var bytes = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        // Export asset to memory then return all strings in asset
        public static async Task<List<string>> GetStringsFromAsset(string assetPath, DefaultFileProvider _provider)
        {
            List<string> output = new();
            Logger.Log(assetPath);
            if (!_provider.TrySavePackage(assetPath, out var assets)) return output;
            foreach (var (_, value) in assets)
            {
                var lastOffset = AnyLength.IndexOfSequence(value, Encoding.ASCII.GetBytes("/Game/")) - 1;
                var startOffset = lastOffset - value[44] * 2 + 2;

                var pathOffset = lastOffset + 1;
                for (var i = startOffset; i <= lastOffset; i += 2)
                {
                    var path = ReadBytes(value, value[i], pathOffset);
                    pathOffset += value[i];


                    if ((Encoding.ASCII.GetString(path).ToLower().Contains("elastic") &&
                         Encoding.ASCII.GetString(path).ToLower().Contains("parts") &&
                         !Encoding.ASCII.GetString(path).ToLower().Contains("anim")) ||
                        (Encoding.ASCII.GetString(path).ToLower().Contains("tv_") &&
                         Encoding.ASCII.GetString(path).ToLower().Contains("material")))
                        path = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(path).Split('.')[0] + '.' +
                                                       SubstringFromLast(Encoding.ASCII.GetString(path).Split('.')[0],
                                                           '/'));
                    output.Add(Encoding.ASCII.GetString(path));
                    Logger.Log(output.Last());
                }
            }

            _stream = null;
            return output;
        }


        // Modified from https://gist.github.com/kyeondiscord/7d1a9088dbd95312f4cc4ac804606a66
        private static byte[] ReadBytes(byte[] Array, long numberOfBytes, long offset)
        {
            _stream ??= new MemoryStream(Array);

            List<byte> array = new();
            _stream.Position = offset;
            for (var i = 0; i < numberOfBytes; i++)
                array.Add((byte)_stream.ReadByte());

            return array.ToArray();
        }

        // Get second index of character in string
        public static int GetSecondIndex(string input, char character)
        {
            var index = input.IndexOf(character);
            return index == -1 ? -1 : input.IndexOf(character, index + 1);
        }

        // Get last index of character in string
        public static int LastIndexOf(string str, char ch)
        {
            var index = str.LastIndexOf(ch);
            if (index == -1) return -1;
            return index + 1;
        }

        // Get substring from last character in string to end
        public static string SubstringFromLast(string str, char ch)
        {
            var index = LastIndexOf(str, ch);
            return index == -1 ? str : str[index..];
        }
    }
}