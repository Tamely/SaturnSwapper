using System.Collections.Generic;

namespace Saturn.Backend.Data.SaturnAPI.Models
{
    public class SaturnItemModel : IModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, CharacterPart> CharacterParts { get; set; }
        public List<SaturnItemModel> Options { get; set; } = new();
    }
}
