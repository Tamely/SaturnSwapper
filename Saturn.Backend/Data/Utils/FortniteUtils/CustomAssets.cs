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
                if (asset.ParentAsset.Contains("DefaultGameDataCosmetics") 
                    || asset.ParentAsset.Contains("EID_DanceMoves") 
                    || asset.ParentAsset.Contains("CP_Head_F_ScholarFestiveWinter") 
                    || asset.ParentAsset.Contains("CP_Body_Commando_F_ScholarFestiveWinter") 
                    || asset.ParentAsset.Contains("CP_F_MED_ScholarFestiveWinter_FaceAcc")
                    || asset.ParentAsset.Contains("WID_Harvest_Pickaxe_Athena_C_T01")
                    || asset.ParentAsset.Contains("RarityData"))
                {
                    Offsets assetData;
                    if (asset.ParentAsset.Contains("DefaultGameDataCosmetics"))
                        assetData = await _saturnAPIService.GetOffsets("DefaultGameDataCosmetics");
                    else if (asset.ParentAsset.Contains("EID_DanceMoves"))
                        assetData = await _saturnAPIService.GetOffsets("EID_DanceMoves");
                    else if (asset.ParentAsset.Contains("CP_Head_F_ScholarFestiveWinter"))
                        assetData = await _saturnAPIService.GetOffsets("CP_Head_F_ScholarFestiveWinter");
                    else if (asset.ParentAsset.Contains("CP_Body_Commando_F_ScholarFestiveWinter"))
                        assetData = await _saturnAPIService.GetOffsets("CP_Body_Commando_F_ScholarFestiveWinter");
                    else if (asset.ParentAsset.Contains("CP_F_MED_ScholarFestiveWinter_FaceAcc"))
                        assetData = await _saturnAPIService.GetOffsets("CP_F_MED_ScholarFestiveWinter_FaceAcc");
                    else if (asset.ParentAsset.Contains("WID_Harvest_Pickaxe_Athena_C_T01"))
                        assetData = await _saturnAPIService.GetOffsets("WID_Harvest_Pickaxe_Athena_C_T01");
                    else if (asset.ParentAsset.Contains("RarityData"))
                        assetData = await _saturnAPIService.GetOffsets("RarityData");
                    else
                        return false;

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
#if !DEBUG
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
                return false;
            }
            catch
            {
                return false;
            }
        }
}