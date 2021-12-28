using CUE4Parse.UE4.Objects.Core.Misc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Models.FortniteAPI
{
    public class DynamicKey
    {
        [JsonProperty("pakFilename")]
        public string PakFilename { get; set; }

        [JsonProperty("pakGuid")]
        public string PakGuid { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }

    public class Data
    {
        [JsonProperty("build")]
        public string Build { get; set; }

        [JsonProperty("mainKey")]
        public string MainKey { get; set; }

        [JsonProperty("dynamicKeys")]
        public List<DynamicKey> DynamicKeys { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }
    }

    public class AES
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }



    public class EncryptionKey
    {
        public FGuid Guid;
        public string FileName;
        public string Key;

        public EncryptionKey()
        {
            Guid = new();
            Key = String.Empty;
        }

        public EncryptionKey(FGuid guid, string key)
        {
            Guid = guid;
            Key = key;
        }
    }
}
