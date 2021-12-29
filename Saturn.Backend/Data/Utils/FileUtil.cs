using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
                if ((uint)value[Vars.HexOffset + 6] > 9)
                    return EFortRarity.Common;
                return (EFortRarity)value[Vars.HexOffset + 6];
            }

            return EFortRarity.Common;

        }

        // Export asset to memory then return all strings in asset
        public static async Task<List<string>> GetStringsFromAsset(string assetPath, DefaultFileProvider _provider)
        {
            List<string> output = new();
            Logger.Log(assetPath);
            if (!_provider.TrySavePackage(assetPath, out var assets)) return output;
            foreach (var (_, value) in assets)
            {
                Vars.StopOffset = 0;
                Vars.CurrentOffset = 0;
                _stream = null;
                Vars.HexOffset = 0;

                while (Engine.Find(Vars.CurrentOffset, value, "/Game/"))
                {
                    Engine.FindStop(value);
                    var item = Encoding.UTF8.GetString(ReadBytes(value, Vars.StopOffset - Vars.CurrentOffset, Vars.CurrentOffset));
                    if (item.Split('.')[0].ToLower().Contains("bp") || item.Split('.')[0].ToLower().Contains("blueprint"))
                        output.Add(item.Split('.')[0] + '.' + SubstringFromLast(item.Split('.')[0], '/') + "_C");
                    else if (item.StartsWith("/Game/MainPlayer"))
                        output.Add("/Game/Animation" + item);
                    else
                        output.Add(item.Split('.')[0] + '.' + SubstringFromLast(item.Split('.')[0], '/'));
                    Logger.Log(output.Last(), Enums.LogLevel.Fatal);
                }
            }

            output.Remove(output.Last());
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