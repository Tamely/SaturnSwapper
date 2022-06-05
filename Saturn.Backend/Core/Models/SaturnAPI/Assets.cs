using System.Collections.Generic;

namespace Saturn.Backend.Core.Models.SaturnAPI
{
    public class CustomAsset
    {
        public string DownloadUrl { get; set; }
    }

    public class Offsets
    {
        public string ParentAsset { get; set; }
        public List<long> CompressedOffsets { get; set; }
        public List<long> DecompressedOffsets { get; set; }
        public string SignatureFile { get; set; }
    }
}