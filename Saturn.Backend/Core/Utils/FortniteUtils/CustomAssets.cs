using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Saturn.Backend.Core.Models.Items;
using Saturn.Backend.Core.Models.SaturnAPI;
using Saturn.Backend.Core.Services;

namespace Saturn.Backend.Core.Utils.FortniteUtils;

public class CustomAssets
{
    public static async Task<bool> TryHandleOffsets(SaturnAsset asset, int compressedLength, int decompressedLength,
            Dictionary<long, byte[]> lengths, string file, ISaturnAPIService _saturnAPIService)
        {
           return false;
        }
} 
