using System.Net;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using SaturnBot.Models;

namespace SaturnBot.Commands;

public class NewItems
{
    private static List<string> _ids = new();

    public static async Task SetNewItems()
    {
        var items = JsonConvert.DeserializeObject<NewItemsModel>(
            new WebClient().DownloadString("https://fortnite-api.com/v2/cosmetics/br/new")).Data;


        foreach (var item in items.Items.Where(item => !string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Id)).Where(item => item.Name != "null" && item.Id != "null"))
        {
            _ids.Add(item.Id);
            if (item.Variants is { Count: > 0 } && item.Type.BackendValue == "AthenaCharacter")
            {
                foreach (var variant in item.Variants)
                {
                    if (variant.Channel.ToLower() is "material" or "part")
                    {
                        _ids.Add(item.Id);
                    }
                }
            }
        }
    }
    
    public static async Task GetNewItems(DiscordGuild saturn)
    {
        Dictionary<string, string> output = new();
        var items = JsonConvert.DeserializeObject<NewItemsModel>(
            new WebClient().DownloadString("https://fortnite-api.com/v2/cosmetics/br/new")).Data;
        
        List<string> newIDs = new();

        foreach (var item in items.Items)
        {
            if (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Id))
            {
                if (item.Name != "null" && item.Id != "null" && item.Type.BackendValue is "AthenaCharacter" or "AthenaBackpack" or "AthenaDance" or "AthenaPickaxe") 
                {
                    if (!_ids.Contains(item.Id))
                    {
                        output.Add(item.Name, item.Id);

                        switch (item.Type.BackendValue)
                        {
                            case "AthenaCharacter":
                                if (item.Variants is { Count: > 0 })
                                {
                                    foreach (var variant in item.Variants)
                                    {
                                        if (variant.Channel.ToLower() is "material" or "part")
                                        {
                                            ItemsCommand.startingIDs.Add(item.Id);
                                            ItemsCommand.startingSkins.Add(item.Id);
                                        }
                                    }
                                }
                                
                                ItemsCommand.startingIDs.Add(item.Id);
                                ItemsCommand.startingSkins.Add(item.Id);
                                break;
                            case "AthenaBackpack":
                                ItemsCommand.startingIDs.Add(item.Id);
                                ItemsCommand.startingBackblings.Add(item.Id);
                                break;
                            case "AthenaDance":
                                ItemsCommand.startingIDs.Add(item.Id);
                                ItemsCommand.startingEmotes.Add(item.Id);
                                break;
                            case "AthenaPickaxe":
                                ItemsCommand.startingIDs.Add(item.Id);
                                ItemsCommand.startingPickaxes.Add(item.Id);
                                break;
                        }
                    }

                    newIDs.Add(item.Id);
                }
            }

        }
        
        _ids = new List<string>(newIDs);

        if (output.Any())
        {
            var embed = new DiscordEmbedBuilder();
            embed.WithTitle("New Items");
            embed.Description = "New items have been added to the swapper.";
            
            foreach (var item in output)
            {
                if (embed.Description.Length < 4000)
                    embed.Description += Environment.NewLine + $"{item.Key} - {item.Value}";
            }

            await saturn.Channels[850817788536422421].SendMessageAsync(embed);
        }
    }
}