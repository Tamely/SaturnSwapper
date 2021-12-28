using CUE4Parse.UE4.Objects.Core.Misc;
using Newtonsoft.Json;
using Saturn.Data.Models.FortniteAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Services
{
    public interface IFortniteAPIService
    {
        public Task<List<Cosmetic>> GetSaturnSkins();
        public Task<List<Cosmetic>> GetSaturnBackblings();
        public Task<List<Cosmetic>> GetSaturnDances();
        public Models.FortniteAPI.Data GetAES();
        public Task<List<Cosmetic>> AreItemsConverted(List<Cosmetic> items);
    }

    public class FortniteAPIService : IFortniteAPIService
    {
        private readonly IConfigService _configService;
        private readonly IDiscordRPCService _discordRPCService;


        public FortniteAPIService(IConfigService configService, IDiscordRPCService discordRPCService)
        {
            _configService = configService;
            _discordRPCService = discordRPCService;
        }


        Uri Base = new("https://fortnite-api.com/v2/");

        public string GetData(Uri uri)
        {
            using var wc = new WebClient();
            return wc.DownloadString(uri);
        }

        public async Task<string> GetDataAsync(Uri uri)
        {
            using var wc = new WebClient();
            return await wc.DownloadStringTaskAsync(uri);
        }

        Uri AES => new Uri(Base, "aes");
        Uri CosmeticsByType(string type) => new Uri(Base, $"cosmetics/br/search/all?backendType={type}");


        public Models.FortniteAPI.Data GetAES()
        {
            var data = GetData(AES);
            return JsonConvert.DeserializeObject<AES>(data).Data;
        }
        public async Task<List<Cosmetic>> GetSaturnDances()
        {
            var data = await GetDataAsync(CosmeticsByType("AthenaDance"));
            var Emotes = JsonConvert.DeserializeObject<CosmeticList>(data);
            Trace.WriteLine($"Deserialized {Emotes.Data.Count} objects");

            _discordRPCService.UpdatePresence($"Looking at {Emotes.Data.Count} different emotes");
            return await AreItemsConverted(Emotes.Data);
        }

        public async Task<List<Cosmetic>> GetSaturnBackblings()
        {
            var data = await GetDataAsync(CosmeticsByType("AthenaBackpack"));
            var Backs = JsonConvert.DeserializeObject<CosmeticList>(data);
            Trace.WriteLine($"Deserialized {Backs.Data.Count} objects");

            _discordRPCService.UpdatePresence($"Looking at {Backs.Data.Count} different backpacks");
            return await AreItemsConverted(Backs.Data);
        }

        public async Task<List<Cosmetic>> GetSaturnSkins()
        {
            var data = await GetDataAsync(CosmeticsByType("AthenaCharacter"));
            var Skins = JsonConvert.DeserializeObject<CosmeticList>(data);
            Trace.WriteLine($"Deserialized {Skins.Data.Count} objects");

            _discordRPCService.UpdatePresence($"Looking at {Skins.Data.Count} different skins");
            return await AreItemsConverted(Skins.Data);
        }

        public async Task<List<Cosmetic>> AreItemsConverted(List<Cosmetic> items)
        {
            List<Cosmetic> ret = items;

            var convertedItems = await _configService.TryGetConvertedItems();
            convertedItems.Any(x => ret.Any(y =>
            {
                if (y.Id != x.ItemDefinition) return false;
                y.IsConverted = true;
                return true;
            }));

            return ret;
        }
    }
}
