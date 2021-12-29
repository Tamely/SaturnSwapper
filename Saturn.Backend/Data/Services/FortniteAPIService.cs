using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils;
using Serilog;
using Type = Saturn.Backend.Data.Models.FortniteAPI.Type;

namespace Saturn.Backend.Data.Services
{
    public interface IFortniteAPIService
    {
        public Task<List<Cosmetic>> GetSaturnSkins();
        public Task<List<Cosmetic>> GetSaturnBackblings();
        public Task<List<Cosmetic>> GetSaturnPickaxes();
        public Task<List<Cosmetic>> GetSaturnDances();
        public Models.FortniteAPI.Data GetAES();
        public Task<List<Cosmetic>> AreItemsConverted(List<Cosmetic> items);
    }

    public class FortniteAPIService : IFortniteAPIService
    {
        private readonly IConfigService _configService;
        private readonly IDiscordRPCService _discordRPCService;
        private readonly ICloudStorageService _cloudStorageService;


        private readonly Uri Base = new("https://fortnite-api.com/v2/");


        public FortniteAPIService(IConfigService configService, IDiscordRPCService discordRPCService, ICloudStorageService cloudStorageService)
        {
            _configService = configService;
            _discordRPCService = discordRPCService;
            _cloudStorageService = cloudStorageService;
        }

        private Uri AES => new(Base, "aes");


        public Models.FortniteAPI.Data GetAES()
        {
            var data = GetData(AES);
            return JsonConvert.DeserializeObject<AES>(data).Data;
        }

        public async Task<List<Cosmetic>> GetSaturnDances()
        {
            var data = await GetDataAsync(CosmeticsByType("AthenaDance"));
            var Emotes = JsonConvert.DeserializeObject<CosmeticList>(data);
            
            Emotes.Data.RemoveAll(x => x.Name.ToLower() is "null" or "tbd");

            foreach (var item in Emotes.Data.Where(item => item.Name.ToLower() == "random"))
            {
                item.IsRandom = true;
            }
            
            Trace.WriteLine($"Deserialized {Emotes.Data.Count} objects");

            _discordRPCService.UpdatePresence($"Looking at {Emotes.Data.Count} different emotes");
            return await AreItemsConverted(Emotes.Data);
        }

        public async Task<List<Cosmetic>> GetSaturnBackblings()
        {
            var data = await GetDataAsync(CosmeticsByType("AthenaBackpack"));
            var Backs = JsonConvert.DeserializeObject<CosmeticList>(data);
            
            Backs.Data.RemoveAll(x => x.Name.ToLower() is "null" or "tbd");

            foreach (var item in Backs.Data.Where(item => item.Name.ToLower() == "random"))
            {
                item.IsRandom = true;
            }
            
            Trace.WriteLine($"Deserialized {Backs.Data.Count} objects");

            _discordRPCService.UpdatePresence($"Looking at {Backs.Data.Count} different backpacks");
            return await AreItemsConverted(Backs.Data);
        }
        
        public async Task<List<Cosmetic>> GetSaturnPickaxes()
        {
            var data = await GetDataAsync(CosmeticsByType("AthenaPickaxe"));
            var Picks = JsonConvert.DeserializeObject<CosmeticList>(data);
            
            Picks.Data.RemoveAll(x => x.Name.ToLower() is "null" or "tbd");

            foreach (var item in Picks.Data.Where(item => item.Name.ToLower() == "random"))
            {
                item.IsRandom = true;
            }
            
            Trace.WriteLine($"Deserialized {Picks.Data.Count} objects");

            _discordRPCService.UpdatePresence($"Looking at {Picks.Data.Count} different pickaxes");
            return await AreItemsConverted(Picks.Data);
        }

        public async Task<List<Cosmetic>> GetSaturnSkins()
        {
            var data = await GetDataAsync(CosmeticsByType("AthenaCharacter"));
            var Skins = JsonConvert.DeserializeObject<CosmeticList>(data);

            
            Skins.Data.RemoveAll(x => x.Name.ToLower() is "null" or "tbd");
            
            foreach (var item in Skins.Data.Where(item => item.Name.ToLower() == "random"))
            {
                item.IsRandom = true;
            }
            
            Trace.WriteLine($"Deserialized {Skins.Data.Count} objects");

            _discordRPCService.UpdatePresence($"Looking at {Skins.Data.Count} different skins");

            return await AreItemsConverted(await IsHatTypeDifferent(Skins.Data));
        }

