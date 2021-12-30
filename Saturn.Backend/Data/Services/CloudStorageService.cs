using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Saturn.Backend.Data.Models.CloudStorage;
using Saturn.Backend.Data.Utils;
using Saturn.Backend.Data.Utils.CloudStorage;

namespace Saturn.Backend.Data.Services
{
    public interface ICloudStorageService
    {
        public string GetChanges(string optionDef, string itemDef);
        public Changes DecodeChanges(string changes);
    }

    public class CloudStorageService : ICloudStorageService
    {
        private readonly ISaturnAPIService _saturnAPIService;
        private readonly IniUtil CloudChanges = new(Config.CloudStoragePath);

        public CloudStorageService(ISaturnAPIService saturnAPIService)
        {
            _saturnAPIService = saturnAPIService;
            Trace.WriteLine("Getting CloudStorage");
            CloudStorage = _saturnAPIService.ReturnEndpoint("api/v1/Saturn/CloudStorage");
            Trace.WriteLine("Done");
            File.WriteAllText(Config.CloudStoragePath, CloudStorage);
        }

        private string CloudStorage { get; }

        public string GetChanges(string optionDef, string itemDef)
            => CloudChanges.Read(optionDef, itemDef);

        public Changes DecodeChanges(string changes)
            => JsonConvert.DeserializeObject<Changes>(changes) ?? new Changes();
    }
}