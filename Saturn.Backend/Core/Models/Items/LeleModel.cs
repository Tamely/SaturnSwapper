using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace Saturn.Backend.Core.Models.Items
{
    public class LelePlugin
    {
        [J("default_icon")] public string DefaultIcon { get; set; }
        [J("default_id")] public string DefaultId { get; set; }
        [J("default_name")] public string DefaultName { get; set; }

        [J("swapped_icon")] public string SwappedIcon { get; set; }
        [J("swapped_id")] public string SwappedId { get; set; }
        [J("swapped_name")] public string SwappedName { get; set; }

        [J("messages")] public List<MessageWithId> Messages { get; set; }
        [J("swaps")] public List<SwapEntry> Swaps { get; set; }
    }

    public class SwapEntry
    {
        [J("log")] public string Log { get; set; }
        [J("AssetPath")] public string AssetPath { get; set; }
        [J("swaps")] public JObject Swaps { get; set; }
        [J("hex")] public string Hex = "";
    }

    public class MessageWithId
    {
        public string Id { get; set; }
        public List<Message> localization { get; set; }
    }

    public class Message
    {
        public string languageId { get; set; }
        public string message { get; set; }
    }
}