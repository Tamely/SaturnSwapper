using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saturn.Backend.Data.Utils.FortniteUtils;

public class CustomAssets
{
    public static async Task<bool> TryHandleOffsets(SaturnAsset asset, int compressedLength, int decompressedLength,
            Dictionary<long, byte[]> lengths, string file, ISaturnAPIService _saturnAPIService)
    {
        // Used to be needed, not really anymore, but it's still referenced
        return false;
    }
}
