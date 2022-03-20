using System.Globalization;
using System.Net.Http.Json;
using DSharpPlus;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using RestSharp;
using SaturnBot.Commands;
using SaturnBot.Models;
using SaturnBot.Utils;

namespace SaturnBot
{
    class Program
    {
        private static DiscordClient client { get; set; }
        
        private static List<ulong> BetaRoleIds = new(){755948859607220325, 784559494902054933, 754879988234322016, 754879988234322014, 850747667944177725, 850747444014612540, 874736280162934815};
        public static List<string> BetaUserIds = new();
        
        public static char Prefix = '.';

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            client = new DiscordClient(new DiscordConfiguration()
            {
                Token = "OTA5MjQ1MTc0MjYwMDQzODI2.YZBejg.mbEZrJSrwT3jOiHjbg3covAbFhI",
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });

            client.MessageCreated += CommandHandler.MessageOnCreate;

            await client.ConnectAsync();

            var saturn = await client.GetGuildAsync(754879988230127647);
            await saturn.GetChannelsAsync();

            await NewItems.SetNewItems();
            await ItemsCommand.SetItems();

            while (true)
            {
                foreach (var member in await saturn.GetAllMembersAsync())
                {
                    if (member == null || !member.Roles.Any()) continue;
                    foreach (var role in member.Roles)
                    {
                        if (!BetaRoleIds.Contains(role.Id) || BetaUserIds.Contains(member.Id.ToString())) continue;
                        BetaUserIds.Add(member.Id.ToString());
                    }
                }
                
                await UpdateDB.UpdateMongoDB();

                await NewItems.GetNewItems(saturn);

                await Task.Delay(600000);
            }

            await Task.Delay(-1);
        }
    }
}