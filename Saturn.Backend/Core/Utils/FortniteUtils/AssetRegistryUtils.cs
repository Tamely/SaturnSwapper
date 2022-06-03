using CUE4Parse.FileProvider;
using CUE4Parse.UE4.AssetRegistry;
using CUE4Parse.UE4.AssetRegistry.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Backend.Core.Services
{
    public class AssetRegistryUtils
    {
        private readonly DefaultFileProvider _provider;
        private readonly FAssetData[] data;

        public AssetRegistryUtils(DefaultFileProvider provider)
        {
            _provider = provider;
            var file = _provider["FortniteGame/AssetRegistry.bin"];
            using var reader = file.CreateReader();
            data = new FAssetRegistryState(reader).PreallocatedAssetDataBuffers;
        }

        public async Task<List<string>> ReturnCPsForSkin(string skinId)
        {
            var result = new List<string>();

            foreach (var obj in data)
            {
                if (obj.AssetName.Text == skinId)
                {
                    if (obj.TaggedAssetBundles.Bundles.Length <= 1) continue;
                    foreach (var cp in obj.TaggedAssetBundles.Bundles[1].BundleAssets)
                    {
                        result.Add(cp.AssetPathName.Text);
                    }

                    break;
                }
            }

            return result;
        }
    }
}
