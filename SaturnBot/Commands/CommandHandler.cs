using System.Reflection.Metadata;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SkiaSharp;

namespace SaturnBot.Commands;

public class CommandHandler
{
    public static async Task MessageOnCreate(object sender, MessageCreateEventArgs e)
    {
        if (e.Message.Author.IsBot)
            return;

        if (e.Message.Content.StartsWith(Program.Prefix))
        {
            var message = e.Message.Content.Remove(0, 1).ToLower();

            if (message == "help")
            {
                var embed = new DiscordEmbedBuilder();
                
                embed.WithTitle("Commands");
                embed.WithDescription("Saturn's current commands are:\n" +
                                      "`help` - Shows this message\n" +
                                      "`items` - Returns the number of items in the swapper\n" +
                                      "`skins` - Returns the number of skins in the swapper\n" +
                                      "`backblings` - Returns the number of backblings in the swapper\n" +
                                      "`pickaxes` - Returns the number of pickaxes in the swapper\n" +
                                      "`emotes` - Returns the number of emotes in the swapper\n" +
                                      "`key` - Returns the link to get a key to the swapper\n");
                
                await e.Message.RespondAsync(embed);
                return;
            }

            if (message is "key")
            {
                var embed = new DiscordEmbedBuilder();
                embed.WithTitle("Saturn Key");
                embed.WithDescription("Here is the link to get a key to the swapper:\n" +
                                      "https://up-to-down.net/88495/saturn-swapper-key");
                await e.Message.RespondAsync(embed);
            }

            if (message is "items" or "skins" or "backblings" or "pickaxes" or "emotes")
            {
                var itemsCount = message switch
                {
                    "items" => ItemsCommand.GetItems(),
                    "skins" => ItemsCommand.GetSkins(),
                    "backblings" => ItemsCommand.GetBackblings(),
                    "pickaxes" => ItemsCommand.GetPickaxes(),
                    "emotes" => ItemsCommand.GetEmotes()
                };
            
                var embed = new DiscordEmbedBuilder();
                embed.WithTitle("Saturn Items");
                embed.WithDescription($"Saturn currently has {itemsCount} swappable {message}.\n" +
                                      "Note: This count does not include item styles and/or special items added through the Hotfix/CloudStorage feature.");

                await e.Message.RespondAsync(embed);
            }

        }
    }
}