        private async Task<List<Cosmetic>> IsHatTypeDifferent(List<Cosmetic> skins)
        {
            Logger.Log("Getting hat types");
            var DifferentHatsStr = _cloudStorageService.GetChanges("Skins", "HatTypes");

            Logger.Log("Decoding hat types");
            var DifferentHats = _cloudStorageService.DecodeChanges(DifferentHatsStr);
            
            foreach (var skin in skins)
            {
                skin.CosmeticOptions= new()
                {
                    new SaturnItem
                    {
                        ItemDefinition = "CID_A_311_Athena_Commando_F_ScholarFestiveWinter",
                        Name = "Blizzabelle",
                        Description = "Voted Teen Queen of Winterfest by a jury of her witchy peers.",
                        Icon = "https://fortnite-api.com/images/cosmetics/br/cid_a_311_athena_commando_f_scholarfestivewinter/smallicon.png",
                        Rarity = "Rare"
                    },
                    new SaturnItem
                    {
                        ItemDefinition = "CID_A_007_Athena_Commando_F_StreetFashionEclipse",
                        Name = "Ruby Shadows",
                        Description = "Sometimes you gotta go dark.",
                        Icon = "https://fortnite-api.com/images/cosmetics/br/cid_a_007_athena_commando_f_streetfashioneclipse/smallicon.png",
                        Rarity = "Epic",
                        Series = "ShadowSeries"
                    },
                    new SaturnItem
                    {
                        ItemDefinition = "CID_936_Athena_Commando_F_RaiderSilver",
                        Name = "Diamond Diva",
                        Description = "Synthetic diamonds need not apply.",
                        Icon = "https://fortnite-api.com/images/cosmetics/br/cid_936_athena_commando_f_raidersilver/smallicon.png",
                        Rarity = "Rare"
                    },
                    new SaturnItem
                    {
                        ItemDefinition = "CID_784_Athena_Commando_F_RenegadeRaiderFire",
                        Name = "Blaze",
                        Description = "Fill the world with flames.",
                        Icon = "https://fortnite-api.com/images/cosmetics/br/cid_784_athena_commando_f_renegaderaiderfire/smallicon.png",
                        Rarity = "Legendary"
                    }
                };
                
                if (skin.IsRandom)
                {
                    skin.CosmeticOptions= new()
                    {
                        new SaturnItem
                        {
                            ItemDefinition = "CID_A_311_Athena_Commando_F_ScholarFestiveWinter",
                            Name = "Blizzabelle",
                            Description = "Voted Teen Queen of Winterfest by a jury of her witchy peers.",
                            Icon = "https://fortnite-api.com/images/cosmetics/br/cid_a_311_athena_commando_f_scholarfestivewinter/smallicon.png",
                            Rarity = "Rare"
                        },
                        new SaturnItem
                        {
                            ItemDefinition = "CID_A_007_Athena_Commando_F_StreetFashionEclipse",
                            Name = "Ruby Shadows",
                            Description = "Sometimes you gotta go dark.",
                            Icon = "https://fortnite-api.com/images/cosmetics/br/cid_a_007_athena_commando_f_streetfashioneclipse/smallicon.png",
                            Rarity = "Epic",
                            Series = "ShadowSeries"
                        },
                        new SaturnItem
                        {
                            ItemDefinition = "CID_936_Athena_Commando_F_RaiderSilver",
                            Name = "Diamond Diva",
                            Description = "Synthetic diamonds need not apply.",
                            Icon = "https://fortnite-api.com/images/cosmetics/br/cid_936_athena_commando_f_raidersilver/smallicon.png",
                            Rarity = "Rare"
                        },
                        new SaturnItem
                        {
                            ItemDefinition = "CID_784_Athena_Commando_F_RenegadeRaiderFire",
                            Name = "Blaze",
                            Description = "Fill the world with flames.",
                            Icon = "https://fortnite-api.com/images/cosmetics/br/cid_784_athena_commando_f_renegaderaiderfire/smallicon.png",
                            Rarity = "Legendary"
                        },
                        new SaturnItem
                        {
                            ItemDefinition = "CID_162_Athena_Commando_F_StreetRacer",
                            Name = "Redline",
                            Description = "Revving beyond the limit.",
                            Icon =
                                "https://fortnite-api.com/images/cosmetics/br/cid_162_athena_commando_f_streetracer/smallicon.png",
                            Rarity = "Epic"
                        }
                    };
                }

                if (DifferentHats.HatSkins.IndexOf(skin.Id) != -1)
                {
                    skin.HatTypes = HatTypes.HT_Hat;
                    skin.CosmeticOptions = new List<SaturnItem>()
                    {
                        new SaturnItem
                        {
                            ItemDefinition = "CID_162_Athena_Commando_F_StreetRacer",
                            Name = "Redline",
                            Description = "Revving beyond the limit.",
                            Icon =
                                "https://fortnite-api.com/images/cosmetics/br/cid_162_athena_commando_f_streetracer/smallicon.png",
                            Rarity = "Epic"
                        }
                    };
                }
            }
            
            return skins;
        }

        public async Task<List<Cosmetic>> AreItemsConverted(List<Cosmetic> items)
        {
            var ret = items;

            var convertedItems = await _configService.TryGetConvertedItems();
            convertedItems.Any(x => ret.Any(y =>
            {

                if (y.Id != x.ItemDefinition) return false;
                y.IsConverted = true;
                return true;
            }));

            return ret;
        }

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

        private Uri CosmeticsByType(string type)
        {
            return new(Base, $"cosmetics/br/search/all?backendType={type}");
        }
    }
}