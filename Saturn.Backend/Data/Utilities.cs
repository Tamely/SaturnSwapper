using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUE4Parse;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Material.Parameters;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.GameplayTags;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Saturn.Backend.Data.Asset;
using Saturn.Backend.Data.Discord;
using Saturn.Backend.Data.Fortnite;
using Saturn.Backend.Data.SaturnAPI;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.SaturnConfig;
using Saturn.Backend.Data.Swapper.Core.Models;
using Saturn.Backend.Data.Swapper.Swapping;
using Saturn.Backend.Data.Swapper.Swapping.Models;
using Saturn.Backend.Data.Variables;
using SkiaSharp;

namespace Saturn.Backend.Data
{
    public static class Utilities
    {
        public static async Task SwapPreset(PresetModel preset, IJSRuntime _jsRuntime)
        {
            if (DiscordUtilities.Member != null && !File.Exists(Constants.ExternalPath + "user.json"))
                File.WriteAllText(Constants.ExternalPath + "user.json", JsonConvert.SerializeObject(DiscordUtilities.Member, Formatting.None));
            
            // There's a race condition from how HTML's OnClick handler works
            if (Constants.IsRemoving)
            {
                Constants.IsRemoving = false;
                return;
            }
            
            SaturnData.Clear();

            if (!Constants.isKeyValid)
            {
                await _jsRuntime.InvokeVoidAsync("saturn.modalManager.showModal", "key");
                return;
            }

            foreach (var item in preset.PresetSwaps)
            {
                List<SwapData> swapData = new();
        
                foreach (var characterPart in item.OptionModel.CharacterParts.Where(characterPart => item.ItemModel.CharacterParts.ContainsKey(characterPart.Key)))
                {
                    var oldPkg = await Constants.Provider.SavePackageAsync(characterPart.Value.Path.Split('.')[0] + ".uasset");
                    Deserializer oldDeserializer = new Deserializer(oldPkg.Values.First());
                    oldDeserializer.Deserialize();
                    
                    var data = SaturnData.ToNonStatic();
                    SaturnData.Clear();
                    
                    var newPkg = await Constants.Provider.SavePackageAsync(item.ItemModel.CharacterParts[characterPart.Key].Path.Split('.')[0] + ".uasset");
                    Deserializer newDeserializer = new Deserializer(newPkg.Values.First());
                    newDeserializer.Deserialize();

                    Serializer serializer = new Serializer(oldDeserializer.Swap(newDeserializer));

                    swapData.Add(new SwapData
                    {
                        SaturnData = data,
                        Data = serializer.Serialize()
                    });

                    SaturnData.Clear();
                }

                foreach (var characterPart in item.OptionModel.CharacterParts.Where(characterPart => !item.ItemModel.CharacterParts.ContainsKey(characterPart.Key)))
                {
                    var oldPkg = await Constants.Provider.SavePackageAsync(characterPart.Value.Path.Split('.')[0] + ".uasset");
                    Deserializer oldDeserializer = new Deserializer(oldPkg.Values.First());
                    oldDeserializer.Deserialize();
                    
                    var data = SaturnData.ToNonStatic();
                    SaturnData.Clear();
                    
                    var realPartType = characterPart.Value.Enums["CharacterPartType"];
                    
                    var newPkg = await Constants.Provider.SavePackageAsync(Constants.EmptyParts[realPartType].Path.Split('.')[0] + ".uasset");
                    Deserializer newDeserializer = new Deserializer(newPkg.Values.First());
                    newDeserializer.Deserialize();

                    Serializer serializer = new Serializer(oldDeserializer.Swap(newDeserializer));

                    swapData.Add(new SwapData
                    {
                        SaturnData = data,
                        Data = serializer.Serialize()
                    });

                    SaturnData.Clear();
                }

                await FileLogic.Convert(swapData);
            
                if (Constants.CanLobbySwap && Constants.ShouldLobbySwap && Constants.isPlus)
                {
                    await FileLogic.ConvertLobby(item.OptionModel.ID, item.ItemModel.ID);
                    SaturnData.Clear();
                }
                else if (Constants.isPlus && Constants.ShouldLobbySwap)
                {
                    Logger.Log("Unable to lobby swap at this time... pakchunk0 was unable to be mounted.", LogLevel.Error);
                }
            }
            
            await _jsRuntime.InvokeVoidAsync("saturn.modalManager.showModal", "finished");
        }
        
        public static async Task<bool> IsKeyValid(ISaturnAPIService saturnApiService)
        {
            if (string.IsNullOrWhiteSpace(Config.Get()._config.Key))
                return false;
            
            KeySearchModel keyData = await saturnApiService.ReturnEndpointAsync<KeySearchModel>($"/api/v1/Saturn/ReturnKeyExists?key={Config.Get()._config.Key}");
            if (!keyData.Found) return false;
            if (keyData.HWID != GetHWID() && keyData.HWID != "NotSet")
            {
                throw new Exception("Key is already in use on another PC. Please contact support if you believe this is an error.");
            }
                
            saturnApiService.ReturnEndpoint($"/api/v1/Saturn/SetHWID?key={Config.Get()._config.Key}&hwid={GetHWID()}");
            return true;

        }
        
