using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mime;
using Newtonsoft.Json.Linq;
using SkiaSharp;

namespace SaturnBot.Commands;

public class ItemsCommand
{
    public static List<string> startingIDs = new List<string>();
    public static List<string> startingSkins = new List<string>();
    public static List<string> startingBackblings = new List<string>();
    public static List<string> startingEmotes = new List<string>();
    public static List<string> startingPickaxes = new List<string>();
    public static long GetItems()
    {
        return startingIDs.Count;
    }
    
    public static long GetSkins()
    {
        return startingSkins.Count;
    }
    
    public static long GetBackblings()
    {
        return startingBackblings.Count;
    }
    
    public static long GetEmotes()
    {
        return startingEmotes.Count;
    }
    
    public static long GetPickaxes()
    {
        return startingPickaxes.Count;
    }

    public static async Task SetItems()
    {
        using var wc = new WebClient();
        foreach (var item in JObject.Parse(wc.DownloadString("https://fortnite-api.com/v2/cosmetics/br"))["data"])
        {
            switch (item["type"]["backendValue"].ToString())
            {
                case "AthenaCharacter":
                    if (item["variants"] != null)
                    {
                        foreach (var variant in item["variants"])
                        {
                            if (variant["channel"].ToString().ToLower() is "material" or "parts")
                            {
                                startingIDs.Add(item["id"].ToString());
                                startingSkins.Add(item["id"].ToString());
                            }                        
                        } 
                    }
                    
                    startingIDs.Add(item["id"].ToString());
                    startingSkins.Add(item["id"].ToString());
                    break;
                case "AthenaBackpack":
                    startingIDs.Add(item["id"].ToString());
                    startingBackblings.Add(item["id"].ToString());
                    break;
                case "AthenaDance":
                    startingIDs.Add(item["id"].ToString());
                    startingEmotes.Add(item["id"].ToString());
                    break;
                case "AthenaPickaxe":
                    startingIDs.Add(item["id"].ToString());
                    startingPickaxes.Add(item["id"].ToString());
                    break;
            }
        }
    }
}