﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IniParser.Model;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.CloudStorage;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils;
using Saturn.Backend.Data.Utils.CloudStorage;
using Serilog;

namespace Saturn.Backend.Data.Services
{
    public interface ICloudStorageService
    {
        public string GetChanges(string optionName, string itemDef);
        public Changes DecodeChanges(string changes);
        public SectionDataCollection GetSections();
        public void SetChanges();
    }

    public class CloudStorageService : ICloudStorageService
    {
        private readonly ISaturnAPIService _saturnAPIService;
        private readonly IniUtil CloudChanges = new (Config.CloudStoragePath);

        public CloudStorageService(ISaturnAPIService saturnAPIService)
        {
            _saturnAPIService = saturnAPIService;
            Trace.WriteLine("Getting CloudStorage");
            CloudStorage = _saturnAPIService.ReturnEndpoint("api/v1/Saturn/CloudStorage");
            Trace.WriteLine("Done");
            File.WriteAllText(Config.CloudStoragePath, CloudStorage);
        }

        private string CloudStorage { get; }

        public string GetChanges(string optionName, string itemDef)
            => CloudChanges.Read(optionName, itemDef);

        public Changes DecodeChanges(string changes)
            => JsonConvert.DeserializeObject<Changes>(changes) ?? new Changes();

        public SectionDataCollection GetSections()
            => CloudChanges.GetSections();

        public void SetChanges()
        {
            Logger.Log("Changes: " + JsonConvert.SerializeObject(new Changes()
            {

            }));
        }
    }
}