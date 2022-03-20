using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Models.SaturnAPI;
using Saturn.Backend.Data.Services;

namespace Saturn.Backend.Data.Utils.FortniteUtils;

public class CustomAssets
{
    public static async Task<bool> TryHandleOffsets(SaturnAsset asset, int compressedLength, int decompressedLength,
            Dictionary<long, byte[]> lengths, string file, ISaturnAPIService _saturnAPIService)
        {
            try
            {
                Offsets assetData;

                if (Config.isMaintenance)
                    return false;

                if (await _saturnAPIService.GetOffsets(Path.GetFileNameWithoutExtension(asset.ParentAsset)) == null)
                    return false;
                    
                assetData = await _saturnAPIService.GetOffsets(Path.GetFileNameWithoutExtension(asset.ParentAsset));

                var n = 1;

                #region Handling compressed offsets

                foreach (var compressedOffset in assetData.CompressedOffsets)
                    if (n == 1)
                    {
                        lengths.Add(compressedOffset,
                            FileUtil.GetBytes(
                                await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
                                compressedOffset, 2));

                        FileUtil.WriteIntToFile(Path.Combine(FortniteUtil.PakPath, file),
                            compressedOffset, compressedLength);

                        n++;
                    }
                    else
                    {
                        lengths.Add(compressedOffset,
                            FileUtil.GetBytes(
                                await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
                                compressedOffset, 2));

                        FileUtil.WriteHexToFile(Path.Combine(FortniteUtil.PakPath, file),
                            compressedOffset, FileUtil.IntToHex(compressedLength));

                        n++;
                    }

                #endregion

                n = 1;

                #region Handling decompressed offsets

                foreach (var decompressedOffset in assetData.DecompressedOffsets)
                    if (n == 1)
                    {
                        lengths.Add(decompressedOffset,
                            FileUtil.GetBytes(
                                await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
                                decompressedOffset, 2));

                        FileUtil.WriteIntToFile(Path.Combine(FortniteUtil.PakPath, file),
                            decompressedOffset, decompressedLength);

                        n++;
                    }
#if DEBUG
                    else if (n == 2)
                    {
                        lengths.Add(decompressedOffset,
                            FileUtil.GetBytes(
                                await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
                                decompressedOffset, 2));

                        FileUtil.WriteHexToFile(Path.Combine(FortniteUtil.PakPath, file),
                            decompressedOffset, FileUtil.IntToHex(decompressedLength + 20));

                        n++;
                    }
#endif
                    else
                    {
                        lengths.Add(decompressedOffset,
                            FileUtil.GetBytes(
                                await File.ReadAllBytesAsync(Path.Combine(FortniteUtil.PakPath, file)),
                                decompressedOffset, 2));

                        FileUtil.WriteHexToFile(Path.Combine(FortniteUtil.PakPath, file),
                            decompressedOffset, FileUtil.IntToHex(decompressedLength));

                        n++;
                    }

                #endregion

                return true;
            }
            catch
            {
                return false;
            }
        }
} 