        public static string GetHWID() => System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;
        
        public static string TitleCase(this string text)
        {
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(text);
        }
        
        public static T GetOrDefault<T>(this UObject obj, params string[] names)
        {
            foreach (var name in names)
            {
                if (obj.Properties.Any(x => x.Name.Text.Equals(name)))
                {
                    return obj.GetOrDefault<T>(name);
                }
            }

            return default;
        }
        
        public static bool CheckEncoding(this string value, Encoding encoding)
        {
            bool retCode;
            var charArray = value.ToCharArray();
            byte[] bytes = new byte[charArray.Length];
            for (int i = 0; i < charArray.Length; i++)
            {
                bytes[i] = (byte)charArray[i];
            }
            retCode = string.Equals(encoding.GetString(bytes, 0, bytes.Length), value, StringComparison.InvariantCulture);
            return retCode;
        }
        
        public static bool AddUnique<T>(this List<T> list, T item)
        {
            if (list.Contains(item)) return false;
            list.Add(item);
            return true;
        }

        public static bool AddUnique<T>(this ObservableCollection<T> list, T item)
        {
            if (list.Contains(item)) return false;
            list.Add(item);
            return true;
        }
        
        public static FLinearColor ToLinearColor(this FStaticComponentMaskParameter componentMask)
        {
            return new FLinearColor
            {
                R = componentMask.R ? 1 : 0,
                G = componentMask.G ? 1 : 0,
                B = componentMask.B ? 1 : 0,
                A = componentMask.A ? 1 : 0
            };
        }
        
        public static bool TryLoadObjectExports(this AbstractFileProvider provider, string path, out IEnumerable<UObject> exports)
        {
            exports = Enumerable.Empty<UObject>();
            try
            {
                exports = provider.LoadAllObjects(path);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            return true;
        }
        
        public static bool TryLoadEditorData<T>(this UObject asset, out T? editorData) where T : UObject
        {
            var path = asset.GetPathName().SubstringBeforeLast(".") + ".o.uasset";
            if (Constants.Provider.TryLoadObjectExports(path, out var exports))
            {
                editorData = exports.FirstOrDefault() as T;
                return editorData is not null;
            }

            editorData = default;
            return false;
        }

        public static FName? GetValueOrDefault(this FGameplayTagContainer tags, string category, FName def = default)
        {
            return tags.GameplayTags is not { Length: > 0 } ? def : tags.GameplayTags.FirstOrDefault(it => it.Text.StartsWith(category), def);
        }

        public static bool ContainsAny(this FGameplayTagContainer tags, params string[] check)
        {
            return check.Any(x => tags.ContainsAny(x));
        }

        public static bool ContainsAny(this FGameplayTagContainer tags, string check)
        {
            if (tags.GameplayTags is null) return false;
            return tags.GameplayTags.Any(x => x.Text.Contains(check));
        }

        public static string GetKeyFromValue(this Dictionary<string, string> dict, string value)
        {
            return dict.FirstOrDefault(x => x.Value == value).Key;
        }

        public static bool IsHexDigit(char c)
        {
            return c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
        }
        
        public static int IndexOfSequence(byte[] buffer, byte[] pattern)
        {
            int i = Array.IndexOf(buffer, pattern[0], 0);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual(pattern))
                    return i;
                i = Array.IndexOf(buffer, pattern[0], i + 1);
            }

            return -1;
        }
        
        public static byte[] WriteBytes(byte[] src, byte[] dst, int offset)
        {
            for (int i = 0; i < src.Length; i++)
            {
                dst[i + offset] = src[i];
            }

            return dst;
        }
        
        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", " ");
        }
        
        public static bool IsHex(string hex)
        {
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
                return false;

            foreach (char c in hex)
            {
                if (!IsHexDigit(c))
                    return false;
            }

            return true;
        }
        
        public static bool IsFileReady(string filename)
        {
            try
            {
                using FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static void CorrectFiles()
        {
            if (Directory.Exists(DataCollection.GetGamePath() + "\\Apple\\"))
            {
                foreach (var file in Directory.EnumerateFiles(DataCollection.GetGamePath() + "\\Apple\\"))
                {
                    File.Move(file, DataCollection.GetGamePath() + "\\" + Path.GetFileName(file));
                }
            }
            
            foreach (var file in Directory.EnumerateFiles(DataCollection.GetGamePath()))
            {
                if (!file.Contains("TamelyClient")) continue;
                if (File.Exists(file.Replace("TamelyClient", "WindowsClient")))
                {
                    File.Delete(file.Replace("TamelyClient", "WindowsClient"));
                }
                File.Move(file, file.Replace("TamelyClient", "WindowsClient"));
            }
        }

        public static void OpenBrowser(string url)
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
