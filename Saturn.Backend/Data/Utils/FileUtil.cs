using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CUE4Parse.FileProvider;
using Saturn.Backend.Data.Enums;

namespace Saturn.Backend.Data.Utils
{
    public class FileUtil
    {
        private static MemoryStream? _stream;

        // Return byte[] in offset and range from byte[]
        public static byte[] GetBytes(byte[] bytes, long offset, int length)
        {
            var result = new byte[length];
            Array.Copy(bytes, offset, result, 0, length);
            return result;
        }

        // Writes an integer to a file
        public static bool WriteIntToFile(string filePath, long offset, int value)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open);
                stream.Position = offset;
                stream.Write(HexToBytes(ReverseHex(IntToHex(value))), 0, ReverseHex(IntToHex(value)).Length / 2);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Writes hex string to file
        public static bool WriteHexToFile(string filePath, long offset, string hex)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open);
                stream.Position = offset;
                stream.Write(HexToBytes(hex), 0, hex.Length / 2);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Remove duplicates from list of strings
        public static void RemoveDuplicates(ref List<string> list)
        {
            var newList = new List<string>();
            foreach (var item in list)
                if (!newList.Contains(item))
                    newList.Add(item);
            list = newList;
        }

        // Hex to byte[]
        public static byte[] HexToBytes(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        // Converts integer to a hex string
        public static string IntToHex(int value)
        {
            return ReverseHex(BitConverter.ToString(BitConverter.GetBytes(value)).Replace("-", ""));
        }

        // Reverses a hex string for utoc processing
        private static string ReverseHex(string hex)
        {
            StringBuilder sb = new();
            sb.Append(hex[2]);
            sb.Append(hex[3]);
            sb.Append(hex[0]);
            sb.Append(hex[1]);
            return sb.ToString();
        }

        // Get shortest string in list of strings
        public static string GetShortest(IEnumerable<string> list)
        {
            return list.OrderBy(x => x.Length).First();
        }

        public static async Task<EFortRarity> GetRarityFromAsset(string assetPath, DefaultFileProvider _provider)
        {
            if (!_provider.TrySavePackage(assetPath, out var assets)) return EFortRarity.Common;

            foreach (var (_, value) in assets)
            {
                Vars.HexOffset = 0;

                if (!Engine.FindHex(0, value, "3E FF FF FF FF")) continue;
                if ((uint)value[Vars.HexOffset + 5] > 9)
                    return EFortRarity.Uncommon;
                return (EFortRarity)value[Vars.HexOffset + 5];
            }

            return EFortRarity.Common;

        }
        
        public static async Task<ECustomHatType> GetHatTypeFromAsset(string assetPath, DefaultFileProvider _provider)
        {
            if (!_provider.TrySavePackage(assetPath, out var assets)) return ECustomHatType.ECustomHatType_None;

            foreach (var (_, value) in assets)
            {
                var fileOffset = value.Length;
                
                while (value[fileOffset - 1] == 0 || value[fileOffset - 2] == 0)
                    fileOffset--;
                
                if (value[fileOffset - 6] == 0 && value[fileOffset - 7] == 0)
                    return (ECustomHatType)value[fileOffset - 2];
                return ECustomHatType.ECustomHatType_None;
            }

            return ECustomHatType.ECustomHatType_None;

        }
        
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
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
                }
                else
                {
                    throw; // you never know what bugs might happen
                }
            }
        }

        // Export asset to memory then return all strings in asset
        public static async Task<List<string>> GetStringsFromAsset(string assetPath, DefaultFileProvider _provider)
        {
            List<string> output = new();
            Logger.Log(assetPath);
            if (!_provider.TrySavePackage(assetPath, out var assets)) return output;
            foreach (var (_, value) in assets)
            {
                int lastOffset = AnyLength.IndexOfSequence(value, Encoding.ASCII.GetBytes("/Game/")) - 1;
                int startOffset = lastOffset - value[44] * 2 + 2;

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

        // Encode a string to base64
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        // Decode a string from base64
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
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
        
        // Get substring from second character in string to end
        public static string SubstringFromSecond(string str, char ch)
        {
            var index = GetSecondIndex(str, ch);
            return index == -1 ? str : str[index..];
        }

    }
}